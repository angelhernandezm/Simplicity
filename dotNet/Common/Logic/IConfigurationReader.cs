using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	public interface IConfigurationReader {
		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <value>
		/// The configuration.
		/// </value>
		ConfigurationSection Configuration {
			get;
		}

		/// <summary>
		/// Refreshes this instance.
		/// </summary>
		void Refresh();
	}
}
