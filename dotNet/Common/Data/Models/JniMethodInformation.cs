using System;
using System.Data.Linq.Mapping;
using Simplicity.dotNet.Common.Logic;
using System.ComponentModel.DataAnnotations;

namespace Simplicity.dotNet.Common.Data.Models {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IEntity" />
	[Table(Name = "JniMethodInformation")]
	public class JniMethodInformation: IEntity {
		/// <summary>
		/// Gets or sets the method identifier.
		/// </summary>
		/// <value>
		/// The method identifier.
		/// </value>
		[Key]
		public string MethodId {
			get; set;
		}

		/// <summary>
		/// Gets or sets the fk class metadata identifier.
		/// </summary>
		/// <value>
		/// The fk class metadata identifier.
		/// </value>
		public string Fk_ClassMetadataId {
			get; set;
		}

		/// <summary>
		/// Gets or sets the java method.
		/// </summary>
		/// <value>
		/// The java method.
		/// </value>
		public string JavaMethod {
			get; set;
		}

		/// <summary>
		/// Gets or sets the jni descriptor.
		/// </summary>
		/// <value>
		/// The jni descriptor.
		/// </value>
		public string JniDescriptor {
			get; set;
		}
	}
}