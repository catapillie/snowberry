using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace Snowberry.Editor.UI.Menus {
    public class UIPluginOptionList : UIElement {
        public class UIOption : UIElement {
            public readonly UIElement Input;

            public UIOption(string name, UIElement input) {
                Input = input;

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

        public readonly Plugin Plugin;

        public UIPluginOptionList(Plugin plugin) {
            Plugin = plugin;
            Refresh();
        }

        public void Refresh() {
            int l = 0;
            int spacing = 13;
            foreach (var option in Plugin.Info.Options) {
                object value = option.Value.GetValue(Plugin);
                if (option.Value.FieldType == typeof(bool)) {
                    Add(BoolOption(option.Key, (bool)value, Plugin, option.Value, l));
                    l += spacing;
                } else if (option.Value.FieldType == typeof(Color)) {
                    Add(ColorOption(option.Key, (Color)value, Plugin, option.Value, l));
                    l += 90;
                } else if (option.Value.FieldType == typeof(string)) {
                    Add(StringOption(option.Key, value?.ToString() ?? "", Plugin, option.Value, l));
                    l += spacing;
                } else if (option.Value.FieldType == typeof(int)) {
                    Add(LiteralValueOption<int>(option.Key, value.ToString(), Plugin, option.Value, l));
                    l += spacing;
                } else if (option.Value.FieldType == typeof(float)) {
                    Add(LiteralValueOption<float>(option.Key, value.ToString(), Plugin, option.Value, l));
                    l += spacing;
                }
            }
            Height = l;
        }

        private UIOption StringOption(string name, string value, Plugin plugin, FieldInfo field, int y) {
            var checkbox = new UITextField(Fonts.Regular, 80, value) {
                OnInputChange = str => field.SetValue(plugin, str),
            };
            return new UIOption(name, checkbox) {
                Position = new Vector2(0, y)
            };
        }

        private UIOption LiteralValueOption<T>(string name, string value, Plugin plugin, FieldInfo field, int y) {
            var checkbox = new UIValueTextField<T>(Fonts.Regular, 80, value) {
                OnValidInputChange = v => field.SetValue(plugin, v),
            };
            return new UIOption(name, checkbox) {
                Position = new Vector2(0, y)
            };
        }

        private UIOption BoolOption(string name, bool value, Plugin plugin, FieldInfo field, int y) {
            var checkbox = new UICheckBox(-1, value) {
                OnPress = b => field.SetValue(plugin, b),
            };
            return new UIOption(name, checkbox) {
                Position = new Vector2(0, y)
            };
        }

        private UIOption ColorOption(string name, Color value, Plugin plugin, FieldInfo field, int y) {
            var colorpicker = new UIColorPicker(100, 80, 16, 12, value) {
                OnColorChange = color => field.SetValue(plugin, color),
            };
            return new UIOption(name, colorpicker) {
                Position = new Vector2(0, y)
            };
        }
    }
}
