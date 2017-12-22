using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Common {
	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="type">The type.</param>
	/// <param name="data">The data.</param>
	public delegate void MessengerDelegate(object sender, MessageType type, object data);

	/// <summary>
	/// 
	/// </summary>
	public static class Extensions {
		/// <summary>
		/// Formats the exception as string.
		/// </summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		public static string FormatExceptionAsString(this Exception ex) {
			var retval = string.Empty;

			if (ex != null)
				retval = $"Error caught - Details are:\nException:{ex}\nMessage:{ex.Message}\nStack Trace:{ex.StackTrace}";

			return retval;
		}

		/// <summary>
		/// Creates the specified configuration.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <param name="host">The host.</param>
		/// <returns></returns>
		public static ServiceConfiguration Create(this ServiceConfiguration config, ref ServiceHost host) {
			ServiceConfiguration retval = null;

			try {
				var type = typeof(ServiceConfiguration);
				var ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance).FirstOrDefault();
				retval = (ServiceConfiguration) ctor.Invoke(new object[] {host });
			} catch {
				// Safe to ignore. If ServiceConfiguration object can't be created we'll return null as error condition
			}

			return retval;
		}
	}
}
