using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using FishGfx.System;

namespace IJSDataplot {
	class Program {
		static RenderWindow RWind;
		static ShaderProgram DrawShader;
		static ShaderProgram ScreenShader;
		static FishGfx.Color ClearColor;

		static bool MoveFd, MoveBk, MoveLt, MoveRt, MoveUp, MoveDn, LeftMouse, RightMouse;

		static Terrain HMap;
		static Texture RayCastingTexture;

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

			// Load shader programs
			DrawShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/defaultRayCast.frag"));

			DrawShader.Uniforms.Camera.SetPerspective(RWind.GetWindowSizeVec());
			DrawShader.Uniforms.Camera.Position = new Vector3(0, 0, 100);
			DrawShader.Uniforms.Camera.MouseMovement = true;

			ScreenShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/default.frag"));

			ScreenShader.Uniforms.Camera.SetOrthogonal(0, 0, 1, 1);

			RWind.OnMouseMoveDelta += (Wnd, X, Y) => DrawShader.Uniforms.Camera.Update(new Vector2(-X, -Y));
			RWind.OnKey += OnKey;

			HMap = new Terrain();
			HMap.LoadFromImage(Image.FromFile("dataset/data2/heightmap.png"), 100, true);

			/*HeightmapThing.OverlayTexture = Texture.FromImage(Image.FromFile("dataset/data2/x_amp.png"));
			HeightmapThing.OverlayTexture.SetFilterSmooth();*/


			RWind.GetWindowSize(out int WindowWidth, out int WindowHeight);
			RenderTexture Screen = new RenderTexture(WindowWidth, WindowHeight);
			RayCastingTexture = Screen.CreateNewColorAttachment(1);
			Screen.Framebuffer.DrawBuffers(0, 1);

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

				// Draw the world onto a render texture
				Screen.Bind();
				{
					Gfx.Clear(FishGfx.Color.Transparent);
					DrawShader.Bind();
					HMap.Draw();
					DrawShader.Unbind();
				}
				Screen.Unbind();

				// Draw render texture to screen
				ScreenShader.Bind();
				Gfx.Clear(ClearColor);
				Screen.Color.BindTextureUnit();
				ScreenQuad.Draw();
				ScreenShader.Unbind();

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

			Camera Cam = DrawShader.Uniforms.Camera;
			Cam.MouseMovement = LeftMouse;
			const float MoveSpeed = 100.0f;

			if (MoveFd)
				Cam.Position += Cam.WorldForwardNormal * MoveSpeed * Dt;

			if (MoveBk)
				Cam.Position -= Cam.WorldForwardNormal * MoveSpeed * Dt;

			if (MoveLt)
				Cam.Position -= Cam.WorldRightNormal * MoveSpeed * Dt;

			if (MoveRt)
				Cam.Position += Cam.WorldRightNormal * MoveSpeed * Dt;

			if (MoveUp)
				Cam.Position += Cam.WorldUpNormal * MoveSpeed * Dt;

			if (MoveDn)
				Cam.Position -= Cam.WorldUpNormal * MoveSpeed * Dt;
		}

		static void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
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

			if (Key == Key.MouseRight)
				RightMouse = Pressed;

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
				}
			}
		}
	}
}
