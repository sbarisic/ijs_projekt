using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IJSDataplot {
	public static class Utils {
		public static void Log(string Msg) {
			if (Debugger.IsAttached)
				Debugger.Log(0, "Notify", Msg + "\n");
		}

		public static void Log(string Fmt, params object[] Args) {
			Log(string.Format(Fmt, Args));
		}
	}
}
