using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IPerfCounters: IDisposable {
		/// <summary>
		/// Gets the created on.
		/// </summary>
		/// <value>
		/// The created on.
		/// </value>
		DateTime CreatedOn {
			get;
		}

		/// <summary>
		/// Gets the instance identifier.
		/// </summary>
		/// <value>
		/// The instance identifier.
		/// </value>
		Guid InstanceId {
			get;
		}

		/// <summary>
		/// Gets the <see cref="PerformanceCounter"/> with the specified counter name.
		/// </summary>
		/// <value>
		/// The <see cref="PerformanceCounter"/>.
		/// </value>
		/// <param name="counterName">Name of the counter.</param>
		/// <returns></returns>
		PerformanceCounter this[Enums.PerfCounter counterName] {
			get;
		}

		/// <summary>
		/// Increments the counter.
		/// </summary>
		/// <param name="counterName">Name of the counter.</param>
		void IncrementCounter(Enums.PerfCounter counterName);
	}
}
