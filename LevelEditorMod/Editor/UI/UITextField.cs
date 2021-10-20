using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UITextField : UIElement {
        private readonly Font font;

        private bool selected;

        private int charIndex, selection;
        private string input;
        private int[] widthAtIndex;

        private float lerp;
        public Color Line = Color.Teal;
        public Color LineSelected = Color.LimeGreen;

        public UITextField(Font font, int width, string input = "") {
            this.font = font;
            UpdateInput(input ?? "null");
            charIndex = selection = this.input.Length;

            Width = Math.Max(1, width);
            Height = font.LineHeight;

            TextInput.OnInput += OnInput;
        }

        private void OnInput(char c) {
            if (Engine.Commands.Open || !selected) 
                return;

            int from = charIndex;
            int to = selection;
            int a, b;
            if (from < to) {
                a = from; b = to;
            } else if (to < from) {
                a = to; b = from;
            } else {
                a = b = from;
            }

            if (c == '\b' && input.Length != 0 && !(a == 0 && b == 0)) {
                int newCharIndex = a == b ? a - 1 : a;
                UpdateInput(input.Substring(0, newCharIndex) + input.Substring(b));
                selection = charIndex = newCharIndex;
            } else if (!char.IsControl(c)) {
                UpdateInput(input.Substring(0, a) + c + input.Substring(b));
                selection = charIndex = a + 1;
            }
        }

        private void UpdateInput(string str) {
            input = str;
            widthAtIndex = new int[input.Length + 1];
            int w = 0;
            for (int i = 0; i < widthAtIndex.Length - 1; i++) {
                widthAtIndex[i] = w;
                w += (int)font.Measure(input[i]).X + 1;
            }
            widthAtIndex[widthAtIndex.Length - 1] = w;
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            int mouseX = (int)EditorInput.Mouse.Screen.X;
            int mouseY = (int)EditorInput.Mouse.Screen.Y;
            bool inside = new Rectangle((int)position.X - 1, (int)position.Y - 1, Width + 2, Height + 2).Contains(mouseX, mouseY);

            if (MInput.Mouse.PressedLeftButton) {
                selected = inside;
                if (inside) {
                    int d = mouseX - (int)position.X + 1;
                    int i;
                    for (i = 0; i < widthAtIndex.Length - 1; i++)
                        if (widthAtIndex[i + 1] >= d)
                            break;
                    charIndex = selection = i;
                }
            }

            if (MInput.Mouse.CheckLeftButton) {
                if (selected) {
                    int d = mouseX - (int)position.X + 1;
                    int i;
                    for (i = 0; i < widthAtIndex.Length - 1; i++)
                        if (widthAtIndex[i + 1] >= d)
                            break;
                    charIndex = i;
                }
            }

            bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;
            bool ctrl = MInput.Keyboard.CurrentState[Keys.LeftControl] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightControl] == KeyState.Down;

            if (selected) {
                if (MInput.Keyboard.Pressed(Keys.Escape)) {
                    selected = false;
                }
                if (MInput.Keyboard.Pressed(Keys.Left)) {
                    if (shift) {
                        charIndex = Calc.Clamp(charIndex - 1, 0, input.Length);
                    } else 
                        charIndex = selection = charIndex == selection ? charIndex - 1 : Math.Min(charIndex, selection);
                }
                if (MInput.Keyboard.Pressed(Keys.Right)) {
                    if (shift) {
                        charIndex = Calc.Clamp(charIndex + 1, 0, input.Length);
                    } else
                        charIndex = selection = charIndex == selection ? charIndex + 1 : Math.Max(charIndex, selection);
                }
            }

            lerp = Calc.Approach(lerp, selected ? 1f : 0f, Engine.DeltaTime * 5f);
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            font.Draw(input, position, Vector2.One, Color.White);

            Draw.Rect(position + Vector2.UnitY * Height, Width, 1, Line);
            if (lerp != 0f) {
                float ease = Ease.ExpoOut(lerp);
                Vector2 p = new Vector2(position.X + (1 - ease) * Width / 2f, position.Y + Height);
                Draw.Rect(p, (Width + 1) * ease, 1, Color.Lerp(Line, LineSelected, lerp));
            }

            if (selected) {
                if (Engine.Scene.TimeActive % 1f < 0.5f) {
                    Draw.Rect(position + Vector2.UnitX * widthAtIndex[charIndex], 1, font.LineHeight, Color.White);
                }
                if (selection != charIndex) {
                    int a = widthAtIndex[charIndex], b = widthAtIndex[selection];
                    if (a < b)
                        Draw.Rect(position + Vector2.UnitX * a, b - a, font.LineHeight, Color.Blue * 0.25f);
                    else
                        Draw.Rect(position + Vector2.UnitX * b, a - b, font.LineHeight, Color.Blue * 0.25f);
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            TextInput.OnInput -= OnInput;
        }
    }
}
