using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {
    [Plugin("cassetteBlock")]
    public class Plugin_CassetteBlock : Entity {
        [Option("index")] public int Index = 0;
        [Option("tempo")] public float Tempo = 1.0f;

        public override int MinWidth => 16;
        public override int MinHeight => 16;

        private static readonly Color[] colors = new Color[4] {
            Calc.HexToColor("49aaf0"),
            Calc.HexToColor("f049be"),
            Calc.HexToColor("fcdc3a"),
            Calc.HexToColor("38e04e"),
        };

        public override void Render() {
            base.Render();

            MTexture block = GFX.Game["objects/cassetteblock/solid"];
            int w = Width / 8;
            int h = Height / 8;
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    int tx = x == 0 ? 0 : (x == w - 1 ? 16 : 8);
                    int ty = y == 0 ? 0 : (y == h - 1 ? 16 : 8);
                    block.GetSubtexture(tx, ty, 8, 8).Draw(Position + new Vector2(x, y) * 8, Vector2.Zero, colors[Index % 4]);
                }
            }
        }

        public static void AddPlacements() {
            Placements.Create("Cassette Block (Blue)", "cassetteBlock", new Dictionary<string, object>() { { "index", 0 } });
            Placements.Create("Cassette Block (Pink)", "cassetteBlock", new Dictionary<string, object>() { { "index", 1 } });
            Placements.Create("Cassette Block (Yellow)", "cassetteBlock", new Dictionary<string, object>() { { "index", 2 } });
            Placements.Create("Cassette Block (Green)", "cassetteBlock", new Dictionary<string, object>() { { "index", 3 } });
        }
    }
}