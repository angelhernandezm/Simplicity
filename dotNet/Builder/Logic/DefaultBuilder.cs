using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;
using Simplicity.dotNet.Common;

namespace Simplicity.dotNet.Builder.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IBuilder" />
	public class DefaultBuilder : IBuilder {
		/// <summary>
		/// The logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The messenger
		/// </summary>
		private IMessenger _messenger;

		/// <summary>
		/// The perf counters
		/// </summary>
		private IPerfCounters _perfCounters;

		/// <summary>
		/// Gets the proxy assemblies location.
		/// </summary>
		/// <value>
		/// The proxy assemblies location.
		/// </value>
		public string ProxyAssembliesLocation => string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"\Proxies\");

		/// <summary>
		/// Gets the stale proxy location.
		/// </summary>
		/// <value>
		/// The stale proxy location.
		/// </value>
		public string StaleProxyLocation => string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"\Proxies\Stale\");

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultBuilder" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="messenger">The messenger.</param>
		/// <param name="counters">The counters.</param>
		public DefaultBuilder(ILogger logger, IMessenger messenger, IPerfCounters counters) {
			_logger = logger;
			_messenger = messenger;
			_perfCounters = counters;
		}		  

		/// <summary>
		/// Builds the proxy assembly.
		/// </summary>
		/// <param name="codeFiles">The code files.</param>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <param name="workFolder">The work folder.</param>
		/// <returns></returns>
		public ExecutionResult BuildProxyAssembly(Dictionary<string, StringBuilder> codeFiles, string jarInformationFile, string workFolder) {
			var retval = ExecutionResult.Empty;

			// Let's check arguments prior processing code snippets to build proxy assembly
			if (codeFiles?.Count > 0 && !string.IsNullOrEmpty(jarInformationFile) && !string.IsNullOrEmpty(workFolder)) {
				try {
					// If everything is Ok, we'll fire an event to perform registration
					if ((retval = BuildProxyAssemblyHelper(codeFiles, workFolder, jarInformationFile)).IsSuccess)
						_messenger.BroadcastNotification(retval.Tag.ToString(), this);
				} catch (Exception e) {
					retval.LastExceptionIfAny = e;
				}
			} else
				retval.LastExceptionIfAny = new ArgumentException("Arguments are missing or incorrect. Unable to continue.");

			return retval;
		}

		/// <summary>
		/// Gets the reference assemblies.
		/// </summary>
		/// <returns></returns>
		private List<string> GetReferenceAssemblies() {
			var retval = new List<string>();
			var referenceFolder = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			retval.Add(string.Concat(referenceFolder, @"\Autofac.dll"));
			retval.Add(string.Concat(referenceFolder, @"\Simplicity.dotNet.Common.dll"));
			retval.Add(string.Concat(referenceFolder, @"\Simplicity.dotNet.Core.dll"));
			retval.Add(string.Concat(referenceFolder, @"\Simplicity.dotNet.Builder.dll"));
			retval.Add(string.Concat(referenceFolder, @"\Simplicity.dotNet.Parser.dll"));

			return retval;
		}

		/// <summary>
		/// Performs the clean up.
		/// </summary>
		/// <param name="codeFiles">The code files.</param>
		/// <param name="jarInformatioFile">The jar informatio file.</param>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <param name="deleteTempFiles">The delete temporary files.</param>
		/// <returns></returns>
		private ExecutionResult PerformCleanUp(List<string> codeFiles, string jarInformationFile, string assemblyName, Action deleteTempFiles) {
			var retval = ExecutionResult.Empty;
			var newProxyLocation = string.Empty;
			var staleFolder = StaleProxyLocation;
			var proxyFolder = ProxyAssembliesLocation;

			try {
				newProxyLocation = string.Concat(proxyFolder, Path.GetFileName(assemblyName));

				// If Proxies folder doesn't exist, we'll create it
				if (!Directory.Exists(proxyFolder))
					Directory.CreateDirectory(proxyFolder);

				// If Stale folder doesn't exist, we'll create it
				if (!Directory.Exists(staleFolder))
					Directory.CreateDirectory(staleFolder);

				// Let's move existing file, but we must close it first because it could've been loaded when service started
				// Moved files are deleted next time the service starts.
				if (File.Exists(newProxyLocation)) {
					var hModule = Win32Helper.LoadLibrary(newProxyLocation);
					Win32Helper.FreeLibrary(hModule);
					var oldProxy = $"{Guid.NewGuid().ToString().Substring(0, 5)}_{Path.GetFileName(assemblyName)}";
					var oldProxyLocation = string.Concat(staleFolder, oldProxy);
					File.Move(newProxyLocation, oldProxyLocation);
				}

				// Let's move new assembly to proxy folder
				File.Move(assemblyName, newProxyLocation);

				// Let's remove jarInformationFile (XML file that contains metadata information)
				File.Delete(jarInformationFile);

				// Let's remove all source code files (codegen)
				codeFiles.ForEach(_ => File.Delete(_));

				// Finally, we'll remove any temp file that were created
				deleteTempFiles();

				retval.IsSuccess = true;
				retval.Tag = newProxyLocation;
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}

		/// <summary>
		/// Builds the proxy assembly helper.
		/// </summary>
		/// <param name="codeFiles">The code files.</param>
		/// <param name="workFolder">The work folder.</param>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <returns></returns>
		private ExecutionResult BuildProxyAssemblyHelper(Dictionary<string, StringBuilder> codeFiles, string workFolder, string jarInformationFile) {
			var newFiles = new List<string>();
			var retval = ExecutionResult.Empty;
			var assemblyName = jarInformationFile.Replace(".xml", ".dll");

			try {
				// If work folder doesn't exist, we'll create it
				if (!Directory.Exists(workFolder))
					Directory.CreateDirectory(workFolder);

				using (var cp = CodeDomProvider.CreateProvider("CSharp")) {
					foreach (var i in codeFiles) {
						var fileName = $@"{workFolder}\{i.Key.Substring(0, i.Key.IndexOf('|'))}.cs";

						// Let's make sure file doesn't exist 
						if (File.Exists(fileName))
							File.Delete(fileName);

						File.WriteAllText(fileName, i.Value.ToString());     // and create the source file
						newFiles.Add(fileName);
					}

					var referenceAssemblies = GetReferenceAssemblies();

					var cParams = new CompilerParameters(new string[] { "System.Linq.dll", "System.ServiceModel.dll", "System.ServiceModel.Web.dll" }) {
						GenerateExecutable = false,
						OutputAssembly = assemblyName,
						CompilerOptions = "/platform:x64"
					};

					// Should we load reference assemblies into AppDomain or not?
					var loadedAlready = Assembly.GetExecutingAssembly().Modules.Any(p => p.Name.Contains("Simplicity.dotNet."));

					referenceAssemblies.ForEach(_ => {
						cParams.ReferencedAssemblies.Add(_);

						if (!loadedAlready) // If reference assemblies hasn't been loaded, we'll load them
							Assembly.Load(AssemblyName.GetAssemblyName(_));
					});

					var result = cp.CompileAssemblyFromFile(cParams, newFiles.ToArray());

					// Any errors at compilation?
					if (result.Errors.Count > 0) {
						retval.Tag = result.Errors;
					} else {
						
						result.TempFiles.KeepFiles = false;
						_perfCounters.IncrementCounter(Enums.PerfCounter.ProxyGeneration);  // Increase "ProxyGeneration" performance counter
						retval = PerformCleanUp(newFiles, jarInformationFile, result.PathToAssembly, result.TempFiles.Delete);
					}
				}
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}
	}
}