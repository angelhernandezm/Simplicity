using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Infrastructure {
	/// <summary>
	/// 
	/// </summary>
	public class Bootstrapper {
		/// <summary>
		/// Runs the specified container setter.
		/// </summary>
		/// <param name="containerSetter">The container setter.</param>
		public static void Run(Action<IContainer> containerSetter) {
			var builder = new ContainerBuilder();
			builder.RegisterModule<TypeRegistration>();
			var container = builder.Build();
			containerSetter(container);
		}
	}
}
