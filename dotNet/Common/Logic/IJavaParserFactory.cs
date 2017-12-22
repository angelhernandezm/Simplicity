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
	public interface IJavaParserFactory {
		/// <summary>
		/// Extracts the jni method definition.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <returns></returns>
		ExecutionResult ExtractJniMethodDefinition(string jarFile);

	}
}
