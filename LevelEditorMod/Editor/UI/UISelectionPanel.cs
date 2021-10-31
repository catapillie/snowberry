using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UISelectionPanel : UIElement {
        public class UIOption<T> : UIElement {
            private readonly string label;
            private readonly UIElement input;
            private readonly Func<T> get;
            private readonly Action<T> set;

            public UIOption(string label, Func<T> get = null, Action<T> set = null) {
                this.label = $"{label} : ";
                int w = (int)Fonts.Regular.Measure(this.label).X;

                this.get = get;
                this.set = set;

                Add(input = new UIValueTextField<T>(Fonts.Regular, 35, get().ToString()) {
                    Position = Vector2.UnitX * w
                });

                Width = w + input.Width;
                Height = Fonts.Regular.LineHeight;
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);
                switch (input) {
                    case UIValueTextField<T> textField:
                        if (textField.Selected) {
                            if (!textField.Error)
                                set?.Invoke(textField.Value);
                        } else {
                            textField.UpdateInput(get?.Invoke().ToString() ?? "null");
                        }
                        break;
                    default:
                        break;
                }
            }

            public override void Render(Vector2 position = default) {
                base.Render(position);
                Fonts.Regular.Draw(label, position, Vector2.One, Color.White);
            }
        }

        public class UIEntry : UIElement {
            private readonly string name;

            public UIEntry(EntitySelection selection) {
                Entity entity = selection.Entity;
                name = entity.Name;
                int spacing = Fonts.Regular.LineHeight + 2;

                UIOption<int> x, y, w, h;
                Add(x = new UIOption<int>("x", () => entity.X, x => selection.SetPosition(new Vector2(x, entity.Y), -1)) {
                    Position = new Vector2(0, spacing),
                });
                Add(y = new UIOption<int>("y", () => entity.Y, y => selection.SetPosition(new Vector2(entity.X, y), -1)) {
                    Position = new Vector2(x.Width + 2, spacing),
                });
                Add(w = new UIOption<int>("width", () => entity.Width, w => selection.SetWidth(w)) {
                    Position = new Vector2(0, spacing * 2),
                });
                Add(h = new UIOption<int>("height", () => entity.Height, h => selection.SetHeight(h)) {
                    Position = new Vector2(w.Width + 2, spacing * 2),
                });
                y.Position.X = h.Position.X = Math.Max(y.Position.X, h.Position.X);

                Height = spacing * 3 + 6;
            }

            public override void Render(Vector2 position = default) {
                base.Render(position);
                Fonts.Regular.Draw(name, position + Vector2.UnitX * Parent.Width / 2, Vector2.One, Vector2.UnitX * 0.5f, Color.DarkKhaki);
            }
        }

        public Color BG = Calc.HexToColor("09090a");

        public void Display(List<EntitySelection> selection) {
            if (selection != null) {
                Clear();
                Vector2 offset = new Vector2(1, 1);
                foreach (EntitySelection s in selection) {
                    UIEntry entry = AddEntry(s, offset);
                    offset.Y += entry.Height;
                }
            }
        }

        private UIEntry AddEntry(EntitySelection s, Vector2 offset) {
            UIEntry entry;
            Add(entry = new UIEntry(s) {
                Position = offset
            });
            return entry;
        }

        public override void Render(Vector2 position = default) {
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = rect;

            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.Rect(rect, BG);

            base.Render(position);

            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }
    }
}
