using Simplicity.dotNet.Common.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IBuilder {
		/// <summary>
		/// Gets the proxy assemblies location.
		/// </summary>
		/// <value>
		/// The proxy assemblies location.
		/// </value>
		string ProxyAssembliesLocation {
			get;
		}

		/// <summary>
		/// Gets the stale proxy location.
		/// </summary>
		/// <value>
		/// The stale proxy location.
		/// </value>
		string StaleProxyLocation {
			get;
		}

		/// <summary>
		/// Builds the proxy assembly.
		/// </summary>
		/// <param name="codeFiles">The code files.</param>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <param name="workFolder">The work folder.</param>
		/// <returns></returns>
		ExecutionResult BuildProxyAssembly(Dictionary<string, StringBuilder> codeFiles, string jarInformationFile, string workFolder); 
	}
} 