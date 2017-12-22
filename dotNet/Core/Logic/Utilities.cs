using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simplicity.dotNet.Common.Interop;
using Common = Simplicity.dotNet.Common;
using Simplicity.dotNet.Core.Configuration;
using System.Configuration;
using System.IO;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IUtilities" />
	public class Utilities : IUtilities {
		/// <summary>
		/// Runs the java env checks.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public ExecutionResult RunJavaEnvChecks() {
			var retval = new ExecutionResult {IsSuccess = true };

			if (!CheckRequiredJreSymLinks(true))
				return new ExecutionResult { LastExceptionIfAny = new Exception(Common.Strings.SymLinksToJreAreMissing)}; 

			if (!CheckForJavaEnvVariables())
				return new ExecutionResult { LastExceptionIfAny = new Exception(Common.Strings.JavaEnvVarAreMissing) };

			return retval;
		}

		/// <summary>
		/// Checks the required jre sym links.
		/// </summary>
		/// <param name="deleteAndCreateifExist">if set to <c>true</c> [delete and createif exist].</param>
		/// <returns></returns>
		private bool CheckRequiredJreSymLinks(bool deleteAndCreateifExist) {
			var successCount = 0;
			var c = ConfigurationManager.AppSettings;

			var symLinks = new Dictionary<string, string> {
				{c[Common.Strings.SymLinkLibNameKey], c[Common.Strings.SymLinkLibTargetKey] },
				{c[Common.Strings.SymLinkBinNameKey], c[Common.Strings.SymLinkBinTargetKey] }
			};

			symLinks.ToList().ForEach(_ => {
				try {
					var fi = new FileInfo(_.Key);

					// Delete SYMLINK if exists
					if (fi.Attributes.HasFlag(FileAttributes.ReparsePoint))
						Directory.Delete(_.Key);

					// Create SYMLINK
					if (Win32Helper.CreateSymbolicLink(_.Key, _.Value, Win32Helper.SymbolicLink.Directory))
						successCount++;
				} catch {
					// Exception safe to ignore, most likely to querying non-existent SYMLINK
				}
			});

			return  successCount == symLinks.Count;
		}

		/// <summary>
		/// Checks for java env variables.
		/// </summary>
		/// <returns></returns>
		private bool CheckForJavaEnvVariables() {
			var count = 0;
			var variables = new List<string> {Common.Strings.JavaHomeEnvVar, Common.Strings.JreHomeEnvVar };

			variables.ForEach(_ => {
				if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(_)))
					count++;
			});

			return variables.Count == count;
		}
	}
}
