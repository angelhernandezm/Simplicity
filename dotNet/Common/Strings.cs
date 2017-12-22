using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common {
	/// <summary>
	/// 
	/// </summary>
	public class Strings {
		public const string JarFileExt = ".jar";
		public const string JreHomeEnvVar = "JRE_HOME";
		public const string JavaHomeEnvVar = "JAVA_HOME";
		public const double CleanUpTimerInterval = 60000;
		public const string ServiceName = "SimplicityDaemon";
		public const string ConfigFileExtension = "*.config";
		public const string SymLinkLibNameKey = "SymLinkLibName";
		public const string SymLinkBinNameKey = "SymLinkBinName";
		public const string ConfigSectionName = "SimplicityDaemon";
		public const string SymLinkLibTargetKey = "SymLinkLibTarget";
		public const string SymLinkBinTargetKey = "SymLinkBinTarget";
		public const string LogCreatedMessage = "SimplicityDaemon log created.";
		public const string DefaultServiceTemplateFileName = "ServiceTemplate.xml";
		public const string TypeContainerCantBeNull = "TypeContainer can't be null";
		public const string DefaultConversionMasterFileName = "ConversionMaster.xml";
		public const string PostReconfigureMessage = "SimplicityDaemon has been reconfigured.";
		public const string InvalidFileSystemEventArgs = "An invalid FileSystemEventArgs was received";
		public const string SymLinksToJreAreMissing = "SymLinks to JRE aren't available. Unable to continue.";
		public const string PreReconfigureMessage = "SimplicityDaemon about to apply new configuration changes.";
		public const string JavaEnvVarAreMissing = "Make sure JAVA_HOME and JRE_HOME environment variables exist. Unable to continue.";
		public const string ConfigurationErrorStopServiceMessage = "There has been a configuration error. SimplicityDaemon will be stopped. Please refer to logs for more information";

		/// <summary>
		/// 
		/// </summary>
		public class DbSpecific {
			/// <summary>
			/// The dynamic library create table
			/// </summary>
			public const string DynamicLibraryCreateTable =
				@"CREATE TABLE `DynamicLibrary` (
								`LibraryId`	varchar PRIMARY KEY,
								`AssemblyName`	varchar NOT NULL,
								`JarFileName`	varchar NOT NULL,
								`AssemblyLocation`	varchar NOT NULL,
								`JarFileLocation`	varchar NOT NULL,
								`IsHostingEnabled`	integer,
								`RegisteredDate`	integer,
								`CommentsIfAny`	varchar
							  );";


			/// <summary>
			/// The hosted library create table
			/// </summary>
			public const string HostedLibraryCreateTable =
				 @"CREATE TABLE `HostedLibrary` (
								`HostedLibraryId`	varchar PRIMARY KEY,
								`Fk_DynamicLibraryId`	varchar  NOT NULL,
								`LibraryURI`	varchar  NOT NULL,
								FOREIGN KEY(`Fk_DynamicLibraryId`) REFERENCES DynamicLibrary (LibraryId)
							  );";

			/// <summary>
			/// The java class metadata create table
			/// </summary>
			public const string JavaClassMetadataCreateTable =
					@"CREATE TABLE `JavaClassMetadata` (
							`MetadataEntryId`	varchar PRIMARY KEY,
							`Fk_DynamicLibraryId`	varchar  NOT NULL,
							`ClassName`	varchar  NOT NULL,
							`JavaClassDefinition`	varchar  NOT NULL,
							FOREIGN KEY(`Fk_DynamicLibraryId`) REFERENCES DynamicLibrary (LibraryId)
						  );";

			/// <summary>
			/// The jni method information create table
			/// </summary>
			public const string JniMethodInformationCreateTable =
				@"CREATE TABLE `JniMethodInformation` (
							`MethodId`	varchar PRIMARY KEY,
							`Fk_ClassMetadataId`	varchar  NOT NULL,
							`JavaMethod`	varchar  NOT NULL,
							`JniDescriptor`	varchar  NOT NULL,
							FOREIGN KEY(`Fk_ClassMetadataId`) REFERENCES JavaClassMetadata (MetadataEntryId)
						  );";


			/// <summary>
			/// The registration delete trigger
			/// </summary>
			public const string RegistrationDeleteTrigger =
				@"CREATE TRIGGER on_delete_remove_dependencies BEFORE DELETE ON DynamicLibrary
					 BEGIN
						 Delete 
                            From JniMethodInformation 
							 Where Fk_ClassMetadataId = (Select MetadataEntryId 
                                                           From JavaClassMetadata
                                                               Where Fk_DynamicLibraryId = old.LibraryId) ;
                         Delete
                            From JavaClassMetadata 
                              Where Fk_DynamicLibraryId = old.LibraryId;

                         Delete 
                            From HostedLibrary 
                               Where Fk_DynamicLibraryId = old.LibraryId;
						END";

			/// <summary>
			/// The dynamic library table index
			/// </summary>
			public const string DynamicLibrary_TableIndex = @"CREATE INDEX ""DynamicLibrary_TableIndex"" on ""DynamicLibrary""(""AssemblyName"")";

			/// <summary>
			/// The hosted library table index
			/// </summary>
			public const string HostedLibrary_TableIndex = @"CREATE INDEX ""HostedLibrary_TableIndex"" on ""HostedLibrary""(""Fk_DynamicLibraryId"")";

			/// <summary>
			/// The java class metadata table index
			/// </summary>
			public const string JavaClassMetadata_TableIndex = @"CREATE INDEX ""JavaClassMetadata_TableIndex"" on ""JavaClassMetadata""(""Fk_DynamicLibraryId"")";

			/// <summary>
			/// The jni method information table index
			/// </summary>
			public const string JniMethodInformation_TableIndex = @"CREATE INDEX ""JniMethodInformation_TableIndex"" on ""JniMethodInformation""(""Fk_ClassMetadataId"")";
		}
	}
}