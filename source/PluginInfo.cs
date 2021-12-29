using Celeste.Mod;
using Snowberry.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Snowberry {
    public class PluginInfo {
        internal static readonly Dictionary<string, PluginInfo> Entities = new();
        internal static readonly Dictionary<string, PluginInfo> Stylegrounds = new();
        internal static readonly Dictionary<string, PluginInfo> OtherPlugins = new();

        private readonly string name;
        private readonly ConstructorInfo ctor;

        public readonly ReadOnlyDictionary<string, FieldInfo> Options;

        public readonly SnowberryModule Module;

        public PluginInfo(string name, Type t, ConstructorInfo ctor, SnowberryModule module) {
            this.name = name;
            this.ctor = ctor;
            Module = module;

            Dictionary<string, FieldInfo> options = new();
            foreach (FieldInfo f in t.GetFields()) {
                if (f.GetCustomAttribute<OptionAttribute>() is OptionAttribute option) {
                    if (option.Name == null || option.Name == string.Empty) {
                        Snowberry.Log(LogLevel.Warn, $"'{f.Name}' ({f.FieldType.Name}) from plugin '{name}' was ignored because it had a null or empty option name!");
                        continue;
                    } else if (!options.ContainsKey(option.Name))
                        options.Add(option.Name, f);
                }
            }

            Options = new ReadOnlyDictionary<string, FieldInfo>(options);
        }

        public T Instantiate<T>() where T : Plugin {
            T plugin = (T)ctor.Invoke(new object[] { });
            plugin.Info = this;
            plugin.Name = name;
            return plugin;
        }

        public static void GenerateFromAssembly(Assembly assembly, SnowberryModule module) {
            foreach (Type t in assembly.GetTypesSafe().Where(t => !t.IsAbstract && typeof(Plugin).IsAssignableFrom(t))) {
                bool isEntity = typeof(Entity).IsAssignableFrom(t);
                bool isStyleground = typeof(Styleground).IsAssignableFrom(t);

                foreach (PluginAttribute pl in t.GetCustomAttributes<PluginAttribute>(inherit: false)) {
                    if (pl.Name == null || pl.Name == string.Empty) {
                        Snowberry.Log(LogLevel.Warn, $"Found plugin with null or empty name! skipping... (Type: {t})");
                        continue;
                    }

                    ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                    if (ctor == null) {
                        Snowberry.Log(LogLevel.Warn, $"'{pl.Name}' does not have a parameterless constructor, skipping...");
                        continue;
                    }


                    PluginInfo info = new PluginInfo(pl.Name, t, ctor, module);

                    if (isEntity)
                        Entities.Add(pl.Name, info);
                    else if (isStyleground)
                        Stylegrounds.Add(pl.Name, info);
                    else
                        OtherPlugins.Add(pl.Name, info);

                    Snowberry.Log(LogLevel.Info, $"Successfully registered '{pl.Name}' plugin");
                }

                if (isEntity) {
                    MethodInfo addPlacements = t.GetMethod("AddPlacements");
                    if (addPlacements != null) {
                        if (addPlacements.GetParameters().Length == 0) {
                            addPlacements.Invoke(null, new object[0]);
                        } else {
                            Snowberry.Log(LogLevel.Warn, $"Found entity plugin with invalid AddPlacements (has parameters)! skipping... (Type: {t})");
                        }
                    } else {
                        Snowberry.Log(LogLevel.Info, $"Found entity plugin without placements. (Type: {t})");
                    }
                }
            }
        }
    }

    internal class UnkownPluginInfo : PluginInfo {
        public UnkownPluginInfo(string name) : base(name, typeof(Plugin), null, CelesteEverest.INSTANCE) { }
    }
}