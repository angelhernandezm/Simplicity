using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IGlobalContainer {
		/// <summary>
		/// Gets the type container.
		/// </summary>
		/// <value>
		/// The type container.
		/// </value>
		IContainer TypeContainer {
			get; set;
		}
	}
}
