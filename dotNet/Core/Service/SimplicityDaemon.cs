using Autofac;
using Simplicity.dotNet.Common.Data;
using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using Simplicity.dotNet.Common.Service;
using Simplicity.dotNet.Core.Configuration;
using Simplicity.dotNet.Core.Infrastructure;
using Simplicity.dotNet.Core.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
namespace Simplicity.dotNet.Core.Service {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.ServiceProcess.ServiceBase" />
	/// <seealso cref="Simplicity.dotNet.Core.Service.ICustomServiceBase{Simplicity.dotNet.Core.Service.IServiceInformation, Autofac.IContainer}" />
	/// <seealso cref="Simplicity.dotNet.Core.Service.IBaseService{Simplicity.dotNet.Core.Service.SimplicityDaemon}" />
	public sealed class SimplicityDaemon : ServiceBase, ICustomServiceBase<IServiceInformation, IContainer>, IBaseService<SimplicityDaemon> {
		/// <summary>
		/// The _logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The _main watcher
		/// </summary>
		private FileSystemWatcher _mainWatcher;

		/// <summary>
		/// The jni bridge manager
		/// </summary>
		private IJniBridgeManager _jniBridgeManager;

		/// <summary>
		/// The data service
		/// </summary>
		private IDataService _dataService;

		/// <summary>
		/// The default parser
		/// </summary>
		private IParser _defaultParser;

		/// <summary>
		/// The default builder
		/// </summary>
		private IBuilder _defaultBuilder;

		/// <summary>
		/// The start arguments
		/// </summary>
		private readonly string[] _startArguments;

		/// <summary>
		/// The _configuration reader
		/// </summary>
		private IConfigurationReader _configurationReader;

		/// <summary>
		/// The messenger
		/// </summary>
		private IMessenger _messenger;

		/// <summary>
		/// The perf counters
		/// </summary>
		private IPerfCounters _perfCounters;

		/// <summary>
		/// The components
		/// </summary>
		private readonly System.ComponentModel.IContainer _components = new System.ComponentModel.Container();

		/// <summary>
		/// The _config file monitor
		/// </summary>
		private readonly FileSystemWatcher _configFileMonitor;

		/// <summary>
		/// Gets or sets the type container.
		/// </summary>
		/// <value>
		/// The type container.
		/// </value>
		public IContainer TypeContainer {
			get {
				return GlobalContainer.Current.TypeContainer;
			}
			set {
				GlobalContainer.Current.TypeContainer = value;
			}
		}

		/// <summary>
		/// Gets the host manager.
		/// </summary>
		/// <value>
		/// The host manager.
		/// </value>
		public IServiceHostManager HostManager {
			get; private set;
		}

		/// <summary>
		/// The files to process
		/// </summary>
		private readonly Dictionary<Guid, KeyValuePair<string, string>> _filesToProcess = new Dictionary<Guid, KeyValuePair<string, string>>();

		/// <summary>
		/// The TMR process file
		/// </summary>
		private System.Timers.Timer _tmrProcessFile;

		/// <summary>
		/// The TMR house keeping
		/// </summary>
		private System.Timers.Timer _tmrHouseKeeping;

		/// <summary>
		/// The protect mutex
		/// </summary>
		private readonly Mutex _protectMutex = new Mutex();

		/// <summary>
		/// Gets the service instance.
		/// </summary>
		/// <value>
		/// The service instance.
		/// </value>
		/// <exception cref="System.NotImplementedException"></exception>
		public SimplicityDaemon ServiceInstance => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimplicityDaemon"/> class.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public SimplicityDaemon(string[] args) {
			_startArguments = args;
			ServiceName = Common.Strings.ServiceName;
			Bootstrapper.Run(InitializeTypeContainer);
			var svcHomeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_configFileMonitor = new FileSystemWatcher(svcHomeDir) {
				EnableRaisingEvents = true, Filter = Common.Strings.ConfigFileExtension,
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
			};

			Task.Run(async () => await SetUpService()); 
		}

		/// <summary>
		/// Sets up service.
		/// </summary>
		private Task SetUpService() {
			return Task.Run(() => {
				_configurationReader = TypeContainer.Resolve<IConfigurationReader>();
				_defaultParser = TypeContainer.Resolve<IParser>();
				_defaultBuilder = TypeContainer.Resolve<IBuilder>();
				_dataService = TypeContainer.Resolve<IDataService>();
				_messenger = TypeContainer.Resolve<IMessenger>();
				_perfCounters = TypeContainer.Resolve<IPerfCounters>();
				ExecutionResult.AttachPerfCounters(_perfCounters);
				ExecutionResult.AttachLogger(_logger);
				HostManager = TypeContainer.Resolve<IServiceHostManager>();
				_messenger.Notify += MessengerNotification;
				_configFileMonitor.Changed += (s, e) => ReloadAndApplyConfigChanges();
				Configure();
			});
		}

		/// <summary>
		/// Messengers the notify.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="type">The type.</param>
		/// <param name="data">The data.</param>
		private void MessengerNotification(object sender, Common.Enums.MessageType type, object data) {
			KeyValuePair<string, Tuple<IEntity, IEntity, IEntity>> single;
			Dictionary<string, Tuple<IEntity, IEntity, IEntity>> registration;

			switch (type) {
				case Common.Enums.MessageType.General:
					break;
				case Common.Enums.MessageType.NewRegistration:
					var result = _dataService.GetDynamicLibraries(Path.GetFileName(data.ToString()));

					if (result.IsSuccess && (registration = result.Tag as
						 Dictionary<string, Tuple<IEntity, IEntity, IEntity>>) != null &&
						  !string.IsNullOrEmpty((single = registration.FirstOrDefault()).Key)) {

						HostManager.ManageHost(Common.Enums.HostAction.StartListening, single.Value.Item1);
					}
					break;
				case Common.Enums.MessageType.RemovedRegistration:
					break;
			}
		}

		/// <summary>
		/// Creates the required folders if required.
		/// </summary>
		private Task CreateRequiredFoldersIfRequired(CustomConfigReader config) {
			return Task.Run(() => {
				var folders = new Dictionary<string, string> { {"JarDropFolder", config.JvmOptions.jarDropFolder },
														   {"LogFileLocation", config.dotNetOptions.logFileLocation},
														   {"workingFolder", config.dotNetOptions.workingFolder}};

				folders.ToList().ForEach(_ => {
					if (!string.IsNullOrEmpty(_.Value)) {
						if (!Directory.Exists(_.Value))
							Directory.CreateDirectory(_.Value);
					} else
						throw new ArgumentNullException($"{_.Key} is missing. Unable to continue.");
				});
			});

		}


		/// <summary>
		/// Reloads the and apply configuration changes.
		/// </summary>
		private void ReloadAndApplyConfigChanges() {
			_logger.Log(Common.Strings.PreReconfigureMessage);
			_configurationReader.Refresh();
			OnStop();
			Configure();
			OnStart(_startArguments);
			_logger.Log(Common.Strings.PostReconfigureMessage);
		}

		/// <summary>
		/// Configures this instance.
		/// </summary>
		private void Configure() {
			var res = ExecutionResult.Empty;

			try {	  
				Task.Run(async () => {
					// If restarting the service, we'll dispose of resources prior reutilizing them
					_mainWatcher?.Dispose();
					_tmrProcessFile?.Dispose();
					_tmrHouseKeeping?.Dispose();

					// Get configuration settings
					var config = (CustomConfigReader)_configurationReader.Configuration;

					// Initialize JNI Bridge
					await InitializeJniBridge(config);

					// Set up timers (for both housecleaning and JAR file processing) 
					_tmrProcessFile = new System.Timers.Timer(5000) { Enabled = true };  // Every 5 seconds
					_tmrHouseKeeping = new System.Timers.Timer(3600000) { Enabled = true }; // Every hour (60 minutes)
					_tmrProcessFile.Elapsed += (s, e) => ProcessDroppedFileHelper();
					_tmrHouseKeeping.Elapsed += (s, e) => DoHouseKeeping();
					_mainWatcher.Changed += (s, e) => ProcessDroppedJarFile(s, e);

					_mainWatcher = new FileSystemWatcher(config.JvmOptions.jarDropFolder) {
						EnableRaisingEvents = true,
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
					};
				});
			} catch (Exception ex) {
				_logger.Log(ex);
				_logger.Log(Common.Strings.ConfigurationErrorStopServiceMessage);
				OnStop();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		private Task InitializeJniBridge(CustomConfigReader config) {
			return Task.Run(async () => {
				await CreateRequiredFoldersIfRequired(config);
				await _jniBridgeManager.LoadJVM(config.JniBridgeOptions.libPath, config.JniBridgeOptions.libConfigFilePath);
			});
		}


		/// <summary>
		/// Processes the dropped jar file.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
		private void ProcessDroppedJarFile(object sender, FileSystemEventArgs e) {
			var fi = new FileInfo(e.FullPath);

			// We just process Jar files
			if (Path.GetExtension(e.Name).Equals(Common.Strings.JarFileExt)) {
				var config = (CustomConfigReader)_configurationReader.Configuration;
				var outputXml = $@"{config.dotNetOptions.workingFolder}\{e.Name.Replace("jar", "xml")}";
				var exists = _filesToProcess.Values.Count(p => !string.IsNullOrEmpty(p.Key) && p.Key == e.FullPath);

				// Let's process those file who have been created completely and have not been processed
				if (fi.Exists && exists == 0)
					_filesToProcess.Add(Guid.NewGuid(), new KeyValuePair<string, string>(e.FullPath, outputXml));
			}
		}

		/// <summary>
		/// Does the house keeping.
		/// </summary>
		private void DoHouseKeeping() {
			// Let's invoke both GCs (CLR & JVM) --- Once every hour
			GC.Collect();
			_jniBridgeManager.InvokeGC();

			// Let's remove temporary files
			RemoveStaleProxiesIfAny();
		}

		/// <summary>
		/// Removes the stale proxies if any.
		/// </summary>
		private void RemoveStaleProxiesIfAny() {
			new Thread(() => {
				var builder = TypeContainer.Resolve<IBuilder>();
				if (Directory.Exists(builder.StaleProxyLocation)) {
					try {
						var staleFiles = Directory.GetFiles(builder.StaleProxyLocation);
						Array.ForEach(staleFiles, File.Delete);
					} catch {
						// Safe to ignore exception here 
					}
				}
			}).Start();
		}

		/// <summary>
		/// Processes the dropped file helper.
		/// </summary>
		private void ProcessDroppedFileHelper() {
			var buffer = string.Empty;
			var alreadyProcessed = new List<Guid>();
			var result = ExecutionResult.Empty;
			var config = (CustomConfigReader)_configurationReader.Configuration;

			if (_filesToProcess.Count > 0) {
				_protectMutex.WaitOne();
				var javaParserFactory = TypeContainer.Resolve<IJavaParserFactory>();
				_tmrProcessFile.Enabled = false;
				Parallel.ForEach(_filesToProcess, (item) => {
					try {
						using (var testStream = new FileStream(item.Value.Key, FileMode.Open, FileAccess.Read, FileShare.Read)) {
							//  First step is to add selected JAR to ClassPath in JVM
							result = _jniBridgeManager.AddPath(item.Value.Key, ref buffer);
							alreadyProcessed.Add(item.Key); // Let's mark it as processed
															// Second step is to extract information on methods in JAR (if this step fails, it's useless to continue)

							if (_jniBridgeManager.SerializeMethodsInJar(item.Value.Key, item.Value.Value).Result.IsSuccess) {
								// Third step is to extract method metadata information (required to call JNIBridge)
								var res = javaParserFactory.ExtractJniMethodDefinition(item.Value.Key);
								_dataService.RemovePreviousRegistrationIfAny(Path.GetFileName(item.Value.Key.Replace(".jar", ".dll")));
								// Fourth step is to register dynamic library information (to be created in next step)
								if (_dataService.RegisterLibrary(item.Value.Key, _configurationReader, res.Tag as Dictionary<string, JniMetadata>).IsSuccess) {
									// Final steps are produce CSharp proxies and build dynamic assemblies
									if ((result = _defaultParser.ParseAndProduceCSharpFile(item.Value.Value)).IsSuccess) {
										var codeSnippets = result.Tag as Dictionary<string, StringBuilder>;
										result = _defaultBuilder.BuildProxyAssembly(codeSnippets, item.Value.Value, config.dotNetOptions.workingFolder);
									}
								}
							}
						}
					} catch (Exception ex) {
						_logger.Log(ex);
					}
				});
				alreadyProcessed.ForEach(_ => _filesToProcess.Remove(_));
				_tmrProcessFile.Enabled = true;
				_protectMutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
		/// </summary>
		/// <param name="args">Data passed by the start command.</param>
		protected override void OnStart(string[] args) {
			RemoveStaleProxiesIfAny();
			HostManager.ManageHost(Common.Enums.HostAction.StartListening);
		}


		/// <summary>
		/// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
		/// </summary>
		protected override void OnStop() {
			HostManager.ManageHost(Common.Enums.HostAction.Stop);
		}

		/// <summary>
		/// Initializes the type container.
		/// </summary>
		/// <param name="container">The container.</param>
		private void InitializeTypeContainer(IContainer container) {
			TypeContainer = container ?? throw new ArgumentNullException(Common.Strings.TypeContainerCantBeNull);
			_logger = TypeContainer.Resolve<ILogger>();
			_configurationReader = TypeContainer.Resolve<IConfigurationReader>();
			_jniBridgeManager = TypeContainer.Resolve<IJniBridgeManager>();
		}
						 

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_tmrHouseKeeping.Elapsed -= (s, e) => DoHouseKeeping();
				_tmrProcessFile.Elapsed -= (s, e) => ProcessDroppedFileHelper();
				_configFileMonitor.Changed -= (s, e) => ReloadAndApplyConfigChanges();
				_mainWatcher.Changed -= (s, e) => ProcessDroppedJarFile(s, e);
				_messenger.Notify -= MessengerNotification;
				_components?.Dispose();
				_configFileMonitor.EnableRaisingEvents = false;
				_configFileMonitor?.Dispose();
				_mainWatcher.EnableRaisingEvents = false;
				_mainWatcher?.Dispose();
				_perfCounters?.Dispose();
				_jniBridgeManager?.Dispose();
				_protectMutex?.Dispose();
				_tmrHouseKeeping?.Dispose();
				_tmrProcessFile?.Dispose();

				// Let's dispose of service hosts (if any)
				var hosts = HostManager?.DynamicHosts?.ToList();
				hosts?.ForEach(_ => _.Value?.Close());
			}
			base.Dispose(disposing);
		}
	}
}