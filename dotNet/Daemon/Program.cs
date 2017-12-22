using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace Simplicity.dotNet.Daemon {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args) {
			var servicesToRun = new ServiceBase[] { new Simplicity.dotNet.Core.Service.SimplicityDaemon(args) };
			ServiceBase.Run(servicesToRun);
		}
	}
}

