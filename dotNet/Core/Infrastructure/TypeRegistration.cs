using Autofac;
using Simplicity.dotNet.Builder.Logic;
using Simplicity.dotNet.Common.Logic;
using Simplicity.dotNet.Common.Service;
using Simplicity.dotNet.Core.Configuration;
using Simplicity.dotNet.Core.Logic;
using Simplicity.dotNet.Core.Service;
using Simplicity.dotNet.Parser.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Infrastructure {
	/// <summary>
	/// 
	/// </summary>
	public class TypeRegistration: Module {
		/// <summary>
		/// Loads the specified builder.
		/// </summary>
		/// <param name="builder">The builder.</param>
		protected override void Load(ContainerBuilder builder) {
			builder.RegisterType<Logger>().As<ILogger>();
			builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
			builder.RegisterType<Utilities>().As<IUtilities>();
			builder.RegisterType<JniBridgeManager>().As<IJniBridgeManager>().SingleInstance();
			builder.RegisterType<ConfigurationReader>().As<IConfigurationReader>();
			builder.RegisterType<DefaultParser>().As<IParser>();
			builder.RegisterType<DefaultBuilder>().As<IBuilder>();
			builder.RegisterType<DataService>().As<IDataService>();
			builder.RegisterType<JavaParserFactory>().As<IJavaParserFactory>();
			builder.RegisterType<ServiceHostManager>().As<IServiceHostManager>();
			builder.RegisterType<GlobalContainer>().As<IGlobalContainer>().SingleInstance();
			builder.RegisterType<PerfCounters>().As<IPerfCounters>().SingleInstance();
			builder.RegisterType<SimplicityDaemon>().As<IBaseService<SimplicityDaemon>>().SingleInstance(); 
		}
	}
}
