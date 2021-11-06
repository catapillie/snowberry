using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LevelEditorMod.Editor {

    using Element = BinaryPacker.Element;

    public class Room {
        public string Name;

        public Rectangle Bounds;

        public Map Map { get; private set; }

        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        // Music data
        public string Music;
        public string AltMusic;
        public string Ambience;
        public bool[] MusicLayers;

        public int MusicProgress;
        public int AmbienceProgress;

        // Camera offset data
        public Vector2 CameraOffset;

        // Misc data
        public bool Dark;
        public bool Underwater;
        public bool Space;
        public WindController.Patterns WindPattern;

        // Tiles
        private readonly VirtualMap<char> fgTileMap;
        private readonly VirtualMap<char> bgTileMap;
        private VirtualMap<MTexture> fgTiles, bgTiles;

        private readonly List<Decal> fgDecals = new List<Decal>();
        private readonly List<Decal> bgDecals = new List<Decal>();

        public readonly List<Entity> Entities = new List<Entity>();
        public readonly List<Entity> Triggers = new List<Entity>();
        public readonly List<Entity> AllEntities = new List<Entity>();

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

        internal Room(LevelData data, Map map)
            : this(data.Name, data.TileBounds) {
            Map = map;

            // Music
            Music = data.Music;
            AltMusic = data.AltMusic;
            Ambience = data.Ambience;

            MusicLayers = new bool[4];
            MusicLayers[0] = data.MusicLayers[0] > 0;
            MusicLayers[1] = data.MusicLayers[1] > 0;
            MusicLayers[2] = data.MusicLayers[2] > 0;
            MusicLayers[3] = data.MusicLayers[3] > 0;

            MusicProgress = data.MusicProgress;
            AmbienceProgress = data.AmbienceProgress;

            // Camera
            CameraOffset = data.CameraOffset;

            // Misc
            Dark = data.Dark;
            Underwater = data.Underwater;
            Space = data.Space;
            WindPattern = data.WindPattern;

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
                    Entities.Add(e);
                    AllEntities.Add(e);
                } else
                    Module.Log(LogLevel.Warn, $"Attempted to load unknown entity ('{entity.Name}')");
            }

            // Player Spawnpoints (excluded from LevelData.Entities)
            foreach (Vector2 spawn in data.Spawns) {
                var spawnEntity = Entity.Create("player", this).SetPosition(spawn);
                Entities.Add(spawnEntity);
                AllEntities.Add(spawnEntity);
            }

            // Triggers
            foreach (EntityData trigger in data.Triggers) {
                if (Entity.TryCreate(this, trigger, out Entity t)) {
                    Triggers.Add(t);
                    AllEntities.Add(t);
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

        public void SetFgTile(Vector2 at, char tile) {
            Vector2 p = (at - Position * 8) / 8;
            char orig = fgTileMap[(int)p.X, (int)p.Y];
            if(orig != tile) {
                fgTileMap[(int)p.X, (int)p.Y] = tile;
                Autotile();
            }
        }

        public void SetBgTile(Vector2 at, char tile) {
            Vector2 p = (at - Position * 8) / 8;
            char orig = bgTileMap[(int)p.X, (int)p.Y];
            if(orig != tile) {
                bgTileMap[(int)p.X, (int)p.Y] = tile;
                Autotile();
            }
        }

        private void Autotile() {
            fgTiles = GFX.FGAutotiler.GenerateMap(fgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
            bgTiles = GFX.BGAutotiler.GenerateMap(bgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
        }

        internal List<EntitySelection> GetSelectedEntities(Rectangle rect) {
            List<EntitySelection> result = new List<EntitySelection>();

            foreach (Entity entity in AllEntities) {
                var rects = entity.SelectionRectangles;
                if (rects != null && rects.Length > 0) {
                    List<EntitySelection.Selection> selection = new List<EntitySelection.Selection>();
                    bool wasSelected = false;
                    for (int i = 0; i < rects.Length; i++) {
                        Rectangle r = rects[i];
                        if (rect.Intersects(r)) {
                            selection.Add(new EntitySelection.Selection(r, i - 1));
                            wasSelected = true;
                        }
                    }
                    if (wasSelected)
                        result.Add(new EntitySelection(entity, selection));
                }
            }

            return result;
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
            foreach (Entity entity in Entities) {
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
            foreach (Entity trigger in Triggers)
                trigger.Render();

            if (this == Editor.SelectedRoom) {
                if (Editor.Selection.HasValue)
                    Draw.Rect(Editor.Selection.Value, Color.Blue * 0.25f);
                if (Editor.SelectedEntities != null) {
                    foreach (EntitySelection s in Editor.SelectedEntities) {
                        foreach (EntitySelection.Selection selection in s.Selections) {
                            Draw.Rect(selection.Rect, Color.Blue * 0.25f);
                        }
                    }
                }
            } else
                Draw.Rect(offset, Width * 8, Height * 8, Color.Black * 0.5f);
        }

        public Element CreateLevelData() {
			Element ret = new Element();
            ret.Attributes = new Dictionary<string, object>();

            ret.Attributes["name"] = "lvl_" + Name;
            ret.Attributes["x"] = X * 8;
            ret.Attributes["y"] = Y * 8;
            ret.Attributes["width"] = Width * 8;
            ret.Attributes["height"] = Height * 8;

            ret.Attributes["music"] = Music;
            ret.Attributes["alt_music"] = AltMusic;
            ret.Attributes["ambience"] = Ambience;
            ret.Attributes["musicLayer1"] = MusicLayers[0];
            ret.Attributes["musicLayer2"] = MusicLayers[1];
            ret.Attributes["musicLayer3"] = MusicLayers[2];
            ret.Attributes["musicLayer4"] = MusicLayers[3];

            ret.Attributes["musicProgress"] = MusicProgress;
            ret.Attributes["ambienceProgress"] = AmbienceProgress;

            ret.Attributes["dark"] = Dark;
            ret.Attributes["underwater"] = Underwater;
            ret.Attributes["space"] = Space;
            ret.Attributes["windPattern"] = WindPattern.ToString();

            ret.Attributes["cameraOffsetX"] = CameraOffset.X;
            ret.Attributes["cameraOffsetY"] = CameraOffset.Y;

			Element entitiesElement = new Element();
            entitiesElement.Attributes = new Dictionary<string, object>();
            entitiesElement.Name = "entities";
            entitiesElement.Children = new List<Element>();
            ret.Children = new List<Element>();
            ret.Children.Add(entitiesElement);

			foreach(var entity in Entities) {
				Element entityElem = new Element();
                entityElem.Name = entity.Name;
                entityElem.Children = new List<Element>();
                entityElem.Attributes = new Dictionary<string, object>();
                entityElem.Attributes["x"] = entity.X - X * 8;
                entityElem.Attributes["y"] = entity.Y - Y * 8;
                entityElem.Attributes["width"] = entity.Width;
                entityElem.Attributes["height"] = entity.Height;
                entityElem.Attributes["originX"] = entity.Origin.X;
                entityElem.Attributes["originY"] = entity.Origin.Y;

				foreach(var opt in entity.Plugin.GetOptions())
                    entityElem.Attributes[opt] = entity.Plugin[entity, opt];

				foreach(var node in entity.Nodes) {
					Element n = new Element();
                    n.Attributes = new Dictionary<string, object>();
                    n.Attributes["x"] = node.X - X * 8;
                    n.Attributes["y"] = node.Y - Y * 8;
                    entityElem.Children.Add(n);
                }

                entitiesElement.Children.Add(entityElem);
            }

			Element triggersElement = new Element();
            triggersElement.Attributes = new Dictionary<string, object>();
            triggersElement.Name = "triggers";
            triggersElement.Children = new List<Element>();
            ret.Children.Add(triggersElement);

            foreach(var tigger in Triggers) {
				Element triggersElem = new Element();
                triggersElem.Name = tigger.Name;
                triggersElem.Children = new List<Element>();
                triggersElem.Attributes = new Dictionary<string, object>();
                triggersElem.Attributes["x"] = tigger.X - X * 8;
                triggersElem.Attributes["y"] = tigger.Y - Y * 8;
                triggersElem.Attributes["width"] = tigger.Width;
                triggersElem.Attributes["height"] = tigger.Height;
                triggersElem.Attributes["originX"] = tigger.Origin.X;
                triggersElem.Attributes["originY"] = tigger.Origin.Y;

                foreach(var opt in tigger.Plugin.GetOptions())
                    triggersElem.Attributes[opt] = tigger.Plugin[tigger, opt];

                foreach(var node in tigger.Nodes) {
					Element n = new Element();
                    n.Attributes = new Dictionary<string, object>();
                    n.Attributes["x"] = node.X - X * 8;
                    n.Attributes["y"] = node.Y - Y * 8;
                    triggersElem.Children.Add(n);
                }

                triggersElement.Children.Add(triggersElem);
            }

			Element fgDecalsElem = new Element();
            fgDecalsElem.Name = "fgdecals";
            fgDecalsElem.Children = new List<Element>();
            ret.Children.Add(fgDecalsElem);
			foreach(var decal in fgDecals) {
                Element decalElem = new Element();
                decalElem.Attributes = new Dictionary<string, object>();
                decalElem.Attributes["x"] = decal.position.X;
                decalElem.Attributes["y"] = decal.position.Y;
                decalElem.Attributes["scaleX"] = decal.scale.X;
                decalElem.Attributes["scaleY"] = decal.scale.Y;
                decalElem.Attributes["texture"] = decal.Texture;
                fgDecalsElem.Children.Add(decalElem);
            }

            Element bgDecalsElem = new Element();
            bgDecalsElem.Name = "bgdecals";
            bgDecalsElem.Children = new List<Element>();
            ret.Children.Add(bgDecalsElem);
            foreach(var decal in bgDecals) {
                Element decalElem = new Element();
                decalElem.Attributes = new Dictionary<string, object>();
                decalElem.Attributes["x"] = decal.position.X;
                decalElem.Attributes["y"] = decal.position.Y;
                decalElem.Attributes["scaleX"] = decal.scale.X;
                decalElem.Attributes["scaleY"] = decal.scale.Y;
                decalElem.Attributes["texture"] = decal.Texture;
                bgDecalsElem.Children.Add(decalElem);
            }

            StringBuilder fgTiles = new StringBuilder();
			for(int y = 0; y < fgTileMap.Rows; y++) {
                for(int x = 0; x < fgTileMap.Columns; x++) {
                    fgTiles.Append(fgTileMap[x, y]);
                }
                fgTiles.Append("\n");
            }
            StringBuilder bgTiles = new StringBuilder();
            for(int y = 0; y < bgTileMap.Rows; y++) {
                for(int x = 0; x < bgTileMap.Columns; x++) {
                    bgTiles.Append(bgTileMap[x, y]);
                }
                bgTiles.Append("\n");
            }

            Element fgElem = new Element();
            fgElem.Attributes = new Dictionary<string, object>();
            fgElem.Name = "solids";
            fgElem.Attributes["innerText"] = fgTiles.ToString();
            ret.Children.Add(fgElem);

			Element bgElem = new Element();
            bgElem.Attributes = new Dictionary<string, object>();
            bgElem.Name = "bg";
            bgElem.Attributes["innerText"] = bgTiles.ToString();
            ret.Children.Add(bgElem);

            return ret;
        }
    }
}
