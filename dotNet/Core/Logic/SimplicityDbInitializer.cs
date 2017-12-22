using Simplicity.dotNet.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="System.Data.Entity.IDatabaseInitializer{T}" />
	internal class SimplicityDbInitializer<T> : IDatabaseInitializer<T> where T : DbContext {
		/// <summary>
		/// The simplicity database
		/// </summary>
		private string _simplicityDb = string.Empty;

		/// <summary>
		/// The connection string
		/// </summary>
		private string _connectionString = string.Empty;

		/// <summary>
		/// Executes the strategy to initialize the database for the given context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <exception cref="NotImplementedException"></exception>
		public void InitializeDatabase(T context) {
			_connectionString = context.Database.Connection.ConnectionString;
			_simplicityDb = _connectionString.Replace("Data Source=", string.Empty);

			if (File.Exists(_simplicityDb)) {
				context.Database.Connection.Open();
			} else {
				var tables = new Dictionary<string, string>() {{Strings.DbSpecific.DynamicLibraryCreateTable, Strings.DbSpecific.DynamicLibrary_TableIndex},
															   {Strings.DbSpecific.HostedLibraryCreateTable, Strings.DbSpecific.HostedLibrary_TableIndex },
															   {Strings.DbSpecific.JavaClassMetadataCreateTable, Strings.DbSpecific.JavaClassMetadata_TableIndex },
															   {Strings.DbSpecific.JniMethodInformationCreateTable, Strings.DbSpecific.JniMethodInformation_TableIndex} };

				SQLiteConnection.CreateFile(_simplicityDb);
				context.Database.Connection.Open();

				// Create tables and indexes
				tables.ToList().ForEach(t => {
					using (var cmd = new SQLiteCommand(t.Key, (SQLiteConnection)context.Database.Connection)) {
						cmd.ExecuteNonQuery();
						cmd.CommandText = t.Value;
						cmd.ExecuteNonQuery();
					}
				});

				// Create delete trigger (Registration removal)
				using (var cmd = new SQLiteCommand(Strings.DbSpecific.RegistrationDeleteTrigger, (SQLiteConnection)context.Database.Connection)) {
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}