using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Service {
	public interface ICustomServiceBase<out T, out TV>
		 where T : IServiceInformation {
		/// <summary>
		/// Gets the type container.
		/// </summary>
		/// <value>
		/// The type container.
		/// </value>
		TV TypeContainer {
			get;
		}

	}
}
