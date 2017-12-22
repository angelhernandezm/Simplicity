using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	public interface ILogger {
		/// <summary>
		/// Logs the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		void Log(string message);

		/// <summary>
		/// Logs the specified caught.
		/// </summary>
		/// <param name="caught">The caught.</param>
		void Log(Exception caught);
	}
}
