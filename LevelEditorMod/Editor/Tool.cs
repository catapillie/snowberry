using Celeste;
using LevelEditorMod.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelEditorMod.Editor {

	// Object Select (rect/lasso??), Object Placement (entity/trigger/decal), Tile Select, Tile Brush (fg/bg)
	// selection filters (entity/trigger/decal, layers/tags??) can be handled in the panel
	public abstract class Tool {

		public static IList<Tool> Tools = new List<Tool>() { new SelectionTool(), new TileBrushTool() };

		public abstract string GetName();

		public abstract UIElement CreatePanel();

		public abstract void Update();

		public virtual void RenderScreenSpace() { }

		public virtual void RenderWorldSpace() { }
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

		public class TilesetData {

			public char Key;

			public string Name;

			public bool Bg;

			public TileGrid Tile, Square;

			public TilesetData(char key, string name, bool bg) {
				Key = key; Name = name; Bg = bg;
				Autotiler autotiler = Bg ? GFX.BGAutotiler : GFX.FGAutotiler;
				Tile = autotiler.GenerateBox(Key, 1, 1).TileGrid;
				Square = autotiler.GenerateBox(Key, 3, 3).TileGrid;
			}
		}

		public static int CurLeftTileset = 1;
		public static bool LeftFg = true;

		public List<TilesetData> FgTilesets = new List<TilesetData>();
		public List<TilesetData> BgTilesets = new List<TilesetData>();

		public TileBrushTool() {
			FgTilesets = GetTilesets(false);
			BgTilesets = GetTilesets(true);
		}

		public List<TilesetData> GetTilesets(bool bg) {
			// todo: cleanup?
			DynamicData autotilerData = new DynamicData(typeof(Autotiler), bg ? GFX.BGAutotiler : GFX.FGAutotiler);
			DynamicData lookupData = new DynamicData(autotilerData.Get("lookup"));
			ICollection<char> keys = (ICollection<char>)lookupData.Get("Keys");
			System.Collections.IEnumerable entries = (System.Collections.IEnumerable)lookupData.Get("Values");
			var chars = new List<char>();
			var paths = new List<string>();
			foreach(var item in keys)
				chars.Add(item);
			int i = 0;
			foreach(var item in entries) {
				var itemData = new DynamicData(item);
				var tilesData = new DynamicData(itemData.Get("Center"));
				string path = GFX.Game.Textures.FirstOrDefault(t => t.Value.Equals(tilesData.Get<List<MTexture>>("Textures")[0].GetParent())).Key ?? "Tileset of " + chars[i];
				paths.Add(path);
				i++;
			}
			List<TilesetData> ret = new List<TilesetData>();
			// not a "real" tileset
			ret.Add(new TilesetData('0', "air", bg));
			for(int i1 = 0; i1 < chars.Count; i1++) {
				char item = chars[i1];
				ret.Add(new TilesetData(item, paths[i1], bg));
			}
			return ret;
		}

		public override string GetName() {
			return "Tile Brush";
		}

		public override UIElement CreatePanel() {
			UIElement panel = new UIScrollPane() {
				Width = 130
			};
			var fgLabel = new UILabel("Foreground");
			fgLabel.Position = new Vector2((panel.Width - fgLabel.Width) / 2, 0);
			fgLabel.FG = Color.DarkKhaki;
			fgLabel.Underline = true;
			panel.AddBelow(fgLabel);
			int i = 0;
			foreach(var item in FgTilesets) {
				int copy = i;
				var button = new UIButton((pos, col) => RenderTileGrid(pos, item.Square, col), 8 * 3, 8 * 3, 6, 6) {
					OnPress = () => {
						CurLeftTileset = copy;
						LeftFg = true;
					},
					FG = Color.White,
					PressedFG = Color.Gray,
					HoveredFG = Color.LightGray,
					Position = new Vector2(i % 2 == 0 ? 12 : 8 * 3 + 52, (i / 2) * (8 * 3 + 30) + fgLabel.Height + 20)
				};
				button.Height += 10;
				var label = new UILabel(item.Name.Split('/').Last(), Fonts.Pico8);
				panel.Add(button);
				label.Position += new Vector2(button.Position.X + (button.Width - Fonts.Pico8.Measure(label.Value()).X) / 2, 8 * 3 + 13 + button.Position.Y);
				panel.Add(label);
				i++;
			}
			panel.AddBelow(new UILabel("Background"));
			i = 0;
			foreach(var item in BgTilesets) {
				int copy = i;
				panel.AddBelow(new UIButton(item.Name.Split('/').Last(), Fonts.Regular, 6, 6) {
					OnPress = () => {
						CurLeftTileset = copy;
						LeftFg = false;
					}
				});
				i++;
			}
			return panel;
		}

		public override void Update() {
			bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;

			if(MInput.Mouse.CheckLeftButton && shift)
				if(Editor.SelectedRoom != null)
					if(Editor.SelectedRoom.Bounds.Contains((int)Editor.Mouse.World.X / 8, (int)Editor.Mouse.World.Y / 8))
						if(LeftFg)
							Editor.SelectedRoom.SetFgTile(Editor.Mouse.World, FgTilesets[CurLeftTileset].Key);
						else
							Editor.SelectedRoom.SetBgTile(Editor.Mouse.World, FgTilesets[CurLeftTileset].Key);
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			TileGrid tile = LeftFg ? FgTilesets[CurLeftTileset].Tile : BgTilesets[CurLeftTileset].Tile;
			var tilePos = new Vector2((float)Math.Floor(Editor.Mouse.World.X / 8) * 8, (float)Math.Floor(Editor.Mouse.World.Y / 8) * 8);
			RenderTileGrid(tilePos, tile);
		}

		private static void RenderTileGrid(Vector2 position, TileGrid tile) {
			RenderTileGrid(position, tile, Color.White);
		}

		private static void RenderTileGrid(Vector2 position, TileGrid tile, Color color) {
			if(tile == null)
				return;
			for(int x = 0; x < tile.Tiles.Columns; x++) {
				for(int y = 0; y < tile.Tiles.Rows; y++) {
					if(tile.Tiles[x, y] != null)
						tile.Tiles[x, y].Draw(position + new Vector2(x, y) * 8, Vector2.Zero, color);
				}
			}
		}
	}
}