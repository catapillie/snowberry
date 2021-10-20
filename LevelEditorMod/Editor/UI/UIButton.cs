using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UIButton : UIElement {
        private readonly Vector2 textOffset;
        private readonly string text;
        private readonly Font font;

        public Color FG = Calc.HexToColor("f0f0f0");
        public Color BG = Calc.HexToColor("1d1d21");
        public Color PressedFG = Calc.HexToColor("4e4ea3");
        public Color PressedBG = Calc.HexToColor("131317");

        private float lerp;
        private bool pressed;

        private readonly MTexture
            top, bottom,
            topFill, bottomFill,
            mid;

        private UIButton() {
            MTexture full = GFX.Gui["editor/button"];
            top = full.GetSubtexture(0, 0, 3, 4);
            topFill = full.GetSubtexture(2, 0, 1, 4);
            bottom = full.GetSubtexture(0, 5, 3, 3);
            bottomFill = full.GetSubtexture(2, 5, 1, 4);
            mid = full.GetSubtexture(0, 4, 2, 1);
        }

        public UIButton(int width, int height) 
            : this() {
            Width = Math.Max(6, width);
            Height = Math.Max(8, height);
        }

        public UIButton(string text, Font font, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
            : this() {
            this.text = text;
            this.font = font;

            Vector2 size = font.Measure(text);
            Width = (int)Calc.Clamp(size.X + 6 + spaceX * 2, Math.Max(6, minWidth), maxWidth);
            Height = (int)Calc.Clamp(size.Y + 3 + spaceY * 2, Math.Max(6, minHeight), maxHeight);

            textOffset = new Vector2(spaceX, spaceY);
        }

        public override void Update() {
            base.Update();

            lerp = Calc.Approach(lerp, (pressed = MInput.Mouse.CheckLeftButton) ? 1f : 0f, Engine.DeltaTime * 25f);
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            int press = pressed ? 1 : 0;

            Color bg = Color.Lerp(BG, PressedBG, lerp);

            top.Draw(new Vector2(position.X, position.Y + press), Vector2.Zero, bg);
            topFill.Draw(new Vector2(position.X + 3, position.Y + press), Vector2.Zero, bg, new Vector2(Width - 6, 1));
            top.Draw(new Vector2(position.X + Width, position.Y + press), Vector2.Zero, bg, new Vector2(-1, 1));

            mid.Draw(new Vector2(position.X, position.Y + Height - 4), Vector2.Zero, bg);
            mid.Draw(new Vector2(position.X + Width, position.Y + Height - 4), Vector2.Zero, bg, new Vector2(-1, 1));
            Draw.Rect(new Vector2(position.X, position.Y + 4 + press), Width, Height - 8, Color.Black);
            Draw.Rect(new Vector2(position.X + 1, position.Y + 4 + press), Width - 2, Height - 8, bg);

            bottom.Draw(new Vector2(position.X, position.Y + Height - 3), Vector2.Zero, bg);
            bottomFill.Draw(new Vector2(position.X + 3, position.Y + Height - 3), Vector2.Zero, bg, new Vector2(Width - 6, 1));
            bottom.Draw(new Vector2(position.X + Width, position.Y + Height - 3), Vector2.Zero, bg, new Vector2(-1, 1));

            Draw.Rect(new Vector2(position.X + 2, position.Y + Height - 4 + press), Width - 4, 1, bg);

            if (text != null && font != null)
                font.Draw(text, position + new Vector2(3 + textOffset.X, press + textOffset.Y), Vector2.One, Color.Lerp(FG, PressedFG, lerp));
        }
    }
}
