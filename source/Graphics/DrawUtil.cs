using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Snowberry {
    public static class DrawUtil {
        public static Rectangle GetDrawBounds() {
            // TODO: cache this maybe
            RenderTargetBinding[] renderTargets = Draw.SpriteBatch.GraphicsDevice.GetRenderTargets();
            if (renderTargets.Length > 0 && renderTargets[0].RenderTarget is RenderTarget2D renderTarget)
                return renderTarget.Bounds;
            else
                return new Rectangle(0, 0, Engine.Graphics.PreferredBackBufferWidth, Engine.Graphics.PreferredBackBufferHeight);
        }

        public static void WithinScissorRectangle(Rectangle rect, Action action, Matrix? matrix = null, bool nested = true, bool additive = false) {
            if (action != null) {
                Rectangle bounds = GetDrawBounds();
                if (!bounds.Intersects(rect))
                    return;

                if (nested)
                    Draw.SpriteBatch.End();

                Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
                RasterizerState rasterizerState = Engine.Instance.GraphicsDevice.RasterizerState;
                if (!Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable)
                    Engine.Instance.GraphicsDevice.RasterizerState = new RasterizerState() { ScissorTestEnable = true, CullMode = CullMode.None };
                Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = rect.ClampTo(bounds);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, additive ? BlendState.Additive : BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Engine.Instance.GraphicsDevice.RasterizerState, null, matrix ?? Matrix.Identity);
                action();
                Draw.SpriteBatch.End();

                Engine.Instance.GraphicsDevice.RasterizerState = rasterizerState;
                Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;

                if (nested)
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Engine.Instance.GraphicsDevice.RasterizerState);
            }
        }

        public static void DottedLine(Vector2 start, Vector2 end, Color color, float dot = 2f, float space = 2f) {
            float d = Vector2.Distance(start, end);
            Vector2 dir = (end - start).SafeNormalize();
            float step = dot + space;
            for (float x = 0f; x < d; x += step) {
                Vector2 a = start + dir * Math.Min(x, d);
                Vector2 b = start + dir * Math.Min(x + dot, d);
                Draw.Line(a, b, color);
            }
        }
    }
}