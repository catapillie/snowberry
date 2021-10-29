﻿using Celeste.Mod;
using LevelEditorMod.Editor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LevelEditorMod {
    internal class PluginInfo {
        public static readonly Dictionary<string, PluginInfo> All = new Dictionary<string, PluginInfo>();

        private readonly Type Type;
        private readonly Dictionary<string, FieldInfo> options = new Dictionary<string, FieldInfo>();
        private readonly ConstructorInfo ctor;

        public object this[Entity entity, string option] {
            get {
                if (entity.GetType() == Type && options.TryGetValue(option, out FieldInfo f)) {
                    return ObjectToRaw(f.GetValue(entity));
                }
                return null;
            }
            set {
                if (entity.GetType() == Type && options.TryGetValue(option, out FieldInfo f)) {
                    object val = RawToObject(f.FieldType, value);
                    if (val != null)
                        f.SetValue(entity, val);
                }
            }
        }

        public PluginInfo(string name, Type t, ConstructorInfo ctor) {
            this.ctor = ctor;
            Type = t;
            foreach (FieldInfo f in t.GetFields()) {
                if (f.GetCustomAttribute<OptionAttribute>() is OptionAttribute option) {
                    if (option.Name == null || option.Name == string.Empty) {
                        Module.Log(LogLevel.Warn, $"'{f.Name}' ({f.FieldType.Name}) from entity '{name}' was ignored because it had a null or empty option name!");
                        continue;
                    } else if (!options.ContainsKey(option.Name))
                        options.Add(option.Name, f);
                }
            }
        }

        public Entity Instantiate()
            => (Entity)ctor.Invoke(new object[] { });

        public static void GenerateFromAssembly(Assembly assembly) {
            foreach (Type t in assembly.GetTypesSafe().Where(t => !t.IsAbstract && typeof(Entity).IsAssignableFrom(t))) {
                foreach (PluginAttribute pl in t.GetCustomAttributes<PluginAttribute>(inherit: false)) {
                    if (pl.Name == null || pl.Name == string.Empty) {
                        Module.Log(LogLevel.Warn, $"Found entity plugin with null or empty name! skipping... (Type: {t})");
                        continue;
                    }

                    ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                    if (ctor == null) {
                        Module.Log(LogLevel.Warn, $"'{pl.Name}' does not have a parameterless constructor, skipping...");
                        continue;
                    }

                    All.Add(pl.Name, new PluginInfo(pl.Name, t, ctor));

                    Module.Log(LogLevel.Info, $"Successfully registered '{pl.Name}' entity plugin");
                }
            }
        }

        private static object RawToObject(Type targetType, object raw) {
            if (targetType == typeof(Color)) {
                return Monocle.Calc.HexToColor(raw.ToString());
            }
            if (targetType.IsEnum) {
                try {
                    ObjectToRaw(Enum.Parse(targetType, raw.ToString()));
                    return Enum.Parse(targetType, raw.ToString());
                } catch {
                    return null;
                }
            }
            if (targetType == typeof(char)) {
                return raw.ToString()[0];
            }
            return raw;
        }

        private static object ObjectToRaw(object obj) {
            return obj switch {
                Color color => BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", string.Empty),
                Enum => obj.ToString(),
                char ch => ch.ToString(),
                _ => obj,
            };
        }
    }
}