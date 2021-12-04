using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {
    [Plugin("switchGate")]
    public class Plugin_SwitchGate : Entity {
        [Option("sprite")] public string Sprite = "block";
        [Option("persistent")] public bool Persistent = false;

        private string last = "block";
        private MTexture[,] nineSlice;
        private MTexture middle;

        public override int MinWidth => 16;
        public override int MinHeight => 16;
        public override int MinNodes => 1;
        public override int MaxNodes => 1;

        public override void Render() {
            base.Render();

			if(last != Sprite || nineSlice == null) {
                last = Sprite;
                LoadTextures();
			}

            DrawBlockStyle(Nodes[0], Width, Height, nineSlice, middle, Color.White * 0.25f);
            DrawBlockStyle(Position, Width, Height, nineSlice, middle, Color.White);
            DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2, Color.White * 0.5f, 8, 4);
        }

		private void LoadTextures() {
            MTexture mTexture;

            mTexture = GFX.Game["objects/switchgate/" + Sprite ?? "block"];
            middle = GFX.Game["objects/switchgate/icon00"];

            nineSlice = new MTexture[3, 3];
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
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
            for(int i = 1; i < num - 1; i++) {
                ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
            }

            for(int j = 1; j < num2 - 1; j++) {
                ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
            }

            for(int k = 1; k < num - 1; k++) {
                for(int l = 1; l < num2 - 1; l++) {
                    ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }

            if(middle != null) {
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
            string[] types = new string[] { "Block", "Mirror", "Stars", "Temple" };
            foreach(var type in types)
                Placements.Create($"Switch Gate ({type})", "switchGate", new Dictionary<string, object>() { { "sprite", type.ToLower() } });
        }
    }
}
