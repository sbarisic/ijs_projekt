using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.IO;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using FishGfx.System;

namespace IJSDataplot {
	class Program {
		static RenderWindow RWind;

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

			ShaderProgram Default = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "data/default3d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "data/default.frag"));

			Vector2 Viewport = RWind.GetWindowSizeVec();
			Default.Uniforms.Viewport = Viewport;
			//Default.Uniforms.Project = Matrix4x4.CreateOrthographicOffCenter(0, Viewport.X, 0, Viewport.Y, -10, 10);
			Default.Uniforms.Project = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 2, Viewport.X / Viewport.Y, 0.1f, 100);
			Default.Uniforms.View = Matrix4x4.CreateTranslation(new Vector3(0, 0, -10));


			Mesh3D TestMesh = new Mesh3D();
			TestMesh.PrimitiveType = PrimitiveType.Triangles;

			TestMesh.SetVertices(Vertex3.FromFloatArray(new float[] { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, -1.0f, -1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f,-1.0f,  1.0f,-1.0f, 1.0f, -1.0f, -1.0f, 1.0f, -1.0f, -1.0f, -1.0f,
				-1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f,-1.0f, 1.0f, -1.0f, -1.0f, 1.0f,
				1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 1.0f, -1.0f, -1.0f, 1.0f,
				1.0f,  1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 1.0f }));


			while (!RWind.ShouldClose) {
				Gfx.Clear();

				Default.Bind();
				TestMesh.Draw();
				Default.Unbind();

				RWind.SwapBuffers();
				Events.Poll();
			}
		}
	}
}
