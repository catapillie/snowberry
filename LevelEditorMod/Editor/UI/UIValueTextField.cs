using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UIValueTextField<T> : UITextField where T : struct {
        new public Color Line = Color.Teal;
        new public Color LineSelected = Color.LimeGreen;
        public Color ErrLine = Calc.HexToColor("db2323");
        public Color ErrLineSelected = Calc.HexToColor("ffbb33");

        private bool err;
        private float errLerp;

        public T Value { get; private set; }

        public UIValueTextField(Font font, int width, string input = "")
            : base(font, width, input) { }

        protected override void Initialize() {
            base.Initialize();
            errLerp = err ? 1f : 0f;
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            errLerp = Calc.Approach(errLerp, err ? 1f : 0f, Engine.DeltaTime * 7f);
            base.Line = Color.Lerp(Line, ErrLine, errLerp);
            base.LineSelected = Color.Lerp(LineSelected, ErrLineSelected, errLerp);
        }

        protected override void OnInputUpdate(string input) {
            try {
                Value = (T) Convert.ChangeType(input, typeof(T));
                err = false;
            } catch {
                Value = default;
                err = true;
            }
        }
    }
}
