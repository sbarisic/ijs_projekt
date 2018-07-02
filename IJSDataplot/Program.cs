using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.IO;
using System.Diagnostics;
//using NuklearDotNet;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using FishGfx.System;

namespace IJSDataplot {
	class Program {
		static RenderWindow RWind;

		static ShaderProgram Shader_DrawRayCast;
		static ShaderProgram Shader_DrawFlat;
		static ShaderProgram Shader_Screen;
		static ShaderProgram Shader_Textured;

		static FishGfx.Color ClearColor;

		static Vector2 RightClickPos;
		static bool MoveFd, MoveBk, MoveLt, MoveRt, MoveUp, MoveDn, LeftMouse, RightMouse;
		static int FunctionMode = 1;

		static Terrain HMap;
		static Texture RayCastingTexture;
		static Texture Background;
		static Texture PinTexture;

		static Vector3 CameraPivot;
		static float DesiredPivotDistance;

		static Mesh3D PinMesh;

		//static OpenGLDevice NuklearDev;

		static void Main(string[] args) {
			/*IBWFile F = IBWLoader.Load("dataset/ibw/Image0018.ibw");

			for (int i = 0; i < F.Depth; i++) {
				Bitmap Bmp = new Bitmap(F.Width, F.Height);

				int D_Dim = i;

				float Min = float.MaxValue;
				float Max = float.MinValue;

				for (int y = 0; y < F.Height; y++) {
					for (int x = 0; x < F.Width; x++) {
						float Flt = (float)F.GetData(x, y, D_Dim) * 1000000000;

						Min = Math.Min(Min, Flt);
						Max = Math.Max(Max, Flt);
					}
				}

				float Offset = -Min;
				float OffsetMax = Max + Offset;
				float ScaleVal = 255.0f / OffsetMax;

				Min = float.MaxValue;
				Max = float.MinValue;

				for (int y = 0; y < F.Height; y++) {
					for (int x = 0; x < F.Width; x++) {
						float Flt = ((float)F.GetData(x, y, D_Dim) * 1000000000 + Offset) * ScaleVal;

						int Clr = (int)Flt;
						Bmp.SetPixel(x, F.Height - y - 1, System.Drawing.Color.FromArgb(255, Clr, Clr, Clr));
					}
				}

				Bmp.Save("test_" + i + ".png");
			}


			File.WriteAllText("text_data.txt", F.NoteData);


			Console.WriteLine("Done!");
			Console.ReadLine();
			return;//*/




			const float Scale = 0.9f;
			ClearColor = new FishGfx.Color(60, 80, 100);

			RenderAPI.GetDesktopResolution(out int W, out int H);
			RWind = new RenderWindow((int)(W * Scale), (int)(H * Scale), "Vector PFM");

			Console.WriteLine("OpenGL {0}", RenderAPI.Version);
			Console.WriteLine("Running on {0}", RenderAPI.Renderer);

			// Load shader programs
			Shader_DrawRayCast = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/defaultRayCast.frag"));
			Shader_DrawRayCast.Uniforms.Camera.SetPerspective(RWind.GetWindowSizeVec());
			Shader_DrawRayCast.Uniforms.Camera.Position = new Vector3(0, 300, 0);
			Shader_DrawRayCast.Uniforms.Camera.PitchClamp = new Vector2(-80, 80);

			Shader_DrawFlat = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/defaultFlatColor.frag"));
			Shader_DrawFlat.Uniforms.Camera = Shader_DrawRayCast.Uniforms.Camera;

			Shader_Screen = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/default.frag"));
			Shader_Screen.Uniforms.Camera.SetOrthogonal(0, 0, 1, 1);

			Shader_Textured = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/default.frag"));
			Shader_Textured.Uniforms.Camera = Shader_DrawRayCast.Uniforms.Camera;

			//NuklearDev = new OpenGLDevice(RWind.GetWindowSizeVec());
			//NuklearAPI.Init(NuklearDev);

			RWind.OnMouseMoveDelta += (Wnd, X, Y) => Shader_DrawRayCast.Uniforms.Camera.Update(new Vector2(-X, -Y));
			RWind.OnKey += OnKey;
			//RWind.OnMouseMove += (Wnd, X, Y) => NuklearDev.OnMouseMove((int)X, (int)Y);

			RWind.OnMouseMoveDelta += (Wnd, X, Y) => {
				if (LeftMouse) {
					const float MoveSpeed = 1.0f;

					if (X != 0 || Y != 0) {
						Camera Cam = Shader_DrawRayCast.Uniforms.Camera;

						if (X != 0)
							Cam.Position -= Cam.WorldRightNormal * MoveSpeed * -X;

						if (Y != 0)
							Cam.Position += Cam.WorldUpNormal * MoveSpeed * -Y;

						RecalcCamera();
					}
				} else if (RightMouse) {
					DesiredPivotDistance += Y;
					RecalcCamera();
				}
			};

			HMap = new Terrain();
			HMap.LoadFromImage(Image.FromFile("dataset/data2/heightmap.png"), 100);
			//HMap.LoadFromImage(Image.FromFile("dataset/height_test.png"), 10);
			//HMap.LoadFromImage(Image.FromFile("dataset/owl.png"), 100);


			DesiredPivotDistance = float.PositiveInfinity;
			CameraPivot = new Vector3(HMap.Width / 2, HMap.GetHeight(HMap.Width / 2, HMap.Height / 2), HMap.Height / 2);
			RecalcCamera();

			PinMesh = new Mesh3D {
				PrimitiveType = PrimitiveType.Triangles
			};
			PinMesh.SetVertices(GfxUtils.LoadObj("data/models/pin/pin.obj"));

			PinTexture = Texture.FromFile("data/models/pin/pin_mat.png");

			Mesh3D Vectors = new Mesh3D {
				PrimitiveType = PrimitiveType.Lines
			};
			{
				//Vertex3[] Verts = new Vertex3[HMap.Width * HMap.Height * 2];
				List<Vertex3> Verts = new List<Vertex3>();

				for (int i = 0; i < HMap.Width * HMap.Height * 2; i += 2) {
					int X = (i / 2) % HMap.Width;
					int Y = (i / 2) / HMap.Width;

					const int Density = 2;
					if (X % Density != 0 || Y % Density != 0)
						continue;

					float Height = HMap.GetHeight(X, Y);

					Verts.Add(new Vertex3(new Vector3(X, Height - 0.5f, Y), FishGfx.Color.Black));
					Verts.Add(new Vertex3(new Vector3(X, Height + 20, Y), FishGfx.Color.White));
				}

				Vectors.SetVertices(Verts.ToArray());
			}

			RWind.GetWindowSize(out int WindowWidth, out int WindowHeight);
			RenderTexture Screen = new RenderTexture(WindowWidth, WindowHeight);
			RayCastingTexture = Screen.CreateNewColorAttachment(1);

			Background = Texture.FromFile("data/background.png");

			Mesh2D ScreenQuad = new Mesh2D();
			ScreenQuad.SetVertices(new Vertex2[] {
				new Vertex2(new Vector2(0, 0), new Vector2(0, 0)),
				new Vertex2(new Vector2(0, 1), new Vector2(0, 1)),
				new Vertex2(new Vector2(1, 1), new Vector2(1, 1)),
				new Vertex2(new Vector2(1, 0), new Vector2(1, 0))
			});
			ScreenQuad.SetElements(new uint[] { 0, 1, 2, 0, 2, 3 }.Reverse().ToArray());

			Stopwatch SWatch = Stopwatch.StartNew();
			float Dt = 0;

			while (!RWind.ShouldClose) {
				Update(Dt);

				// Draw the world onto a render texture including the ray casting buffer
				Screen.Bind(0, 1);
				{
					Shader_DrawRayCast.Bind();
					Gfx.Clear(FishGfx.Color.Transparent);

					/*Gfx.EnableCullFace(false);
					HMap.Draw();
					Gfx.EnableCullFace(true);*/

					// Draw back face
					Gfx.CullFront();
					Texture Orig = HMap.OverlayTexture;
					HMap.OverlayTexture = Background;
					HMap.Draw();

					// Draw front face
					Gfx.CullBack();
					HMap.OverlayTexture = Orig;
					HMap.Draw();

					Shader_DrawRayCast.Unbind();
				}
				Screen.Unbind();

				// Draw other stuff
				Screen.Bind(0);
				{
					Shader_DrawFlat.Bind();
					Vectors.Draw();
					Shader_DrawFlat.Unbind();

					Shader_Textured.Bind();
					Shader_Textured.SetModelMatrix(Matrix4x4.CreateScale(2) * Matrix4x4.CreateTranslation(CameraPivot));
					PinTexture.BindTextureUnit();
					PinMesh.Draw();
					PinTexture.UnbindTextureUnit();
					Shader_Textured.Unbind();
				}
				Screen.Unbind();

				// Draw render texture and GUI to screen
				Shader_Screen.Bind();
				{
					Gfx.Clear(ClearColor);
					Gfx.EnableDepthDest(false);

					if (FunctionMode == 1)
						Screen.Color.BindTextureUnit();
					else if (FunctionMode == 2)
						RayCastingTexture.BindTextureUnit();

					ScreenQuad.Draw();
					//NuklearAPI.Frame(DrawGUI);
					Gfx.EnableDepthDest(true);
				}
				Shader_Screen.Unbind();

				// Swap buffers, do magic
				RWind.SwapBuffers();
				Events.Poll();

				while (SWatch.ElapsedMilliseconds / 1000.0f < 1.0f / 60.0f)
					;
				Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();
			}
		}

		static void Update(float Dt) {
			if (Dt == 0)
				return;

			//NuklearAPI.SetDeltaTime(Dt);
		}

		static void DrawGUI() {
			//NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable | NkPanelFlags.ScrollAutoHide;
			/*NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.Minimizable;

			NuklearAPI.Window("Hello World!", 100, 100, 200, 200, Flags, () => {
				NuklearAPI.LayoutRowDynamic(35);

				for (int i = 0; i < 10; i++) {
					if (NuklearAPI.ButtonLabel("Hello Button!")) {
						Console.WriteLine("Buttone!");
					}
				}
			});*/
		}

		static void RecalcCamera() {
			Camera Cam = Shader_DrawRayCast.Uniforms.Camera;
			Vector3 CamNormal = Vector3.Normalize(Cam.Position - CameraPivot);

			DesiredPivotDistance = DesiredPivotDistance.Clamp(50, 1000);
			Cam.Position = CameraPivot + CamNormal * DesiredPivotDistance;
			Cam.LookAt(CameraPivot);
		}

		static void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			/*if (Key == Key.MouseLeft)
				NuklearDev.OnMouseButton(NuklearEvent.MouseButton.Left, Wnd.MouseX, Wnd.MouseY, Pressed);
			if (Key == Key.MouseRight)
				NuklearDev.OnMouseButton(NuklearEvent.MouseButton.Right, Wnd.MouseX, Wnd.MouseY, Pressed);
			if (Key == Key.MouseMiddle)
				NuklearDev.OnMouseButton(NuklearEvent.MouseButton.Middle, Wnd.MouseX, Wnd.MouseY, Pressed);*/

			if (Key == Key.M && Pressed) {
				Camera Cam = Shader_DrawRayCast.Uniforms.Camera;

				Console.WriteLine("Pos: {0}", Cam.Position);
				Console.WriteLine("Rot: {0}", Cam.Rotation);
			}

			if (Key == Key.W)
				MoveFd = Pressed;

			if (Key == Key.A)
				MoveLt = Pressed;

			if (Key == Key.S)
				MoveBk = Pressed;

			if (Key == Key.D)
				MoveRt = Pressed;

			if (Key == Key.Space)
				MoveUp = Pressed;

			if (Key == Key.C)
				MoveDn = Pressed;

			if (Key == Key.MouseLeft)
				LeftMouse = Pressed;

			// Distance from pivot point
			if (Key == Key.MouseRight) {
				RightMouse = Pressed;

				if (Pressed) {
					RightClickPos = Wnd.MousePos;
				}
			}

			if (Key == Key.F1 && Pressed)
				FunctionMode = 1;

			if (Key == Key.F2 && Pressed)
				FunctionMode = 2;

			if (Key == Key.MouseMiddle && Pressed) {
				Wnd.ReadPixels();
				//FishGfx.Color C = Wnd.GetPixel(Wnd.MouseX, Wnd.MouseY);
				//FishGfx.Color[] Colors = RayCastingTexture.GetPixels();
				FishGfx.Color C = RayCastingTexture.GetPixel(Wnd.MouseX, Wnd.MouseY);
				if (C.A != 0) {
					C.A = 0;
					int Idx = C.ColorInt;

					int X = Idx % HMap.Width;
					int Y = Idx / HMap.Width;

					Console.WriteLine("{0}, {1} - {2}, {3}", X, Y, Idx, C);

					CameraPivot = new Vector3(X, HMap.GetHeight(X, Y) - 0.5f, Y);
					RecalcCamera();
				}
			}
		}
	}
}
