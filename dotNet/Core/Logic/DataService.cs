using Simplicity.dotNet.Common;
using Simplicity.dotNet.Common.Data;
using Simplicity.dotNet.Common.Data.Models;
using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using Simplicity.dotNet.Core.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IDataService" />
	public class DataService : IDataService, IDisposable {
		/// <summary>
		/// The logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The _is disposed
		/// </summary>
		private bool _isDisposed = false;

		/// <summary>
		/// The data context
		/// </summary>
		private SimplicityContext _dataContext = new SimplicityContext(SimplicityContext.ConnectionString);

		/// <summary>
		/// Gets the simplicity database.
		/// </summary>
		/// <value>
		/// The simplicity database.
		/// </value>
		public string SimplicityDb => _dataContext?.Database?.Connection?.ConnectionString?.Replace("Data Source=", string.Empty);

		/// <summary>
		/// Gets the get epoch time.
		/// </summary>
		/// <value>
		/// The get epoch time.
		/// </value>
		public long EpochTime => (new Func<long>(() => ((long)(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc) -
									 new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds)))();

		/// <summary>
		/// Initializes a new instance of the <see cref="DataService"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public DataService(ILogger logger) {
			_logger = logger;
			_dataContext.Database.Initialize(false);
		}


		/// <summary>
		/// Registers the library.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="registration">The registration.</param>
		/// <returns></returns>
		public ExecutionResult RegisterLibrary(string jarFile, IConfigurationReader config, Dictionary<string, JniMetadata> registration) {
			var retval = ExecutionResult.Empty;

			if (!string.IsNullOrEmpty(jarFile) && config != null && registration != null) {
				if (File.Exists(jarFile)) {
					try {
						retval = RegisterLibraryHelper(jarFile, config, registration);
					} catch (Exception e) {
						retval.LastExceptionIfAny = e;
					}
				} else
					retval.LastExceptionIfAny = new FileNotFoundException("Specified jarFile not found. Unable to continue");
			} else
				retval.LastExceptionIfAny = new ArgumentException("Arguments are missing. Unable to continue.");

			return retval;
		}

		/// <summary>
		/// Gets the dynamic libraries (If not assembly is specified, everything is returned)
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns>
		/// ExecutionResult's Tag contains a Dictionary[string, Tuple[IEntity, IEntity, IEntity]] with information required by Service host.
		/// </returns>
		public ExecutionResult GetDynamicLibraries(string assemblyName = "") {
			var retval = ExecutionResult.Empty;
			var registration = new Dictionary<string, Tuple<IEntity, IEntity, IEntity>>();

			var query = (from x in _dataContext.DynamicLibrary.ToList()
						 join z in _dataContext.HostedLibrary.ToList()
						 on x.LibraryId equals z.Fk_DynamicLibraryId
						 join y in _dataContext.JavaClassMetadata.ToList()
						 on x.LibraryId equals y.Fk_DynamicLibraryId
						 select new {
							 Library = x,
							 Uri = z,
							 Class = y
						 }).Where(p => !string.IsNullOrEmpty(assemblyName) ?
										string.Equals(p.Library.AssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase) :
										!string.IsNullOrEmpty(p.Library.AssemblyName)).ToList();

			query.ForEach(r => {
				if (!registration.ContainsKey(r.Library.AssemblyName))
					registration.Add(r.Library.AssemblyName, new Tuple<IEntity, IEntity, IEntity>(r.Library, r.Uri, r.Class));
			});

			retval.Tag = registration;
			retval.IsSuccess = registration.Count > 0;

			return retval;
		}



		/// <summary>
		/// Registers the library helper.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="registration">The registration.</param>
		/// <returns></returns>
		private ExecutionResult RegisterLibraryHelper(string jarFile, IConfigurationReader config,
			Dictionary<string, JniMetadata> registration) {
			var r = registration?.ToList();
			var libraryId = Guid.NewGuid();
			var retval = ExecutionResult.Empty;
			var c = (CustomConfigReader)config.Configuration;
			var assembly = Path.GetFileName(jarFile).Replace(".jar", ".dll");
			var assemblyLocation =
				$@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Proxies\{assembly}";

			// If connection is closed then we'll open it
			if (_dataContext.Database.Connection.State == System.Data.ConnectionState.Closed)
				_dataContext.Database.Connection.Open();

			using (var transaction = _dataContext.Database.Connection.BeginTransaction()) {
				// We first register our proxy assembly
				_dataContext.DynamicLibrary.Add(new DynamicLibrary() {
					AssemblyName = assembly,
					IsHostingEnabled = true,
					JarFileLocation = jarFile,
					AssemblyLocation = assemblyLocation,
					JarFileName = Path.GetFileName(jarFile),
					LibraryId = libraryId.ToString(),
					RegisteredDate = EpochTime
				});

				try {
					r?.ForEach(_ => ProvisionTablesAtRegistration(_, jarFile, assembly, c, libraryId));
					_dataContext.SaveChanges();
					transaction.Commit();
					retval.IsSuccess = true;
				} catch (Exception e) {
					var z = e.Message;
					transaction.Rollback();
					retval.IsSuccess = false;
				} finally {
					_dataContext.Database.Connection.Close();
				}
			}

			return retval;
		}

		/// <summary>
		/// Provisions the tables at registration.
		/// </summary>
		/// <param name="r">The r.</param>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="assembly">The assembly.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="libraryId">The library identifier.</param>
		private void ProvisionTablesAtRegistration(KeyValuePair<string, JniMetadata> r, string jarFile, string assembly, CustomConfigReader config, Guid libraryId) {
			var metadataId = Guid.NewGuid();
			var hostedLibrary = Guid.NewGuid();
			var methods = r.Value?.MethodDescriptor?.ToList();
			var clazz = r.Key.Substring(r.Key.LastIndexOf('.') + 1);

			_dataContext.HostedLibrary.Add(new HostedLibrary() {
				Fk_DynamicLibraryId = libraryId.ToString(),
				HostedLibraryId = hostedLibrary.ToString(),
				LibraryURI = $"/{clazz}"
			});

			_dataContext.JavaClassMetadata.Add(new JavaClassMetadata() {
				ClassName = r.Key,
				Fk_DynamicLibraryId = libraryId.ToString(),
				MetadataEntryId = metadataId.ToString(),
				JavaClassDefinition = r.Value?.JavaClassDefinition
			});

			methods?.ForEach(_ => _dataContext.JniMethodInformation.Add(new JniMethodInformation() {
				JavaMethod = _.Value.Key,
				JniDescriptor = _.Value.Value,
				MethodId = Guid.NewGuid().ToString(),
				Fk_ClassMetadataId = metadataId.ToString()
			}));
		}

		/// <summary>
		/// Gets the table definition.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public string GetTableDefinition<T>(T t) where T : IEntity {
			var retval = string.Empty;

			if (t is DynamicLibrary) {
				retval = Common.Strings.DbSpecific.DynamicLibraryCreateTable;
			} else if (t is HostedLibrary) {
				retval = Common.Strings.DbSpecific.HostedLibraryCreateTable;
			} else if (t is JavaClassMetadata) {
				retval = Common.Strings.DbSpecific.JavaClassMetadataCreateTable;
			} else if (t is JniMethodInformation) {
				retval = Common.Strings.DbSpecific.JniMethodInformationCreateTable;
			}

			return retval;
		}

		/// <summary>
		/// Removes the previous registration if any.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns></returns>
		public ExecutionResult RemovePreviousRegistrationIfAny(string assemblyName) {
			var retval = ExecutionResult.Empty;

			if (!string.IsNullOrEmpty(assemblyName)) {
				var registration = _dataContext.DynamicLibrary.ToList()
					.FirstOrDefault(_ => string.Equals(_.AssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase));

				if (registration != null) {
					try {
						using (var ts = new TransactionScope(TransactionScopeOption.Required)) {
							// If transaction is closed then we'll open it
							if (_dataContext.Database.Connection.State == System.Data.ConnectionState.Closed)
								_dataContext.Database.Connection.Open();

							_dataContext.DynamicLibrary.Remove(registration);
							_dataContext.SaveChanges();
							ts.Complete();
							_dataContext.Database.Connection.Close();
						}
						retval.IsSuccess = true;
					} catch (Exception e) {
						retval.LastExceptionIfAny = e;
					}
				}
			} else
				retval.LastExceptionIfAny = new ArgumentNullException("Argument is missing. Unable to continue.");

			return retval;
		}

		/// <summary>
		/// Gets the method information for jni call.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="argList">The argument list.</param>
		/// <returns>ExecutionResult that contains a Tuple[[IEntity, IEntity, IEntity]] with information required to call method through JNI.</returns>
		public ExecutionResult GetMethodInformationForJniCall(Type service, StackFrame callingFrame, params object[] argList) {
			var retval = ExecutionResult.Empty;
			var methodPrototype = new Regex(@"\w+[a-z,A-Z]\((.*?)\)");

			var checkArgs = new Func<StackFrame, string, object[], bool>((a, b, c) => {
				var found = false;
				List<Match> matches;
				var method = callingFrame.GetMethod();
				var methodName = method.Name.Split(' ').FirstOrDefault(); // Method name will always be index 0
				var argCount = method.GetParameters().Length;

				// Let's find matching method prototype with a regex
				if (!string.IsNullOrEmpty(b) && a != null && (matches = methodPrototype.Matches(b)
							.Cast<Match>().Where(r => r.Value.StartsWith(methodName)).ToList())?.Count > 0) {
					found = argList?.Length == argCount;
				}

				return found;
			});

			var query = (from x in _dataContext.DynamicLibrary.ToList()
						 join z in _dataContext.HostedLibrary.ToList()
						 on x.LibraryId equals z.Fk_DynamicLibraryId
						 join y in _dataContext.JavaClassMetadata.ToList()
						 on x.LibraryId equals y.Fk_DynamicLibraryId
						 join w in _dataContext.JniMethodInformation.ToList()
						 on y.MetadataEntryId equals w.Fk_ClassMetadataId
						 select new {
							 Library = x,
							 Method = w,
							 Class = y
						 }).FirstOrDefault(p => string.Equals(p.Class.ClassName,
										   service?.FullName?.Replace("_", ".")) &&
										   checkArgs(callingFrame, p.Method.JavaMethod, argList));

			if (query != null) {
				retval.IsSuccess = true;
				retval.Tag = Tuple.Create<IEntity, IEntity, IEntity>(query.Class, query.Library, query.Method);
			}

			return retval;
		}


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool isDisposing) {
			if (_isDisposed)
				return;

			if (isDisposing) {
				_dataContext?.Dispose();
			}
			_isDisposed = true;
		}
	}
}