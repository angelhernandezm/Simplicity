using System;
using System.Data.Linq.Mapping;
using Simplicity.dotNet.Common.Logic;
using System.ComponentModel.DataAnnotations;

namespace Simplicity.dotNet.Common.Data.Models {
	[Table(Name = "JavaClassMetadata")]
	public class JavaClassMetadata: IEntity {
		/// <summary>
		/// Gets or sets the metadata entry identifier.
		/// </summary>
		/// <value>
		/// The metadata entry identifier.
		/// </value>
		[Key]
		public string MetadataEntryId {
			get; set;
		}

		/// <summary>
		/// Gets or sets the fk dynamic library identifier.
		/// </summary>
		/// <value>
		/// The fk dynamic library identifier.
		/// </value>
		public string Fk_DynamicLibraryId {
			get; set;
		}

		/// <summary>
		/// Gets or sets the name of the class.
		/// </summary>
		/// <value>
		/// The name of the class.
		/// </value>
		public string ClassName {
			get; set;
		}

		/// <summary>
		/// Gets or sets the java class definition.
		/// </summary>
		/// <value>
		/// The java class definition.
		/// </value>
		public string JavaClassDefinition {
			get; set;
		}
	}
}