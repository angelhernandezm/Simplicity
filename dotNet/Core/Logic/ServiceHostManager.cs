using Simplicity.dotNet.Common;
using Simplicity.dotNet.Common.Data.Models;
using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using Simplicity.dotNet.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IServiceHostManager" />
	public class ServiceHostManager : IServiceHostManager {
		/// <summary>
		/// The logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The data service
		/// </summary>
		private IDataService _dataService;

		/// <summary>
		/// The configuration
		/// </summary>
		private IConfigurationReader _configuration;

		/// <summary>
		/// The jni bridge manager
		/// </summary>
		private IJniBridgeManager _jniBridgeManager;

		/// <summary>
		/// Gets the dynamic hosts.
		/// </summary>
		/// <value>
		/// The dynamic hosts.
		/// </value>
		public Dictionary<IEntity, ServiceHost> DynamicHosts {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceHostManager" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="dataService">The data service.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="bridgeManager">The bridge manager.</param>
		public ServiceHostManager(ILogger logger, IDataService dataService, IConfigurationReader config, IJniBridgeManager bridgeManager) {
			_logger = logger;
			_configuration = config;
			_dataService = dataService;
			_jniBridgeManager = bridgeManager;
			DynamicHosts = new Dictionary<IEntity, ServiceHost>();
		}

		/// <summary>
		/// Manages the hosts (starts & stops). If no entity is passed in then it'll load everything
		/// as per configuration in database, otherwise it'll load the one that's been specified.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="host">The host.</param>
		/// <returns>Collection of errors encountered.</returns>
		public IEnumerable<ExecutionResult> ManageHost(HostAction action, IEntity host = null) {
			DynamicLibrary library;
			var retval = new List<ExecutionResult>();
			KeyValuePair<string, Tuple<IEntity, IEntity, IEntity>> single;
			Dictionary<string, Tuple<IEntity, IEntity, IEntity>> registration;
			var libraries = new Dictionary<string, Tuple<IEntity, IEntity, IEntity>>();

			try {
				// Do we have to process one library or all of them?
				library = host as DynamicLibrary;
				var result = _dataService.GetDynamicLibraries(library?.AssemblyName);

				if (result.IsSuccess && (registration = result.Tag as
						 Dictionary<string, Tuple<IEntity, IEntity, IEntity>>) != null) {

					if (!string.IsNullOrEmpty((single = registration.FirstOrDefault()).Key))
						libraries.Add(single.Key, single.Value);
					else {
						var allRegistrations = registration.ToList();
						allRegistrations.ForEach(r => libraries.Add(r.Key, r.Value));
					}
					retval.AddRange(ManageHostHelper(action, libraries.ToList()));
				}
			} catch (Exception e) {
				retval.Add(new ExecutionResult() { LastExceptionIfAny = e });
			}

			return retval;
		}


		/// <summary>
		/// Gets the type of the dynamic proxy.
		/// </summary>
		/// <param name="selectedLibrary">The selected library.</param>
		/// <returns></returns>
		private Tuple<Type, Type> GetDynamicProxyType(Tuple<IEntity, IEntity, IEntity> selectedLibrary) {
			Tuple<Type, Type> retval = null;

			try {
				var classFqn = ((JavaClassMetadata)selectedLibrary.Item3).ClassName;
				var classSep = classFqn.LastIndexOf('.');
				var className = string.Concat(classFqn.Substring(0, classSep).Replace('.', '_'), classFqn.Substring(classSep));
				var interfaceName = className.Insert(classSep + 1, "I"); // Add the I for interface to the class name
				var asm = Assembly.LoadFrom(((DynamicLibrary)selectedLibrary.Item1).AssemblyLocation);
				var concrete = asm.GetTypes().FirstOrDefault(t => string.Equals(t.FullName, className, StringComparison.OrdinalIgnoreCase));
				var definition = asm.GetTypes().FirstOrDefault(t => string.Equals(t.FullName, interfaceName, StringComparison.OrdinalIgnoreCase));
				retval = Tuple.Create(concrete, definition);
			} catch {
				// Safe to swallow exception here because we'll use
				// return value as error or success condition
			}

			return retval;
		}

		/// <summary>
		/// Manages the host helper.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="allRegistrations">All registrations.</param>
		/// <returns></returns>
		private IEnumerable<ExecutionResult> ManageHostHelper(HostAction action, List<KeyValuePair<string, Tuple<IEntity, IEntity, IEntity>>> allRegistrations) {
			Tuple<IEntity, IEntity, IEntity> selected;
			var retval = new List<ExecutionResult>();
			var config = (CustomConfigReader)_configuration.Configuration;
			var baseAddress = $"http://{Environment.MachineName}:{config.dotNetOptions.hostPort}";

			foreach (var r in allRegistrations) {
				try {
					selected = r.Value;
					var buffer = string.Empty;
					ServiceHost newHost = null;
					Tuple<Type, Type> hostType = null;
					ServiceConfiguration svcConfig = null;
					var hosting = (HostedLibrary)selected.Item2;
					var metadata = (JavaClassMetadata)selected.Item3;
					var selectedLibrary = (DynamicLibrary)selected.Item1;
					// Let's load specified Jar (if it's not loaded)
					 _jniBridgeManager.AddPath(selectedLibrary.JarFileLocation, ref buffer);

					if ((hostType = GetDynamicProxyType(selected)) != null) {
						var address = string.Concat(baseAddress, hosting.LibraryURI);
						// Let's instantiate our ServiceHost
						newHost = new ServiceHost(hostType.Item1, new Uri[] { new Uri(address) });
						// Let's configure the service
						var svc = svcConfig.Create(ref newHost);
						var se = new ServiceEndpoint(ContractDescription.GetContract(hostType.Item2),
														   new BasicHttpBinding(), new EndpointAddress(new Uri(address)));
						svc?.AddServiceEndpoint(se);
						svc?.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });

						// If debug is not enabled, we'll enable it
						if (svc?.Description.Behaviors.FirstOrDefault(p => p.GetType() == typeof(ServiceDebugBehavior)) == null)
							svc?.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
					} else  // If we couldn't load dynamic library, we just skip processing the offending library
						throw new NullReferenceException("Type for ServiceHost couldn't be created. Unable to continue.");

					// Let's see if DynamicHosts already has a ServiceHost for the selected library
					var found = DynamicHosts.FirstOrDefault(p => string.Equals(p.Value?.Description?.ConfigurationName?.Replace("_", "."), metadata.ClassName));

					// Should we start listening or stop our ServiceHost?
					if (action == HostAction.StartListening) {
						if (found.Key != null && DynamicHosts.ContainsKey(found.Key)) {
							DynamicHosts[found.Key].Close();
							DynamicHosts[found.Key] = newHost;
						} else {
							DynamicHosts.Add(selectedLibrary, newHost);
						}
						DynamicHosts[found.Key ?? selectedLibrary].Open();
					} else {
						var key = found.Key ?? selectedLibrary;
						if (DynamicHosts.ContainsKey(key)) {
							DynamicHosts[key].Close();
							DynamicHosts.Remove(key);
						}
					}
				} catch (Exception e) {
					retval.Add(new ExecutionResult() { LastExceptionIfAny = e });
				}
			}

			return retval;
		}
	}
}