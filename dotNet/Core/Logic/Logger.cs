using Simplicity.dotNet.Common;
using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Logic {
	public class Logger : ILogger, IDisposable {
		/// <summary>
		/// The _is disposed
		/// </summary>
		private bool _isDisposed = false;

		/// <summary>
		/// The _event log
		/// </summary>
		private EventLog _eventLog;

		/// <summary>
		/// Initializes a new instance of the <see cref="Logger" /> class.
		/// </summary>
		public Logger() {
			Initialize();
		}

		/// <summary>
		/// Logs the specified message.
		/// </summary>
		/// <param name="caught"></param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Log(Exception caught) {
			if (caught != null)
				LogEntry(caught.FormatExceptionAsString());
		}

		/// <summary>
		/// Logs the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Log(string message) {
			if (!string.IsNullOrEmpty(message))
				LogEntry(message);
		}

		/// <summary>
		/// Logs the entry.
		/// </summary>
		/// <param name="message">The message.</param>
		protected virtual void LogEntry(string message) {
			if (!string.IsNullOrEmpty(message))
				_eventLog?.WriteEntry(message);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool isDisposing) {
			if (!_isDisposed) {
				if (isDisposing)
					_eventLog?.Dispose();
				_isDisposed = true;
			}
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		private void Initialize() {
			if (!EventLog.SourceExists(Strings.ServiceName)) {
				EventLog.CreateEventSource(Strings.ServiceName, Strings.ServiceName);
				_eventLog = new EventLog { Source = Strings.ServiceName };
				_eventLog.WriteEntry(Strings.LogCreatedMessage);
			} else
				_eventLog = new EventLog { Source = Strings.ServiceName };
		}								

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}										