using System;
using System.Data.Linq.Mapping;
using Simplicity.dotNet.Common.Logic;
using System.ComponentModel.DataAnnotations;

namespace Simplicity.dotNet.Common.Data.Models {
	[Table(Name = "HostedLibrary")]
	public class HostedLibrary: IEntity {
		/// <summary>
		/// Gets or sets the hosted library identifier.
		/// </summary>
		/// <value>
		/// The hosted library identifier.
		/// </value>
		[Key]
		public string HostedLibraryId {
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
		/// Gets or sets the library URI.
		/// </summary>
		/// <value>
		/// The library URI.
		/// </value>
		public string LibraryURI {
			get; set;
		} 
	}
}
