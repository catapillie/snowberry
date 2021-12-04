using Monocle;
using Microsoft.Xna.Framework;
using Celeste;
using System;

namespace Snowberry.Editor.UI.Menus {
    public class UIConfirmMessage : UIElement {
        private UIElement display;
        private readonly UIElement buttons;

        private Action confirm;

        private float lerp;
        public bool Shown { get; private set; }

        public UIConfirmMessage() {
            UIButton yes = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_YES"), Fonts.Regular, 4, 6) {
                FG = Util.Colors.White,
                BG = Util.Colors.Blue,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Blue,
                HoveredBG = Util.Colors.DarkBlue,
                OnPress = () => confirm?.Invoke(),
            };
            UIButton no = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_NO"), Fonts.Regular, 4, 6) {
                FG = Util.Colors.White,
                BG = Util.Colors.Red,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Red,
                HoveredBG = Util.Colors.DarkRed,
                Position = new Vector2(yes.Position.X + yes.Width + 4, yes.Position.Y),
                OnPress = Hide,
            };

            Add(buttons = Regroup(yes, no));
            buttons.Visible = false;
        }

        protected override void Initialize() {
            base.Initialize();
            buttons.Position.X = (Width - buttons.Width) / 2;
        }

        public void Show(UIElement display, Action onConfirm = null) {
            if (this.display != null)
                Remove(this.display);

            confirm = onConfirm;
            Add(this.display = display);
            display.Position.X = (Width - display.Width) / 2;

            if (!Shown) {
                lerp = 0f;
                Shown = true;
            }
        }

        public void Hide() {
            if (Shown) {
                lerp = 1f;
                Shown = false;
            }
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            lerp = Calc.Approach(lerp, Shown.Bit(), Engine.DeltaTime * 2f);
            float ease = (Shown ? Ease.ExpoOut : Ease.ExpoIn)(lerp);

            int h = 0;
            buttons.Visible = lerp > 0;
            if (display != null) {
                display.Visible = buttons.Visible;
                h = display.Height;
                display.Position.Y = (int)(((Height - display.Height) / 2 + h * 2) * ease - h * 2);
            }

            buttons.Position.Y = (int)(Height + buttons.Height - ((Height - h - 6) / 2 + buttons.Height) * ease);
        }

        public override void Render(Vector2 position = default) {
            Draw.Rect(position, Width, Height, Color.Black * lerp * 0.75f);
            base.Render(position);
        }
    }
}