using Celeste.Mod;
using LevelEditorMod.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LevelEditorMod {
    internal static class Plugins {
        internal static readonly Dictionary<string, Func<Entity>> Entities = new Dictionary<string, Func<Entity>>();

        internal static void Register(Assembly assembly) {
            foreach (Type t in assembly.GetTypes()) {
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

                    Entities.Add(pl.Name, () => (Entity)ctor.Invoke(new object[] { }));

                    Module.Log(LogLevel.Info, $"Successfully registered '{pl.Name}' entity plugin");
                }
            }
        }
    }
}
