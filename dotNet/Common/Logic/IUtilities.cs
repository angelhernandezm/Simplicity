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
	public interface IUtilities {
		/// <summary>
		/// Runs the java env checks.
		/// </summary>
		/// <returns></returns>
		ExecutionResult RunJavaEnvChecks();

	}
}
