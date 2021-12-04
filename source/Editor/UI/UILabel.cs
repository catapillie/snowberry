using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Snowberry.Editor.UI {
    class UILabel : UIElement {

        public Func<string> Value { get; private set; }
        private readonly Font font;
        public Color FG = Calc.HexToColor("f0f0f0");
        public bool Underline = false;

        public UILabel(Func<string> text) : this(Fonts.Regular, (int)Fonts.Regular.Measure(text()).X, text) { }

        public UILabel(string text) : this(Fonts.Regular, (int)Fonts.Regular.Measure(text).X, () => text) { }

        public UILabel(string text, Font font) : this(font, (int)font.Measure(text).X, () => text) { }

        public UILabel(Font font, int width, Func<string> input) {
            this.font = font;
            Width = Math.Max(1, width);
            Height = font.LineHeight;
            Value = input;
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            font.Draw(Value(), position, Vector2.One, FG);
            if (Underline)
                Draw.Rect(position + Vector2.UnitY * Height, Width, 1, FG);
        }
    }
}
