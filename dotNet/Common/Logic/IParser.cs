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
	public interface IParser {
		/// <summary>
		/// Parses the and produce c sharp file.
		/// </summary>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <param name="serviceTemplateFile">The service template file.</param>
		/// <param name="conversionMasterFile">The conversion master file.</param>
		/// <returns></returns>
		ExecutionResult ParseAndProduceCSharpFile(string jarInformationFile, string serviceTemplateFile = "", string conversionMasterFile = "");
	}
}
