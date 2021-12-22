using Celeste.Mod;

using NLua;

using Snowberry.Editor;
using Snowberry.Editor.Entities;
using Snowberry.Modules;

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

		private readonly ReadOnlyDictionary<string, PluginOption> options;

		public readonly SnowberryModule Module;

        // virtual for missing plugins and lua plugins
		public virtual ReadOnlyDictionary<string, PluginOption> Options => options;

		public PluginInfo(string name, Type t, ConstructorInfo ctor, SnowberryModule module) {
            this.name = name;
            this.ctor = ctor;
            Module = module;

            Dictionary<string, PluginOption> options = new();
            foreach (FieldInfo f in t.GetFields()) {
                if (f.GetCustomAttribute<OptionAttribute>() is OptionAttribute option) {
                    if (option.Name == null || option.Name == string.Empty) {
                        Snowberry.Log(LogLevel.Warn, $"'{f.Name}' ({f.FieldType.Name}) from plugin '{name}' was ignored because it had a null or empty option name!");
                        continue;
                    } else if (!options.ContainsKey(option.Name))
                        options.Add(option.Name, new FieldOption(f, option.Name));
                }
            }
            this.options = new ReadOnlyDictionary<string, PluginOption>(options);
        }

        public virtual T Instantiate<T>() where T : Plugin {
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

		public UnkownPluginInfo(string name) : base(name, typeof(Plugin), null, CelesteEverest.INSTANCE) {}
	}

	internal class LuaPluginInfo : PluginInfo {

        private readonly LuaTable plugin;
        private readonly string name;

        private readonly ReadOnlyDictionary<string, PluginOption> options;

        public readonly ReadOnlyDictionary<string, object> Defaults;

        public LuaPluginInfo(string name, LuaTable plugin) : base(name, typeof(LuaEntity), null, LoennSupport.INSTANCE ?? new LoennSupport()) {
            this.plugin = plugin;
            this.name = name;

            Dictionary<string, PluginOption> options = new();
            Dictionary<string, object> defaults = new();
            // if placements is a table of tables, check all placements, else directly get options
            LuaTable placements = plugin["placements"] as LuaTable;
            if(placements.Keys.OfType<string>().Any(k => k.Equals("data"))) {
                LuaTable data = placements["data"] as LuaTable;
                foreach(var item in data.Keys.OfType<string>())
                    options[item] = new LuaEntityOption(item, data[item].GetType());
            } else if(placements.Keys.Count >= 1 && placements[1] is LuaTable) {
                for(int i = 1; i < placements.Keys.Count + 1; i++) {
					if(placements[i] is not LuaTable ptable)
						continue;
					if(ptable["data"] is LuaTable data)
						foreach(var item in data.Keys.OfType<string>())
							options[item] = new LuaEntityOption(item, data[item].GetType());
				}
				if(placements["default"] is LuaTable defData)
                    foreach(var item in defData.Keys.OfType<string>()) {
						options[item] = new LuaEntityOption(item, defData[item].GetType());
                        defaults[item] = defData[item];
                    }
			}

            // TODO: check for field information that specifies more specific type info
            this.options = new ReadOnlyDictionary<string, PluginOption>(options);
            this.Defaults = new ReadOnlyDictionary<string, object>(defaults);
        }

		public override T Instantiate<T>() {
			if(typeof(T).IsAssignableFrom(typeof(LuaEntity))) {
                return new LuaEntity(name, this, plugin) as T;
			}
            return null;
		}

		public override ReadOnlyDictionary<string, PluginOption> Options => options;
	}

    public interface PluginOption {

        object Get(Plugin from);

        void Set(Plugin on, object value);

        Type Type();

        string Key();
    }

	public class FieldOption : PluginOption {

        private readonly FieldInfo field;
        private readonly string key;

        public FieldOption(FieldInfo field, string key) {
            this.field = field;
            this.key = key;
        }

		public string Key() {
            return key;
		}

		public void Set(Plugin on, object value) {
			field.SetValue(on, value);
		}

		public Type Type() {
            return field.FieldType;
		}

		object PluginOption.Get(Plugin from) {
            return field.GetValue(from);
		}
	}

	public class LuaEntityOption : PluginOption {

        private readonly string key;
        private readonly Type type;

        public LuaEntityOption(string key, Type type) {
            this.key = key;
            this.type = type;
        }

		public object Get(Plugin from) {
			return from is LuaEntity e && e.Values.TryGetValue(key, out var value) ? value : null;
		}

		public string Key() {
			return key;
		}

		public void Set(Plugin on, object value) {
            if(on is LuaEntity e)
                e.Values[key] = value;
		}

		public Type Type() {
			return type;
		}
	}
}