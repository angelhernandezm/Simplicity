using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplicity.dotNet.Parser.Logic {
	/// <summary>
	/// 
	/// </summary>
	 class CodeStubStrings {
  		/// <summary>
		/// The interface method definition
		/// </summary>
		public const string InterfaceMethodDefinition =
			@"	  [OperationContract]
	  [WebGet(UriTemplate = ""/~ClassNameInJar~/~MethodName~/~WebArguments~"")]
	  ~ReturnType~ ~MethodName~(~Arguments~);
";

		/// <summary>
		/// The class method definition
		/// </summary>
		public const string ClassMethodDefinition =
			@"	   public ~ReturnType~ ~MethodName~(~Arguments~) {
	       var retval = default(~ReturnType~);

               /////////////// Let's call JNIBridge //////////////

                  var parameters = new List<object>();

~ProcessArguments~

                  var jniBridgeManager =  Simplicity.dotNet.Core.Logic.GlobalContainer.Current.TypeContainer.Resolve<Simplicity.dotNet.Common.Logic.IJniBridgeManager>();
                  var result = jniBridgeManager.RunAsJavaPassThrough(new StackFrame(), this.GetType(), parameters.ToArray());

                  if (result != null && result.IsSuccess)
					  retval = (~ReturnType~) result.Tag;

               ///////////////////////////////////////////////////

               return retval;
           }
"; 
	}
}
