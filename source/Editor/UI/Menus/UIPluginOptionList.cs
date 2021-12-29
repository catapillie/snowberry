using Microsoft.Xna.Framework;
using System;

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
                    UIOption ui;
                    Add(ui = BoolOption(option.Key, (bool)value, Plugin));
                    ui.Position.Y = l;
                    l += spacing;
                } else if (option.Value.FieldType == typeof(Color)) {
                    UIOption ui;
                    Add(ui = ColorOption(option.Key, (Color)value, Plugin));
                    ui.Position.Y = l;
                    l += 91;
                } else if (option.Value.FieldType == typeof(int)) {
                    UIOption ui;
                    Add(ui = LiteralValueOption<int>(option.Key, value.ToString(), Plugin));
                    ui.Position.Y = l;
                    l += spacing;
                } else if (option.Value.FieldType == typeof(float)) {
                    UIOption ui;
                    Add(ui = LiteralValueOption<float>(option.Key, value.ToString(), Plugin));
                    ui.Position.Y = l;
                    l += spacing;
                } else {
                    UIOption ui;
                    Add(ui = StringOption(option.Key, value?.ToString() ?? "", Plugin));
                    ui.Position.Y = l;
                    l += spacing;
                }
                Height = l;
            }
        }

        public static UIOption StringOption(string name, string value, Action<string> onChange) {
            var checkbox = new UITextField(Fonts.Regular, 80, value) {
                OnInputChange = str => onChange?.Invoke(str),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption StringOption(string name, string value, Plugin plugin) {
            var checkbox = new UITextField(Fonts.Regular, 80, value) {
                OnInputChange = str => plugin.Set(name, str),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption LiteralValueOption<T>(string name, string value, Action<T> onChange) {
            var checkbox = new UIValueTextField<T>(Fonts.Regular, 80, value) {
                OnValidInputChange = v => onChange?.Invoke(v),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption LiteralValueOption<T>(string name, string value, Plugin plugin) {
            var checkbox = new UIValueTextField<T>(Fonts.Regular, 80, value) {
                OnValidInputChange = v => plugin.Set(name, v),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption BoolOption(string name, bool value, Action<bool> onChange) {
            var checkbox = new UICheckBox(-1, value) {
                OnPress = b => onChange?.Invoke(b),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption BoolOption(string name, bool value, Plugin plugin) {
            var checkbox = new UICheckBox(-1, value) {
                OnPress = b => plugin.Set(name, b),
            };
            return new UIOption(name, checkbox);
        }

        public static UIOption ColorOption(string name, Color value, Action<Color> onChange) {
            var colorpicker = new UIColorPicker(100, 80, 16, 12, value) {
                OnColorChange = color => onChange?.Invoke(color),
            };
            return new UIOption(name, colorpicker);
        }

        public static UIOption ColorOption(string name, Color value, Plugin plugin) {
            var colorpicker = new UIColorPicker(100, 80, 16, 12, value) {
                OnColorChange = color => plugin.Set(name, color),
            };
            return new UIOption(name, colorpicker);
        }
    }
}