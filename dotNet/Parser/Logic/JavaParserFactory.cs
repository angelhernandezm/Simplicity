using Simplicity.dotNet.Common.Data;
using Simplicity.dotNet.Common.Interop;
using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Parser.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IJavaParseFactory" />
	public class JavaParserFactory : IJavaParserFactory {
		/// <summary>
		/// Extracts the jni method definition.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <returns></returns>
		public ExecutionResult ExtractJniMethodDefinition(string jarFile) {
			var retval = ExecutionResult.Empty;
			var jarOutput = new StringBuilder();

			if (!string.IsNullOrEmpty(jarFile)) {
				if (File.Exists(jarFile)) {
					try {
						using (var jarProc = new Process() {
							StartInfo = new ProcessStartInfo("jar.exe") {
								UseShellExecute = false,
								RedirectStandardOutput = true,
								Arguments = $" tf \"{jarFile}\"",

							}, EnableRaisingEvents = true
						}) {
							jarProc.OutputDataReceived += (s, e) => jarOutput.AppendLine(e.Data);
							jarProc.Start();
							jarProc.BeginOutputReadLine();
							jarProc.WaitForExit();
							retval = ExtractMethodDefinitionFromClasses(jarFile, jarOutput.ToString());
						}

					} catch (Exception e) {
						retval.LastExceptionIfAny = e;
					}
				} else
					retval.LastExceptionIfAny = new FileNotFoundException("jarFile specified not found. Unable to continue");
			} else
				retval.LastExceptionIfAny = new ArgumentNullException("jarFile parameter is missing. Unable to continue.");

			return retval;
		}

		/// <summary>
		/// Extracts the method definition from classes.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="classesInJar">The classes in jar.</param>
		/// <returns></returns>
		private ExecutionResult ExtractMethodDefinitionFromClasses(string jarFile, string classesInJar) {
			var parsed = new List<string>();
			var retval = ExecutionResult.Empty;
			var outputBuffer = new StringBuilder();
			var classesMetadata = new Dictionary<string, JniMetadata>();

			if (!string.IsNullOrEmpty(jarFile) && !string.IsNullOrEmpty(classesInJar)) {
				try {
					// We'll process every class in Jar except "Main"
					var lines = classesInJar.Split("\r\n".ToCharArray()).Where(x => x.Contains(".class") && !x.Contains("Main"))?.ToList();
					lines.ForEach(r => parsed.Add(r.Replace('/', '.').Replace(".class", string.Empty)));

					Parallel.ForEach(parsed, c => {
						try {
							using (var javapProc = new Process() {
								StartInfo = new ProcessStartInfo("javap.exe") {
									UseShellExecute = false,
									RedirectStandardOutput = true,
									Arguments = $" -s -classpath \"{jarFile}\" {c}",

								}, EnableRaisingEvents = true
							}) {
								javapProc.OutputDataReceived += (s, e) => outputBuffer.AppendLine(e.Data);
								javapProc.Start();
								javapProc.BeginOutputReadLine();
								javapProc.WaitForExit();
								classesMetadata.Add(c, PrepareMetadata(c, outputBuffer.ToString()));
							}

						} catch {
							// Safe to ignore exception   (it's most likely due to issues parsing java class)
							// We'll return what could be parsed
						}
					});

					retval.Tag = classesMetadata;
					retval.IsSuccess = classesMetadata.Count > 0;
				} catch (Exception e) {
					retval.LastExceptionIfAny = e;
				}
			}

			return retval;
		}

		/// <summary>
		/// Prepares the metadata.
		/// </summary>
		/// <param name="clazz">The clazz.</param>
		/// <param name="clazzDefinition">The clazz definition.</param>
		/// <returns></returns>
		private JniMetadata PrepareMetadata(string clazz, string clazzDefinition) {
			var count = 0;
			var retval = new JniMetadata();
			var methodRegex = new Regex(@".*[^\S\n\r\f].*");
			var cleanMethodDef = clazzDefinition;
			cleanMethodDef = cleanMethodDef.Substring(cleanMethodDef.IndexOf('{') + 1).Replace("}", string.Empty).Trim();
			var methods = methodRegex.Matches(cleanMethodDef);
			retval.ClassName = clazz;
			retval.JavaClassDefinition = clazzDefinition;

			if (methods?.Count > 0) {
				// Regex split method prototype and JNI prototype, being the odd index 
				// the method and even index the descriptor respectively 
				for (var index = 0; index < methods.Count;) {
					// Some methods might come up without any descriptor, so odd index has method information but odd index the descriptor is misssing
					// If that's the case, matches collection count is odd (e.g.: 23 elements)
					if (index + 1 < methods.Count && !methods[index + 1].ToString().Contains("descriptor:")) {
						index++;
						continue;
					}

					retval.MethodDescriptor.Add(++count,
						new KeyValuePair<string, string>(methods[index].ToString().Replace(";", string.Empty).Trim(),
														methods[index + 1].ToString().Replace("descriptor:", string.Empty).Trim()));

					index = index + 2;
				}
			}

			return retval;
		}
	}
}