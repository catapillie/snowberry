using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Snowberry.Editor.Entities {
    [Plugin("dreamBlock")]
    public class Plugin_DreamBlock : Entity {
        [Option("fastMoving")] public bool Fast = false;
        [Option("oneUse")] public bool OneUse = false;
        [Option("below")] public bool Below = false;

        public override int MinWidth => 8;
        public override int MinHeight => 8;
        public override int MaxNodes => 1;

        public MTexture[] ParticleTextures = new MTexture[4] {
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        };

        public override void Render() {
            base.Render();

            Draw.Rect(Position, Width, Height, Color.Black * 0.25f);

            if (Editor.FancyRender) {
                int numParticles = (int)(Width / 8f * (Height / 8f) * 0.7f);
                for (int i = 0; i < numParticles; i++) {
                    var pos = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                    var layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                    var timeOffset = Calc.Random.NextFloat();
                    Color colour = layer switch {
                        0 => Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310")),
                        1 => Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C")),
                        2 => Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64")),
                        _ => Color.LightGray * (0.5f + layer / 2f * 0.5f)
                    };

                    pos += Editor.Instance.Camera.Position * (0.3f + 0.25f * layer);
                    pos = PutInside(pos);
                    MTexture mTexture;
                    int animTimer = 0; // todo: animated entities?
                    switch (layer) {
                        case 0: {
                            int num2 = (int)((timeOffset * 4f + animTimer) % 4f);
                            mTexture = ParticleTextures[3 - num2];
                            break;
                        }
                        case 1: {
                            int num = (int)((timeOffset * 2f + animTimer) % 2f);
                            mTexture = ParticleTextures[1 + num];
                            break;
                        }
                        default:
                            mTexture = ParticleTextures[2];
                            break;
                    }

                    if (pos.X >= X + 2f && pos.Y >= Y + 2f && pos.X < X + Width - 2f && pos.Y < Y + Height - 2f) {
                        mTexture.DrawCentered(pos, colour);
                    }
                }

                WobbleLine(Position, new Vector2(X + Width, Y), 0f);
                WobbleLine(new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height), 0.7f);
                WobbleLine(new Vector2(X + Width, Y + Height), new Vector2(X, Y + Height), 1.5f);
                WobbleLine(new Vector2(X, Y + Height), Position, 2.5f);
                Draw.Rect(Position, 2f, 2f, Color.White);
                Draw.Rect(new Vector2(X + Width - 2f, Y), 2f, 2f, Color.White);
                Draw.Rect(new Vector2(X, Y + Height - 2f), 2f, 2f, Color.White);
                Draw.Rect(new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, Color.White);
            } else
                Draw.HollowRect(Position, Width, Height, Color.White);
        }

        private Vector2 PutInside(Vector2 pos) {
            if (pos.X > X + Width) {
                pos.X -= (float)Math.Ceiling((pos.X - (X + Width)) / Width) * Width;
            } else if (pos.X < X) {
                pos.X += (float)Math.Ceiling((X - pos.X) / Width) * Width;
            }

            if (pos.Y > Y + Height) {
                pos.Y -= (float)Math.Ceiling((pos.Y - (Y + Height)) / Height) * Height;
            } else if (pos.Y < Y) {
                pos.Y += (float)Math.Ceiling((Y - pos.Y) / Height) * Height;
            }

            return pos;
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset) {
            float num = (to - from).Length();
            Vector2 value = Vector2.Normalize(to - from);
            Vector2 vector = new Vector2(value.Y, 0f - value.X);
            Color color = Color.White;
            Color color2 = Color.Black * 0.25f;

            float scaleFactor = 0f;
            int num2 = 16;
            for (int i = 2; i < num - 2f; i += num2) {
                float num3 = MathHelper.Lerp(LineAmplitude(0.2f + offset, i), LineAmplitude((float)Math.PI + offset, i), 0.5f);
                if ((i + num2) >= num) {
                    num3 = 0f;
                }

                float num4 = Math.Min(num2, num - 2f - i);
                Vector2 vector2 = from + value * i + vector * scaleFactor;
                Vector2 vector3 = from + value * (i + num4) + vector * num3;
                Draw.Line(vector2 - vector, vector3 - vector, color2);
                Draw.Line(vector2 - vector * 2f, vector3 - vector * 2f, color2);
                Draw.Line(vector2, vector3, color);
                scaleFactor = num3;
            }
        }

        private float LineAmplitude(float seed, float index) {
            return (float)(Math.Sin(seed + index / 16f + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        public override void HQRender() {
            base.HQRender();

            if (Nodes.Length != 0)
                DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White, 4, 2);
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
            Placements.Create("Dream Block", "dreamBlock");
        }
    }
}