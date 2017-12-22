using Simplicity.dotNet.Common;
using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IMessenger" />
	public class Messenger : IMessenger {
		/// <summary>
		/// The instance
		/// </summary>
		private static IMessenger _instance;

		/// <summary>
		/// The instance identifier
		/// </summary>
		private Guid _instanceId;

		/// <summary>
		/// Gets the current.
		/// </summary>
		/// <value>
		/// The current.
		/// </value>
		public static IMessenger Current {
			get {
				lock (new object()) {
					if (_instance == null)
						_instance = new Messenger(Guid.NewGuid());
				}
				return _instance;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Messenger"/> class.
		/// </summary>
		public Messenger() {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Messenger"/> class.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		private Messenger(Guid instanceId) {
			_instanceId = instanceId;
		}

		/// <summary>
		/// Gets the default.
		/// </summary>
		/// <value>
		/// The default.
		/// </value>
		public IMessenger Default => Current;

		/// <summary>
		/// Occurs when [notify].
		/// </summary>
		public event MessengerDelegate Notify;

		/// <summary>
		/// Broadcasts the notification.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="type">The type.</param>
		public void BroadcastNotification(object data, object sender = null, MessageType type = MessageType.NewRegistration) {
			Notify?.Invoke(sender ?? this, type, data);
		}
	}
}	 