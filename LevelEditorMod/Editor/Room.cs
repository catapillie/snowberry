using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LevelEditorMod.Editor {
    public class Room {
        public string Name { get; private set; }

        public Rectangle Bounds { get; private set; }

        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        private readonly VirtualMap<char> fgTileMap;
        private readonly VirtualMap<char> bgTileMap;
        private VirtualMap<MTexture> fgTiles, bgTiles;

        private readonly List<Decal> fgDecals = new List<Decal>();
        private readonly List<Decal> bgDecals = new List<Decal>();

        private readonly List<Entity> entities = new List<Entity>();
        private readonly List<Entity> triggers = new List<Entity>();

        private readonly Dictionary<Entity, Selection> SelectionByEntity = new Dictionary<Entity, Selection>();

        public int LoadSeed {
            get {
                int num = 0;
                string name = Name;
                foreach (char c in name) {
                    num += c;
                }
                return num;
            }
        }

        private static readonly Regex tileSplitter = new Regex("\\r\\n|\\n\\r|\\n|\\r");

        internal Room(string name, Rectangle bounds) {
            Name = name;
            Bounds = bounds;
            fgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
            bgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
        }

        internal Room(LevelData data)
            : this(data.Name, data.TileBounds) {
            // BgTiles
            string[] array = tileSplitter.Split(data.Bg);
            for (int i = 0; i < array.Length; i++) {
                for (int j = 0; j < array[i].Length; j++) {
                    bgTileMap[j, i] = array[i][j];
                }
            }

            // FgTiles
            string[] array2 = tileSplitter.Split(data.Solids);
            for (int i = 0; i < array2.Length; i++) {
                for (int j = 0; j < array2[i].Length; j++) {
                    fgTileMap[j, i] = array2[i][j];
                }
            }

            Autotile();

            // BgDecals
            foreach (DecalData decal in data.BgDecals) {
                bgDecals.Add(new Decal(this, decal));
            }

            // FgDecals
            foreach (DecalData decal in data.FgDecals) {
                fgDecals.Add(new Decal(this, decal));
            }

            // Entities
            foreach (EntityData entity in data.Entities) {
                if (Entity.TryCreate(this, entity, out Entity e)) {
                    entities.Add(e);
                    SelectionByEntity.Add(e, e.Select());
                } else
                    Module.Log(LogLevel.Warn, $"Attempted to load unknown entity ('{entity.Name}')");
            }

            // Player Spawnpoints (excluded from LevelData.Entities)
            foreach (Vector2 spawn in data.Spawns) {
                entities.Add(Entity.Create("player", this).SetPosition(spawn));
            }

            // Triggers
            foreach (EntityData trigger in data.Triggers) {
                if (Entity.TryCreate(this, trigger, out Entity t)) {
                    triggers.Add(t);
                    SelectionByEntity.Add(t, t.Select());
                } else
                    Module.Log(LogLevel.Warn, $"Attempted to load unknown trigger ('{trigger.Name}')");
            }
        }

        public char GetFgTile(Vector2 at) {
            Vector2 p = (at - Position * 8) / 8;
            return fgTileMap[(int)p.X, (int)p.Y];
        }

        public char GetBgTile(Vector2 at) {
            Vector2 p = (at - Position * 8) / 8;
            return bgTileMap[(int)p.X, (int)p.Y];
        }

        private void Autotile() {
            fgTiles = GFX.FGAutotiler.GenerateMap(fgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
            bgTiles = GFX.BGAutotiler.GenerateMap(bgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
        }

        internal KeyValuePair<Entity, Selection>[] GetSelectedEntities(Rectangle rect) {
            List<KeyValuePair<Entity, Selection>> selected = new List<KeyValuePair<Entity, Selection>>();
            foreach (KeyValuePair<Entity, Selection> s in SelectionByEntity) {
                Selection selection = s.Value;
                if (selection.Main.HasValue) {
                    if (rect.Intersects(selection.Main.Value)) {
                        selected.Add(s);
                    } else if (selection.Nodes != null) {
                        foreach (Rectangle r in selection.Nodes)
                            if (rect.Intersects(r))
                                selected.Add(s);
                    }
                }
            }
            return selected.ToArray();
        }

        internal void Render(Rectangle viewRect, Editor.Camera camera) {
            Vector2 offset = Position * 8;

            Vector2 zero = Calc.Round(Vector2.Transform(offset, camera.Matrix));
            Vector2 size = Calc.Round(Vector2.Transform(offset + new Vector2(Width * 8, Height * 8), camera.Matrix) - zero);
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(
                (int)zero.X, (int)zero.Y,
                (int)size.X, (int)size.Y);

            Draw.Rect(offset, Width * 8, Height * 8, Color.White * 0.1f);

            int startX = Math.Max(0, (viewRect.Left - X * 8) / 8);
            int startY = Math.Max(0, (viewRect.Top - Y * 8) / 8);
            int endX = Math.Min(Width, Width + (viewRect.Right - (X + Width) * 8) / 8);
            int endY = Math.Min(Height, Height + (viewRect.Bottom - (Y + Height) * 8) / 8);

            // BgTiles
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    if (bgTiles[x, y] != null)
                        bgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // BgDecals
            foreach (Decal decal in bgDecals)
                decal.Render(offset);

            // Entities
            foreach (Entity entity in entities) {
                Calc.PushRandom(entity.GetHashCode());
                entity.Render();
                Calc.PopRandom();
            }

            // FgTiles
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    if (fgTiles[x, y] != null)
                        fgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // FgDecals
            foreach (Decal decal in fgDecals)
                decal.Render(offset);

            // Triggers
            foreach (Entity trigger in triggers)
                trigger.Render();

            if (this == Editor.SelectedRoom) {
                if (Editor.Selection.HasValue)
                    Draw.Rect(Editor.Selection.Value, Color.Blue * 0.25f);
                if (Editor.SelectedEntities != null) {
                    foreach (var s in Editor.SelectedEntities) {
                        Draw.Rect(s.Value.Main.Value, Color.Blue * 0.25f);
                        if (s.Value.Nodes != null)
                            foreach (Rectangle r in s.Value.Nodes)
                                Draw.Rect(r, Color.Blue * 0.25f);
                    }
                }
            } else
                Draw.Rect(offset, Width * 8, Height * 8, Color.Black * 0.5f);
        }
    }
}
