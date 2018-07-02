using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
//using NuklearDotNet;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using FishGfx.System;
using System.Runtime.InteropServices;

namespace IJSDataplot {
	/*class OpenGLDevice : NuklearDeviceTex<Texture> {
		Mesh2D GUIMesh;
		Vector2 WindowSize;

		public OpenGLDevice(Vector2 WindowSize) {
			this.WindowSize = WindowSize;
		}

		public override Texture CreateTexture(int W, int H, IntPtr Data) {
			return Texture.FromPixels(W, H, Data);
		}

		public override void SetBuffer(NkVertex[] VertexBuffer, ushort[] IndexBuffer) {
			if (GUIMesh == null)
				GUIMesh = new Mesh2D(BufferUsage.DynamicDraw);

			GUIMesh.SetVertices(VertexBuffer.Select((V) =>
				new Vertex2(new Vector2(V.Position.X, WindowSize.Y - V.Position.Y) / WindowSize, new Vector2(V.UV.X, V.UV.Y), new FishGfx.Color(V.Color.R, V.Color.G, V.Color.B, V.Color.A))).ToArray());
			GUIMesh.SetElements(IndexBuffer.Select((I) => (uint)I).ToArray());
		}

		public override void Render(NkHandle Userdata, Texture Texture, NkRect ClipRect, uint Offset, uint Count) {
			Gfx.FrontFace(true);
			Gfx.Scissor((int)ClipRect.X, (int)(WindowSize.Y - ClipRect.Y - ClipRect.H), (int)ClipRect.W, (int)ClipRect.H, true);
			Texture.BindTextureUnit();

			GUIMesh.DrawElements((int)Offset * sizeof(int), (int)Count);

			Texture.UnbindTextureUnit();
			Gfx.Scissor(0, 0, 0, 0, false);
			Gfx.FrontFace();
		}
	}*/
}
