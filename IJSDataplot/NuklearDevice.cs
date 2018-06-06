using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuklearDotNet;

using FishGfx;
using FishGfx.Graphics;

namespace IJSDataplot {
	class OpenGLDevice : NuklearDeviceTex<Texture>, IFrameBuffered {
		public override Texture CreateTexture(int W, int H, IntPtr Data) {

			return null;
		}

		public void BeginBuffering() {
		}

		public override void SetBuffer(NkVertex[] VertexBuffer, ushort[] IndexBuffer) {
		}

		public override void Render(NkHandle Userdata, Texture Texture, NkRect ClipRect, uint Offset, uint Count) {
		}

		public void EndBuffering() {
		}

		public void RenderFinal() {
		}
	}
}
