using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Snowberry.Editor.UI {
    public class UISelectionPanel : UIScrollPane {
        public class UIOption : UIElement {
            public readonly UIElement Input;
            private readonly string tooltip;

            public UIOption(string name, UIElement input, string tooltip = default) {
                Input = input;
                this.tooltip = tooltip;

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

			public override string Tooltip() {
				return tooltip;
			}
		}

        public class UIEntry : UIElement {
            private readonly UILabel label;

            public UIEntry(EntitySelection selection, int width) {
                Width = width;

                Entity entity = selection.Entity;
                int spacing = Fonts.Regular.LineHeight + 2;

                Add(label = new UILabel($"{entity.Name} (#{entity.EntityID})") {
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
                foreach (var option in entity.Info.Options) {
                    object value = option.Value.Get(entity);
                    if (option.Value.Type() == typeof(bool)) {
                        Add(BoolOption(option.Key, (bool)value, entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.Type() == typeof(Color)) {
                        Add(ColorOption(option.Key, (Color)value, entity, option.Value, l));
                        l += 90;
                    } else if (option.Value.Type() == typeof(string)) {
                        Add(StringOption(option.Key, value?.ToString() ?? "", entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.Type() == typeof(int)) {
                        Add(LiteralValueOption<int>(option.Key, value.ToString(), entity, option.Value, l));
                        l += spacing;
                    } else if (option.Value.Type() == typeof(float)) {
                        Add(LiteralValueOption<float>(option.Key, value.ToString(), entity, option.Value, l));
                        l += spacing;
                    }
                }

                Height = l + 8;
            }

            private UIOption StringOption(string name, string value, Entity entity, PluginOption field, int y) {
                var checkbox = new UITextField(Fonts.Regular, 80, value) {
                    OnInputChange = str => field.Set(entity, str),
                };
                return new UIOption(name, checkbox, field.Tooltip()) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption LiteralValueOption<T>(string name, string value, Entity entity, PluginOption field, int y) {
                var checkbox = new UIValueTextField<T>(Fonts.Regular, 80, value) {
                    OnValidInputChange = v => field.Set(entity, v),
                };
                return new UIOption(name, checkbox, field.Tooltip()) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption BoolOption(string name, bool value, Entity entity, PluginOption field, int y) {
                var checkbox = new UICheckBox(-1, value) {
                    OnPress = b => field.Set(entity, b),
                };
                return new UIOption(name, checkbox, field.Tooltip()) {
                    Position = new Vector2(0, y)
                };
            }

            private UIOption ColorOption(string name, Color value, Entity entity, PluginOption field, int y) {
                var colorpicker = new UIColorPicker(100, 80, 16, 12, value) {
                    OnColorChange = color => field.Set(entity, color),
                };
                return new UIOption(name, colorpicker) {
                    Position = new Vector2(0, y)
                };
            }
        }

        public UISelectionPanel() {
            GrabsClick = true;
            TopPadding = 10;
        }

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
    }
}
