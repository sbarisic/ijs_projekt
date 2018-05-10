using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace IJSDataplot {
	public static unsafe class Utils {
		public static void Log(string Msg) {
			if (Debugger.IsAttached)
				Debugger.Log(0, "Notify", Msg + "\n");
		}

		public static void Log(string Fmt, params object[] Args) {
			Log(string.Format(Fmt, Args));
		}

		public static void Read(this BinaryReader Reader, byte* Dest, int Len) {
			for (int i = 0; i < Len; i++)
				Dest[i] = Reader.ReadByte();
		}

		public static void Read(this BinaryReader Reader, IntPtr Dest, int Len) {
			Reader.Read((byte*)Dest.ToPointer(), Len);
		}

		public static T ReadStruct<T>(this BinaryReader Reader) where T : struct {
			byte[] Mem = Reader.ReadBytes(Marshal.SizeOf<T>());
			fixed (byte* MemPtr = Mem)
				return Marshal.PtrToStructure<T>((IntPtr)MemPtr);
		}

		public static DateTime MacTimestampToDateTime(this uint UnitTimestamp) {
			return new DateTime(1904, 1, 1, 0, 0, 0).AddSeconds(UnitTimestamp);
		}

		public static DateTime UnixTimestampToDateTime(this uint UnitTimestamp) {
			return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(UnitTimestamp);
		}
	}
}
