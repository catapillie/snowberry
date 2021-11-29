using Celeste;

using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Stylegrounds {

	[Plugin("parallax")]
	internal class Plugin_Parallax : Styleground {

		[Option("texture")] public string Texture = "";
		[Option("atlas")] public string Atlas = "game";
		[Option("blendmode")] public string BlendMode = "alphablend";
		[Option("fadeIn")] public bool FadeIn = false;

		// image: "bgs/blah" in "game"
		public override string Title() => $"{Dialog.Clean("SNOWBERRY_STYLEGROUNDS_IMAGE")}: \"{Texture}\" {(Atlas != "game" ? $"in {Atlas}" : "")}";

		public override void Render() {
			base.Render();

			Editor editor = Editor.Instance;
			if(editor == null)
				return;
			
			MTexture mtex = (Atlas == "game" && GFX.Game.Has(Texture)) ? GFX.Game[Texture] : (Atlas == "gui" && GFX.Gui.Has(Texture) ? GFX.Gui[Texture] : GFX.Misc[Texture]);
			Vector2 cameraPos = Editor.Instance.Camera.Position.Floor();
			Vector2 pos = (Position - cameraPos * Scroll).Floor();

			if(Color.A <= 1)
				return;

			if(LoopX) {
				while(pos.X < 0f)
					pos.X += mtex.Width;
				while(pos.X > 0f)
					pos.X -= mtex.Width;
			}

			if(LoopY) {
				while(pos.Y < 0f)
					pos.Y += mtex.Height;
				while(pos.Y > 0f)
					pos.Y -= mtex.Height;
			}

			SpriteEffects flip = SpriteEffects.None;
			if(FlipX && FlipY) {
				flip = (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
			} else if(FlipX) {
				flip = SpriteEffects.FlipHorizontally;
			} else if(FlipY) {
				flip = SpriteEffects.FlipVertically;
			}

			if(BlendMode == "additive") {
				Draw.SpriteBatch.End();
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, editor.Camera.Matrix);
			}

			for(float num2 = pos.X; num2 < 320f; num2 += mtex.Width) {
				for(float num3 = pos.Y; num3 < 180f; num3 += mtex.Height) {
					mtex.Draw(new Vector2(num2, num3), Vector2.Zero, Color, 1f, 0f, flip);
					if(!LoopY)
						break;
				}

				if(!LoopX)
					break;
			}
			
			if(BlendMode == "additive") {
				Draw.SpriteBatch.End();
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, editor.Camera.Matrix);
			}
		}
	}
}
