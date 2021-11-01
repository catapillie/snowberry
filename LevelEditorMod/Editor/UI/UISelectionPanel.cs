using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UISelectionPanel : UIElement {
        public class UIOption : UIElement {
            private readonly UIElement input;
            //private readonly Func<object> get;
            //private readonly Action set;

            public UIOption(string name, UIElement input) {
                this.input = input;

                UILabel label;
                Add(label = new UILabel($"{name} : "));
                int w = label.Width + 1;

                if (input != null) {
                    Add(input);
                    input.Position = Vector2.UnitX * w;
                }

                Width = w + (input?.Width ?? 0);
                Height = Math.Max(Fonts.Regular.LineHeight, input?.Height ?? 0);
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);
                if (input != null) {
                    //switch (input) {
                    //    case UIValueTextField<T> textField:
                    //        if (textField.Selected) {
                    //            if (!textField.Error)
                    //                set?.Invoke(textField.Value);
                    //        } else {
                    //            textField.UpdateInput(get?.Invoke().ToString() ?? "null");
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
        }

        public class UIEntry : UIElement {
            private UILabel label;

            public UIEntry(EntitySelection selection, int width) {
                Width = width;

                Entity entity = selection.Entity;
                int spacing = Fonts.Regular.LineHeight + 2;

                Add(label = new UILabel(entity.Name) {
                    FG = Color.DarkKhaki
                });
                label.Position = Vector2.UnitX * (width / 2 - label.Width / 2);

                UIOption x, y, w, h;
                Add(x = new UIOption("x", new UIValueTextField<int>(Fonts.Regular, 35, entity.X.ToString())) {
                    Position = new Vector2(0, spacing),
                });
                Add(y = new UIOption("y", new UIValueTextField<int>(Fonts.Regular, 35, entity.Y.ToString())) {
                    Position = new Vector2(x.Width + 2, spacing),
                });
                Add(w = new UIOption("width", new UIValueTextField<int>(Fonts.Regular, 35, entity.Width.ToString())) {
                    Position = new Vector2(0, spacing * 2),
                });
                Add(h = new UIOption("height", new UIValueTextField<int>(Fonts.Regular, 35, entity.Height.ToString())) {
                    Position = new Vector2(w.Width + 2, spacing * 2),
                });
                y.Position.X = h.Position.X = Math.Max(y.Position.X, h.Position.X);

                int l = 3 * spacing;
                foreach (var option in entity.Plugin.OptionDict) {
                    object value = option.Value.GetValue(entity);
                    if (option.Value.FieldType == typeof(bool)) {
                        Add(new UIOption(option.Key, new UICheckBox(-1, (bool)value)) {
                            Position = new Vector2(0, l),
                        });
                        l += 11;
                    } else if (option.Value.FieldType == typeof(Color)) {
                        Add(new UIOption(option.Key, new UIColorPicker(100, 80, 16, 12, (Color)value)) {
                            Position = new Vector2(0, l + 2),
                        });
                        l += 90;
                    }
                    
                }

                Height = l + 8;
            }

            public override void Render(Vector2 position = default) {
                base.Render(position);
                Draw.Rect(position + label.Position + Vector2.UnitY * label.Height, label.Width, 1, label.FG);
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
            Add(entry = new UIEntry(s, Width) {
                Position = offset
            });
            return entry;
        }

        public override void Render(Vector2 position = default) {
            Draw.SpriteBatch.End();

            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = rect;

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
