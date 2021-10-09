using Celeste.Mod;
using LevelEditorMod.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LevelEditorMod {
    internal static class Plugins {
        internal static readonly Dictionary<string, Func<EntityPlugin>> Entities = new Dictionary<string, Func<EntityPlugin>>();

        internal static void Register(Assembly assembly) {
            foreach (Type t in assembly.GetTypes()) {
                if (t.GetCustomAttribute<EntityPluginAttribute>() is EntityPluginAttribute pl) {
                    ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                    if (ctor == null) {
                        Module.Log(LogLevel.Error, $"'{pl.Name}' does not have a parameterless constructor, skipping...");
                        continue;
                    }

                    Entities.Add(pl.Name, () => (EntityPlugin)ctor.Invoke(new object[] { }));

                    Module.Log(LogLevel.Info, $"Successfully registered '{pl.Name}' entity plugin");
                }
            }
        }
    }
}
