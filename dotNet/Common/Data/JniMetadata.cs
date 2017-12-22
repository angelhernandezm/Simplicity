using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Data {
	/// <summary>
	/// 
	/// </summary>
	public class JniMetadata {
		/// <summary>
		/// The method descriptor
		/// </summary>
		private Dictionary<int, KeyValuePair<string, string>> _methodDescriptor = new Dictionary<int, KeyValuePair<string, string>>();

		/// <summary>
		/// Gets or sets the name of the class.
		/// </summary>
		/// <value>
		/// The name of the class.
		/// </value>
		public string ClassName {
			get; set;
		}

		/// <summary>
		/// Gets or sets the java class definition.
		/// </summary>
		/// <value>
		/// The java class definition.
		/// </value>
		public string JavaClassDefinition {
			get; set;
		}

		/// <summary>
		/// Gets the method descriptor.
		/// </summary>
		/// <value>
		/// The method descriptor.
		/// </value>
		public Dictionary<int, KeyValuePair<string, string>> MethodDescriptor {
			get {
				return _methodDescriptor;
			}
		}
	}
} 