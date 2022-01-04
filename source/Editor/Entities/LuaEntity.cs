using System;
using System.Collections.Generic;
using System.Linq;

using Celeste;
using Celeste.Mod;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Monocle;

using NLua;
using NLua.Exceptions;

namespace Snowberry.Editor.Entities {

	public class LuaEntity : Entity{

		private LuaTable plugin;

		// setup when placed
		private int defaultWidth = -1, defaultHeight = -1;
		private int minNodes = 0, maxNodes = 0;
		private Vector2 justify = Vector2.One * 0.5f;
		private string text = null;

		// refreshed when modified
		Color? color, fillColor, borderColor;
		List<SpriteWithPos> sprites;
		string texture, nodeTexture;
		Rectangle[] selects;
		bool initialized = false;

		public Dictionary<string, object> Values = new();

		public LuaEntity(string name, PluginInfo info, LuaTable plugin, bool isTrigger) {
			Name = name;
			Info = info;
			IsTrigger = isTrigger;
			this.plugin = plugin;

			Tracked = true;

			if(CallOrGet<LuaTable>("nodeLimits") is LuaTable limits) {
				minNodes = (int)Float(limits, 1, 0);
				maxNodes = (int)Float(limits, 2, 0);
			}

			if(plugin["justification"] is LuaTable justification) {
				justify = new Vector2(Float(justification, 1, 0.5f), Float(justification, 2, 0.5f));
			}

			if(isTrigger)
				text = Name; // Entities that are loaded do not get UpdatePostPlacement called
		}

		public override void UpdatePostPlacement(Placements.Placement placement) {
			if(placement.Defaults.ContainsKey("width") && (placement.Defaults["width"] is long width)) {
				defaultWidth = (int)width;
			} else if(IsTrigger) {
				defaultWidth = 8;
			}
			if(placement.Defaults.ContainsKey("height") && placement.Defaults["height"] is long height) {
				defaultHeight = (int)height;
			} else if(IsTrigger) {
				defaultHeight = 8;
			}
			foreach(var option in Info.Options)
				if(!Values.ContainsKey(option.Key))
					Values[option.Key] = ((LuaPluginInfo)Info).Defaults.TryGetValue(option.Key, out var val) ? val : Default(option.Value.Type());
			text = placement.Name;
		}

		public override int MinNodes => minNodes;
		public override int MaxNodes => maxNodes;

		public override int MinWidth => defaultWidth;
		public override int MinHeight => defaultHeight;

		public override void Render() {
			base.Render();

			if(!initialized || Room.DirtyTrackedEntities.ContainsKey(typeof(LuaEntity)) && Room.DirtyTrackedEntities[typeof(LuaEntity)]) {
				color = (CallOrGet<LuaTable>("color") is LuaTable c) ? TableColor(c) : null;
				fillColor = (CallOrGet<LuaTable>("fillColor") is LuaTable f) ? TableColor(f) : null;
				borderColor = (CallOrGet<LuaTable>("borderColor") is LuaTable b) ? TableColor(b) : null;

				sprites = Sprites();

				texture = CallOrGet<string>("texture");
				nodeTexture = CallOrGet<string>("nodeTexture");

				selects = MakeSelections();

				initialized = true;
			}

			if(texture != null) {
				GFX.Game[texture].DrawJustified(Center, justify);
			}

			if(fillColor is Color fill) {
				Draw.Rect(Position, Width, Height, fill);
				if(borderColor is Color border)
					Draw.HollowRect(Position, Width, Height, border);
			} else if(color is Color c) {
				Draw.Rect(Position, Width, Height, c);
			}

			foreach(var sprite in sprites) {
				sprite.texture.DrawJustified(Center + sprite.pos, justify, sprite.color, sprite.scale);
			}

			if(nodeTexture != null)
				foreach(var node in Nodes)
					GFX.Game[nodeTexture].DrawCentered(node);

			if(IsTrigger && text != null) {
				var col = Calc.HexToColor("0c5f7a");
				Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
				Draw.Rect(rect, col * 0.3f);
				Draw.HollowRect(rect, col);
				Fonts.Pico8.Draw(text, Center, Vector2.One, Vector2.One * 0.5f, Color.Black);
			}
		}

		protected override Rectangle[] Select() {
			return selects ?? new Rectangle[0];
		}

		protected Rectangle[] MakeSelections() {
			if(CallOrGetAll("selection") is object[] selections && selections.Length > 0 && selections[0] is LuaTable t) {
				List<LuaTable> checking = new();
				checking.Add(t); // TODO: this is never called???

				if(selections.Length > 1 && selections[1] is object[] nodes)
					foreach(var item in nodes)
						if(item is LuaTable t2)
							checking.Add(t2);
				return checking.Select(k => new Rectangle((int)Float(k, "x", X), (int)Float(k, "y", Y), (int)Float(k, "width", 8), (int)Float(k, "height", 8)) ).ToArray();
			} else {
				List<Rectangle> ret = new();
				MTexture nodeTexture = null;
				if(CallOrGet<string>("texture") is string tex) {
					MTexture texture = GFX.Game[tex];
					nodeTexture = texture;
					ret.Add(RectOnPos(Center, texture));
				} else if(Sprites() is List<SpriteWithPos> sprites && sprites.Count > 0) {
					ret.Add(RectOnPos(Center, sprites[0].texture));
				} else
					ret.Add(new Rectangle(X, Y, Width < 8 ? 8 : Width, Height < 8 ? 8 : Height));
				
				if(CallOrGet<string>("nodeTexture") is string nodeTex)
					nodeTexture = GFX.Game[nodeTex];

				foreach(var n in Nodes)
					if(nodeTexture != null)
						ret.Add(RectOnPos(n, nodeTexture));
					else
						ret.Add(new Rectangle((int)(n.X - 4), (int)(n.Y - 4), 8, 8));

				return ret.ToArray();
			}

			Rectangle RectOnPos(Vector2 pos, MTexture texture) {
				return new Rectangle((int)(pos.X - justify.X * texture.Width), (int)(pos.Y - justify.Y * texture.Height), texture.Width, texture.Height);
			}
		}

		private List<SpriteWithPos> Sprites() {
			List<SpriteWithPos> ret = new();
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

								ret.Add(new SpriteWithPos(tex, new Vector2(x, y) - Center, new Vector2(sX, sY), color));
								sp.Dispose();
							}
						}
						sprite.Dispose();
					}
				}
			return ret;
		}

		private static float Float<T>(LuaTable from, T index, float def = 1f) {
			if(index is int ix) // lua table prefer longs
				return Float(from, (long)ix, def);
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
			Color color1 = new Color(Float(from, 1), Float(from, 2), Float(from, 3), Float(from, 4));
			from.Dispose();
			return color1;
		}

		private T CallOrGet<T>(string name, T orElse = default) where T : class {
			return CallOrGetAll(name, orElse).FirstOrDefault() as T;
		}

		private object[] CallOrGetAll(string name, object orElse = default) {
			using LuaTable entity = WrapEntity();
			using LuaTable empty = EmptyTable();
			if(entity == null)
				return new object[] { orElse };
			if(plugin[name] is LuaFunction f) {
				try {
					return (f.Call(empty, entity, empty)) ?? new object[] { orElse };
				} catch {
					return new object[] { orElse };
				}
			} else if(plugin[name] is object s) {
				return new object[] { s };
			} else
				return new object[] { orElse };
			
		}

		private static bool AllocFailed = false;
		private static LuaTable EmptyTable() {
			try {
				return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
			} catch(LuaScriptException) { // this can fail after many tables are allocated
				if(!AllocFailed) {
					AllocFailed = true;
					Snowberry.Log(LogLevel.Error, "Failed to allocate empty lua table for Lua entities! Lua entity functionality will be limited. Try restarting the game.");
				}
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

		private class SpriteWithPos {
			public readonly MTexture texture;
			public readonly Vector2 pos;
			public readonly Vector2 scale;
			public readonly Color color;

			public SpriteWithPos(MTexture texture, Vector2 pos, Vector2 scale, Color color) {
				this.texture = texture;
				this.pos = pos;
				this.scale = scale;
				this.color = color;
			}
		}
	}
}