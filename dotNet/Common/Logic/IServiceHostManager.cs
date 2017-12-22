using Simplicity.dotNet.Common.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IServiceHostManager {
		/// <summary>
		/// Gets the dynamic hosts.
		/// </summary>
		/// <value>
		/// The dynamic hosts.
		/// </value>
		Dictionary<IEntity, ServiceHost> DynamicHosts {
			get;
		}

		/// <summary>
		/// Manages the hosts (starts & stops). If no entity is passed in then it'll load everything
		/// as per configuration in database, otherwise it'll load the one that's been specified.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="host">The host.</param>
		/// <returns>Collection of errors encountered.</returns>
		IEnumerable<ExecutionResult> ManageHost(HostAction action, IEntity host = null);
	}
}
