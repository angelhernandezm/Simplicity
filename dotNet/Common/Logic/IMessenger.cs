using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IMessenger {
		/// <summary>
		/// Occurs when [notify].
		/// </summary>
		event MessengerDelegate Notify;

		/// <summary>
		/// Gets the default.
		/// </summary>
		/// <value>
		/// The default.
		/// </value>
		IMessenger Default {
			get;
		}

		/// <summary>
		/// Broadcasts the notification.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="type">The type.</param>
		void BroadcastNotification(object data, object sender = null, MessageType type = MessageType.NewRegistration);
	}
}
