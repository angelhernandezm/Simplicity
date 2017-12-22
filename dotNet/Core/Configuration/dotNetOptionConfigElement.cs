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
	public class dotNetOptionConfigElement : ConfigurationElement {
		/// <summary>
		/// Gets or sets the host port.
		/// </summary>
		/// <value>
		/// The host port.
		/// </value>
		[ConfigurationProperty(Strings.HostPortPropName,
			DefaultValue = "8080",
			IsRequired = true)]
		public uint hostPort {
			get {
				return (uint)this[Strings.HostPortPropName];
			}
			set {
				this[Strings.HostPortPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the log file location.
		/// </summary>
		/// <value>
		/// The log file location.
		/// </value>
		[ConfigurationProperty(Strings.LogFileLocationPropName,
			DefaultValue = "Specify location where log files will be stored",
			IsRequired = true)]
		public string logFileLocation {
			get {
				return this[Strings.LogFileLocationPropName].ToString();
			}
			set {
				this[Strings.LogFileLocationPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [use HTTPS].
		/// </summary>
		/// <value>
		///   <c>true</c> if [use HTTPS]; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty(Strings.UseHttpsPropName,
			DefaultValue = "false",
			IsRequired = true)]
		public bool useHttps {
			get {
				return (bool)this[Strings.UseHttpsPropName];
			}
			set {
				this[Strings.UseHttpsPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the working folder.
		/// </summary>
		/// <value>
		/// The working folder.
		/// </value>
		[ConfigurationProperty(Strings.WorkingFolderPropName,
			DefaultValue = "Specify working folder where Simplicity will create temporary files and similar",
			IsRequired = true)]
		public string workingFolder {
			get {
				return this[Strings.WorkingFolderPropName].ToString();
			}
			set {
				this[Strings.WorkingFolderPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [enable perf counters].
		/// </summary>
		/// <value>
		///   <c>true</c> if [enable perf counters]; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty(Strings.EnablePerfCountersPropName,
			DefaultValue = "false",
			IsRequired = true)]
		public bool enablePerfCounters {
			get {
				return (bool) this[Strings.EnablePerfCountersPropName];
			}
			set {
				this[Strings.EnablePerfCountersPropName] = value;
			}
		}

		/// <summary>
		/// Gets or sets the trace out put file.
		/// </summary>
		/// <value>
		/// The trace out put file.
		/// </value>
		[ConfigurationProperty(Strings.TraceOutputFilePropName,
			DefaultValue = @"c:\Temp\service_output.log",
			IsRequired = true)]
		public string traceOutPutFile {
			get {
				return this[Strings.TraceOutputFilePropName].ToString();
			}
			set {
				this[Strings.TraceOutputFilePropName] = value;
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