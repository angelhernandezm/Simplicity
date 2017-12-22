using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Interop {
	public class ExecutionResult {
		/// <summary>
		/// The is success
		/// </summary>
		private bool _isSuccess = false;

		/// <summary>
		/// The jni error message
		/// </summary>
		private string _jniErrorMessage;

		/// <summary>
		/// The last exception if any
		/// </summary>
		private Exception _lastExceptionIfAny;

		/// <summary>
		/// The perf counters
		/// </summary>
		private static IPerfCounters _perfCounters;

		/// <summary>
		/// The logger
		/// </summary>
		private static ILogger _logger;

		/// <summary>
		/// The empty
		/// </summary>
		public static ExecutionResult Empty => new ExecutionResult();

		/// <summary>
		/// Attaches the perf counters.
		/// </summary>
		/// <param name="counters">The counters.</param>
		public static void AttachPerfCounters(IPerfCounters counters) {
			if (_perfCounters == null)
				_perfCounters = counters;
		}

		/// <summary>
		/// Attaches the logger.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public static void AttachLogger(ILogger logger) {
			if (_logger == null)
				_logger = logger;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is success.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is success; otherwise, <c>false</c>.
		/// </value>
		public bool IsSuccess {
			get {
				return _isSuccess;
			}
			set {
				_isSuccess = value;

				// Let's increase "SuccessfulExecution" performance counter
				if (value)
					_perfCounters?.IncrementCounter(Enums.PerfCounter.SuccessfulExecution);
			}
		}

		/// <summary>
		/// Gets or sets the last exception if any.
		/// </summary>
		/// <value>
		/// The last exception if any.
		/// </value>
		public Exception LastExceptionIfAny {
			get {
				return _lastExceptionIfAny;
			}
			set {
				_lastExceptionIfAny = value;
				_logger?.Log(value);
			}
		}

		/// <summary>
		/// Gets or sets the jni error message.
		/// </summary>
		/// <value>
		/// The jni error message.
		/// </value>
		public string JniErrorMessage {
			get {
				return _jniErrorMessage;
			}
			set {
				_jniErrorMessage = value;


				if (!string.IsNullOrEmpty(value) && value.Length > 0) {
					// Let's increase "JniException" performance counter
					_perfCounters?.IncrementCounter(Enums.PerfCounter.JniException);

					// Let's log JniException also
					_logger?.Log($"JNI exception occurred\n{value}");
				}
			}
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>
		/// The tag.
		/// </value>
		public object Tag {
			get; set;
		}
	}
}
