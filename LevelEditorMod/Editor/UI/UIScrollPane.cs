using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditorMod.Editor.UI {

	class UIScrollPane : UIElement {

		public Color BG = Calc.HexToColor("202929");

		public UIScrollPane() {
			BG.A = 127;
		}

		public override void Render(Vector2 position = default) {
			Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);
			Draw.Rect(rect, BG);
			base.Render(position);
		}
	}
}