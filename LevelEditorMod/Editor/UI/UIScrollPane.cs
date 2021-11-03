using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {

	class UIScrollPane : UIElement {

		public Color BG = Calc.HexToColor("202929");
        public int BottomPadding = 0;

		public UIScrollPane() {
			BG.A = 127;
			GrabsScroll = true;
		}

        public override void Render(Vector2 position = default) {
            Draw.SpriteBatch.End();

            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = rect;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.Rect(rect, BG);

            base.Render(position);

            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }

        public override void Update(Vector2 position = default) {
			base.Update(position);
            if(Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y)) {
                int wheel = Math.Sign(MInput.Mouse.WheelDelta);
                UIElement low = null, high = null;
				foreach(var item in children) {
                    if(low == null || item.Position.Y > low.Position.Y) low = item;
                    if(high == null || item.Position.Y < high.Position.Y) high = item;
                }
                if(wheel > 0 && high.Position.Y + high.Height + 13 < Position.Y)
                    children.ForEach(ch => ch.Position += Vector2.UnitY * 13);
                else if(wheel < 0 && low.Position.Y + 13 + BottomPadding > Position.Y + Height)
                    children.ForEach(ch => ch.Position -= Vector2.UnitY * 13);
            }
        }
	}
}