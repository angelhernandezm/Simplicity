using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Parser.Logic {
	/// <summary>
	/// 
	/// </summary>
	public class ParserMarkup {
		#region "Consts"
		public const string Namespace = "~NamespaceName~";
		public const string Interface = "~InterfaceName~";
		public const string Class = "~ClassNameInJar~";
		public const string Return = "~ReturnType~";
		public const string Method = "~MethodName~";
		public const string Arguments = "~Arguments~";
		public const string WebArguments = "~WebArguments~";
		public const string ProcessArguments = "~ProcessArguments~";
		public const string ForEachMethodClass = "<for_each_method_class/>";
		public const string ForEachMethodInterface = "<for_each_method_interface/>";
		private const string SearchForTagRegex = @"(~[A-Z])\w+~";
		#endregion

		/// <summary>
		/// The used tags
		/// </summary>
		private static Dictionary<string, string> _usedTags = new Dictionary<string, string>() {
			{Namespace, "Namespace"},
			{Interface, "Interface"},
			{Class, "Class"},
			{Return, "Return"},
			{Method, "Method"},
			{Arguments, "Arguments"},
			{WebArguments, "WebArguments"},
			{ProcessArguments, "ProcessArguments"},
			{ForEachMethodClass, "ForEachMethodClass"},
			{ForEachMethodInterface, "ForEachMethodInterface"}
		};

		/// <summary>
		/// Determines whether [is markup present] [the specified line].
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="found">The found.</param>
		/// <returns></returns>
		public static MarkupType IsMarkupPresent(string line, out Dictionary<string, string> found) {
			var retval = MarkupType.Undefined;
			var regex = new Regex(SearchForTagRegex);
			found = new Dictionary<string, string>();
			var keywordCheck = new Func<string, bool>(p => {
				var result = false;

				if (!string.IsNullOrEmpty(p)) {
					if (p.Contains(Namespace) || p.Contains(Interface) || p.Contains(Class) ||
						 p.Contains(Return) || p.Contains(Method) || p.Contains(Arguments) ||
						 p.Contains(WebArguments))
						result = true;

				}
				return result;
			});

			if (!string.IsNullOrEmpty(line)) {
				if (line.Contains(ForEachMethodInterface) || line.Contains(ForEachMethodClass))
					retval = MarkupType.Statement;
				else if (keywordCheck(line)) {
					retval = MarkupType.Keyword;
					var matches = regex.Matches(line);

					foreach (var item in matches) {
					  	found.Add(item.ToString(), _usedTags[item.ToString()]);
					}
				}
			}

			return retval;
		}
	}
}
