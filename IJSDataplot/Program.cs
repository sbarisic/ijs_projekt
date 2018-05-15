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
		static ShaderProgram Shader;

		static bool MoveFd, MoveBk, MoveLt, MoveRt, MoveUp, MoveDn;

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

			RenderAPI.GetDesktopResolution(out int W, out int H);
			RWind = new RenderWindow((int)(W * Scale), (int)(H * Scale), "Vector PFM");

			Shader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/default.frag"));

			Shader.Uniforms.Camera.SetPerspective(RWind.GetWindowSizeVec());
			Shader.Uniforms.Camera.Position = new Vector3(0, 0, 100);
			Shader.Uniforms.Camera.MouseMovement = true;

			RWind.OnMouseMoveDelta += (Wnd, X, Y) => Shader.Uniforms.Camera.Update(new Vector2(-X, -Y));
			RWind.OnKey += OnKey;

			Terrain HeightmapThing = new Terrain();
			HeightmapThing.LoadFromImage(Image.FromFile("dataset/heightmap.png"), 100);


			Stopwatch SWatch = Stopwatch.StartNew();
			float Dt = 0;

			while (!RWind.ShouldClose) {
				Gfx.Clear();

				Update(Dt);

				Shader.Bind();
				//TestMesh.Draw();
				HeightmapThing.Draw();
				Shader.Unbind();

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

			Camera Cam = Shader.Uniforms.Camera;
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
		}
	}
}
