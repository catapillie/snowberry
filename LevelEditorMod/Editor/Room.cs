using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LevelEditorMod.Editor {

    using Element = BinaryPacker.Element;

    public class Room {
        public string Name { get; private set; }

        public Rectangle Bounds { get; private set; }

        public Map Map { get; private set; }

        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        // Music data
        public string Music { get; private set; }
        public string AltMusic { get; private set; }
        public string Ambience { get; private set; }
        public bool[] MusicLayers { get; private set; }

        public int MusicProgress { get; private set; }
        public int AmbienceProgress { get; private set; }

        // Camera offset data
        public Vector2 CameraOffset { get; private set; }

        // Misc data
        public bool Dark { get; private set; }
        public bool Underwater { get; private set; }
        public bool Space { get; private set; }
        public WindController.Patterns WindPattern { get; private set; }

        // Tiles
        private readonly VirtualMap<char> fgTileMap;
        private readonly VirtualMap<char> bgTileMap;
        private VirtualMap<MTexture> fgTiles, bgTiles;

        private readonly List<Decal> fgDecals = new List<Decal>();
        private readonly List<Decal> bgDecals = new List<Decal>();

        private readonly List<Entity> entities = new List<Entity>();
        private readonly List<Entity> triggers = new List<Entity>();
        private readonly List<Entity> allEntities = new List<Entity>();

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
                    entities.Add(e);
                    allEntities.Add(e);
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
                    allEntities.Add(t);
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

        internal List<EntitySelection> GetSelectedEntities(Rectangle rect) {
            List<EntitySelection> result = new List<EntitySelection>();

            foreach (Entity entity in allEntities) {
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

			foreach(var entity in entities) {
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

				foreach(var opt in entity.plugin.GetOptions())
                    entityElem.Attributes[opt] = entity.plugin[entity, opt];

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

            foreach(var tigger in triggers) {
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

                foreach(var opt in tigger.plugin.GetOptions())
                    triggersElem.Attributes[opt] = tigger.plugin[tigger, opt];

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

            string fgTiles = "";
			for(int x = 0; x < fgTileMap.Rows; x++) {
                for(int y = 0; y < fgTileMap.Columns; y++) {
                    fgTiles += /*((int)*/fgTileMap[y, x]/*).ToString()*/;
                    //if(y != fgTileMap.Rows - 1) fgTiles += ",";
                }
                fgTiles += "\n";
            }
            string bgTiles = "";
            for(int x = 0; x < bgTileMap.Rows; x++) {
                for(int y = 0; y < bgTileMap.Columns; y++) {
                    bgTiles += /*((int)*/bgTileMap[y, x]/*).ToString()*/;
                    //if(y != bgTileMap.Rows - 1) bgTiles += ",";
                }
                bgTiles += "\n";
            }

			Element fgElem = new Element();
            fgElem.Attributes = new Dictionary<string, object>();
            fgElem.Name = "solids";
            fgElem.Attributes["innerText"] = fgTiles;
            ret.Children.Add(fgElem);

			Element bgElem = new Element();
            bgElem.Attributes = new Dictionary<string, object>();
            bgElem.Name = "bg";
            bgElem.Attributes["innerText"] = bgTiles;
            ret.Children.Add(bgElem);

            return ret;
        }
    }
}
