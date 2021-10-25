using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.ZipMover;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("zipMover")]
    public class Plugin_ZipMover : Entity {
        private static readonly Color ropeColor = Calc.HexToColor("663931");

        [Option("theme")] public Themes Theme = Themes.Normal;

        public override void Render() {
            base.Render();

            MTexture block, light, cog;
            bool outline;
            switch (Theme) {
                case Themes.Moon:
                    block = GFX.Game["objects/zipmover/moon/block"];
                    light = GFX.Game["objects/zipmover/moon/light01"];
                    cog = GFX.Game["objects/zipmover/moon/cog"];
                    outline = false;
                    break;

                default:
                case Themes.Normal:
                    block = GFX.Game["objects/zipmover/block"];
                    light = GFX.Game["objects/zipmover/light01"];
                    cog = GFX.Game["objects/zipmover/cog"];
                    outline = true;
                    break;
            }

            Vector2 start = Center;
            Vector2 end = Nodes[0] + new Vector2(Width, Height) / 2f;

            if (start != end) {
                Vector2 perp = (end - start).SafeNormalize().Perpendicular();
                Vector2 a = perp * 3f;
                Vector2 b = -perp * 4f;
                Draw.Line(start + a, end + a, ropeColor);
                Draw.Line(start + b, end + b, ropeColor);
            }

            cog.DrawCentered(end);

            if (outline)
                Draw.Rect(X - 1, Y - 1, Width + 2, Height + 2, Color.Black);
            else
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Black);

            int w = Width / 8;
            int h = Height / 8;
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    int tx = x == 0 ? 0 : (x == w - 1 ? 16 : 8);
                    int ty = y == 0 ? 0 : (y == h - 1 ? 16 : 8);
                    if (tx != 8 || ty != 8)
                        block.GetSubtexture(tx, ty, 8, 8).Draw(Position + new Vector2(x * 8, y * 8));
                }
            }

            light.DrawJustified(Position + Vector2.UnitX * Width / 2f, new Vector2(0.5f, 0.0f));
        }
    }
}
