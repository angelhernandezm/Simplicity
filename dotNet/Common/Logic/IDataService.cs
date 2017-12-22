using Simplicity.dotNet.Common.Data;
using Simplicity.dotNet.Common.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Logic {
	/// <summary>
	/// 
	/// </summary>
	public interface IDataService {
		/// <summary>
		/// Gets the table definition.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		string GetTableDefinition<T>(T t) where T : IEntity;

		/// <summary>
		/// Gets the simplicity database.
		/// </summary>
		/// <value>
		/// The simplicity database.
		/// </value>
		string SimplicityDb {
			get;
		}

		/// <summary>
		/// Gets the get epoch time.
		/// </summary>
		/// <value>
		/// The get epoch time.
		/// </value>
		long EpochTime {
			get;
		}

		/// <summary>
		/// Registers the library.
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="config">The configuration.</param>
		/// <param name="registration">The registration.</param>
		/// <returns></returns>
		ExecutionResult RegisterLibrary(string jarFile, IConfigurationReader config, Dictionary<string, JniMetadata> registration);

		/// <summary>
		/// Removes the previous registration if any.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns></returns>
		ExecutionResult RemovePreviousRegistrationIfAny(string assemblyName);

		/// <summary>
		/// Gets the dynamic libraries (If not assembly is specified, everything is returned)
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <returns>ExecutionResult that contains a Dictionary[string, KeyValuePair[IEntity, IEntity]] with information required by Service host. </returns>
		ExecutionResult GetDynamicLibraries(string assemblyName = "");

		/// <summary>
		/// Gets the method information for jni call.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="callingFrame">The calling frame.</param>
		/// <param name="argList">The argument list.</param>
		/// <returns>ExecutionResult that contains a Tuple[[IEntity, IEntity, IEntity]] with information required to call method through JNI.</returns>
		ExecutionResult GetMethodInformationForJniCall(Type service, StackFrame callingFrame, params object[] argList);
	}
}
