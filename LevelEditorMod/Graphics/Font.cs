using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace LevelEditorMod {
    public class Font {
        internal struct Glyph {
            public Rectangle Bounds;
            public Vector2 Offset;

            public Glyph(Rectangle bounds, Vector2 offset) {
                Bounds = bounds;
                Offset = offset;
            }
        }

        private readonly Texture2D texture;
        private readonly Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();
        private readonly int lineHeight;

        public Font(Texture2D texture, List<char> characters, List<Rectangle> bounds, List<Vector2> offsets, int lineHeight) {
            this.texture = texture;
            for (int i = 0; i < characters.Count; i++) {
                glyphs.Add(characters[i], new Glyph(bounds[i], offsets[i]));
            }
            this.lineHeight = lineHeight;
        }

        public void Draw(string str, Vector2 position, Color color, Vector2 scale) {
            float startX = position.X;

            foreach (char c in str) {
                switch (c) {
                    case '\n':
                        position.X = startX;
                        position.Y += lineHeight * scale.Y;
                        break;

                    default:
                        if (glyphs.TryGetValue(c, out Glyph g)) {
                            Monocle.Draw.SpriteBatch.Draw(texture, position, g.Bounds, color, 0f, g.Offset, scale, SpriteEffects.None, 0f);
                            position.X += (g.Bounds.Width + 1) * scale.X;
                        }
                        break;
                }
            }
        }

        public Vector2 Measure(string str) {
            Vector2 size = Vector2.Zero;
            foreach (char c in str) {
                switch (c) {
                    case '\n':
                        size.Y += lineHeight;
                        break;

                    default:
                        if (glyphs.TryGetValue(c, out Glyph g))
                            size.X += g.Bounds.Width + 1;
                        break;
                }
            }
            return size;
        }
    }
}
