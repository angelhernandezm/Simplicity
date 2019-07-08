using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simplicity.dotNet.Common.Interop;
using System.IO;
using System.Reflection;
using Simplicity.dotNet.Common;
using System.Xml.Linq;
using static Simplicity.dotNet.Common.Enums;

namespace Simplicity.dotNet.Parser.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IParser" />
	public class DefaultParser : IParser {
		/// <summary>
		/// The logger
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The service template file path
		/// </summary>
		private string _serviceTemplateFilePath;

		/// <summary>
		/// The conversion master file path
		/// </summary>
		private string _conversionMasterFilePath;

		/// <summary>
		/// The jar information file
		/// </summary>
		private string _jarInformationFile;

		/// <summary>
		/// The service template document
		/// </summary>
		private XDocument _serviceTemplateDoc;

		/// <summary>
		/// The jar information document
		/// </summary>
		private XDocument _jarInformationDoc;

		/// <summary>
		/// The conversion master
		/// </summary>
		private XDocument _conversionMaster;

		/// <summary>
		/// The c sharp file contents
		/// </summary>
		private Dictionary<string, StringBuilder> _cSharpFileContents = new Dictionary<string, StringBuilder>();

		/// <summary>
		/// The ignored methods
		/// </summary>
		private const string IgnoredMethods = ".wait;.equals;.toString;.notify;.notifyAll;.hashCode;.getClass;.main";

		/// <summary>
		/// For each method interface
		/// </summary>
		private const string ForEachMethodInterface = "<for_each_method_interface/>";

		/// <summary>
		/// For each method class
		/// </summary>
		private const string ForEachMethodClass = "<for_each_method_class/>";

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParser"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public DefaultParser(ILogger logger) {
			_logger = logger;
		}

		/// <summary>
		/// Parses the and produce c sharp file.
		/// </summary>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <param name="serviceTemplateFile">The service template file.</param>
		/// <param name="conversionMasterFile">The conversion master file.</param>
		/// <returns></returns>
		public ExecutionResult ParseAndProduceCSharpFile(string jarInformationFile, string serviceTemplateFile = "", string conversionMasterFile = "") {
			var templateFile = string.Empty;
			var retval = ExecutionResult.Empty;

			if (!string.IsNullOrEmpty(jarInformationFile)) {
				if (File.Exists(jarInformationFile)) {
					_jarInformationFile = jarInformationFile;

					// Does the template file exist?
					if (!string.IsNullOrEmpty(serviceTemplateFile) && File.Exists(serviceTemplateFile))
						_serviceTemplateFilePath = serviceTemplateFile;
					else if (!string.IsNullOrEmpty((templateFile = LocateServiceTemplateFile(TemplateFile.ServiceTemplate))))
						_serviceTemplateFilePath = templateFile;

					// Does the conversion master file exist?
					if (!string.IsNullOrEmpty(conversionMasterFile) && File.Exists(conversionMasterFile))
						_conversionMasterFilePath = conversionMasterFile;
					else if (!string.IsNullOrEmpty((templateFile = LocateServiceTemplateFile(TemplateFile.ConversionMaster))))
						_conversionMasterFilePath = templateFile;

					if (!string.IsNullOrEmpty(_serviceTemplateFilePath) && !string.IsNullOrEmpty(_conversionMasterFilePath)) {
						retval = ParseAndProduceCSFileHelper(jarInformationFile);
					} else
						retval.LastExceptionIfAny = new FileNotFoundException("TemplateFile or ConversionMaster files are missing. Unable to continue.");
				} else
					retval.LastExceptionIfAny = new FileNotFoundException("File that contains metadata information on Jar file is missing. Unable to continue.");
			} else
				retval.LastExceptionIfAny = new ArgumentNullException("Unable to process file if arguments are missing.");

			return retval;
		}

		/// <summary>
		/// Parses the and produce cs file helper.
		/// </summary>
		/// <param name="jarInformationFile">The jar information file.</param>
		/// <returns></returns>
		private ExecutionResult ParseAndProduceCSFileHelper(string jarInformationFile) {
			var successCount = 0;
			var retval = ExecutionResult.Empty;
			var steps = new List<Func<XAttribute, string, ExecutionResult>>() { ExtractNamespaces, ExtractJarProxy };

			try {
				using (var sr = new StreamReader(_serviceTemplateFilePath)) {
					_serviceTemplateDoc = XDocument.Load(sr);

					using (var jarReader = new StreamReader(_jarInformationFile))
						_jarInformationDoc = XDocument.Load(jarReader);
					using (var conversionReader = new StreamReader(_conversionMasterFilePath))
						_conversionMaster = XDocument.Load(conversionReader);
				}

				// Let's process every class except Main
				var classes = _jarInformationDoc.Element("simplicity").Elements("class")
							  .Attributes("name").Where(_ => !_.Value.Contains("Main"));

				// We produce one file per jarProxy (Interface & Implementation)
				// [CONSIDER] Refactor this method to run it in parallel 	(which I don't think it's required) [CONSIDER]
				foreach (var c in classes) {
					var key = $"{c.Value}|{Guid.NewGuid().ToString()}";
					_cSharpFileContents.Add(key, new StringBuilder());
					steps.ForEach(_ => {
						if (_(c, key).IsSuccess)
							++successCount;
					});
				}

				retval.Tag = _cSharpFileContents;
				retval.IsSuccess = true; // Let's parse and process the files we can
			} catch (Exception ex)
			{
				retval.IsSuccess = false;
				retval.LastExceptionIfAny = ex;
			}

			return retval;
		}

		/// <summary>
		/// Checks for ignored methods.
		/// </summary>
		/// <param name="methodDef">The method definition.</param>
		/// <returns></returns>
		private bool CheckForIgnoredMethods(string methodDef) {
			var retval = false;
			var ignored = IgnoredMethods.Split(';');

			foreach (var i in ignored) {
				if (methodDef.Contains(i)) {
					retval = true;
					break;
				}
			}

			return retval;
		}

		/// <summary>
		/// Extracts the namespaces.
		/// </summary>
		/// <param name="c">The c.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private ExecutionResult ExtractNamespaces(XAttribute c, string key) {
			var retval = ExecutionResult.Empty;
			var namespaces = new StringBuilder();
			var cdataElement = _serviceTemplateDoc.Element("serviceDefinitionForJar").Element("namespaces").Value.Trim();
			var cdataArray = cdataElement.Split("\r\n".ToCharArray());

			try {
				Array.ForEach(cdataArray, x => namespaces.AppendLine(x));
				retval.IsSuccess = true;
				_cSharpFileContents[key].AppendLine(namespaces.ToString());
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}

		/// <summary>
		/// Extracts the jar proxy.
		/// </summary>
		/// <param name="c">The c.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private ExecutionResult ExtractJarProxy(XAttribute c, string key) {
			Dictionary<string, string> found;
			var retval = ExecutionResult.Empty;
			var proxySection = new StringBuilder();
			var cdataElement = _serviceTemplateDoc.Element("serviceDefinitionForJar").Element("jarProxy").Value.Trim();
			var cdataArray = cdataElement.Split("\r\n".ToCharArray());

			// Let's loop through jarProxy lines section and update accordingly
			try {
				foreach (var line in cdataArray) {
					var markup = ParserMarkup.IsMarkupPresent(line, out found);

					if (markup == Enums.MarkupType.Keyword || markup == Enums.MarkupType.Statement) {
						proxySection.AppendLine(ParseLine(line, markup, c, found));
					} else
						proxySection.AppendLine(line);
				}
				retval.IsSuccess = true;
				_cSharpFileContents[key].Append(proxySection);
			} catch (Exception e) {
				retval.LastExceptionIfAny = e;
			}

			return retval;
		}


		/// <summary>
		/// Parses the line.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="type">The type.</param>
		/// <param name="clazz">The clazz.</param>
		/// <param name="tags">The tags.</param>
		/// <returns></returns>
		private string ParseLine(string line, Enums.MarkupType type, XAttribute clazz, Dictionary<string, string> tags) {
			var retval = new StringBuilder(line);
			var methodStubs = new StringBuilder();
			var classPos = clazz.Value.LastIndexOf('.') + 1;
			var clazzname = clazz.Value.Substring(classPos);
			var origNameSpc = clazz.Value.Substring(0, classPos - 1);
			var namespc = origNameSpc.Replace('.', '_');
			var interfaceName = $"I{clazzname}";
			var methods = clazz.Parent.Elements("Method").Where(__ => !CheckForIgnoredMethods(__.Value)).ToList();

			if (type == Enums.MarkupType.Keyword) {
				foreach (var x in tags) {
					if (x.Key == ParserMarkup.Namespace)
						retval = retval.Replace(x.Key, namespc);

					if (x.Key == ParserMarkup.Interface)
						retval = retval.Replace(x.Key, interfaceName);

					if (x.Key == ParserMarkup.Class)
						retval = retval.Replace(x.Key, clazzname);
				}
			} else if (type == Enums.MarkupType.Statement) {
				var isInterface = line.Contains(ForEachMethodInterface);
				var forEachSection = new StringBuilder(isInterface ? CodeStubStrings.InterfaceMethodDefinition : CodeStubStrings.ClassMethodDefinition);
				methods.ForEach(_ => methodStubs.AppendLine(FormatMethodProtoype(_, clazzname, forEachSection, isInterface, origNameSpc)));
				retval = retval.Replace(line, methodStubs.ToString());
			}

			return retval.ToString();
		}

		/// <summary>
		/// Maps the java arguments to c sharp.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="method">The method.</param>
		/// <param name="clazz">The clazz.</param>
		/// <returns></returns>
		private Dictionary<string, string> MapJavaArgsToCSharp(string args, string returnType, string method, string clazz = "") {
			XAttribute returns = null;
			var webParms = string.Empty;
			var parameters = string.Empty;
			var argSplit = args.Split(',');
			var webParams = new StringBuilder();
			var methodParams = new StringBuilder();
			var retval = new Dictionary<string, string>();
			var startParam = 'a'; // Let's assign letters for each parameter

			if (!string.IsNullOrEmpty(args)) {
				for (var x = 0; x < argSplit.Length; x++) {
					var equivalence = _conversionMaster.Element("mappings").Elements("translation")
									  .Attributes("javaStatement").FirstOrDefault(_ => _.Value == argSplit[x]);

					webParams.Append("{" + string.Format("{0}", (char)startParam) + "}/");
					methodParams.Append($"{equivalence.Parent.Attribute("cSharpStatement")?.Value ?? argSplit[x]} {(char)startParam}, ");
					startParam++;
				}

				webParms = webParams.ToString();
				parameters = methodParams.ToString();
				webParms = webParms.Substring(0, webParms.LastIndexOf('/'));
				parameters = parameters.Substring(0, parameters.LastIndexOf(','));
				returns = _conversionMaster.Element("mappings").Elements("translation")
							  .Attributes("javaStatement").FirstOrDefault(_ => _.Value == returnType);
			} else {
				// Let's check whether method returns object of same class (e.g.: static MyClass GetInstance())
				var replacementCount = method.Replace(clazz, "*").Count(p => p.Equals('*'));

				if (replacementCount == 2) {
					webParams.Append("{a}/");
					returnType = clazz;
				}
			}

			retval.Add("WebParams", webParms);
			retval.Add("MethodParams", parameters);
			retval.Add("Return", returns?.Parent.Attribute("cSharpStatement")?.Value ?? returnType);

			return retval;
		}

		/// <summary>
		/// Formats the method protype.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <param name="clazzName">Name of the clazz.</param>
		/// <param name="codeStub">The code stub.</param>
		/// <param name="isInterface">if set to <c>true</c> [is interface].</param>
		/// <param name="namespc">The namespc.</param>
		/// <returns></returns>
		private string FormatMethodProtoype(XElement method, string clazzName, StringBuilder codeStub, bool isInterface, string namespc) {
			// Drop decorations like throws (if any)
			var paramCollection = new StringBuilder();
			var methodPrototype = method.Value.Substring(0, method.Value.LastIndexOf(')') + 1);
			var methodWithReturn = methodPrototype.Substring(0, methodPrototype.LastIndexOf('('));
			var methodName = methodWithReturn.Substring(methodWithReturn.LastIndexOf('.') + 1);
			var argumentPos = methodPrototype.IndexOf('(');
			var javaArgs = methodPrototype.Substring(argumentPos).Replace("(", string.Empty).Replace(")", string.Empty);

			var returnTypeSplit = method.Value.Substring(0, method.Value.IndexOf(namespc))
								.Replace("final", string.Empty).Replace("native", string.Empty)
								.Trim().Split(' ');

			//TODO: Fix issue when static method returns instance of same class

			var returnType = returnTypeSplit?.Length > 0 ? returnTypeSplit[returnTypeSplit.Length - 1] : string.Empty;

			var args = MapJavaArgsToCSharp(javaArgs, returnType, methodPrototype, clazzName);

			// Let's extract parameters (if any)
			if (!string.IsNullOrEmpty(args["MethodParams"])) {
				var split = args["MethodParams"].Split(' ');

				for (var i = 0; i < split.Length; i++) {
					if (i % 2 != 0)
						paramCollection.AppendLine(string.Format("\t\t\tparameters.Add({0});", split[i].Replace(",", string.Empty).Trim()));
				}
			}

			// Let's map placeholder to replacement value
			var replacement = new Dictionary<string, string>() {
				{"~ReturnType~", args["Return"] },
				{"~MethodName~", methodName },
				{"~Arguments~", args["MethodParams"] },
				{"~ProcessArguments~", paramCollection.ToString() }
			};


			// We only add specific values when processing interface
			if (isInterface) {
				replacement.Add("~ClassNameInJar~", clazzName);
				replacement.Add("~WebArguments~", args["WebParams"]);
			}

			var retval = new StringBuilder(codeStub.ToString());

			replacement.ToList().ForEach(x => {
				retval = retval.Replace(x.Key, x.Value);
			});

			return retval.ToString();
		}


		/// <summary>
		/// Locates the service template file.
		/// </summary>
		/// <returns></returns>
		private string LocateServiceTemplateFile(TemplateFile fileType) {
			var retval = string.Empty;
			var selectedFile = fileType == TemplateFile.ServiceTemplate ?
										   Strings.DefaultServiceTemplateFileName :
										   Strings.DefaultConversionMasterFileName;

			var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var locations = new List<string>() {string.Concat(currentDir, $@"\{selectedFile}"),
												string.Concat(currentDir,  $@"\XML\{selectedFile}") };

			locations.ForEach(_ => {
				var temp = string.Empty;
				if (string.IsNullOrEmpty(retval)) {
					if (File.Exists(_))
						retval = _;
				}
			});

			return retval;
		}
	}
}