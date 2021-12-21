using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste.Mod;

using NLua;
using NLua.Exceptions;

using Snowberry.Editor;

namespace Snowberry {

	/*	A Loenn plugin is a table returned by a lua script.
	 *	These scripts can reference Love2D libraries, Loenn libraries, or their own libraries.
	 *	Some scripts reference no libraries and only return a table.
	 *	Common Love2D and Loenn libraries will have to be provided by us, while mod libraries are packaged as part of the mod.
	 */
	public class LoennPluginLoader {

		internal static void LoadEntities() {
			Snowberry.Log(LogLevel.Info, $"Trying to load Loenn plugins where possible.");

			Dictionary<string, LuaTable> plugins = new();

			try {
				if(Everest.LuaLoader.Context["snowberrySetupRequire"] is not true) {
					Everest.LuaLoader.Context.DoString("loennLoader = require(\"#Snowberry.LoennPluginLoader\") table.insert(package.searchers, function(name) return loennLoader.EverestRequire(name) end)");//("package.path = \"Loenn/?.lua;LoennHelpers/?.lua;\" .. package.path");
					Everest.LuaLoader.Context["snowberrySetupRequire"] = true;
				}
			} catch (Exception e) {
				Snowberry.Log(LogLevel.Info, e.ToString());
				return;
			}

			foreach(var asset in Everest.Content.Mods
										.SelectMany(k => k.List) 
										.Where(k => k.Type == typeof(AssetTypeLua))
										.Where(k => k.PathVirtual.StartsWith("Loenn/entities/"))) {
				Snowberry.Log(LogLevel.Info, $"Trying to load Loenn plugin at \"{asset.PathVirtual}\"");
				try {
					string text;
					using(var reader = new StreamReader(asset.Stream)) {
						text = reader.ReadToEnd();
					}

					object[] pluginTables = Everest.LuaLoader.Context.DoString(text, asset.PathVirtual);
					foreach(var p in pluginTables) {
						var pluginTable = p as LuaTable;
						plugins[(string)pluginTable["name"]] = pluginTable;
						Snowberry.Log(LogLevel.Info, $"Loaded Loenn plugin for \"{pluginTable["name"]}\"");
					}
				} catch(Exception e) {
					Snowberry.Log(LogLevel.Warn, $"Failed to load Loenn plugin at \"{asset.PathVirtual}\": {e}");
				}
			}

			foreach(var plugin in plugins) {
				LuaPluginInfo info = new LuaPluginInfo(plugin.Key, plugin.Value);
				PluginInfo.Entities[plugin.Key] = info;

				Dictionary<string, object> options = new();
				LuaTable placements = plugin.Value["placements"] as LuaTable;
				if(placements.Keys.OfType<string>().Any(k => k.Equals("data"))) {
					LuaTable data = placements["data"] as LuaTable;
					foreach(var item in data.Keys.OfType<string>()) {
						options[item] = data[item];
					}
				}
				Placements.Create("Loenn: " + plugin.Key, plugin.Key, options);
			}
		}

		private static LuaTable EmptyTable() {
			return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
		}

		private static LuaTable WrapTable(IDictionary<string, object> dict) {
			var table = EmptyTable();
			foreach(var pair in dict)
				table[pair.Key] = pair.Value;
			return table;
		}

		public static object EverestRequire(string name) {
			// name could be "mods", "structs.rectangle", "libraries.jautils", etc
			Snowberry.Log(LogLevel.Info, "Trying to load " + name);

			// TODO: just put our helpers in Loenn
			if(name.StartsWith("LoennHelpers/LoennHelpers/") || name.StartsWith("LoennHelpers/Loenn/") || name.StartsWith("Loenn/LoennHelpers/") || name.StartsWith("Loenn/Loenn/")) {
				return "\n\tAlready a Loenn library reference: " + name;
			}

			try {
				LuaFunction h = Everest.LuaLoader.Context.DoString("return function() return require(\"Loenn/" + name.Replace(".", "/") + "\") end").FirstOrDefault() as LuaFunction;
				if(h.Call().FirstOrDefault() is not null) return h;
			} catch(LuaScriptException e) {
				if(!e.ToString().Contains("not found:")) 
					Snowberry.Log(LogLevel.Warn, $"Failed to load at {name}: {e}");
			}

			try {
				LuaFunction h = Everest.LuaLoader.Context.DoString("return function() return require(\"LoennHelpers/" + name.Replace(".", "/") + "\") end").FirstOrDefault() as LuaFunction;
				if(h.Call().FirstOrDefault() is not null) return h;
			} catch(LuaScriptException e) {
				if(!e.ToString().Contains("not found:"))
					Snowberry.Log(LogLevel.Warn, $"Failed to load at {name}: {e}");
			}

			return "\n\tCould not find Loenn library: " + name;
		}
	}
}