using Celeste;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Monocle;

using MonoMod.Utils;

using Snowberry.Editor.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor.Tools {
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
            foreach (var item in keys)
                chars.Add(item);
            int i = 0;
            foreach (var item in entries) {
                var itemData = new DynamicData(item);
                var tilesData = new DynamicData(itemData.Get("Center"));
                string path = GFX.Game.Textures.FirstOrDefault(t => t.Value.Equals(tilesData.Get<List<MTexture>>("Textures")[0].GetParent())).Key ?? "Tileset of " + chars[i];
                paths.Add(path);
                i++;
            }
			List<TilesetData> ret = new List<TilesetData> {
				// not a "real" tileset
				new TilesetData('0', "air", bg)
			};
			for (int i1 = 0; i1 < chars.Count; i1++) {
                char item = chars[i1];
                ret.Add(new TilesetData(item, paths[i1], bg));
            }
            return ret;
        }

        public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_TILEBRUSH");
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
            var fgLabel = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_FOREGROUND"));
            fgLabel.Position = new Vector2((tilesetsPanel.Width - fgLabel.Width) / 2, 0);
            fgLabel.FG = Color.DarkKhaki;
            fgLabel.Underline = true;
            tilesetsPanel.AddBelow(fgLabel);
            int i = 0;
            foreach (var item in FgTilesets) {
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
            var bgLabel = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_BACKGROUND"));
            bgLabel.Position = new Vector2((tilesetsPanel.Width - bgLabel.Width) / 2, (int)Math.Ceiling(FgTilesets.Count / 2f) * (8 * 3 + 30) + fgLabel.Height + 40);
            bgLabel.FG = Color.DarkKhaki;
            bgLabel.Underline = true;
            tilesetsPanel.Add(bgLabel);
            i = 0;
            foreach (var item in BgTilesets) {
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
            foreach (var mode in Enum.GetValues(typeof(TileBrushMode))) {
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

            if (Editor.SelectedRoom == null)
                holoFgTileMap = holoBgTileMap = null;
            else if (holoFgTileMap == null || holoFgTileMap.Columns != Editor.SelectedRoom.Width || holoFgTileMap.Rows != Editor.SelectedRoom.Height || clear) {
                holoFgTileMap = new VirtualMap<char>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, '0');
                holoBgTileMap = new VirtualMap<char>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, '0');
                holoSetTiles = new VirtualMap<bool>(Editor.SelectedRoom.Width, Editor.SelectedRoom.Height, false);
                isPainting = false;
            }

            bool middlePan = Snowberry.Settings.MiddleClickPan;
            bool left = (MInput.Mouse.CheckLeftButton || (middlePan && MInput.Mouse.ReleasedLeftButton)) && (middlePan || !MInput.Keyboard.Check(Keys.LeftAlt));
            bool fg = left ? LeftFg : RightFg;
            int tileset = left ? CurLeftTileset : CurRightTileset;
            bool retile = false;

            if (canClick && (MInput.Mouse.PressedLeftButton || (middlePan && MInput.Mouse.PressedRightButton))) {
                isPainting = true;
            } else if (MInput.Mouse.ReleasedLeftButton || (middlePan && MInput.Mouse.ReleasedRightButton)) {
                if (Editor.SelectedRoom != null && canClick && isPainting)
                    for (int x = 0; x < holoFgTileMap.Columns; x++)
                        for (int y = 0; y < holoFgTileMap.Rows; y++)
                            if (fg) {
                                if (holoSetTiles[x, y])
                                    retile |= Editor.SelectedRoom.SetFgTile(x, y, holoFgTileMap[x, y]);
                            } else
                                if (holoSetTiles[x, y])
                                retile |= Editor.SelectedRoom.SetBgTile(x, y, holoBgTileMap[x, y]);
                if (retile) {
                    Editor.SelectedRoom.Autotile();
                }

                isPainting = false;
                holoFgTileMap = null;
                holoBgTileMap = null;
                holoGrid = null;
            } else if ((MInput.Mouse.CheckLeftButton || (middlePan && MInput.Mouse.CheckRightButton)) && Editor.SelectedRoom != null) {
                var tilePos = new Vector2((float)Math.Floor(Editor.Mouse.World.X / 8 - Editor.SelectedRoom.Position.X), (float)Math.Floor(Editor.Mouse.World.Y / 8 - Editor.SelectedRoom.Position.Y));
                int x = (int)tilePos.X; int y = (int)tilePos.Y;
                if (Editor.SelectedRoom.Bounds.Contains((int)(x + Editor.SelectedRoom.Position.X), (int)(y + Editor.SelectedRoom.Position.Y))) {
                    var lastPress = (Editor.Instance.worldClick / 8).Floor();
                    var roomLastPress = (Editor.Instance.worldClick / 8).Floor() - Editor.SelectedRoom.Position;
                    int ax = (int)Math.Min(x, roomLastPress.X);
                    int ay = (int)Math.Min(y, roomLastPress.Y);
                    int bx = (int)Math.Max(x, roomLastPress.X);
                    int by = (int)Math.Max(y, roomLastPress.Y);
                    var rect = new Rectangle(ax, ay, bx - ax, by - ay);
                    TileBrushMode mode = left ? LeftMode : RightMode;
                    switch (mode) {
                        case TileBrushMode.Brush:
                            SetHoloTile(fg, tileset, x, y);
                            break;
                        case TileBrushMode.HollowRect:
                        case TileBrushMode.Rect:
                            for (int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
                                for (int y2 = 0; y2 < holoFgTileMap.Rows; y2++) {
                                    bool set = rect.Contains(x2, y2);
                                    if (mode == TileBrushMode.HollowRect) {
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
                            if (!holoSetTiles[x, y]) {
                                char origTile = Editor.SelectedRoom.GetTile(fg, new Vector2((x + Editor.SelectedRoom.X) * 8, (y + Editor.SelectedRoom.Y) * 8));
                                bool inside(int cx, int cy) {
                                    return (cx >= 0 && cy >= 0 && cx < Editor.SelectedRoom.Width && cy < Editor.SelectedRoom.Height) && Editor.SelectedRoom.GetTile(fg, new Vector2((cx + Editor.SelectedRoom.X) * 8, (cy + Editor.SelectedRoom.Y) * 8)) == origTile;
                                }

                                Queue<Point> toCheck = new Queue<Point>();
                                void scan(int lx, int rx, int y) {
                                    bool added = false;
                                    for (int i = lx; i <= rx; i++) {
                                        if (!inside(i, y))
                                            added = false;
                                        else if (!added && !holoSetTiles[i, y]) {
                                            toCheck.Enqueue(new Point(i, y));
                                            added = true;
                                        }
                                    }
                                }
                                toCheck.Enqueue(new Point(x, y));
                                while (toCheck.Count > 0) {
                                    Point checking = toCheck.Dequeue();
                                    int x2 = checking.X, y2 = checking.Y;
                                    var lx = x2;
                                    while (inside(lx - 1, y2)) {
                                        SetHoloTile(fg, tileset, lx - 1, y2);
                                        lx--;
                                    }
                                    while (inside(x2, y2)) {
                                        SetHoloTile(fg, tileset, x2, y2);
                                        x2++;
                                    }
                                    scan(lx, x2 - 1, y2 + 1);
                                    scan(lx, x2 - 1, y2 - 1);
                                }
                            }

                            break;
                        case TileBrushMode.Line:
                            for (int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
                                for (int y2 = 0; y2 < holoFgTileMap.Rows; y2++)
                                    SetHoloTile(fg, 0, x2, y2, true);
                            if (roomLastPress.X - x == 0 && roomLastPress.Y - y == 0)
                                SetHoloTile(fg, tileset, x, y);
                            else if (Math.Abs(roomLastPress.X - x) > Math.Abs(roomLastPress.Y - y)) {
                                int sign = -Math.Sign(roomLastPress.X - x);
                                float grad = (roomLastPress.Y - y) / (roomLastPress.X - x);
                                for (int p = 0; p < rect.Width; p++) {
                                    SetHoloTile(fg, tileset, (int)(sign * p + roomLastPress.X), (int)(sign * p * grad + roomLastPress.Y));
                                }
                            } else {
                                int sign = -Math.Sign(roomLastPress.Y - y);
                                float grad = (roomLastPress.X - x) / (roomLastPress.Y - y);
                                for (int p = 0; p < rect.Height; p++) {
                                    SetHoloTile(fg, tileset, (int)(sign * p * grad + roomLastPress.X), (int)(sign * p + roomLastPress.Y));
                                }
                            }
                            break;
                        case TileBrushMode.Circle:
                            for (int x2 = 0; x2 < holoFgTileMap.Columns; x2++)
                                for (int y2 = 0; y2 < holoFgTileMap.Rows; y2++) {
                                    bool set = ((lastPress.X - x2) * (lastPress.X - x2) + (lastPress.Y - y2) * (lastPress.Y - y2)) < (rect.Width * rect.Width + rect.Height * rect.Height) / Math.Sqrt(2);
                                    SetHoloTile(fg, set ? tileset : 0, x2, y2, !set);
                                }
                            break;
                        default:
                            break;
                    }
                    if (holoRetile) {
                        holoRetile = false;
                        holoGrid = (fg ? GFX.FGAutotiler : GFX.BGAutotiler).GenerateMap(fg ? holoFgTileMap : holoBgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid;
                    }
                }
            }

            // TODO: cleanup a bit
            for (int i = 0; i < fgTilesetButtons.Count; i++) {
                UIButton button = fgTilesetButtons[i];
                if (LeftFg && RightFg && CurLeftTileset == CurRightTileset && CurLeftTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = BothSelectedBtnBg;
                else if (LeftFg && CurLeftTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = LeftSelectedBtnBg;
                else if (RightFg && CurRightTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = RightSelectedBtnBg;
                else {
                    button.ResetBgColors();
                }
            }
            for (int i = 0; i < bgTilesetButtons.Count; i++) {
                UIButton button = bgTilesetButtons[i];
                if (!LeftFg && !RightFg && CurLeftTileset == CurRightTileset && CurLeftTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = BothSelectedBtnBg;
                else if (!LeftFg && CurLeftTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = LeftSelectedBtnBg;
                else if (!RightFg && CurRightTileset == i)
                    button.BG = button.PressedBG = button.HoveredBG = RightSelectedBtnBg;
                else {
                    button.ResetBgColors();
				}
            }
            for (int i = 0; i < modeButtons.Count; i++) {
                UIButton button = modeButtons[i];
                if (LeftMode == RightMode && RightMode == (TileBrushMode)i)
                    button.BG = button.PressedBG = button.HoveredBG = BothSelectedBtnBg;
                else if (LeftMode == (TileBrushMode)i)
                    button.BG = button.PressedBG = button.HoveredBG = LeftSelectedBtnBg;
                else if (RightMode == (TileBrushMode)i)
                    button.BG = button.PressedBG = button.HoveredBG = RightSelectedBtnBg;
                else {
                    button.ResetBgColors();
                }
            }
        }

        public void SetHoloTile(bool fg, int tileset, int x, int y, bool unset = false) {
            char tile = fg ? FgTilesets[tileset].Key : BgTilesets[tileset].Key;
            VirtualMap<char> tiles = fg ? holoFgTileMap : holoBgTileMap;
            char prev = tiles[x, y];
            bool reset = !holoSetTiles[x, y] || (holoSetTiles[x, y] && unset);
            if (prev != tile || reset) {
                tiles[x, y] = tile;
                holoSetTiles[x, y] = !unset;
                holoRetile = holoRetile || prev != tile;
            }
        }

        public override void RenderWorldSpace() {
            base.RenderWorldSpace();
            TileGrid tile = LeftFg ? FgTilesets[CurLeftTileset].Tile : BgTilesets[CurLeftTileset].Tile;
            var tilePos = new Vector2((float)Math.Floor(Editor.Mouse.World.X / 8) * 8, (float)Math.Floor(Editor.Mouse.World.Y / 8) * 8);
            if (isPainting && Editor.SelectedRoom != null) {
                var fg = MInput.Mouse.CheckLeftButton ? LeftFg : RightFg;
                var map = fg ? holoFgTileMap : holoBgTileMap;
                Vector2 p = Editor.SelectedRoom.Position * 8;
                RenderTileGrid(p, holoGrid, Color.White * 0.75f);
                var prog = (float)Math.Abs(Math.Sin(Engine.Scene.TimeActive * 3));
                for (int x = 0; x < map.Columns; x++)
                    for (int y = 0; y < map.Rows; y++)
                        if (holoSetTiles[x, y] && map[x, y] == '0')
                            Draw.Rect(p.X + x * 8, p.Y + y * 8, 8, 8, Color.Red * (prog / 3f + 0.35f));
            } else
                RenderTileGrid(tilePos, tile, Color.White * 0.5f);
        }

        public static void RenderTileGrid(Vector2 position, TileGrid tile, Color color) {
            if (tile == null)
                return;
            for (int x = 0; x < tile.Tiles.Columns; x++)
                for (int y = 0; y < tile.Tiles.Rows; y++)
                    if (tile.Tiles[x, y] != null)
                        tile.Tiles[x, y].Draw(position + new Vector2(x, y) * 8, Vector2.Zero, color);
        }
    }
}