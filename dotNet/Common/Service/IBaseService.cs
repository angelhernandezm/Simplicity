using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Service {
	public interface IBaseService<out T> where T : Component {
		/// <summary>
		/// Gets the service instance.
		/// </summary>
		/// <value>
		/// The service instance.
		/// </value>
		T ServiceInstance {
			get;
		}

		/// <summary>
		/// Gets the host manager.
		/// </summary>
		/// <value>
		/// The host manager.
		/// </value>
		IServiceHostManager HostManager {
			get;
		}
	}
}
