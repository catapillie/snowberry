using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste;

using Microsoft.Xna.Framework;

using Monocle;

namespace Snowberry.Editor.UI {

	// Segmented UIButton that hovers and clicks different entries separately
	public class UIDropdown : UIElement{

		public class DropdownEntry {
			public string Label;
			public Action OnPress, OnAlternatePress;
			public Color FG = UIButton.DefaultFG;
			public Color BG = UIButton.DefaultBG;
			public Color PressedFG = UIButton.DefaultPressedFG;
			public Color PressedBG = UIButton.DefaultPressedBG;
			public Color HoveredFG = UIButton.DefaultHoveredFG;
			public Color HoveredBG = UIButton.DefaultHoveredBG;

			public DropdownEntry(string label, Action onPress, Action onAltPress = null) {
				Label = label;
				OnPress = onPress;
				OnAlternatePress = onAltPress;
			}
		}

		private readonly Vector2 spacing = new Vector2(4);
		private Font font;
		private float[] lerps;
		private int hoverIdx = -1, pressIdx = -1;
		private readonly List<DropdownEntry> entries = new List<DropdownEntry>();

		private readonly MTexture
			top, bottom,
			topFill, bottomFill,
			mid;

		public UIDropdown(Font font, params DropdownEntry[] entries) {
			this.entries.AddRange(entries);
			lerps = new float[entries.Count()];
			this.font = font;

			MTexture full = GFX.Gui["Snowberry/button"];
			top = full.GetSubtexture(0, 0, 3, 4);
			topFill = full.GetSubtexture(2, 0, 1, 4);
			bottom = full.GetSubtexture(0, 5, 3, 3);
			bottomFill = full.GetSubtexture(2, 5, 1, 4);
			mid = full.GetSubtexture(0, 4, 2, 1);

			float maxWidth = 6;
			foreach(var entry in entries) {
				var area = font.Measure(entry.Label);
				maxWidth = Math.Max(maxWidth, area.X) + 3;
				Height += (int)area.Y;
			}
			Width = (int)maxWidth + 6;
			Height += 8;
		}

		public override void Update(Vector2 position = default) {
			base.Update();
			hoverIdx = -1;

			int mouseX = (int)Editor.Mouse.Screen.X;
			int mouseY = (int)Editor.Mouse.Screen.Y;
			for(int i = 0; i < entries.Count; i++) {
				if(new Rectangle((int)position.X + 1, (int)(position.Y + YPosFor(i)) + 1 + 4, Width - 2, (int)font.Measure(entries[i].Label).Y + 4).Contains(mouseX, mouseY)) {
					hoverIdx = i;
				}
			}

			bool hovering = hoverIdx != -1;

			if(hovering && (ConsumeLeftClick() || ConsumeAltClick()))
				pressIdx = hoverIdx;
			else if(hovering && pressIdx != -1) {
				if(ConsumeAltClick(pressed: false, released: true)) {
					entries[pressIdx].OnAlternatePress?.Invoke();
					pressIdx = -1;
				} else if(ConsumeLeftClick(pressed: false, released: true)) {
					entries[pressIdx].OnPress?.Invoke();
					pressIdx = -1;
				}
			}

			for(int i = 0; i < lerps.Count(); i++) {
				lerps[i] = Calc.Approach(lerps[i], pressIdx == i ? 1f : 0f, Engine.DeltaTime * 20f);
			}
		}

		private float YPosFor(int i) {
			return entries.Take(i).Select(k => font.Measure(k.Label).Y + 4).Sum();
		}

		public override void Render(Vector2 position = default) {
			base.Render(position);

			// draw top
			var defaultColor = ColorForEntry(0);
			top.Draw(new Vector2(position.X, position.Y), Vector2.Zero, defaultColor);
			topFill.Draw(new Vector2(position.X + 3, position.Y), Vector2.Zero, defaultColor, new Vector2(Width - 6, 1));
			top.Draw(new Vector2(position.X + Width, position.Y), Vector2.Zero, defaultColor, new Vector2(-1, 1));
			// draw each entry
			for(int i = 0; i < entries.Count; i++) {
				DropdownEntry entry = entries[i];
				var ePos = position + Vector2.UnitY * YPosFor(i);
				var press = (pressIdx == i) ? 1 : 0;
				var bg = ColorForEntry(i);
				float h = font.Measure(entry.Label).Y;
				mid.Draw(new Vector2(ePos.X, ePos.Y + h - 4), Vector2.Zero, bg);
				mid.Draw(new Vector2(ePos.X + Width, ePos.Y + h - 4), Vector2.Zero, bg, new Vector2(-1, 1));
				Draw.Rect(new Vector2(ePos.X, ePos.Y + 4), Width, h + 4, Color.Black);
				Draw.Rect(new Vector2(ePos.X + 1, ePos.Y + 4), Width - 2, h + 4, bg);
				Color fg = Color.Lerp((hoverIdx == i) ? entry.HoveredFG : entry.FG, entry.PressedFG, lerps[i]);
				font.Draw(entry.Label, ePos + new Vector2(4 + press, 5), Vector2.One, fg);
			}
			// draw bottom
			bottom.Draw(new Vector2(position.X, position.Y + Height + 12), Vector2.Zero, defaultColor);
			bottomFill.Draw(new Vector2(position.X + 3, position.Y + Height + 12), Vector2.Zero, defaultColor, new Vector2(Width - 6, 1));
			bottom.Draw(new Vector2(position.X + Width, position.Y + Height + 12), Vector2.Zero, defaultColor, new Vector2(-1, 1));
		}

		public Color ColorForEntry(int index) {
			if(index >= entries.Count) {
				return UIButton.DefaultBG;
			}
			DropdownEntry e = entries[index];
			return Color.Lerp(hoverIdx == index ? e.HoveredBG : e.BG, e.PressedBG, lerps[index]);
		}
	}
}