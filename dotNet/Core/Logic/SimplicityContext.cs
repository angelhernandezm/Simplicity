using Simplicity.dotNet.Common.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	internal class SimplicityContext : DbContext {
		/// <summary>
		/// The simplicity database
		/// </summary>
		private static readonly string _simplicityDb;

		/// <summary>
		/// The connection string
		/// </summary>
		public static readonly string ConnectionString;

		/// <summary>
		/// Initializes the <see cref="SimplicityContext"/> class.
		/// </summary>
		static SimplicityContext() {
			var currentDir = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"\DB\");
			_simplicityDb = string.Concat(currentDir, "Simplicity.db3");

			// If DB folder doesn't exist, we create it
			if (!Directory.Exists(currentDir))
				Directory.CreateDirectory(currentDir);

			ConnectionString = $@"Data Source={_simplicityDb}";
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="SimplicityContext"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public SimplicityContext(string connectionString) : base(new SQLiteConnection() { ConnectionString = connectionString }, true) {

		}

		/// <summary>
		/// This method is called when the model for a derived context has been initialized, but
		/// before the model has been locked down and used to initialize the context.  The default
		/// implementation of this method does nothing, but it can be overridden in a derived class
		/// such that the model can be further configured before it is locked down.
		/// </summary>
		/// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
		/// <remarks>
		/// Typically, this method is called only once when the first instance of a derived context
		/// is created.  The model for that context is then cached and is for all further instances of
		/// the context in the app domain.  This caching can be disabled by setting the ModelCaching
		/// property on the given ModelBuidler, but note that this can seriously degrade performance.
		/// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
		/// classes directly.
		/// </remarks>
		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			var initializer = new SimplicityDbInitializer<SimplicityContext>();
			Database.SetInitializer(initializer);
		}

		/// <summary>
		/// Gets or sets the dynamic libraries.
		/// </summary>
		/// <value>
		/// The dynamic libraries.
		/// </value>
		public DbSet<DynamicLibrary> DynamicLibrary {
			get; set;
		}

		/// <summary>
		/// Gets or sets the hosted library.
		/// </summary>
		/// <value>
		/// The hosted library.
		/// </value>
		public DbSet<HostedLibrary> HostedLibrary {
			get; set;
		}

		/// <summary>
		/// Gets or sets the java class metadata.
		/// </summary>
		/// <value>
		/// The java class metadata.
		/// </value>
		public DbSet<JavaClassMetadata> JavaClassMetadata {
			get; set;
		}

		/// <summary>
		/// Gets or sets the jni method information.
		/// </summary>
		/// <value>
		/// The jni method information.
		/// </value>
		public DbSet<JniMethodInformation> JniMethodInformation {
			get; set;
		}  
	}
}