using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;

namespace IJSDataplot {
	class Program {
		static void Main(string[] args) {
			CSharpCodeProvider Provider = new CSharpCodeProvider();
			CompilerParameters Params = new CompilerParameters();
			Params.ReferencedAssemblies.Add("IJSDataplot.exe");
			Params.GenerateInMemory = true;
			Params.GenerateExecutable = false;

			string[] CSFiles = Directory.GetFiles("source", "*.cs");
			CompilerResults Res = Provider.CompileAssemblyFromFile(Params, CSFiles);

			if (Res.Errors.HasErrors) {
				foreach (CompilerError E in Res.Errors)
					Console.WriteLine("Error ({0}): {1}", E.ErrorNumber, E.ErrorText);

			} else {
				Assembly A = Res.CompiledAssembly;
				MethodInfo MainMethod;

				foreach (var T in A.ExportedTypes)
					if ((MainMethod = T.GetMethod("Main")) != null)
						MainMethod.Invoke(null, null);
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
