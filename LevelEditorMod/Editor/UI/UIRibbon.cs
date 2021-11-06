using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.UI {
    public class UIRibbon : UIElement {
        private readonly Font font;
        private readonly string text;

        public Color FG = Util.Colors.White;
        public Color BG = Util.Colors.MediumBlue;
        public Color BGAccent = Color.Black * 0.5f;

        private readonly int leftSpace;
        private readonly bool leftEdge, rightEdge;
        public int Accent;

        public UIRibbon(string text, int leftSpace = 8, int rightSpace = 8, bool leftEdge = false, bool rightEdge = true)
            : this(text, Fonts.Regular, leftSpace, rightSpace, leftEdge, rightEdge) { }

        public UIRibbon(string text, Font font, int leftSpace = 8, int rightSpace = 8, bool leftEdge = false, bool rightEdge = true) {
            this.text = text;
            this.font = font;
            this.leftSpace = leftSpace;
            this.leftEdge = leftEdge; this.rightEdge = rightEdge;

            Vector2 size = font.Measure(text);
            Width = (int)size.X + leftSpace + rightSpace + (leftEdge ? 5 : 0) + (rightEdge ? 5 : 0);
            Height = (int)size.Y;
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            float length = Width - (leftEdge ? 5 : 0) - (rightEdge ? 5 : 0);
            float offset = leftEdge ? 5 : 0;
            if (Accent > 0)
                Draw.Rect(position + Vector2.UnitX * length, Accent, Height, BGAccent);
            Draw.Rect(position + Vector2.UnitX * offset, length, Height, BG);
            if (leftEdge)
                Fonts.Regular.Draw("\uE0B2", position, Vector2.One, BG);
            if (rightEdge) {
                if (Accent > 0)
                    Fonts.Regular.Draw("\uE0B0", new Vector2(position.X + length + offset + Accent, position.Y), Vector2.One, BGAccent);
                Fonts.Regular.Draw("\uE0B0", new Vector2(position.X + length + offset, position.Y), Vector2.One, BG);
            }
            font.Draw(text, new Vector2(position.X + offset + leftSpace, position.Y), Vector2.One, FG);
        }
    }
}