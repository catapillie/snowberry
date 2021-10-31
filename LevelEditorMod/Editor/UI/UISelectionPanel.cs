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

            public UIOption(string label, Func<T> get, Action<T> set) {
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
                                set(textField.Value);
                        } else {
                            textField.UpdateInput(get().ToString());
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
                Width = (int)Fonts.Regular.Measure(entity.Name).X + 3 + 16;
                int spacing = Fonts.Regular.LineHeight + 1;

                Add(new UIOption<int>("x", () => entity.X, x => selection.SetPosition(new Vector2(x, entity.Y), -1)) {
                    Position = Vector2.UnitY * spacing,
                });
                Add(new UIOption<int>("y", () => entity.Y, y => selection.SetPosition(new Vector2(entity.X, y), -1)) {
                    Position = Vector2.UnitY * spacing * 2,
                });

                //Add(new UIOption<int>("width", entity.Width) {
                //    Position = Vector2.UnitY * spacing * 3,
                //});
                //Add(new UIOption<int>("height", entity.Height) {
                //    Position = Vector2.UnitY * spacing * 4,
                //});
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);
                Height = Parent.Height - 1;
            }

            public override void Render(Vector2 position = default) {
                base.Render(position);
                Fonts.Regular.Draw(name, position, Vector2.One, Color.White);
                Draw.Rect(position.X + Width - 2, position.Y, 1, Height, Color.DarkGray);
            }
        }

        public Color BG = Calc.HexToColor("09090a");
        public Color Line = Color.DarkGray;

        public void Display(List<EntitySelection> selection) {
            if (selection != null) {
                Clear();
                Vector2 offset = new Vector2(1, 2);
                foreach (EntitySelection s in selection) {
                    UIEntry entry = AddEntry(s, offset);
                    offset.X += entry.Width;
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
            Draw.Rect(position.X + 1, position.Y + 1, Width - 2, 1, Line);

            base.Render(position);

            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }
    }
}
