using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.ClutterBlock;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("redBlocks")]
    [Plugin("yellowBlocks")]
    [Plugin("greenBlocks")]
    public class Plugin_ClutterBlock : Entity {
        private List<MTexture> blocks;

        public override void Initialize() {
            base.Initialize();

            ClutterBlock.Colors color = Name switch {
                "greenBlocks" => Colors.Green,
                "yellowBlocks" => Colors.Yellow,
                _ => Colors.Red,
            };
            blocks = GFX.Game.GetAtlasSubtextures($"objects/resortclutter/{color}_");
        }

        public override void Render() {
            base.Render();

            List<MTexture> blocks = new List<MTexture>(this.blocks);

            int w = Width / 8;
            int h = Height / 8;
            VirtualMap<bool> drawn = new VirtualMap<bool>(new bool[w, h], emptyValue: true);

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (!drawn[x, y]) {
                        Calc.Shuffle(blocks);

                        foreach (MTexture block in blocks) {
                            int bw = block.Width / 8;
                            int bh = block.Height / 8;

                            if (CheckAndFill(drawn, x, y, x + bw, y + bh)) {
                                block.Draw(Position + new Vector2(x, y) * 8);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool CheckAndFill(VirtualMap<bool> map, int x, int y, int sx, int sy) {
            for (int i = x; i < sx; i++)
                for (int j = y; j < sy; j++)
                    if (map[i, j]) 
                        return false;
            for (int i = x; i < sx; i++)
                for (int j = y; j < sy; j++)
                    map[i, j] = true;
            return true;
        }
    }
}
