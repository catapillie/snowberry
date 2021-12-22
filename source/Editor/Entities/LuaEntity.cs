using System;
using System.Collections.Generic;
using System.Linq;

using Celeste;
using Celeste.Mod;

using Microsoft.Xna.Framework;

using Monocle;

using NLua;
using NLua.Exceptions;

namespace Snowberry.Editor.Entities {

	public class LuaEntity : Entity{

		private LuaTable plugin;

		private int defaultWidth = -1, defaultHeight = -1;
		private int minNodes = 0, maxNodes = 0;
		private Vector2 justify = Vector2.One * 0.5f;

		public Dictionary<string, object> Values = new();

		public LuaEntity(string name, PluginInfo info, LuaTable plugin) {
			Name = name;
			Info = info;
			this.plugin = plugin;

			if(plugin["nodeLimits"] is LuaTable limits) {
				minNodes = (int)Float(limits, 1, 0);
				maxNodes = (int)Float(limits, 2, 0);
			}

			if(plugin["justification"] is LuaTable justification) {
				justify = new Vector2(Float(justification, 1, 0.5f), Float(justification, 2, 0.5f));
			}
		}

		public override void UpdatePostPlacement(Placements.Placement placement) {
			if(placement.Defaults.ContainsKey("width") && (placement.Defaults["width"] is long width)) {
				defaultWidth = (int)width;
			}
			if(placement.Defaults.ContainsKey("height") && placement.Defaults["height"] is long height) {
				defaultHeight = (int)height;
			}
			foreach(var option in Info.Options)
				if(!Values.ContainsKey(option.Key))
					Values[option.Key] = ((LuaPluginInfo)Info).Defaults.TryGetValue(option.Key, out var val) ? val : Default(option.Value.Type());
		}

		public override int MinNodes => minNodes;
		public override int MaxNodes => maxNodes;

		public override int MinWidth => defaultWidth;
		public override int MinHeight => defaultHeight;

		public override void Render() {
			base.Render();
			
			if(CallOrGet<string>("texture") is string texture) {
				GFX.Game[texture].DrawJustified(Center, justify);
			}

			if(CallOrGet<LuaTable>("color") is LuaTable c) { // seems to be the same as fillColor???
				Draw.Rect(Position, Width, Height, Color(c));
			}
			if(CallOrGet<LuaTable>("fillColor") is LuaTable fill) {
				Draw.Rect(Position, Width, Height, Color(fill));
			}
			if(CallOrGet<LuaTable>("borderColor") is LuaTable border) {
				Draw.HollowRect(Position, Width, Height, Color(border));
			}

			foreach(var node in Nodes) {
				if(CallOrGet<string>("nodeTexture") is string nodeTexture) {
					GFX.Game[nodeTexture].DrawCentered(node);
				}
			}
		}

		private static float Float(LuaTable from, int index, float def = 1f) {
			if(from.Keys.Count >= index) { // 1-indexed
				object value = from[index];
				if(value is float f)
					return f;
				else if(value is int i)
					return i;
				else if(value is long l)
					return l;
				else if(value is double d)
					return (float)d;
				else if(value is string s)
					return float.Parse(s);
				else return def;
			} else
				return def;
		}

		private static Color Color(LuaTable from) {
			return new Color(Float(from, 1), Float(from, 2), Float(from, 3), Float(from, 4));
		}

		private T CallOrGet<T>(string name, T orElse = default) where T : class {
			LuaTable entity = WrapEntity();
			if(entity == null)
				return orElse;
			if(plugin[name] is T s) {
				return s;
			} else if(plugin[name] is LuaFunction f) {
				try {
					return (f.Call(EmptyTable(), entity).FirstOrDefault() as T) ?? orElse;
				} catch {
					return orElse;
				}
			}
			return orElse;
		}

		private static LuaTable EmptyTable() {
			try {
				return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
			} catch(LuaScriptException) { // that can stack overflow. somehow???????
				return null;
			}
		}

		private static LuaTable WrapTable(IDictionary<string, object> dict) {
			var table = EmptyTable();
			if(table != null)
				foreach(var pair in dict)
					table[pair.Key] = pair.Value;
			return table;
		}

		private LuaTable WrapEntity() {
			LuaTable table = WrapTable(Values);

			if(table != null) {
				table["name"] = Name;
				table["width"] = Width;
				table["height"] = Height;
			}

			return table;
		}

		private object Default(Type t) {
			if(t == typeof(string))
				return "";
			else if(t.IsEnum)
				return t.GetEnumValues().GetValue(0);
			else if(t == typeof(int))
				return 0;
			else if(t == typeof(short))
				return (short)0;
			else if(t == typeof(byte))
				return (byte)0;
			else if(t == typeof(long))
				return (long)0;
			else if(t == typeof(float))
				return (float)0;
			else if(t == typeof(double))
				return (double)0;
			else if(t == typeof(bool))
				return (bool)false;
			else
				return null;
		}
	}
}