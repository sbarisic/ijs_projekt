using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using Glfw3;
using OpenGL;
using System.Diagnostics;

namespace IJSDataplot {
	class Program {
		static Glfw.Window Window;

		static Glfw.Monitor GetDesktopResolution(out int Width, out int Height) {
			Glfw.Monitor Monitor = Glfw.GetPrimaryMonitor();

			Glfw.VideoMode VideoMode = Glfw.GetVideoMode(Monitor);
			Width = VideoMode.Width;
			Height = VideoMode.Height;

			return Monitor;
		}

		static void CenterWindow(Glfw.Window Wnd) {
			GetDesktopResolution(out int W, out int H);
			Glfw.GetWindowSize(Wnd, out int WW, out int WH);

			int X = W / 2 - WW / 2;
			int Y = H / 2 - WH / 2;

			Glfw.SetWindowPos(Wnd, X, Y);
		}

		static void Main(string[] args) {
			Glfw.ConfigureNativesDirectory("dlls/native/glfw3_64");
			if (!Glfw.Init())
				throw new Exception("Could not initialize GLFW");

			Glfw.WindowHint(Glfw.Hint.Resizable, true);

			//Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.None);
			Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.OpenGL);
			Glfw.WindowHint(Glfw.Hint.ContextCreationApi, Glfw.ContextApi.Native);
			Glfw.WindowHint(Glfw.Hint.Doublebuffer, true);
			Glfw.WindowHint(Glfw.Hint.ContextVersionMajor, 4);
			Glfw.WindowHint(Glfw.Hint.ContextVersionMinor, 0);

			//Glfw.WindowHint(Glfw.Hint.Samples, 2);
			Glfw.WindowHint(Glfw.Hint.OpenglForwardCompat, true);
			Glfw.WindowHint(Glfw.Hint.OpenglProfile, Glfw.OpenGLProfile.Core);
#if DEBUG
			Glfw.WindowHint(Glfw.Hint.OpenglDebugContext, true);
#endif

			const float WindowSizeScale = 0.9f;

			GetDesktopResolution(out int W, out int H);
			Window = Glfw.CreateWindow((int)(W * WindowSizeScale), (int)(H * WindowSizeScale), nameof(IJSDataplot));
			CenterWindow(Window);

			Gl.Initialize();
			Glfw.MakeContextCurrent(Window);

#if DEBUG
			Gl.DebugMessageCallback((Src, DbgType, ID, Severity, Len, Buffer, UserPtr) => {
				if (Severity == Gl.DebugSeverity.Notification)
					return;

				if ((/*Severity == Gl.DebugSeverity.Medium ||*/ Severity == Gl.DebugSeverity.High) && Debugger.IsAttached)
					Debugger.Break();
			}, IntPtr.Zero);
#endif

			while (!Glfw.WindowShouldClose(Window)) {
				Glfw.PollEvents();
			}

			Environment.Exit(0);
		}
	}
}
