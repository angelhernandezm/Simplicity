using Simplicity.dotNet.Common.Logic;
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
	public class CustomConfigReader : ConfigurationSection {
		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <returns></returns>
		public static CustomConfigReader GetConfiguration() {
			var retval = ConfigurationManager.GetSection(Strings.ConfigSectionName) as CustomConfigReader;

			return retval ?? new CustomConfigReader();
		}

		/// <summary>
		/// Gets or sets the jni bridge options.
		/// </summary>
		/// <value>
		/// The jni bridge options.
		/// </value>
		[ConfigurationProperty(Strings.JniBridgeOptionPropName)]
		public JniBridgeOptionConfigElement JniBridgeOptions {
			get {
				return (JniBridgeOptionConfigElement)this[Strings.JniBridgeOptionPropName];
			}
			set {
				this[Strings.JniBridgeOptionPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the JVM options.
		/// </summary>
		/// <value>
		/// The JVM options.
		/// </value>
		[ConfigurationProperty(Strings.JvmOptionPropName)]
		public JvmOptionConfigElement JvmOptions {
			get {
				return (JvmOptionConfigElement)this[Strings.JvmOptionPropName];
			}
			set {
				this[Strings.JvmOptionPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the dot net options.
		/// </summary>
		/// <value>
		/// The dot net options.
		/// </value>
		[ConfigurationProperty(Strings.DotNetOptionPropName)]
		public dotNetOptionConfigElement dotNetOptions {
			get {
				return (dotNetOptionConfigElement)this[Strings.DotNetOptionPropName];
			}
			set {
				this[Strings.DotNetOptionPropName] = value;
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