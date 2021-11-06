using Celeste;
using LevelEditorMod.Editor.Triggers;
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

		public static IList<Tool> Tools = new List<Tool>() { new SelectionTool(), new TileBrushTool(), new RoomTool(), new PlacementTool() };

		public abstract string GetName();

		public abstract UIElement CreatePanel();

		public abstract void Update(bool canClick);

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

		public override void Update(bool canClick) {
			var editor = Editor.GetCurrent();

			if(MInput.Mouse.CheckLeftButton && canClick) {
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

			bool entitiesRemoved = false;
			if(MInput.Keyboard.Check(Keys.Delete)) {
				foreach(var item in Editor.SelectedEntities) {
					entitiesRemoved = true;
					item.Entity.Room.AllEntities.Remove(item.Entity);
					item.Entity.Room.Entities.Remove(item.Entity);
					item.Entity.Room.Triggers.Remove(item.Entity);
				}
				Editor.SelectedEntities.Clear();
			}

			if((MInput.Mouse.ReleasedLeftButton && canClick) || entitiesRemoved) {
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

		public static int CurLeftTileset = 2;
		public static bool LeftFg = true;
		public static int CurRightTileset = 0;
		public static bool RightFg = true;

		public List<TilesetData> FgTilesets = new List<TilesetData>();
		public List<TilesetData> BgTilesets = new List<TilesetData>();

		private List<UIButton> fgTilesetButtons = new List<UIButton>();
		private List<UIButton> bgTilesetButtons = new List<UIButton>();

		private static readonly Color LeftTilesetBtnBg = Calc.HexToColor("274292");
		private static readonly Color RightTilesetBtnBg = Calc.HexToColor("922727");
		private static readonly Color BothTilesetBtnBg = Calc.HexToColor("7d2792");

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
			bgTilesetButtons.Clear();
			fgTilesetButtons.Clear();
			UIScrollPane panel = new UIScrollPane() {
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
					OnRightPress = () => {
						CurRightTileset = copy;
						RightFg = true;
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
				fgTilesetButtons.Add(button);
			}
			var bgLabel = new UILabel("Background");
			bgLabel.Position = new Vector2((panel.Width - bgLabel.Width) / 2, (int)Math.Ceiling(FgTilesets.Count / 2f) * (8 * 3 + 30) + fgLabel.Height + 40);
			bgLabel.FG = Color.DarkKhaki;
			bgLabel.Underline = true;
			panel.Add(bgLabel);
			i = 0;
			foreach(var item in BgTilesets) {
				int copy = i;
				var button = new UIButton((pos, col) => RenderTileGrid(pos, item.Square, col), 8 * 3, 8 * 3, 6, 6) {
					OnPress = () => {
						CurLeftTileset = copy;
						LeftFg = false;
					},
					OnRightPress = () => {
						CurRightTileset = copy;
						RightFg = false;
					},
					FG = Color.White,
					PressedFG = Color.Gray,
					HoveredFG = Color.LightGray,
					Position = new Vector2(i % 2 == 0 ? 12 : 8 * 3 + 52, (i / 2) * (8 * 3 + 30) + (bgLabel.Position.Y) + 20)
				};
				button.Height += 10;
				var label = new UILabel(item.Name.Split('/').Last(), Fonts.Pico8);
				panel.Add(button);
				label.Position += new Vector2(button.Position.X + (button.Width - Fonts.Pico8.Measure(label.Value()).X) / 2, 8 * 3 + 13 + button.Position.Y);
				panel.Add(label);
				i++;
				bgTilesetButtons.Add(button);
			}
			return panel;
		}

		public override void Update(bool canClick) {
			bool fg = MInput.Mouse.CheckLeftButton ? LeftFg : RightFg;
			int tileset = MInput.Mouse.CheckLeftButton ? CurLeftTileset : CurRightTileset;
			if((MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton) && canClick)
				if(Editor.SelectedRoom != null)
					if(Editor.SelectedRoom.Bounds.Contains((int)Editor.Mouse.World.X / 8, (int)Editor.Mouse.World.Y / 8))
						if(fg)
							Editor.SelectedRoom.SetFgTile(Editor.Mouse.World, FgTilesets[tileset].Key);
						else
							Editor.SelectedRoom.SetBgTile(Editor.Mouse.World, BgTilesets[tileset].Key);

			for(int i = 0; i < fgTilesetButtons.Count; i++) {
				UIButton button = fgTilesetButtons[i];
				if(LeftFg && RightFg && CurLeftTileset == CurRightTileset && CurLeftTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = BothTilesetBtnBg;
				else if(LeftFg && CurLeftTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = LeftTilesetBtnBg;
				else if(RightFg && CurRightTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = RightTilesetBtnBg;
				else {
					button.BG = UIButton.DefaultBG;
					button.PressedBG = UIButton.DefaultPressedBG;
					button.HoveredBG = UIButton.DefaultHoveredBG;
				}
			}
			for(int i = 0; i < bgTilesetButtons.Count; i++) {
				UIButton button = bgTilesetButtons[i];
				if(!LeftFg && !RightFg && CurLeftTileset == CurRightTileset && CurLeftTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = BothTilesetBtnBg;
				else if(!LeftFg && CurLeftTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = LeftTilesetBtnBg;
				else if(!RightFg && CurRightTileset == i)
					button.BG = button.PressedBG = button.HoveredBG = RightTilesetBtnBg;
				else {
					button.BG = UIButton.DefaultBG;
					button.PressedBG = UIButton.DefaultPressedBG;
					button.HoveredBG = UIButton.DefaultHoveredBG;
				}
			}
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

	public class RoomTool : Tool {

		private Room lastSelected = null;
		public static bool ScheduledRefresh = false;

		public override UIElement CreatePanel() {
			// room selection panel containing room metadata
			var ret = new UIRoomSelectionPanel() {
				Width = 160
			};
			ret.Refresh();
			return ret;
		}

		public override string GetName() {
			return "Room Tool";
		}

		public override void Update(bool canClick) {
			// move, resize, add rooms
			if(canClick) {

			}

			// refresh the display
			if(lastSelected != Editor.SelectedRoom || ScheduledRefresh) {
				ScheduledRefresh = false;
				lastSelected = Editor.SelectedRoom;
				if(Editor.GetCurrent().ToolPanel is UIRoomSelectionPanel selectionPanel)
					selectionPanel.Refresh();
			}
		}
	}

	public class PlacementTool : Tool {

		string curLeftSelection = null, curRightSelection = null;
		Dictionary<string, UIButton> placementButtons = new Dictionary<string, UIButton>();
		Entity preview = null;

		private static readonly Color LeftPlacementBtnBg = Calc.HexToColor("274292");
		private static readonly Color RightPlacementBtnBg = Calc.HexToColor("922727");
		private static readonly Color BothPlacementBtnBg = Calc.HexToColor("7d2792");

		public override UIElement CreatePanel() {
			placementButtons.Clear();
			var ret = new UIScrollPane();
			ret.Width = 180;
			foreach(var item in PluginInfo.All) {
				UIButton b;
				ret.AddBelow(b = new UIButton(item.Key, Fonts.Regular, 4, 4) {
					OnPress = () => curLeftSelection = curLeftSelection != item.Key ? item.Key : null,
					OnRightPress = () => curRightSelection = curRightSelection != item.Key ? item.Key : null
				});
				placementButtons[item.Key] = b;
			}
			return ret;
		}

		public override string GetName() {
			return "Object Placement";
		}

		public override void Update(bool canClick) {
			string selection = MInput.Mouse.PressedLeftButton ? curLeftSelection : curRightSelection;
			if((MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton) && canClick && selection != null && Editor.SelectedRoom != null && Editor.SelectedRoom.Bounds.Contains((int)Editor.Mouse.World.X / 8, (int)Editor.Mouse.World.Y / 8)) {
				Entity toAdd = Entity.Create(selection, Editor.SelectedRoom);
				toAdd.SetPosition((Editor.Mouse.World / 8).Round() * 8);
				Editor.SelectedRoom.AllEntities.Add(toAdd);
				if(toAdd is Plugin_Trigger) Editor.SelectedRoom.Triggers.Add(toAdd);
				else Editor.SelectedRoom.Entities.Add(toAdd);
			}

			if((preview == null && curLeftSelection != null) || (preview != null && curLeftSelection != null && !preview.Name.Equals(curLeftSelection))) {
				preview = Entity.Create(curLeftSelection, Editor.SelectedRoom);
			} else if(curLeftSelection == null)
				preview = null;
			preview?.SetPosition((Editor.Mouse.World / 8).Round() * 8);
			// this is Stupid
			preview?.ApplyDefaults();

			foreach(var item in placementButtons) {
				var button = item.Value;
				if(item.Key.Equals(curLeftSelection) && item.Key.Equals(curRightSelection))
					button.BG = button.PressedBG = button.HoveredBG = BothPlacementBtnBg;
				else if(item.Key.Equals(curLeftSelection))
					button.BG = button.PressedBG = button.HoveredBG = LeftPlacementBtnBg;
				else if(item.Key.Equals(curRightSelection))
					button.BG = button.PressedBG = button.HoveredBG = RightPlacementBtnBg;
				else {
					button.BG = UIButton.DefaultBG;
					button.HoveredBG = UIButton.DefaultHoveredBG;
					button.PressedBG = UIButton.DefaultPressedBG;
				}
			}
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			if(preview != null) {
				Calc.PushRandom(preview.GetHashCode());
				preview.Render();
				Calc.PopRandom();
			}
		}
	}
}