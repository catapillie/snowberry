using System.Collections.Generic;

using Celeste;

using Microsoft.Xna.Framework;

using Monocle;

using NLua;

namespace Snowberry.Editor.Entities {

	public class LuaEntity : Entity{

		private LuaTable plugin;
		public Dictionary<string, object> Values = new();

		public LuaEntity(string name, PluginInfo info, LuaTable plugin) {
			Name = name;
			Info = info;
			this.plugin = plugin;
		}

		public override void Render() {
			base.Render();
			// if we have a texture, render it
			if(plugin["texture"] is string texture) {
				GFX.Game[texture].DrawCentered(Center);
			}
			// if we have a fill and/or border, display those
			if(plugin["fillColor"] is LuaTable fill) {
				Draw.Rect(Position, Width, Height, Color(fill));
			}
			if(plugin["borderColor"] is LuaTable border) {
				Draw.HollowRect(Position, Width, Height, Color(border));
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
	}
}