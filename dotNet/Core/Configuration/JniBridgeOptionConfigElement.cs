using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Configuration {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Configuration.ConfigurationElement" />
	public class JniBridgeOptionConfigElement : ConfigurationElement {
		/// <summary>
		/// Gets or sets the library path.
		/// </summary>
		/// <value>
		/// The library path.
		/// </value>
		[ConfigurationProperty(Strings.LibPathPropName,
			DefaultValue = "Specify path to JniBridge library",
			IsRequired = true)]
		public string libPath {
			get {
				return this[Strings.LibPathPropName].ToString();
			}
			set {
				this[Strings.LibPathPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the configuration file path.
		/// </summary>
		/// <value>
		/// The configuration file path.
		/// </value>
		[ConfigurationProperty(Strings.ConfigFilePathPropName,
			DefaultValue = "Specify path to JniBridge library's config file",
			IsRequired = true)]
		public string libConfigFilePath {
			get {
				return this[Strings.ConfigFilePathPropName].ToString();
			}
			set {
				this[Strings.ConfigFilePathPropName] = value;
			}
		}


		/// <summary>
		/// Gets or sets the selected JVM.
		/// </summary>
		/// <value>
		/// The selected JVM.
		/// </value>
		[ConfigurationProperty(Strings.SelectedJvmPropName,
			DefaultValue = "Specify path to selected JVM ",
			IsRequired = true)]
		public string selectedJvm {
			get {
				return this[Strings.SelectedJvmPropName].ToString();
			}
			set {
				this[Strings.SelectedJvmPropName] = value;
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