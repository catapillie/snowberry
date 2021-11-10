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

		public static IList<Tool> Tools = new List<Tool>() { new SelectionTool(), new DecalSelectionTool(), new TileBrushTool(), new RoomTool(), new PlacementTool() };

		public abstract string GetName();

		public abstract UIElement CreatePanel();

		public abstract void Update(bool canClick);

		public virtual void RenderScreenSpace() { }

		public virtual void RenderWorldSpace() { }
	}

	public class SelectionTool : Tool {

		static bool canSelect;

		public override string GetName() {
			return "Entity Select";
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

	public class DecalSelectionTool : Tool {

		static List<Decal> SelectedDecals = new List<Decal>();
		static bool fg = false;
		static bool bg = true;
		static bool canSelect;

		public override string GetName() {
			return "Decal Select";
		}

		public override UIElement CreatePanel() {
			var panel = new UIElement() {
				Width = 80
			};
			panel.AddBelow(new UISelectionPanel.UIOption("foreground", new UICheckBox(-1, fg) {
				OnPress = val => fg = val
			}), Vector2.UnitY * 4);
			panel.AddBelow(new UISelectionPanel.UIOption("background", new UICheckBox(-1, bg) {
				OnPress = val => bg = val
			}), Vector2.UnitY * 4);
			return panel;
		}

		public override void Update(bool canClick) {
			var editor = Editor.GetCurrent();

			if(MInput.Mouse.CheckLeftButton && canClick) {
				if(MInput.Mouse.PressedLeftButton) {
					Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

					canSelect = true;
					if(SelectedDecals != null) {
						foreach(var s in SelectedDecals) {
							if(s.Bounds.Contains(mouse)) {
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

					SelectedDecals = new List<Decal>();
					var selectFrom = new List<Decal>();
					if(fg) selectFrom.AddRange(Editor.SelectedRoom.FgDecals);
					if(bg) selectFrom.AddRange(Editor.SelectedRoom.BgDecals);
					foreach(var item in selectFrom) {
						if(item.Bounds.Intersects(Editor.Selection.Value))
							SelectedDecals.Add(item);
					}
				} else if(SelectedDecals != null) {
					Vector2 worldSnapped = (Editor.Mouse.World / 8).Floor() * 8;
					Vector2 worldLastSnapped = (Editor.Mouse.WorldLast / 8).Floor() * 8;
					Vector2 move = worldSnapped - worldLastSnapped;
					foreach(Decal s in SelectedDecals)
						s.Position += move;
				}
			} else
				Editor.Selection = null;

			if(MInput.Keyboard.Check(Keys.Delete)) {
				foreach(var item in SelectedDecals) {
					item.Room.FgDecals.Remove(item);
					item.Room.BgDecals.Remove(item);
				}
				SelectedDecals.Clear();
			}
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			foreach(var item in SelectedDecals)
				Draw.Rect(item.Bounds, Color.Blue * 0.25f);
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

		Placements.Placement curLeftSelection = null, curRightSelection = null;
		Dictionary<Placements.Placement, UIButton> placementButtons = new Dictionary<Placements.Placement, UIButton>();
		Entity preview = null;
		Vector2? lastPress = null;

		private static readonly Color LeftPlacementBtnBg = Calc.HexToColor("274292");
		private static readonly Color RightPlacementBtnBg = Calc.HexToColor("922727");
		private static readonly Color BothPlacementBtnBg = Calc.HexToColor("7d2792");

		public override UIElement CreatePanel() {
			placementButtons.Clear();
			var ret = new UIScrollPane();
			ret.Width = 180;
			foreach(var item in Placements.All) {
				UIButton b;
				ret.AddBelow(b = new UIButton(item.Name, Fonts.Regular, 4, 4) {
					OnPress = () => {
						curLeftSelection = curLeftSelection != item ? item : null;
						RefreshPreview(true);
					},
					OnRightPress = () => curRightSelection = curRightSelection != item ? item : null
				});
				placementButtons[item] = b;
			}
			return ret;
		}

		public override string GetName() {
			return "Object Placement";
		}

		public override void Update(bool canClick) {
			Editor editor = Editor.GetCurrent();
			Rectangle area;
			if(lastPress != null) {
				var mpos = (Editor.Mouse.World / 8).Round() * 8;
				int ax = (int)Math.Min(mpos.X, lastPress.Value.X);
				int ay = (int)Math.Min(mpos.Y, lastPress.Value.Y);
				int bx = (int)Math.Max(mpos.X, lastPress.Value.X);
				int by = (int)Math.Max(mpos.Y, lastPress.Value.Y);
				area = new Rectangle(ax, ay, bx - ax, by - ay);
			} else
				area = Rectangle.Empty;

			Placements.Placement selection = (MInput.Mouse.CheckRightButton || MInput.Mouse.ReleasedRightButton) ? curRightSelection : curLeftSelection;
			if((MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) && canClick && selection != null && Editor.SelectedRoom != null && Editor.SelectedRoom.Bounds.Contains((int)Editor.Mouse.World.X / 8, (int)Editor.Mouse.World.Y / 8)) {
				Entity toAdd = selection.Build(Editor.SelectedRoom);
				UpdateEntity(toAdd, area);
				Editor.SelectedRoom.AllEntities.Add(toAdd);
				if(toAdd is Plugin_Trigger) Editor.SelectedRoom.Triggers.Add(toAdd);
				else Editor.SelectedRoom.Entities.Add(toAdd);
			}

			RefreshPreview(false);
			if(preview != null) {
				UpdateEntity(preview, area);
			}

			if(MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton)
				lastPress = Editor.Mouse.World;
			else if(!MInput.Mouse.CheckLeftButton && !MInput.Mouse.CheckRightButton)
				lastPress = null;

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

		private void RefreshPreview(bool changedPlacement) {
			Placements.Placement selection = (MInput.Mouse.CheckRightButton || MInput.Mouse.ReleasedRightButton) ? curRightSelection : curLeftSelection;
			if((preview == null && selection != null) || changedPlacement) {
				preview = selection.Build(Editor.SelectedRoom);
			} else if(selection == null)
				preview = null;
		}

		private void UpdateEntity(Entity e, Rectangle area) {
			// need to apply its defaults, to update its size, to set its position, to apply its (node and size) defaults, and update size again
			// this is Stupid
			e.ApplyDefaults();
			UpdateSize(e, area);
			var mpos = (Editor.Mouse.World / 8).Round() * 8;
			if(lastPress != null)
				e.SetPosition(new Vector2(e.Width > 0 ? (area.Left / 8) * 8 : mpos.X, e.Height > 0 ? (area.Top / 8) * 8 : mpos.Y));
			else
				e.SetPosition(mpos);
			e.ApplyDefaults();
			UpdateSize(e, area);
		}

		private void UpdateSize(Entity e, Rectangle area) {
			if(MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton || MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) {
				if(e.Width > 0)
					e.SetWidth(Math.Max((int)Math.Ceiling(area.Width / 8f) * 8, e.Width));
				if(e.Height > 0)
					e.SetHeight(Math.Max((int)Math.Ceiling(area.Height / 8f) * 8, e.Height));
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