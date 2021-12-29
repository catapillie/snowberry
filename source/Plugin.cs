using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace Snowberry {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PluginAttribute : Attribute {
        internal readonly string Name;

        public PluginAttribute(string entityName) {
            Name = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OptionAttribute : Attribute {
        internal readonly string Name;

        public OptionAttribute(string optionName) {
            Name = optionName;
        }
    }

    public abstract class Plugin {
        public PluginInfo Info { get; internal set; }
        public string Name { get; internal set; }

        // overriden by generic plugins
        public virtual void Set(string option, object value) {
            if (Info.Options.TryGetValue(option, out FieldInfo f)) {
                object v = value is string str ? RawToObject(f.FieldType, str) : value;
                try {
                    f.SetValue(this, v);
                } catch (ArgumentException e) {
                    Snowberry.Log(LogLevel.Warn, "Tried to set field " + option + " to an invalid value " + v);
                    Snowberry.Log(LogLevel.Warn, e.ToString());
                }
            }
        }

        public virtual object Get(string option) {
            if (Info.Options.TryGetValue(option, out FieldInfo f))
                return ObjectToRaw(f.GetValue(this));
            return null;
        }

        protected static object RawToObject(Type targetType, string raw) {
            if (targetType == typeof(Color)) {
                return Monocle.Calc.HexToColor(raw);
            }

            if (targetType.IsEnum) {
                try {
                    return Enum.Parse(targetType, raw);
                } catch {
                    return null;
                }
            }

            if (targetType == typeof(char)) {
                return raw[0];
            }

            return raw;
        }

        protected static object ObjectToRaw(object obj) {
            return obj switch {
                Color color => BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", string.Empty),
                Enum => obj.ToString(),
                char ch => ch.ToString(),
                _ => obj,
            };
        }
    }
}