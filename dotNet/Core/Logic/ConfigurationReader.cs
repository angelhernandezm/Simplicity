using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using Simplicity.dotNet.Core.Configuration;

namespace Simplicity.dotNet.Core.Logic {
	public class ConfigurationReader : IConfigurationReader {
		/// <summary>
		/// The _logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <value>
		/// The configuration.
		/// </value>
		public ConfigurationSection Configuration {
			get; private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationReader"/> class.
		/// </summary>
		public ConfigurationReader() {
			Configuration = ConfigurationManager.GetSection(Strings.ConfigSectionName) as CustomConfigReader;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationReader"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public ConfigurationReader(ILogger logger) : this() {
			_logger = logger;
		}


		/// <summary>
		/// Rehydrates the value from disk.
		/// </summary>
		/// <param name="elements">The elements.</param>
		private void RehydrateValueFromDisk(IEnumerable<XElement> elements) {
			var elementsList = elements.ToList();
			var tempConfig = (CustomConfigReader)Configuration;

			elementsList.ForEach(p => {
				//TODO: Update this code accordingly to reflect config section in use

				/* if (p.Name == "SSISBroker") {
					tempConfig.SSISBroker.LocalFolderPath = p.Attribute("LocalFolderPath").Value;
				} else if (p.Name == "UploadRole") {
					tempConfig.UploadRole.LocalFolderPath = p.Attribute("LocalFolderPath").Value;
					tempConfig.UploadRole.RemoteFolderPath = p.Attribute("RemoteFolderPath").Value;
					tempConfig.UploadRole.ShapeFileExtension = p.Attribute("ShapeFileExtension").Value;
					tempConfig.UploadRole.FileExtensionToBeProcessed = p.Attribute("FileExtensionToBeProcessed").Value;
				} else if (p.Name == "DTExec") {
					tempConfig.DTExec.ImagePath = p.Attribute("ImagePath").Value;
					tempConfig.DTExec.PackageFilePath = p.Attribute("PackageFilePath").Value;
					tempConfig.DTExec.DeleteFileOnCompletion = bool.Parse(p.Attribute("DeleteFileOnCompletion").Value);
					tempConfig.DTExec.InputFilePackageParameterName = p.Attribute("InputFilePackageParameterName").Value;
				}  */
			});
		}

		/// <summary>
		/// Refreshes this instance.
		/// </summary>
		public void Refresh() {
			try {
				using (var sr = new StreamReader(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath)) {
					var config = XDocument.Load(sr);
					RehydrateValueFromDisk(config.Element("configuration")?.Element("SimplicityDaemon")?.Descendants());
				}
			} catch (Exception ex) {
				_logger.Log(ex);
			}
		}
	}
}
