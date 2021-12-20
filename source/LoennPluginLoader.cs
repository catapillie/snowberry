using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste.Mod;

using NLua;

using Snowberry.Editor;

namespace Snowberry {

	/*	A Loenn plugin is a table returned by a lua script.
	 *	These scripts can reference Love2D libraries, Loenn libraries, or their own libraries.
	 *	Some scripts reference no libraries and only return a table.
	 *	Common Love2D and Loenn libraries will have to be provided by us, while mod libraries are packaged as part of the mod.
	 */
	internal class LoennPluginLoader {

		internal static void LoadEntities() {
			Dictionary<string, LuaTable> plugins = new();

			foreach(var asset in Everest.Content.Mods
										.SelectMany(k => k.List) 
										.Where(k => k.Type == typeof(AssetTypeLua))
										.Where(k => k.PathVirtual.StartsWith("Loenn/entities/"))) {
				try {
					LuaTable pluginTable = Everest.LuaLoader.Require(asset.PathVirtual) as LuaTable;
					plugins[(string)pluginTable["name"]] = pluginTable;
					Snowberry.Log(LogLevel.Info, $"Found Loenn plugin for \"{pluginTable["name"]}\"");
				} catch {
					Snowberry.Log(LogLevel.Warn, $"Could not load Loenn plugin at \"{asset.PathVirtual}\"");
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
						//options[item] = new LuaEntityOption(item, data[item].GetType());
						options[item] = data[item];
					}
				}
				Placements.Create("Loenn: " + plugin.Key, plugin.Key, options);
			}
		}
	}
}