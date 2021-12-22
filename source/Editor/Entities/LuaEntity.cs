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

			if(CallOrGet<LuaTable>("nodeLimits") is LuaTable limits) {
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
				Draw.Rect(Position, Width, Height, TableColor(c));
			}
			if(CallOrGet<LuaTable>("fillColor") is LuaTable fill) {
				Draw.Rect(Position, Width, Height, TableColor(fill));
			}
			if(CallOrGet<LuaTable>("borderColor") is LuaTable border) {
				Draw.HollowRect(Position, Width, Height, TableColor(border));
			}

			if(CallOrGetAll("sprite") is object[] sprites)
				foreach(var item in sprites) {
					if(item is LuaTable sprite) {
						foreach(var k in sprite.Keys) {
							if(sprite[k] is LuaTable sp && sp["meta"] is LuaTable meta && meta["image"] is string image && meta["atlas"] is string atlasName) {
								Atlas atlas = atlasName.ToLowerInvariant().Equals("gui") ? GFX.Gui : atlasName.ToLowerInvariant().Equals("misc") ? GFX.Misc : GFX.Game;
								MTexture tex = atlas[image];
								int x = X, y = Y;
								float sX = Float(sp, "scaleX"), sY = Float(sp, "scaleY");
								Color color = Color.White;

								if(sp["x"] is int spX) { x = spX; }
								if(sp["y"] is int spY) { x = spY; }
								if(sp["color"] is LuaTable ct) { color = TableColor(ct); }

								tex.DrawJustified(new Vector2(x, y), justify, color, new Vector2(sX, sY));
							}
						}
					}
				}

			foreach(var node in Nodes) {
				if(CallOrGet<string>("nodeTexture") is string nodeTexture) {
					GFX.Game[nodeTexture].DrawCentered(node);
				}
			}
		}

		protected override Rectangle[] Select() {
			/*if(CallOrGetAll("selection") is object[] selections) {
				List<LuaTable> checking = new();
				if(selections.Length > 0 && selections[0] is LuaTable t)
					checking.Add(t);
				if(selections.Length > 1 && selections[1] is object[] nodes)
					foreach(var item in nodes)
						if(item is LuaTable t2)
							checking.Add(t2);
				return checking.Select(k => new Rectangle((int)Float(k, "x", X), (int)Float(k, "y", Y), (int)Float(k, "width", 8), (int)Float(k, "height", 8)) ).ToArray();
			}*/

			return base.Select();
		}

		private static float Float<T>(LuaTable from, T index, float def = 1f) {
			if(from.Keys.OfType<T>().Any(k => k.Equals(index))) {
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

		private static Color TableColor(LuaTable from) {
			return new Color(Float(from, 1), Float(from, 2), Float(from, 3), Float(from, 4));
		}

		private T CallOrGet<T>(string name, T orElse = default) where T : class {
			return CallOrGetAll(name, orElse).FirstOrDefault() as T;
		}

		private object[] CallOrGetAll(string name, object orElse = default) {
			LuaTable entity = WrapEntity();
			if(entity == null)
				return new object[] { orElse };
			if(plugin[name] is LuaFunction f) {
				try {
					return (f.Call(EmptyTable(), entity, EmptyTable())) ?? new object[] { orElse };
				} catch {
					return new object[] { orElse };
				}
			}
			else if(plugin[name] is object s) {
				return new object[] { s };
			} else
				return new object[] { orElse };
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