using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {

	public class UIScrollPane : UIElement {

		public Color BG = Calc.HexToColor("202929");
        public int BottomPadding = 0, TopPadding = 0;
        public bool ShowScrollBar = true;

		public UIScrollPane() {
			BG.A = 185;
            Background = BG;
			GrabsScroll = true;
            GrabsClick = true;
		}

        public override void Render(Vector2 position = default) {
            Draw.SpriteBatch.End();

            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = rect;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            base.Render(position);

            // this is extremely stupid
            // todo: make this not extremely stupid
            if(ShowScrollBar) {
                UIElement low = null, high = null;
                foreach(var item in children) {
                    if(low == null || item.Position.Y > low.Position.Y) low = item;
                    if(high == null || item.Position.Y < high.Position.Y) high = item;
                }
                if(high != null && low != null) {
                    var scrollPoints = new Vector2(high.Position.Y + TopPadding + 13, low.Position.Y + low.Height + 13 + BottomPadding);
                    var scrollSize = Math.Abs(scrollPoints.X - scrollPoints.Y);
                    var offset = position.Y - scrollPoints.X;
                    Draw.Rect(position + new Vector2(Width - 4, (offset / scrollSize) * (Height + 40)), 2, 40, Color.DarkCyan);
                }
            }
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }

        public override void Update(Vector2 position = default) {
			base.Update(position);
            if(Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y)) {
                int wheel = (MInput.Mouse.WheelDelta);
                var points = ScrollPoints(13);
                if(wheel > 0 && points.X < 0)
                    children.ForEach(ch => ch.Position += Vector2.UnitY * 13);
                else if(wheel < 0 && points.Y > Height)
                    children.ForEach(ch => ch.Position -= Vector2.UnitY * 13);
            }
            // TODO: hackfix to make the tile brushes show up
            Height = Height == 0 ? Parent?.Height ?? 0 : Height;
        }

        // X,Y = Top, Bottom
        public Vector2 ScrollPoints(int scrollSpeed) {
            UIElement low = null, high = null;
            foreach(var item in children) {
                if(low == null || item.Position.Y > low.Position.Y) low = item;
                if(high == null || item.Position.Y < high.Position.Y) high = item;
            }
            return new Vector2(high != null ? (high.Position.Y + scrollSpeed - TopPadding) : 0, low != null ? (low.Position.Y + low.Height + scrollSpeed + BottomPadding) : 0);
        }
	}
}