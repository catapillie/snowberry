using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Celeste;
using Celeste.Mod;

using Monocle;

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

		public static Dictionary<string, KeyValuePair<string, string>> LoennText = new();

		internal static void LoadEntities() {
			Snowberry.Log(LogLevel.Info, "Trying to load Loenn plugins where possible.");

			Dictionary<string, LuaTable> plugins = new();
			HashSet<string> triggers = new();

			try {
				if(Everest.LuaLoader.Context["snowberrySetupRequire"] is not true) {
					Everest.LuaLoader.Context.DoString("loennLoader = require(\"#Snowberry.LoennPluginLoader\") table.insert(package.searchers, function(name) return loennLoader.EverestRequire(name) end)");
					Everest.LuaLoader.Context["snowberrySetupRequire"] = true;
				}
			} catch (Exception e) {
				Snowberry.Log(LogLevel.Info, e.ToString());
				return;
			}

			foreach(var asset in Everest.Content.Mods
										.SelectMany(k => k.List) 
										.Where(k => k.Type == typeof(AssetTypeLua))
										.Where(k => k.PathVirtual.StartsWith("Loenn/entities/") || k.PathVirtual.StartsWith("Loenn/triggers/"))) {
				try {
					string text;
					using(var reader = new StreamReader(asset.Stream)) {
						text = reader.ReadToEnd();
					}

					object[] pluginTables = Everest.LuaLoader.Context.DoString(text, asset.PathVirtual);
					foreach(var p in pluginTables) {
						var pluginTable = p as LuaTable;
						string name = (string)pluginTable["name"];
						plugins[name] = pluginTable;
						if(asset.PathVirtual.StartsWith("Loenn/triggers/"))
							triggers.Add(name);
						Snowberry.Log(LogLevel.Info, $"Loaded Loenn plugin for \"{pluginTable["name"]}\"");
					}
				} catch(Exception e) {
					string ex = e.ToString();
					if(ex.Contains("error in error handling")) {
						Snowberry.Log(LogLevel.Error, $"Could not load Loenn plugin at \"{asset.PathVirtual}\" because of internal Lua errors. No more Lua entities will be loaded. Try restarting the game.");
						break;
					} else
						Snowberry.Log(LogLevel.Warn, $"Failed to load Loenn plugin at \"{asset.PathVirtual}\": {ex}");
				}
			}

			if(plugins.Count > 0) {
				// TODO: support loading other language files
				string textFileName = "en_gb";
				foreach(var asset in Everest.Content.Mods
										.SelectMany(k => k.List)
										.Where(k => k.PathVirtual.StartsWith("Loenn/lang/" + textFileName))) {
					string text;
					using(var reader = new StreamReader(asset.Stream)) {
						text = reader.ReadToEnd();
					}

					foreach(var entry in text.Split('\n').Select(k => k.Split('#')[0])) {
						if(!string.IsNullOrWhiteSpace(entry)) {
							var split = entry.Split('=');
							if(split.Length == 2 && !string.IsNullOrWhiteSpace(split[0]) && !string.IsNullOrWhiteSpace(split[1])) {
								LoennText[split[0]] = new KeyValuePair<string, string>(split[1].Trim(), asset.Source.Mod.Name);
							}
						}
					}
				}
				Snowberry.Log(LogLevel.Info, $"Loaded {LoennText.Count} dialog entries from {textFileName} language files for Loenn plugins.");
			}

			foreach(var plugin in plugins) {
				bool isTrigger = triggers.Contains(plugin.Key);
				LuaPluginInfo info = new LuaPluginInfo(plugin.Key, plugin.Value, isTrigger);
				PluginInfo.Entities[plugin.Key] = info;
				
				LuaTable placements = plugin.Value["placements"] as LuaTable;

				if(placements.Keys.OfType<string>().Any(k => k.Equals("data"))) {
					Dictionary<string, object> options = new();
					LuaTable data = placements["data"] as LuaTable;
					foreach(var item in data.Keys.OfType<string>()) {
						options[item] = data[item];
					}
					string placementName = placements["name"] as string ?? "";
					placementName = LoennText.TryGetValue($"{(isTrigger ? "triggers" : "entities")}.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} ({name.Value})" : "Loenn: " + plugin.Key;
					Placements.Create(placementName, plugin.Key, options);
				} else if(placements.Keys.Count >= 1 && placements[1] is LuaTable) {
					for(int i = 1; i < placements.Keys.Count + 1; i++) {
						Dictionary<string, object> options = new();
						if(placements[i] is LuaTable ptable && ptable["data"] is LuaTable data) {
							foreach(var item in data.Keys.OfType<string>()) {
								options[item] = data[item];
							}
							string placementName = ptable["name"] as string ?? "";
							placementName = LoennText.TryGetValue($"entities.{plugin.Key}.placements.name.{placementName}", out var name) ? $"{name.Key} ({name.Value})" : $"Loenn: {plugin.Key} :: {ptable["name"]}";
							Placements.Create(placementName, plugin.Key, options);
						}
					}
				}
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

			// TODO: just put our helpers in Loenn/ ?
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

		public static object LuaGetImage(string textureName, string atlasName) {
			var meta = EmptyTable();

			atlasName ??= "game";

			// Not sure if Loenn uses the same format, but we render these so we can pick whatever format we like
			meta["image"] = textureName;
			meta["atlas"] = atlasName;

			Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;
			MTexture texture = atlas[textureName];
			meta["width"] = meta["realWidth"] = texture.Width;
			meta["height"] = meta["realHeight"] = texture.Height;
			meta["offsetX"] = meta["offsetY"] = 0;

			return meta;
		}
	}
}