using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Snowberry {
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
        public readonly int LineHeight;

        public Font(Texture2D texture, List<char> characters, List<Rectangle> bounds, List<Vector2> offsets, int lineHeight) {
            this.texture = texture;
            for (int i = 0; i < characters.Count; i++) {
                glyphs.Add(characters[i], new Glyph(bounds[i], offsets[i]));
            }
            LineHeight = lineHeight;
        }

        public void Draw(string str, Vector2 position, Vector2 scale, Color color)
            => Draw(str, position, scale, Vector2.Zero, color);

        public void Draw(string str, Vector2 position, Vector2 scale, Vector2 justify, Color color) {
            if (justify != Vector2.Zero)
                position -= Measure(str) * scale * justify;

            position = Calc.Round(position);
            float startX = position.X;

            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                switch (c) {
                    case '\n':
                        position.X = startX;
                        position.Y += LineHeight * scale.Y;
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

        public void Draw(string str, Vector2 position, Vector2 scale, Vector2 justify, Color[] colorByChar) {
            if (justify != Vector2.Zero)
              position -= Measure(str) * scale * justify;

            position = Calc.Round(position);
            float startX = position.X;

            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                switch (c) {
                    case '\n':
                        position.X = startX;
                        position.Y += LineHeight * scale.Y;
                        break;

                    default:
                        if (glyphs.TryGetValue(c, out Glyph g)) {
                            Monocle.Draw.SpriteBatch.Draw(texture, position, g.Bounds, colorByChar[Math.Min(i, colorByChar.Length - 1)], 0f, g.Offset, scale, SpriteEffects.None, 0f);
                            position.X += (g.Bounds.Width + 1) * scale.X;
                        }
                        break;
                }
            }
        }

        public void Draw(FormattedText text, Vector2 position, Vector2 scale, params object[] values)
            => Draw(text.Format(out Color[] colors, values), position, scale, Vector2.Zero, colors);

        public void Draw(FormattedText text, Vector2 position, Vector2 scale, Vector2 justify, params object[] values)
            => Draw(text.Format(out Color[] colors, values), position, scale, justify, colors);

        public Vector2 Measure(char c) {
            if (glyphs.TryGetValue(c, out Glyph g))
                return new Vector2(g.Bounds.Width, g.Bounds.Height);
            return Vector2.Zero;
        }

        public Vector2 Measure(string str) {
            Vector2 size = Vector2.Zero;
            int currentWidth = 0;
            foreach (char c in str + '\n') {
                switch (c) {
                    case '\n':
                        if (currentWidth > size.X)
                            size.X = currentWidth;
                        currentWidth = 0;
                        size.Y += LineHeight;
                        break;

                    default:
                        if (glyphs.TryGetValue(c, out Glyph g))
                            currentWidth += g.Bounds.Width + 1;
                        break;
                }
            }
            return size - Vector2.UnitX;
        }
    }
}