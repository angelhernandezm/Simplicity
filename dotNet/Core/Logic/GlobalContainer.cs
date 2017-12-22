using Autofac;
using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IGlobalContainer" />
	public class GlobalContainer : IGlobalContainer {
		/// <summary>
		/// The instance
		/// </summary>
		private static IGlobalContainer _instance;

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
		public static IGlobalContainer Current {
			get {
				lock (new object()) {
					if (_instance == null)
						_instance = new GlobalContainer(Guid.NewGuid());
				}
				return _instance;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalContainer"/> class.
		/// </summary>
		public GlobalContainer() {

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalContainer"/> class.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		private GlobalContainer(Guid instanceId) {
			_instanceId = instanceId;
		}

		/// <summary>
		/// Gets the type container.
		/// </summary>
		/// <value>
		/// The type container.
		/// </value>
		public IContainer TypeContainer {
			get;
			set;
		}
	}
}  