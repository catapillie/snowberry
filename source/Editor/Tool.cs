using Celeste;
using Snowberry.Editor.Triggers;
using Snowberry.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor {

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
					bool noSnap = (MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl));
					Vector2 worldSnapped = noSnap ? Editor.Mouse.World : (Editor.Mouse.World / 8).Round() * 8;
					Vector2 worldLastSnapped = noSnap? Editor.Mouse.WorldLast : (Editor.Mouse.WorldLast / 8).Round() * 8;
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
					item.Entity.Room.RemoveEntity(item.Entity);
				}
				Editor.SelectedEntities.Clear();
			}

			if((MInput.Mouse.ReleasedLeftButton && canClick) || entitiesRemoved) {
				if(canSelect && editor.ToolPanel is UISelectionPanel selectionPanel)
					selectionPanel.Display(Editor.SelectedEntities);
			}
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			if(Editor.SelectedRoom != null)
				foreach(var item in Editor.SelectedRoom.GetSelectedEntities(new Rectangle((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y, 0, 0)))
					if(Editor.SelectedEntities == null || !Editor.SelectedEntities.Contains(item))
						foreach(var s in item.Selections)
							Draw.Rect(s.Rect, Color.Blue * 0.15f);
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

		public enum TileBrushMode {
			Brush, Rect, HollowRect, Fill, Line, Circle
		}

		public static int CurLeftTileset = 2;
		public static bool LeftFg = true;
		public static int CurRightTileset = 0;
		public static bool RightFg = true;

		public static TileBrushMode LeftMode, RightMode;
		// tile, hasTile
		private static VirtualMap<char> holoFgTileMap;
		private static VirtualMap<char> holoBgTileMap;
		private static VirtualMap<bool> holoSetTiles;
		private static TileGrid holoGrid;
		private static bool holoRetile = false;

		public List<TilesetData> FgTilesets = new List<TilesetData>();
		public List<TilesetData> BgTilesets = new List<TilesetData>();

		private List<UIButton> fgTilesetButtons = new List<UIButton>();
		private List<UIButton> bgTilesetButtons = new List<UIButton>();
		private List<UIButton> modeButtons = new List<UIButton>();

		private static bool isPainting;

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
			modeButtons.Clear();
			UIElement panel = new UIElement() {
				Width = 160,
				Background = Calc.HexToColor("202929") * (185 / 255f),
				GrabsClick = true,
				GrabsScroll = true
			};
			UIScrollPane tilesetsPanel = new UIScrollPane() {
				Width = 130,
				TopPadding = 10
			};
			var fgLabel = new UILabel("Foreground");
			fgLabel.Position = new Vector2((tilesetsPanel.Width - fgLabel.Width) / 2, 0);
			fgLabel.FG = Color.DarkKhaki;
			fgLabel.Underline = true;
			tilesetsPanel.AddBelow(fgLabel);
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
				tilesetsPanel.Add(button);
				label.Position += new Vector2(button.Position.X + (button.Width - Fonts.Pico8.Measure(label.Value()).X) / 2, 8 * 3 + 13 + button.Position.Y);
				tilesetsPanel.Add(label);
				i++;
				fgTilesetButtons.Add(button);
			}
			var bgLabel = new UILabel("Background");
			bgLabel.Position = new Vector2((tilesetsPanel.Width - bgLabel.Width) / 2, (int)Math.Ceiling(FgTilesets.Count / 2f) * (8 * 3 + 30) + fgLabel.Height + 40);
			bgLabel.FG = Color.DarkKhaki;
			bgLabel.Underline = true;
			tilesetsPanel.Add(bgLabel);
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
				tilesetsPanel.Add(button);
				label.Position += new Vector2(button.Position.X + (button.Width - Fonts.Pico8.Measure(label.Value()).X) / 2, 8 * 3 + 13 + button.Position.Y);
				tilesetsPanel.Add(label);
				i++;
				bgTilesetButtons.Add(button);
			}
			UIElement brushTypes = new UIElement() {
				Width = 30
			};
			foreach(var mode in Enum.GetValues(typeof(TileBrushMode))) {
				var t = mode.ToString();
				var button = new UIButton(t.Substring(0, 1), Fonts.Regular, 6, 6) {
					OnPress = () => LeftMode = (TileBrushMode)mode,
					OnRightPress = () => RightMode = (TileBrushMode)mode
				};
				brushTypes.AddBelow(button, Vector2.One * 5);
				modeButtons.Add(button);
			}
			panel.Add(brushTypes);
			tilesetsPanel.Position.X = brushTypes.Width + 5;
			tilesetsPanel.Background = null;
			panel.Add(tilesetsPanel);
			return panel;
		}

		public override void Update(bool canClick) {
			bool clear = MInput.Keyboard.Check(Keys.X);

			if(Editor.SelectedRoom == null)
				holoFgTileMap = holoBgTileMap = null;
			else if(holoFgTileMap == null || holoFgTileMap.Columns != Editor.SelectedRoom.Width || holoFgTileMap.Rows != Editor.SelectedRoom.Height || clear) {
				holoFgTileMap = new VirtualMap<char>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, '0');
				holoBgTileMap = new VirtualMap<char>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, '0');
				holoSetTiles = new VirtualMap<bool>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, false);
				isPainting = false;
			}

			bool left = MInput.Mouse.CheckLeftButton || MInput.Mouse.ReleasedLeftButton;
			bool fg = left ? LeftFg : RightFg;
			int tileset = left ? CurLeftTileset : CurRightTileset;
			bool retile = false;

			if(canClick && (MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton)) {
				isPainting = true;
			} else if(MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) {
				if(Editor.SelectedRoom != null && canClick && isPainting)
					for(int x = 0; x < holoFgTileMap.Columns; x++)
						for(int y = 0; y < holoFgTileMap.Rows; y++)
							if(fg) {
								if(holoSetTiles[x, y])
									retile |= Editor.SelectedRoom.SetFgTile(x, y, holoFgTileMap[x, y]);
							} else
								if(holoSetTiles[x, y])
								retile |= Editor.SelectedRoom.SetBgTile(x, y, holoBgTileMap[x, y]);
				if(retile) {
					Editor.SelectedRoom.Autotile();
				}

				isPainting = false;
				holoFgTileMap = null;
				holoBgTileMap = null;
				holoGrid = null;
			} else if((MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton) && Editor.SelectedRoom != null) {
				var tilePos = new Vector2((float)Math.Floor(Editor.Mouse.World.X / 8 - Editor.SelectedRoom.Position.X), (float)Math.Floor(Editor.Mouse.World.Y / 8 - Editor.SelectedRoom.Position.Y));
				int x = (int)tilePos.X; int y = (int)tilePos.Y;
				if(Editor.SelectedRoom.Bounds.Contains((int)(x + Editor.SelectedRoom.Position.X), (int)(y + Editor.SelectedRoom.Position.Y))) {
					var lastPress = (Editor.GetCurrent().worldClick / 8).Ceiling();
					var roomLastPress = (Editor.GetCurrent().worldClick / 8).Ceiling() - Editor.SelectedRoom.Position;
					int ax = (int)Math.Min(x, roomLastPress.X);
					int ay = (int)Math.Min(y, roomLastPress.Y);
					int bx = (int)Math.Max(x, roomLastPress.X);
					int by = (int)Math.Max(y, roomLastPress.Y);
					var rect = new Rectangle(ax, ay, bx - ax, by - ay);
					TileBrushMode mode = left ? LeftMode : RightMode;
					switch(mode) {
						case TileBrushMode.Brush:
							SetHoloTile(fg, tileset, x, y);
							break;
						case TileBrushMode.HollowRect:
						case TileBrushMode.Rect:
							for(int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
								for(int y2 = 0; y2 < holoFgTileMap.Rows; y2++) {
									bool set = rect.Contains(x2, y2);
									if(mode == TileBrushMode.HollowRect) {
										set &= !new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2).Contains(x2, y2);
									}
									SetHoloTile(fg, set ? tileset : 0, x2, y2, !set);
								}
							break;
						case TileBrushMode.Fill:
							// start at x,y
							// while new tiles have been found:
							//   for each tile found:
							//     check their neighbors
							if(!holoSetTiles[x, y]) {
								char origTile = Editor.SelectedRoom.GetTile(fg, new Vector2((x + Editor.SelectedRoom.X) * 8, (y + Editor.SelectedRoom.Y) * 8));
								bool inside(int cx, int cy) => (cx >= 0 && cy >= 0 && cx < Editor.SelectedRoom.Width && cy < Editor.SelectedRoom.Height) && Editor.SelectedRoom.GetTile(fg, new Vector2((cx + Editor.SelectedRoom.X) * 8, (cy + Editor.SelectedRoom.Y) * 8)) == origTile;
								Queue<Point> toCheck = new Queue<Point>();
								void scan(int lx, int rx, int y) {
									bool added = false;
									for(int i = lx; i <= rx; i++) {
										if(!inside(i, y))
											added = false;
										else if(!added && !holoSetTiles[i, y]) {
											toCheck.Enqueue(new Point(i, y));
											added = true;
										}
									}
								}
								toCheck.Enqueue(new Point(x, y));
								while(toCheck.Count > 0) {
									Point checking = toCheck.Dequeue();
									int x2 = checking.X, y2 = checking.Y;
									var lx = x2;
									while(inside(lx - 1, y2)) {
										SetHoloTile(fg, tileset, lx - 1, y2);
										lx--;
									}
									while(inside(x2, y2)) {
										SetHoloTile(fg, tileset, x2, y2);
										x2++;
									}
									scan(lx, x2 - 1, y2 + 1);
									scan(lx, x2 - 1, y2 - 1);
								}
							}

							break;
						case TileBrushMode.Line:
							for(int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
								for(int y2 = 0; y2 < holoFgTileMap.Rows; y2++)
									SetHoloTile(fg, 0, x2, y2, true);
							if(roomLastPress.X - x == 0 && roomLastPress.Y - y == 0)
								SetHoloTile(fg, tileset, x, y);
							else if(Math.Abs(roomLastPress.X - x) > Math.Abs(roomLastPress.Y - y)) {
								int sign = -Math.Sign(roomLastPress.X - x);
								float grad = (roomLastPress.Y - y) / (roomLastPress.X - x);
								for(int p = 0; p < rect.Width; p++) {
									SetHoloTile(fg, tileset, (int)(sign * p + roomLastPress.X), (int)(sign * p * grad + roomLastPress.Y));
								}
							} else {
								int sign = -Math.Sign(roomLastPress.Y - y);
								float grad = (roomLastPress.X - x) / (roomLastPress.Y - y);
								for(int p = 0; p < rect.Height; p++) {
									SetHoloTile(fg, tileset, (int)(sign * p * grad + roomLastPress.X), (int)(sign * p + roomLastPress.Y));
								}
							}
							break;
						case TileBrushMode.Circle:
							for(int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
								for(int y2 = 0; y2 < holoFgTileMap.Rows; y2++) {
									bool set = ((lastPress.X - x2) * (lastPress.X - x2) + (lastPress.Y - y2) * (lastPress.Y - y2)) < (rect.Width * rect.Width + rect.Height * rect.Height) / Math.Sqrt(2);
									SetHoloTile(fg, set ? tileset : 0, x2, y2, !set);
								}
							break;
						default:
							break;
					}
					if(holoRetile) {
						holoRetile = false;
						holoGrid = (fg ? GFX.FGAutotiler : GFX.BGAutotiler).GenerateMap(fg ? holoFgTileMap : holoBgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid;
					}
				}
			}

			// TODO: cleanup a bit
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
			for(int i = 0; i < modeButtons.Count; i++) {
				UIButton button = modeButtons[i];
				if(LeftMode == RightMode && RightMode == (TileBrushMode)i)
					button.BG = button.PressedBG = button.HoveredBG = BothTilesetBtnBg;
				else if(LeftMode == (TileBrushMode)i)
					button.BG = button.PressedBG = button.HoveredBG = LeftTilesetBtnBg;
				else if(RightMode == (TileBrushMode)i)
					button.BG = button.PressedBG = button.HoveredBG = RightTilesetBtnBg;
				else {
					button.BG = UIButton.DefaultBG;
					button.PressedBG = UIButton.DefaultPressedBG;
					button.HoveredBG = UIButton.DefaultHoveredBG;
				}
			}
		}

		public void SetHoloTile(bool fg, int tileset, int x, int y, bool unset = false) {
			char tile = fg ? FgTilesets[tileset].Key : BgTilesets[tileset].Key;
			VirtualMap<char> tiles = fg ? holoFgTileMap : holoBgTileMap;
			char prev = tiles[x, y];
			bool reset = !holoSetTiles[x, y] || (holoSetTiles[x, y] && unset);
			if(prev != tile || reset) {
				tiles[x, y] = tile;
				holoSetTiles[x, y] = !unset;
				holoRetile = holoRetile || prev != tile;
			}
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			TileGrid tile = LeftFg ? FgTilesets[CurLeftTileset].Tile : BgTilesets[CurLeftTileset].Tile;
			var tilePos = new Vector2((float)Math.Floor(Editor.Mouse.World.X / 8) * 8, (float)Math.Floor(Editor.Mouse.World.Y / 8) * 8);
			if(isPainting && Editor.SelectedRoom != null) {
				var fg = MInput.Mouse.CheckLeftButton ? LeftFg : RightFg;
				var map = fg ? holoFgTileMap : holoBgTileMap;
				Vector2 p = Editor.SelectedRoom.Position * 8;
				RenderTileGrid(p, holoGrid, Color.White * 0.75f);
				var prog = (float)Math.Abs(Math.Sin(Engine.Scene.TimeActive * 3));
				for(int x = 0; x < map.Columns; x++)
					for(int y = 0; y < map.Rows; y++)
						if(holoSetTiles[x, y] && map[x, y] == '0')
							Draw.Rect(p.X + x * 8, p.Y + y * 8, 8, 8, Color.Red * (prog / 3f + 0.35f));
			} else
				RenderTileGrid(tilePos, tile, Color.White * 0.5f);
		}

		public static void RenderTileGrid(Vector2 position, TileGrid tile, Color color) {
			if(tile == null)
				return;
			for(int x = 0; x < tile.Tiles.Columns; x++)
				for(int y = 0; y < tile.Tiles.Rows; y++)
					if(tile.Tiles[x, y] != null)
						tile.Tiles[x, y].Draw(position + new Vector2(x, y) * 8, Vector2.Zero, color);
		}
	}

	public class RoomTool : Tool {

		private Room lastSelected = null;
		private int lastFillerSelected = -1;
		public static bool ScheduledRefresh = false;

		private Vector2? lastRoomOffset = null;
		private static bool resizingX, resizingY;
		private static int newWidth, newHeight;
		private static Rectangle oldRoomBounds;
		private static bool justSwitched = false;

		public static Rectangle? PendingRoom = null;

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
			// refresh the display
			if(lastSelected != Editor.SelectedRoom || lastFillerSelected != Editor.SelectedFillerIndex || ScheduledRefresh) {
				justSwitched = true;
				ScheduledRefresh = false;
				lastSelected = Editor.SelectedRoom;
				lastFillerSelected = Editor.SelectedFillerIndex;
				if(Editor.GetCurrent().ToolPanel is UIRoomSelectionPanel selectionPanel)
					selectionPanel.Refresh();
				if(Editor.SelectedRoom != null) {
					lastRoomOffset = Editor.SelectedRoom.Position - (Editor.Mouse.World / 8);
					oldRoomBounds = Editor.SelectedRoom.Bounds;
				}
			}

			// move, resize, add rooms
			if(canClick && Editor.SelectedRoom != null && !justSwitched) {
				if(MInput.Mouse.PressedLeftButton) {
					lastRoomOffset = Editor.SelectedRoom.Position - (Editor.Mouse.World / 8);
					// check if the mouse is 8 pixels from the room's borders
					resizingX = resizingY = false;
					if(Math.Abs(Editor.Mouse.World.X / 8f - (Editor.SelectedRoom.Position.X + Editor.SelectedRoom.Width)) < 1)
						resizingX = true;
					if(Math.Abs(Editor.Mouse.World.Y / 8f - (Editor.SelectedRoom.Position.Y + Editor.SelectedRoom.Height)) < 1)
						resizingY = true;
					oldRoomBounds = Editor.SelectedRoom.Bounds;
				} else if(MInput.Mouse.CheckLeftButton) {
					Vector2 world = Editor.Mouse.World / 8;
					var offset = lastRoomOffset ?? Vector2.Zero;
					var newX = (int)(world + offset).X;
					var newY = (int)(world + offset).Y;
					var diff = new Vector2(newX - Editor.SelectedRoom.Bounds.X, newY - Editor.SelectedRoom.Bounds.Y);
					if(!resizingX && !resizingY) {
						Editor.SelectedRoom.Bounds.X = (int)(world + offset).X;
						Editor.SelectedRoom.Bounds.Y = (int)(world + offset).Y;
						foreach(var e in Editor.SelectedRoom.AllEntities) {
							e.Move(diff * 8);
							for(int i = 0; i < e.Nodes.Length; i++) {
								e.MoveNode(i, diff * 8);
							}
						}
					} else {
						if(resizingX) {
							newWidth = (int)Math.Ceiling(world.X - Editor.SelectedRoom.Bounds.X);
							Editor.SelectedRoom.Bounds.Width = Math.Max(newWidth, 1);
						}
						if(resizingY) {
							newHeight = (int)Math.Ceiling(world.Y - Editor.SelectedRoom.Bounds.Y);
							Editor.SelectedRoom.Bounds.Height = Math.Max(newHeight, 1);
						}
					}
				} else {
					lastRoomOffset = null;
					if(!oldRoomBounds.Equals(Editor.SelectedRoom.Bounds)) {
						oldRoomBounds = Editor.SelectedRoom.Bounds;
						Editor.SelectedRoom.UpdateBounds();
					}
					resizingX = resizingY = false;
					newWidth = newHeight = 0;
				}
			}

			if(MInput.Mouse.ReleasedLeftButton) {
				justSwitched = false;
			}

			// room creation
			if(canClick) {
				if(Editor.SelectedRoom == null) {
					if(MInput.Mouse.CheckLeftButton) {
						var lastPress = (Editor.GetCurrent().worldClick / 8).Ceiling() * 8;
						var mpos = (Editor.Mouse.World / 8).Ceiling() * 8;
						int ax = (int)Math.Min(mpos.X, lastPress.X);
						int ay = (int)Math.Min(mpos.Y, lastPress.Y);
						int bx = (int)Math.Max(mpos.X, lastPress.X);
						int by = (int)Math.Max(mpos.Y, lastPress.Y);
						var newRoom = new Rectangle(ax, ay, bx - ax, by - ay);
						if(newRoom.Width > 0 || newRoom.Height > 0) {
							newRoom.Width = Math.Max(newRoom.Width, 8);
							newRoom.Height = Math.Max(newRoom.Height, 8);
							if(!PendingRoom.HasValue)
								ScheduledRefresh = true;
							PendingRoom = newRoom;
						} else {
							ScheduledRefresh = true;
							PendingRoom = null;
						}
					}
				} else {
					if(PendingRoom.HasValue) {
						PendingRoom = null;
						ScheduledRefresh = true;
					}
				}
			}
		}

		public override void RenderWorldSpace() {
			base.RenderWorldSpace();
			if(PendingRoom.HasValue) {
				var prog = (float)Math.Abs(Math.Sin(Engine.Scene.TimeActive * 3));
				Draw.Rect(PendingRoom.Value, Color.Lerp(Color.White, Color.Cyan, prog) * 0.6f);
				Draw.HollowRect(PendingRoom.Value.X, PendingRoom.Value.Y, 40 * 8, 23 * 8, Color.Lerp(Color.Orange, Color.White, prog) * 0.6f);
			}
		}
	}

	public class PlacementTool : Tool {

		Placements.Placement curLeftSelection = null, curRightSelection = null;
		Dictionary<Placements.Placement, UIButton> placementButtons = new Dictionary<Placements.Placement, UIButton>();
		Entity preview = null;
		Vector2? lastPress = null;
		Placements.Placement lastPlacement = null;

		private static readonly Color LeftPlacementBtnBg = Calc.HexToColor("274292");
		private static readonly Color RightPlacementBtnBg = Calc.HexToColor("922727");
		private static readonly Color BothPlacementBtnBg = Calc.HexToColor("7d2792");

		public override UIElement CreatePanel() {
			placementButtons.Clear();
			var ret = new UIScrollPane();
			ret.Width = 180;
			ret.TopPadding = 10;
			foreach(var item in Placements.All.OrderBy(k => k.Name)) {
				UIButton b;
				ret.AddBelow(b = new UIButton(item.Name, Fonts.Regular, 4, 4) {
					OnPress = () => curLeftSelection = curLeftSelection != item ? item : null,
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
				// TODO: find lowest unoccupied ID
				int highestID = 0;
				foreach(var item in Editor.GetCurrent().Map.Rooms.SelectMany(k => k.AllEntities)) {
					if(item.EntityID > highestID)
						highestID = item.EntityID;
				}
				if(toAdd.Name != "player")
					toAdd.EntityID = highestID + 1;
				Editor.SelectedRoom.AddEntity(toAdd);
			}

			RefreshPreview(lastPlacement != selection);
			lastPlacement = selection;
			if(preview != null)
				UpdateEntity(preview, area);

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
			if((preview == null || changedPlacement) && selection != null) {
				preview = selection.Build(Editor.SelectedRoom);
			} else if(selection == null)
				preview = null;
		}

		private void UpdateEntity(Entity e, Rectangle area) {
			UpdateSize(e, area);
			var mpos = (MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl)) ? Editor.Mouse.World : (Editor.Mouse.World / 8).Round() * 8;
			if(lastPress != null)
				e.SetPosition(new Vector2(e.Width > 0 ? (area.Left / 8) * 8 : mpos.X, e.Height > 0 ? (area.Top / 8) * 8 : mpos.Y));
			else
				e.SetPosition(mpos);
			e.ResetNodes();
			while(e.Nodes.Length < e.MinNodes)
				e.AddNode((e.Nodes.Length > 0 ? e.Nodes.Last() : e.Position) + Vector2.UnitX * 24);
			e.ApplyDefaults();
			e.Initialize();
		}

		private void UpdateSize(Entity e, Rectangle area) {
			if(MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton || MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) {
				if(e.MinWidth > -1)
					e.SetWidth(Math.Max((int)Math.Ceiling(area.Width / 8f) * 8, e.MinWidth));
				if(e.MinHeight > -1)
					e.SetHeight(Math.Max((int)Math.Ceiling(area.Height / 8f) * 8, e.MinHeight));
			} else {
				e.SetWidth(e.MinWidth != -1 ? e.MinWidth : 0);
				e.SetHeight(e.MinWidth != -1 ? e.MinWidth : 0);
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