using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Core.Interop {
	/// <summary>
	/// 
	/// </summary>
	public class JniBridgeMethod {
		/// <summary>
		/// The add path
		/// </summary>
		public const string AddPath = "AddPath";
		/// <summary>
		/// The invoke gc
		/// </summary>
		public const string InvokeGC = "ForceGC";
		/// <summary>
		/// The shutdown JVM
		/// </summary>
		public const string ShutdownJvm = "ShutdownJvm";
		/// <summary>
		/// The load JVM and jar
		/// </summary>
		public const string LoadJVMAndJar = "LoadJVMAndJar";
		/// <summary>
		/// The run method in jar
		/// </summary>
		public const string RunMethodInJar = "RunMethodInJar";
		/// <summary>
		/// The serialize methods in jar
		/// </summary>
		public const string SerializeMethodsInJar = "SerializeMethodsInJar";
	}
}	