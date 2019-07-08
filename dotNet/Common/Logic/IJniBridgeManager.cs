using Simplicity.dotNet.Common.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IJniBridgeManager: IDisposable {
		/// <summary>
		/// Loads the JVM.
		/// </summary>
		/// <param name="JniBridgeLibraryPath">The jni bridge library path.</param>
		/// <param name="jniBridgeLibConfigPath">The jni bridge library configuration path.</param>
		/// <returns></returns>
		Task<ExecutionResult> LoadJVM(string JniBridgeLibraryPath, string jniBridgeLibConfigPath);

		/// <summary>
		/// Adds the path.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		ExecutionResult AddPath(string jarFile, ref string result);

		/// <summary>
		/// Serializes the methods in jar.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="xmlPath">The XML path.</param>
		/// <returns></returns>
		ExecutionResult SerializeMethodsInJar(string jarFile, string xmlPath);

		/// <summary>
		/// Invokes the gc.
		/// </summary>
		/// <returns></returns>
		ExecutionResult InvokeGC();

		/// <summary>
		/// Runs as java pass through.
		/// </summary>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="service">The service.</param>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		ExecutionResult RunAsJavaPassThrough(StackFrame callingFrame, Type service, params object[] list);
	}
}
