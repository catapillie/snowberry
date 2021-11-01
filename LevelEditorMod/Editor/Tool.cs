using LevelEditorMod.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {

	// Object Select (rect/lasso??), Object Placement (entity/trigger/decal), Tile Select, Tile Brush (fg/bg)
	// selection filters (entity/trigger/decal, layers/tags??) can be handled in the panel
	public abstract class Tool {

		public static IList<Tool> Tools = new List<Tool>() { new SelectionTool(), new TileBrushTool() };

		public abstract string GetName();

		public abstract UIElement CreatePanel();

		public abstract void Update();

		public void Render() { }

		public IList<string> GetModes() => new List<string>() { "default" };
	}

	public class SelectionTool : Tool {

		static bool canSelect;

		public override string GetName() {
			return "Object Select";
		}

		public override UIElement CreatePanel() {
			return new UISelectionPanel() {
				Width = 160,
			};
		}

		public override void Update() {
			var editor = Editor.GetCurrent();

			bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;

			if(MInput.Mouse.CheckLeftButton && shift) {
				if(MInput.Mouse.PressedLeftButton) {
					Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

					canSelect = true;
					if(Editor.SelectedEntities != null) {
						foreach(EntitySelection s in Editor.SelectedEntities) {
							if(s.Contains(mouse)) {
								canSelect = false;
								break;
							}
						}
					}
				}

				if(canSelect && Editor.SelectedRoom != null) {
					int ax = (int)Math.Min(Editor.Mouse.World.X, editor.worldClick.X);
					int ay = (int)Math.Min(Editor.Mouse.World.Y, editor.worldClick.Y);
					int bx = (int)Math.Max(Editor.Mouse.World.X, editor.worldClick.X);
					int by = (int)Math.Max(Editor.Mouse.World.Y, editor.worldClick.Y);
					Editor.Selection = new Rectangle(ax, ay, bx - ax, by - ay);

					Editor.SelectedEntities = Editor.SelectedRoom.GetSelectedEntities(Editor.Selection.Value);
				} else if(Editor.SelectedEntities != null) {
					Vector2 worldSnapped = (Editor.Mouse.World / 8).Floor() * 8;
					Vector2 worldLastSnapped = (Editor.Mouse.WorldLast / 8).Floor() * 8;
					Vector2 move = worldSnapped - worldLastSnapped;
					foreach(EntitySelection s in Editor.SelectedEntities)
						s.Move(move);
				}
			} else
				Editor.Selection = null;

			if(MInput.Mouse.ReleasedLeftButton && shift) {
				if(canSelect && editor.ToolPanel is UISelectionPanel selectionPanel)
					selectionPanel.Display(Editor.SelectedEntities);
			}
		}
	}

	public class TileBrushTool : Tool {

		public override string GetName() {
			return "Tile Brush";
		}

		public override UIElement CreatePanel() {
			return new UIElement() {
				Width = 120
			};
		}

		public override void Update() {

		}
	}
}