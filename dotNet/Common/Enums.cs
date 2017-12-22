using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common {
	/// <summary>
	/// 
	/// </summary>
	public class Enums {
		/// <summary>
		/// 
		/// </summary>
		public enum MethodType {
			WebMethod,
			Interface,
			Implementation
		}

		/// <summary>
		/// 
		/// </summary>
		public enum MarkupType {
			Undefined,
			Keyword,
			Statement
		}

		/// <summary>
		/// 
		/// </summary>
		public enum TemplateFile {
			ServiceTemplate,
			ConversionMaster
		}

		/// <summary>
		/// 
		/// </summary>
		public enum HostAction {
			StartListening,
			Stop
		}

		/// <summary>
		/// 
		/// </summary>
		public enum MessageType {
			General,
			NewRegistration,
			RemovedRegistration,
		}

		/// <summary>
		/// 
		/// </summary>
		public enum PerfCounter {
			JniException,
			TotalRequests,
			SuccessfulExecution,
			CachedJniMethodCall,
			ProxyGeneration,
			DbJniMethodCall
		}
	}
}	 