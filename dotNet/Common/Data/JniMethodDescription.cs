using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Data {
	/// <summary>
	/// 
	/// </summary>
	public class JniMethodDescription {
		/// <summary>
		/// Gets or sets the type of the target.
		/// </summary>
		/// <value>
		/// The type of the target.
		/// </value>
		public Type TargetType {
			get; set;
		}

		/// <summary>
		/// Gets or sets the name of the method.
		/// </summary>
		/// <value>
		/// The name of the method.
		/// </value>
		public string MethodName {
			get; set;
		}

		/// <summary>
		/// Gets or sets the argument count.
		/// </summary>
		/// <value>
		/// The argument count.
		/// </value>
		public int ArgCount {
			get; set;
		}

		/// <summary>
		/// Gets or sets the type of the return.
		/// </summary>
		/// <value>
		/// The type of the return.
		/// </value>
		public string ReturnType {
			get; set;
		}

		/// <summary>
		/// Gets the method signature.
		/// </summary>
		/// <value>
		/// The method signature.
		/// </value>
		public string MethodSignature => $"{MethodName}~{ArgCount}~{ReturnType}~{TargetType.FullName}";

		/// <summary>
		/// Initializes a new instance of the <see cref="JniMethodDescription" /> class.
		/// </summary>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="targetType">Type of the target.</param>
		public JniMethodDescription(StackFrame callingFrame, Type targetType) {
			TargetType = targetType;
			var method = callingFrame.GetMethod();
			ArgCount = method.GetParameters().Length;
			MethodName = method.Name.Split(' ').FirstOrDefault(); // Method name will always be index 0
			ReturnType = ((MethodInfo)method).ReturnParameter.ToString().ToLower().Trim();
		}
	}
}
