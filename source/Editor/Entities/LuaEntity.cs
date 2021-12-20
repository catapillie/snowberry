using System.Collections.Generic;
using System.Linq;

using Celeste;
using Celeste.Mod;

using Microsoft.Xna.Framework;

using Monocle;

using NLua;

namespace Snowberry.Editor.Entities {

	public class LuaEntity : Entity{

		private LuaTable plugin;
		private int defaultWidth = -1, defaultHeight = -1;
		private int minNodes = 0, maxNodes = 0;
		public Dictionary<string, object> Values = new();

		public LuaEntity(string name, PluginInfo info, LuaTable plugin) {
			Name = name;
			Info = info;
			this.plugin = plugin;

			if(plugin["nodeLimits"] is LuaTable limits) {
				minNodes = (int)Float(limits, 1, 0);
				maxNodes = (int)Float(limits, 2, 0);
			}
		}

		public override void UpdatePostPlacement(Placements.Placement placement) {
			if(placement.Defaults.ContainsKey("width") && placement.Defaults["width"] is int width) {
				defaultWidth = width;
			}
			if(placement.Defaults.ContainsKey("height") && placement.Defaults["height"] is int height) {
				defaultHeight = height;
			}
		}

		public override int MinNodes => minNodes;
		public override int MaxNodes => maxNodes;

		public override int MinWidth => defaultWidth;
		public override int MinHeight => defaultHeight;

		public override void Render() {
			base.Render();
			
			if(CallOrGet("texture") is string texture) {
				GFX.Game[texture].DrawCentered(Center);
			}
			
			if(plugin["fillColor"] is LuaTable fill) {
				Draw.Rect(Position, Width, Height, Color(fill));
			}
			if(plugin["borderColor"] is LuaTable border) {
				Draw.HollowRect(Position, Width, Height, Color(border));
			}

			foreach(var node in Nodes) {
				if(CallOrGet("nodeTexture") is string nodeTexture) {
					GFX.Game[nodeTexture].DrawCentered(node);
				}
			}
		}

		private static float Float(LuaTable from, int index, float def = 1f) {
			if(from.Keys.Count >= index) { // 1-indexed
				object value = from[index];
				if(value is float f) {
					return f;
				} else if(value is int i)
					return i;
				else if(value is string s)
					return float.Parse(s);
				else return def;
			} else
				return def;
		}

		private static Color Color(LuaTable from) {
			return new Color(Float(from, 1), Float(from, 2), Float(from, 3), Float(from, 4));
		}

		private string CallOrGet(string name, string orElse = null) {
			LuaTable entity = WrapEntity();
			if(plugin[name] is string s) {
				return s;
			} else if(plugin[name] is LuaFunction f) {
				try {
					return (f.Call(EmptyTable(), entity).FirstOrDefault() as string) ?? orElse;
				} catch {
					return orElse;
				}
			}
			return orElse;
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

		private LuaTable WrapEntity() {
			LuaTable table = WrapTable(Values);

			table["name"] = Name;
			table["width"] = Width;
			table["height"] = Height;

			return table;
		}
	}
}