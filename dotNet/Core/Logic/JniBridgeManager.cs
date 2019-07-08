using Simplicity.dotNet.Common.Data.Models;
using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using Simplicity.dotNet.Core.Configuration;
using Simplicity.dotNet.Core.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Simplicity.dotNet.Common.Data;
using Simplicity.dotNet.Common;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Core.Logic.IJniBridgeManager" />
	public class JniBridgeManager : IJniBridgeManager {
		/// <summary>
		/// The _logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The dataservice
		/// </summary>
		private IDataService _dataservice;

		/// <summary>
		/// The _is disposed
		/// </summary>
		private bool _isDisposed = false;

		/// <summary>
		/// The perf counters
		/// </summary>
		private IPerfCounters _perfCounters;

		/// <summary>
		/// The _configuration
		/// </summary>
		private IConfigurationReader _configuration;

		/// <summary>
		/// The utilities
		/// </summary>
		private IUtilities _utilities;

		/// <summary>
		/// Gets or sets the JVM handles.
		/// </summary>
		/// <value>
		/// The JVM handles.
		/// </value>
		protected Dictionary<string, IntPtr> jvmHandles {
			get; set;
		}

		/// <summary>
		/// Gets or sets the cached jni methods.
		/// </summary>
		/// <value>
		/// The cached jni methods.
		/// </value>
		protected Dictionary<string, KeyValuePair<JniMethodDescription, Tuple<IEntity, IEntity, IEntity>>> CachedJniMethods {
			get; set;
		}

		/// <summary>
		/// Gets the is JVM loaded.
		/// </summary>
		/// <value>
		/// The is JVM loaded.
		/// </value>
		protected bool? IsJvmLoaded => jvmHandles?.ContainsKey("hModuleJniBridge");

		/// <summary>
		/// Initializes a new instance of the <see cref="JniBridgeManager"/> class.
		/// </summary>
		public JniBridgeManager() {
			jvmHandles = new Dictionary<string, IntPtr>();
			CachedJniMethods = new Dictionary<string, KeyValuePair<JniMethodDescription, Tuple<IEntity, IEntity, IEntity>>>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JniBridgeManager" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="configuration">The configuration.</param>
		/// <param name="utilities">The utilities.</param>
		/// <param name="dataservice">The dataservice.</param>
		/// <param name="counters">The counters.</param>
		public JniBridgeManager(ILogger logger, IConfigurationReader configuration, IUtilities utilities, IDataService dataservice, IPerfCounters counters) : this() {
			_logger = logger;
			_utilities = utilities;
			var hModule = IntPtr.Zero;
			_dataservice = dataservice;
			_configuration = configuration;
			_perfCounters = counters;

			// Let's check for required SYMLINKS & Environment variables (Java)
			if (_utilities.RunJavaEnvChecks().IsSuccess) {
				// Let's load JVM (as specified in configuration)
				if ((hModule = Win32Helper.LoadLibrary(((CustomConfigReader)configuration.Configuration).JniBridgeOptions.selectedJvm)) != IntPtr.Zero)
					jvmHandles.Add("hModuleJvm", hModule);
			}
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
				// Let's shutdown JVM prior releasing handles
				var hProcAddress = Win32Helper.GetProcAddress(jvmHandles["hModuleJniBridge"], JniBridgeMethod.ShutdownJvm);
				var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.ShutdownJvm)) as Win32Helper.ShutdownJvm;
				functor();

				// Let's close handles
				foreach (var item in jvmHandles) {
					try {
						Win32Helper.CloseHandle(item.Value);
					} catch {
						// Safe to ignore exceptions (if any) when closing handles
					}
				}
			}
			_isDisposed = true;
		}


		/// <summary>
		/// Adds the path.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public ExecutionResult AddPath(string jarFile, ref string result) {
			IntPtr hProcAddress;
			var retval = ExecutionResult.Empty;
			var buffer = new StringBuilder(Win32Helper.Max_Path);
			var exceptions = new StringBuilder(Win32Helper.Max_BufferSizeForMessagesFromJni);

			try {
				if (IsJvmLoaded.Value) {
					if ((hProcAddress = Win32Helper.GetProcAddress(jvmHandles["hModuleJniBridge"], JniBridgeMethod.AddPath)) != IntPtr.Zero) {
						var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.AddPath)) as Win32Helper.AddPath;
						retval.IsSuccess = functor(jarFile, buffer, exceptions) == 0;
						result = buffer.ToString();
						retval.JniErrorMessage = exceptions.ToString();
					}
				} else
					retval.LastExceptionIfAny = new Exception("Unable to proceed without JVM being loaded - AddPath()");
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}

		/// <summary>
		/// Invokes the gc.
		/// </summary>
		/// <returns></returns>
		public Task<ExecutionResult> InvokeGC() {
			return Task.Run(() => {	 
				IntPtr hProcAddress;
				var retval = ExecutionResult.Empty;

				try {
					if (IsJvmLoaded.Value) {
						if ((hProcAddress = Win32Helper.GetProcAddress(jvmHandles["hModuleJniBridge"], JniBridgeMethod.InvokeGC)) != IntPtr.Zero) {
							var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.InvokeGC)) as Win32Helper.InvokeGC;
							functor();
							retval.IsSuccess = true;
						}
					} else
						retval.LastExceptionIfAny = new Exception("Unable to proceed without JVM being loaded - InvokeGC()");
				} catch (Exception e) {
					retval.LastExceptionIfAny = e;
				}

				return retval;
			});
		}

		/// <summary>
		/// Loads the JVM.
		/// </summary>
		/// <param name="JniBridgeLibraryPath">The jni bridge library path.</param>
		/// <param name="jniBridgeLibConfigPath">The jni bridge library configuration path.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public Task<ExecutionResult> LoadJVM(string JniBridgeLibraryPath, string jniBridgeLibConfigPath) {
			return Task.Run(() => {
				IntPtr hModule, hProcAddress;
				var retval = ExecutionResult.Empty;

				try {
					if (!IsJvmLoaded.Value && (hModule = Win32Helper.LoadLibrary(JniBridgeLibraryPath)) != IntPtr.Zero) {
						jvmHandles.Add("hModuleJniBridge", hModule);
						if ((hProcAddress = Win32Helper.GetProcAddress(hModule, JniBridgeMethod.LoadJVMAndJar)) != IntPtr.Zero) {
							var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.InitializeJvm)) as Win32Helper.InitializeJvm;
							retval.IsSuccess = functor(jniBridgeLibConfigPath) == 0;
						}
					}
				} catch (Exception e) {
					retval.LastExceptionIfAny = e;
				}

				return retval;
			});
		}

		/// <summary>
		/// Serializes the methods in jar.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="xmlPath">The XML path.</param>
		/// <returns></returns>
		public Task<ExecutionResult> SerializeMethodsInJar(string jarFile, string xmlPath) {
			return Task.Run(() => {
				IntPtr hProcAddress;
				var retval = ExecutionResult.Empty;
				var exceptions = new StringBuilder(Win32Helper.Max_BufferSizeForMessagesFromJni);

				if (!string.IsNullOrEmpty(jarFile) && !string.IsNullOrEmpty(xmlPath)) {
					if (File.Exists(jarFile)) {
						try {
							// If XML file with methods' information exists previously is deleted
							if (File.Exists(xmlPath))
								File.Delete(xmlPath);

							// Let's call our method in our Inspector.jar library
							if (IsJvmLoaded.Value) {
								if ((hProcAddress = Win32Helper.GetProcAddress(jvmHandles["hModuleJniBridge"], JniBridgeMethod.SerializeMethodsInJar)) != IntPtr.Zero) {
									var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.SerializeMethodsInJar)) as Win32Helper.SerializeMethodsInJar;
									retval.IsSuccess = functor(jarFile, xmlPath, exceptions) == 0;
									retval.JniErrorMessage = exceptions.ToString();
								}
							} else
								retval.LastExceptionIfAny = new Exception("Unable to proceed without JVM being loaded - SerializeMethodsInJar()");
						} catch (Exception e) {
							retval.LastExceptionIfAny = e;
						}
					} else
						retval.LastExceptionIfAny = new FileNotFoundException("Specified jar file doesn't exist.");
				} else
					retval.LastExceptionIfAny = new ArgumentNullException("Unable to proceed if jar file or resulting xml file aren't specified.");

				return retval;
			});
		}

		/// <summary>
		/// Runs as java pass through.
		/// </summary>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="service">The service.</param>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public ExecutionResult RunAsJavaPassThrough(StackFrame callingFrame, Type service, params object[] list) {		
			var retval = ExecutionResult.Empty;
		    var result = ExecutionResult.Empty;
			KeyValuePair<JniMethodDescription, Tuple<IEntity, IEntity, IEntity>>? found = null;
			var exceptions = new StringBuilder(Win32Helper.Max_BufferSizeForMessagesFromJni);

			var returnValueGetter = ReturnDelegateBasedOnType(retval);

			try {
				if (IsJvmLoaded.Value) {
					_perfCounters.IncrementCounter(Enums.PerfCounter.TotalRequests); // Increase "TotalRequest" performance counter
					var methodKey = new JniMethodDescription(callingFrame, service);
					// Let's retrieve method metadata from dictionary (if exists), otherwise we'll get it from database
					if (CachedJniMethods.ContainsKey(methodKey.MethodSignature)) {
						found = CachedJniMethods[methodKey.MethodSignature];
						_perfCounters.IncrementCounter(Enums.PerfCounter.CachedJniMethodCall); // Increase "CachedJniMethodCall"	performance counter
					} else {
						result = _dataservice.GetMethodInformationForJniCall(service, callingFrame, list);
						_perfCounters.IncrementCounter(Enums.PerfCounter.DbJniMethodCall); // Increase "DbJniMethodCall"	performance counter
						var metadata = result.Tag as Tuple<IEntity, IEntity, IEntity>;

						if (result.IsSuccess && metadata != null) {
							found = new KeyValuePair<JniMethodDescription, Tuple<IEntity, IEntity, IEntity>>(methodKey, metadata);
							CachedJniMethods.Add(methodKey.MethodSignature, found.Value);
						}
					}

					RunAsJavaPassThroughHelper(callingFrame, list, found, retval, methodKey, returnValueGetter, exceptions);
				}
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}

		/// <summary>
		/// Returns the type of the delegate based on.
		/// </summary>
		/// <param name="retval">The retval.</param>
		/// <returns></returns>
		private Dictionary<string, Delegate> ReturnDelegateBasedOnType(ExecutionResult retval) {
			// Return type will allow us choose the right and expected delegate
			var returnValueGetter = new Dictionary<string, Delegate>()	  {
				{"long", new Win32Helper.ReturnDelegateForLong(x => { retval.Tag = x; })},
				{"byte", new Win32Helper.ReturnDelegateForByte(x => { retval.Tag = x; })},
				{"char", new Win32Helper.ReturnDelegateForChar(x => { retval.Tag = x; })},
				{"short", new Win32Helper.ReturnDelegateForShort(x => { retval.Tag = x; })},
				{"int", new Win32Helper.ReturnDelegateForInteger(x => { retval.Tag = x; })},
				{"single", new Win32Helper.ReturnDelegateForFloat(x => { retval.Tag = x; })},
				{"bool", new Win32Helper.ReturnDelegateForBoolean(x => { retval.Tag = x; })},
				{"double", new Win32Helper.ReturnDelegateForDouble(x => { retval.Tag = x; })},
				{"object", new Win32Helper.ReturnDelegateForObject(x => { retval.Tag = x; })},
				{"string", new Win32Helper.ReturnDelegateForString(x => { retval.Tag = x; })}
			};

			return returnValueGetter;
		}

		/// <summary>Runs as java pass through helper.</summary>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="list">The list.</param>
		/// <param name="found">The found.</param>
		/// <param name="retval">The retval.</param>
		/// <param name="methodKey">The method key.</param>
		/// <param name="returnValueGetter">The return value getter.</param>
		/// <param name="exceptions">The exceptions.</param>
		private void RunAsJavaPassThroughHelper(StackFrame callingFrame, object[] list, KeyValuePair<JniMethodDescription, Tuple<IEntity, IEntity, IEntity>>? found,
			ExecutionResult retval, JniMethodDescription methodKey, Dictionary<string, Delegate> returnValueGetter, StringBuilder exceptions) {
			IntPtr hProcAddress;

			if (found?.Value != null) {
				var paramType = new List<string>();
				var method = callingFrame.GetMethod();
				var paramList = method.GetParameters().ToList();
				var dynamicLibrary = (DynamicLibrary)found.Value.Value.Item2;
				var methodInformation = (JavaClassMetadata)found.Value.Value.Item1;
				var libraryInformation = (JniMethodInformation)found.Value.Value.Item3;
				paramList.ForEach(_ => paramType.Add(_.ParameterType.Name.ToLower()));
				var returnType = ((MethodInfo)method)?.ReturnParameter?.ToString().ToLower().Trim();

				if ((hProcAddress = Win32Helper.GetProcAddress(jvmHandles["hModuleJniBridge"], JniBridgeMethod.RunMethodInJar)) != IntPtr.Zero) {
					var functor = Marshal.GetDelegateForFunctionPointer(hProcAddress, typeof(Win32Helper.RunMethodInJar)) as
							Win32Helper.RunMethodInJar;

					retval.IsSuccess = functor(methodInformation.ClassName.Replace(".", "/"), methodKey.MethodName,
										   libraryInformation.JavaMethod.Contains("static"),
										   libraryInformation.JniDescriptor, list, paramType.ToArray(), list.Length,
										   Marshal.GetFunctionPointerForDelegate(returnValueGetter[returnType]), exceptions,
										   returnType) == 0;

					retval.JniErrorMessage = exceptions.ToString();
				}
			}
		}
	}
}	  