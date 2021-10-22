﻿using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UITextField : UIElement {
        private bool selected;
        private int charIndex, selection;

        private string input;
        private int[] widthAtIndex;
        private readonly Font font;

        private float lerp;
        public Color Line = Color.Teal;
        public Color LineSelected = Color.LimeGreen;

        private float timeOffset;

        private static string clipboard;

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

            GetSelection(out int a, out int b);

            if (c == '\b' && input.Length != 0 && !(a == 0 && b == 0)) {
                int nextCharIndex = a == b ? a - 1 : a;
                InsertString(nextCharIndex, b);
                selection = charIndex = nextCharIndex;
                timeOffset = Engine.Scene.TimeActive;
            } else if (!char.IsControl(c)) {
                UpdateInput(input.Substring(0, a) + c + input.Substring(b));
                selection = charIndex = a + 1;
                timeOffset = Engine.Scene.TimeActive;
            }
        }

        private void InsertString(int from, int to, string str = null)
            => UpdateInput(input.Substring(0, from) + str + input.Substring(to));

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

        private void GetSelection(out int a, out int b) {
            if (charIndex < selection) {
                a = charIndex; b = selection;
            } else if (selection < charIndex) {
                a = selection; b = charIndex;
            } else {
                a = b = charIndex;
            }
        }

        private static bool MustSeparate(char at, char previous, bool ignoreWhiteSpace = true) {
            if (ignoreWhiteSpace)
                return !char.IsWhiteSpace(at) && char.GetUnicodeCategory(char.ToLower(at)) != char.GetUnicodeCategory(char.ToLower(previous));
            else
                return char.GetUnicodeCategory(char.ToLower(at)) != char.GetUnicodeCategory(char.ToLower(previous));

        }

        private int MoveIndex(int step, bool stepByWord, bool ignoreWhiteSpace = true) {
            int next = charIndex;

            if (stepByWord) {
                next += step;
                while (next > 0 && next < input.Length && !MustSeparate(input[next], input[next - 1], ignoreWhiteSpace))
                    next += step;
            } else
                next += step;

            return Calc.Clamp(next, 0, input.Length); ;
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            if (MInput.Mouse.CheckLeftButton) {
                int mouseX = (int)EditorInput.Mouse.Screen.X;
                int mouseY = (int)EditorInput.Mouse.Screen.Y;
                bool inside = new Rectangle((int)position.X - 1, (int)position.Y - 1, Width + 2, Height + 2).Contains(mouseX, mouseY);

                bool click = MInput.Mouse.PressedLeftButton;

                if (click)
                    selected = inside;

                if (selected) {
                    int i, d = mouseX - (int)position.X + 1;

                    for (i = 0; i < widthAtIndex.Length - 1; i++)
                        if (widthAtIndex[i + 1] >= d)
                            break;

                    if (i != charIndex) {
                        charIndex = i;
                        if (click)
                            selection = i;
                        timeOffset = Engine.Scene.TimeActive;
                    }
                }
            }

            if (selected) {
                bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;
                bool ctrl = MInput.Keyboard.CurrentState[Keys.LeftControl] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightControl] == KeyState.Down;
                
                if (MInput.Keyboard.Pressed(Keys.Escape)) {
                    selected = false;
                } else {
                    bool moved = false;
                    if (moved |= MInput.Keyboard.Pressed(Keys.Left))
                        charIndex = MoveIndex(-1, ctrl);
                    else if (moved |= MInput.Keyboard.Pressed(Keys.Right))
                        charIndex = MoveIndex(1, ctrl);
                    if (moved) {
                        timeOffset = Engine.Scene.TimeActive;
                        if (!shift)
                            selection = charIndex;
                    }
                }

                if (ctrl) {
                    bool copy = MInput.Keyboard.Pressed(Keys.C), cut = MInput.Keyboard.Pressed(Keys.X);

                    if (MInput.Keyboard.Pressed(Keys.A)) {
                        charIndex = input.Length;
                        selection = 0;
                    }
                    
                    if (selection != charIndex && (copy || cut)) {
                        GetSelection(out int a, out int b);
                        clipboard = input.Substring(a, b - a);
                        if (cut) {
                            InsertString(a, b);
                            selection = charIndex = a;
                        }
                    } else if (MInput.Keyboard.Pressed(Keys.V) && clipboard != null) {
                        GetSelection(out int a, out int b);
                        InsertString(a, b, clipboard);
                        selection = charIndex = a + clipboard.Length;
                        timeOffset = Engine.Scene.TimeActive;
                    }
                }
            }

            lerp = Calc.Approach(lerp, selected ? 1f : 0f, Engine.DeltaTime * 4f);
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
                if ((Engine.Scene.TimeActive - timeOffset) % 1f < 0.5f) {
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