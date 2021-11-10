using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("templeMirror")]
    public class Plugin_TempleMirror : Entity {
        [Option("reflectX")] public float ReflectX = 0.0f;
        [Option("reflectY")] public float ReflextY = 0.0f;

        private static readonly Color fill = new Color(5, 7, 14, 255);

        public override void Render() {
            base.Render();

            MTexture frame = GFX.Game["scenery/templemirror"];

            Draw.Rect(X + 3, Y + 3, Width - 6, Height - 6, fill);

            int w = Width / 8;
            int h = Height / 8;
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    int tx = x == 0 ? 0 : (x == w - 1 ? 16 : 8);
                    int ty = y == 0 ? 0 : (y == h - 1 ? 16 : 8);
                    if (tx != 8 || ty != 8)
                        frame.GetSubtexture(tx, ty, 8, 8).Draw(Position + new Vector2(x * 8, y * 8));
                }
            }
        }

        public static void AddPlacements() {
            Placements.Create("Temple Mirror", "templeMirror");
        }
    }
}
