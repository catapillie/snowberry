using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LevelEditorMod.Editor.UI {
    public class UISelectionPanel : UIElement {
        public class UIOption : UIElement {
            private readonly UIElement input;

            public UIOption(string name, UIElement input) {
                this.input = input;

                UILabel label;
                Add(label = new UILabel($"{name} : ") {
                    FG = Color.Gray,
                });
                int w = label.Width + 1;

                if (input != null) {
                    Add(input);
                    input.Position = Vector2.UnitX * w;
                }

                Width = w + (input?.Width ?? 0);
                Height = Math.Max(Fonts.Regular.LineHeight, input?.Height ?? 0);
            }
        }

        public class UIEntry : UIElement {
            private readonly UILabel label;

            public UIEntry(EntitySelection selection, int width) {
                Width = width;

                Entity entity = selection.Entity;
                int spacing = Fonts.Regular.LineHeight + 2;

                Add(label = new UILabel(entity.Name) {
                    FG = Color.DarkKhaki,
                    Underline = true
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
                        Add(BoolOption(option.Key, (bool)value, entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.FieldType == typeof(Color)) {
                        Add(ColorOption(option.Key, (Color)value, entity, option.Value, l));
                        l += 90;
                    } else if (option.Value.FieldType == typeof(string)) {
                        Add(StringOption(option.Key, value.ToString(), entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.FieldType == typeof(int)) {
                        Add(LiteralValueOption<int>(option.Key, value.ToString(), entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.FieldType == typeof(float)) {
                        Add(LiteralValueOption<float>(option.Key, value.ToString(), entity, option.Value, l));
                        l += spacing;
                    }
                }

                Height = l + 8;
            }

            private UIOption StringOption(string name, string value, Entity entity, FieldInfo field, int y) {
                var checkbox = new UITextField(Fonts.Regular, 80, value) {
                    OnInputChange = str => field.SetValue(entity, str),
                };
                return new UIOption(name, checkbox) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption LiteralValueOption<T>(string name, string value, Entity entity, FieldInfo field, int y) {
                var checkbox = new UIValueTextField<T>(Fonts.Regular, 80, value) {
                    OnValidInputChange = v => field.SetValue(entity, v),
                };
                return new UIOption(name, checkbox) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption BoolOption(string name, bool value, Entity entity, FieldInfo field, int y) {
                var checkbox = new UICheckBox(-1, value) {
                    OnPress = b => field.SetValue(entity, b),
                };
                return new UIOption(name, checkbox) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption ColorOption(string name, Color value, Entity entity, FieldInfo field, int y) {
                var colorpicker = new UIColorPicker(100, 80, 16, 12, value) {
                    OnColorChange = color => field.SetValue(entity, color),
                };
                return new UIOption(name, colorpicker) {
                    Position = new Vector2(0, y)
                };
            }
        }

        public UISelectionPanel() {
            GrabsClick = true;
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
