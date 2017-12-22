using System;
using System.Data.Linq.Mapping;
using Simplicity.dotNet.Common.Logic;
using System.ComponentModel.DataAnnotations;

namespace Simplicity.dotNet.Common.Data.Models {
	[Table(Name = "DynamicLibrary")]
	public class DynamicLibrary : IEntity {
		/// <summary>
		/// Gets or sets the library identifier.
		/// </summary>
		/// <value>
		/// The library identifier.
		/// </value>
		[Key]
		public string LibraryId {
			get; set;
		}

		/// <summary>
		/// Gets or sets the name of the assembly.
		/// </summary>
		/// <value>
		/// The name of the assembly.
		/// </value>
		public string AssemblyName {
			get; set;
		}

		/// <summary>
		/// Gets or sets the name of the jar file.
		/// </summary>
		/// <value>
		/// The name of the jar file.
		/// </value>
		public string JarFileName {
			get; set;
		}

		/// <summary>
		/// Gets or sets the assembly location.
		/// </summary>
		/// <value>
		/// The assembly location.
		/// </value>
		public string AssemblyLocation {
			get; set;
		}

		/// <summary>
		/// Gets or sets the jar file location.
		/// </summary>
		/// <value>
		/// The jar file location.
		/// </value>
		public string JarFileLocation {
			get; set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is hosting enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is hosting enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsHostingEnabled {
			get; set;
		}

		/// <summary>
		/// Gets or sets the registered date.
		/// </summary>
		/// <value>
		/// The registered date.
		/// </value>
		public long RegisteredDate {
			get; set;
		}

		/// <summary>
		/// Gets or sets the comments if any.
		/// </summary>
		/// <value>
		/// The comments if any.
		/// </value>
		public string CommentsIfAny {
			get; set;
		}

		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <returns></returns>
		public DateTime GetDate() {
			var retval = DateTime.Now;

			try {
				var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				retval = dtDateTime.AddSeconds(RegisteredDate).ToLocalTime();
			} catch {
				// Safe to swallow the exception. We'll return Current DateTime instead
				// AddSeconds might throw an exception if number passed in is too big
			}

			return retval;
		}
	}
}
