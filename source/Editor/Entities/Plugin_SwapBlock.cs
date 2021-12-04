using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {
    [Plugin("swapBlock")]
    public class Plugin_SwapBlock : Entity {
        [Option("theme")] public SwapBlock.Themes Theme = SwapBlock.Themes.Normal;

        private SwapBlock.Themes last = SwapBlock.Themes.Normal;
        private MTexture[,] nineSliceGreen;
        private MTexture[,] nineSliceTarget;
        private MTexture lights;
        private MTexture clipTexture = new MTexture();

        public override int MinWidth => 16;
        public override int MinHeight => 16;
        public override int MinNodes => 1;
        public override int MaxNodes => 1;

        public override void Render() {
            base.Render();

            if (last != Theme || nineSliceGreen == null) {
                last = Theme;
                LoadTextures();
            }

            int num = (int)MathHelper.Min(X, Nodes[0].X);
            int num2 = (int)MathHelper.Min(Y, Nodes[0].Y);
            int num3 = (int)MathHelper.Max(X + Width, Nodes[0].X + Width);
            int num4 = (int)MathHelper.Max(Y + Height, Nodes[0].Y + Height);
            var moveRect = new Rectangle(num, num2, num3 - num, num4 - num2);

            var pathTexture = GFX.Game["objects/swapblock/path" + ((Position.X == Nodes[0].X) ? "V" : "H")];

            if (Theme != SwapBlock.Themes.Moon) {
                for (int i = moveRect.Left; i < moveRect.Right; i += Width) {
                    for (int j = moveRect.Top; j < moveRect.Bottom; j += Height) {
                        pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.Width, moveRect.Right - i), Math.Min(pathTexture.Height, moveRect.Bottom - j), clipTexture);
                        clipTexture.DrawCentered(new Vector2(i + clipTexture.Width / 2, j + clipTexture.Height / 2), Color.White);
                    }
                }
            }

            DrawBlockStyle(new Vector2(moveRect.X, moveRect.Y), moveRect.Width, moveRect.Height, nineSliceTarget, null, Color.White * 0.7f);
            DrawBlockStyle(Nodes[0], Width, Height, nineSliceGreen, lights, Color.White * 0.25f);
            DrawBlockStyle(Position, Width, Height, nineSliceGreen, lights, Color.White);
        }

        private void LoadTextures() {
            MTexture mTexture;
            MTexture mTexture3;

            if (Theme == SwapBlock.Themes.Moon) {
                mTexture = GFX.Game["objects/swapblock/moon/block"];
                mTexture3 = GFX.Game["objects/swapblock/moon/target"];
                lights = GFX.Game["objects/swapblock/moon/midBlock00"];
            } else {
                mTexture = GFX.Game["objects/swapblock/block"];
                mTexture3 = GFX.Game["objects/swapblock/target"];
                lights = GFX.Game["objects/swapblock/midBlock00"];
            }

            nineSliceGreen = new MTexture[3, 3];
            nineSliceTarget = new MTexture[3, 3];
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    nineSliceGreen[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceTarget[i, j] = mTexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
        }

        private void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, MTexture middle, Color color) {
            int num = (int)(width / 8f);
            int num2 = (int)(height / 8f);
            ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
            ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
            ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
            ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
            for (int i = 1; i < num - 1; i++) {
                ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
            }

            for (int j = 1; j < num2 - 1; j++) {
                ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
            }

            for (int k = 1; k < num - 1; k++) {
                for (int l = 1; l < num2 - 1; l++) {
                    ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }

            if (middle != null) {
                middle.DrawCentered(pos + new Vector2(width / 2f, height / 2f), color);
            }
        }

        protected override Rectangle[] Select() {
            if (Nodes.Length != 0) {
                Vector2 node = Nodes[0];
                return new Rectangle[] {
                    Bounds, new Rectangle((int)node.X, (int)node.Y, Width, Height)
                };
            } else {
                return new Rectangle[] { Bounds };
            }
        }

        public static void AddPlacements() {
            Placements.Create("Swap Block (Normal)", "swapBlock", new Dictionary<string, object>() { { "theme", "Normal" } });
            Placements.Create("Swap Block (Moon)", "swapBlock", new Dictionary<string, object>() { { "theme", "Moon" } });
        }
    }
}
