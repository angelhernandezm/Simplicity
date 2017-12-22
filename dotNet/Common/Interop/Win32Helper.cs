using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Common.Interop {
	public class Win32Helper {
		#region "Consts"
		/// <summary>
		/// The maximum path
		/// </summary>
		public const int Max_Path = 260;

		/// <summary>
		/// The maximum buffer size for messages from jni
		/// </summary>
		public const int Max_BufferSizeForMessagesFromJni = 4096;

		/// <summary>
		/// The file share read
		/// </summary>
		public const int FILE_SHARE_READ = 0x00000001;

		/// <summary>
		/// The file share write
		/// </summary>
		public const int FILE_SHARE_WRITE = 0x00000002;

		/// <summary>
		/// The file share delete
		/// </summary>
		public const int FILE_SHARE_DELETE = 0x00000004;

		/// <summary>
		/// The standard rights read
		/// </summary>
		public const uint GENERIC_READ = 0x80000000;

		/// <summary>
		/// The generic write
		/// </summary>
		public const uint GENERIC_WRITE = 0x40000000;

		/// <summary>
		/// The open existing
		/// </summary>
		public const int OPEN_EXISTING = 3;

		/// <summary>
		/// The open always
		/// </summary>
		public const int OPEN_ALWAYS = 4;

		/// <summary>
		/// The file attribute normal
		/// </summary>
		public const int FILE_ATTRIBUTE_NORMAL = 128;

		#endregion

		#region "Enums"

		/// <summary>
		/// 
		/// </summary>
		public enum SymbolicLink {
			File = 0,
			Directory = 1
		}

		#endregion

		#region "Delegates"
		/// <summary>
		/// 
		/// </summary>
		/// <param name="configFile">The configuration file.</param>
		/// <param name="ptrInstance">The PTR instance.</param>
		/// <returns></returns>
		public delegate int InitializeJvm(string configFile);

		/// <summary>
		/// 
		/// </summary>
		public delegate void ShutdownJvm();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="result">The result.</param>
		/// <param name="exceptions">The exceptions.</param>
		/// <returns></returns>
		public delegate int AddPath(string jarFile, StringBuilder result, StringBuilder exceptions);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="jarFile">The jar file.</param>
		/// <param name="xmlPath">The XML path.</param>
		/// <param name="exceptions">The exceptions.</param>
		/// <returns></returns>
		public delegate int SerializeMethodsInJar(string jarFile, string xmlPath, StringBuilder exceptions);

		/// <summary>
		/// 
		/// </summary>
		public delegate void InvokeGC();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="results">The results.</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ResultCallback(IntPtr results);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="className">Name of the class.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="isStatic">if set to <c>true</c> [is static].</param>
		/// <param name="methodProto">The method proto.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="paramType">Type of the parameter.</param>
		/// <param name="arrayLength">Length of the array.</param>
		/// <param name="resultCallback">The result callback.</param>
		/// <param name="exceptions">The exceptions.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <returns></returns>
		public delegate int RunMethodInJar(string className, string methodName, bool isStatic, string methodProto,
			 [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] object[] parameters,
			 [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] string[] paramType,
			 int arrayLength, IntPtr resultCallback, StringBuilder exceptions, string returnType);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForFloat(float value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public delegate void ReturnDelegateForBoolean(bool value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForByte(byte value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForChar(char value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForShort(short value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForInteger(int value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForLong(long value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForDouble(double value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForObject(object value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">The value.</param>
		public delegate void ReturnDelegateForString(string value);

		#endregion

		#region "Imports - Win32 specific"
		/// <summary>
		/// Loads the library.
		/// </summary>
		/// <param name="lpFileName">Name of the lp file.</param>
		/// <returns></returns>
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		/// <summary>
		/// Closes the handle.
		/// </summary>
		/// <param name="hHandle">The h handle.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hHandle);

		/// <summary>
		/// Frees the library.
		/// </summary>
		/// <param name="hModule">The h module.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FreeLibrary(IntPtr hModule);

		/// <summary>
		/// Gets the proc address.
		/// </summary>
		/// <param name="hModule">The h module.</param>
		/// <param name="procName">Name of the proc.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);


		/// <summary>
		/// Creates the symbolic link.
		/// </summary>
		/// <param name="lpSymlinkFileName">Name of the lp symlink file.</param>
		/// <param name="lpTargetFileName">Name of the lp target file.</param>
		/// <param name="dwFlags">The dw flags.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

		/// <summary>
		/// Creates the file.
		/// </summary>
		/// <param name="lpFileName">Name of the lp file.</param>
		/// <param name="dwDesiredAccess">The dw desired access.</param>
		/// <param name="dwShareMode">The dw share mode.</param>
		/// <param name="SecurityAttributes">The security attributes.</param>
		/// <param name="dwCreationDisposition">The dw creation disposition.</param>
		/// <param name="dwFlagsAndAttributes">The dw flags and attributes.</param>
		/// <param name="hTemplateFile">The h template file.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr SecurityAttributes,
											uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		/// <summary>
		/// Gets the module handle.
		/// </summary>
		/// <param name="lpModuleName">Name of the lp module.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		/// <summary>
		/// Frees the library and exit thread.
		/// </summary>
		/// <param name="hModule">The h module.</param>
		/// <param name="exitCode">The exit code.</param>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern void FreeLibraryAndExitThread(IntPtr hModule, int exitCode);

		#endregion
	}
}
