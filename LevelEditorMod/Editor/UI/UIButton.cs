using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UIButton : UIElement {
        private readonly Vector2 offset;
        private readonly string text;
        private readonly Font font;
        private readonly MTexture texture;

        public Color FG = Calc.HexToColor("f0f0f0");
        public Color BG = Calc.HexToColor("1d1d21");
        public Color PressedFG = Calc.HexToColor("4e4ea3");
        public Color PressedBG = Calc.HexToColor("131317");
        public Color HoveredFG = Calc.HexToColor("f0f0f0");
        public Color HoveredBG = Calc.HexToColor("18181c");

        private float lerp;
        private bool pressed, hovering;

        private readonly MTexture
            top, bottom,
            topFill, bottomFill,
            mid;

        public Action OnPress;

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

            offset = new Vector2(spaceX, spaceY);
        }

        public UIButton(MTexture texture, int spaceX = 0, int spaceY = 0, int minWidth = 6, int minHeight = 8, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue) 
            : this() {
            this.texture = texture;

            Width = Calc.Clamp(texture.Width + 6 + spaceX * 2, Math.Max(6, minWidth), maxWidth);
            Height = Calc.Clamp(texture.Height + 3 + spaceY * 2, Math.Max(6, minHeight), maxHeight);

            offset = new Vector2(spaceX, spaceY);
            FG = PressedFG = HoveredFG = Color.White;
        }

        public override void Update(Vector2 position = default) {
            base.Update();

            int mouseX = (int)EditorInput.Mouse.Screen.X;
            int mouseY = (int)EditorInput.Mouse.Screen.Y;
            hovering = new Rectangle((int)position.X + 1, (int)position.Y + 1, Width - 2, Height - 2).Contains(mouseX, mouseY);

            if (MInput.Mouse.PressedLeftButton && hovering)
                pressed = true;
            else if (MInput.Mouse.ReleasedLeftButton) {
                if (hovering && pressed)
                    OnPress?.Invoke();
                pressed = false;
            }

            lerp = Calc.Approach(lerp, pressed ? 1f : 0f, Engine.DeltaTime * 20f);
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            int press = pressed ? 1 : 0;

            Color bg = Color.Lerp(hovering ? HoveredBG : BG, PressedBG, lerp);

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

            Vector2 at = position + new Vector2(3 + offset.X, press + offset.Y);
            Color fg = Color.Lerp(hovering ? HoveredFG : FG, PressedFG, lerp);
            if (text != null && font != null)
                font.Draw(text, at, Vector2.One, fg);
            else if (texture != null)
                texture.Draw(at, Vector2.Zero, fg);
        }
    }
}
