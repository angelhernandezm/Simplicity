using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
namespace Simplicity.dotNet.Core.Configuration {
	/// <summary>
	/// 
	/// </summary>
	public class JvmOptionConfigElement: ConfigurationElement {
		/// <summary>
		/// Gets or sets the jar drop folder.
		/// </summary>
		/// <value>
		/// The jar drop folder.
		/// </value>
		[ConfigurationProperty(Strings.JarDropFolderPropName,
			DefaultValue = "Specify folders where JAR files are expected to be dropped in",
			IsRequired = true)]
		public string jarDropFolder {
			get {
				return this[Strings.JarDropFolderPropName].ToString();
			}
			set {
				this[Strings.JarDropFolderPropName] = value;
			}
		}

		/// <summary>
		/// Determines whether [is read only].
		/// </summary>
		/// <returns></returns>
		public override bool IsReadOnly() {
			return false;
		}
	}
}
