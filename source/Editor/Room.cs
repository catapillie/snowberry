using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using Snowberry.Editor.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Snowberry.Editor {

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

        public Rectangle ScissorRect { get; private set; }

        // Music data
        public string Music = "";
        public string AltMusic = "";
        public string Ambience = "";
        public bool[] MusicLayers = new bool[4];

        public int MusicProgress;
        public int AmbienceProgress;

        // Camera offset data
        public Vector2 CameraOffset;

        // Misc data
        public bool Dark;
        public bool Underwater;
        public bool Space;
        public WindController.Patterns WindPattern = WindController.Patterns.None;

        // Tiles
        private VirtualMap<char> fgTileMap;
        private VirtualMap<char> bgTileMap;
        private VirtualMap<MTexture> fgTiles, bgTiles;

        public readonly List<Decal> FgDecals = new List<Decal>();
        public readonly List<Decal> BgDecals = new List<Decal>();

        public readonly List<Entity> Entities = new List<Entity>();
        public readonly List<Entity> Triggers = new List<Entity>();
        public readonly List<Entity> AllEntities = new List<Entity>();

        public readonly Dictionary<Type, List<Entity>> TrackedEntities = new Dictionary<Type, List<Entity>>();
        public readonly Dictionary<Type, bool> DirtyTrackedEntities = new Dictionary<Type, bool>();

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
            Autotile();
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
                BgDecals.Add(new Decal(this, decal));
            }

            // FgDecals
            foreach (DecalData decal in data.FgDecals) {
                FgDecals.Add(new Decal(this, decal));
            }

            // Entities
            foreach (EntityData entity in data.Entities) {
                if (Entity.TryCreate(this, entity, out Entity e)) {
                    AddEntity(e);
                } else
                    Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown entity ('{entity.Name}')");
            }

            // Player Spawnpoints (excluded from LevelData.Entities)
            foreach (Vector2 spawn in data.Spawns) {
                var spawnEntity = Entity.Create("player", this).SetPosition(spawn);
                AddEntity(spawnEntity);
            }

            // Triggers
            foreach (EntityData trigger in data.Triggers) {
                if (Entity.TryCreate(this, trigger, out Entity t)) {
                    AddEntity(t);
                } else
                    Snowberry.Log(LogLevel.Warn, $"Attempted to load unknown trigger ('{trigger.Name}')");
            }
        }

        public char GetTile(bool fg, Vector2 at) {
            return fg ? GetFgTile(at) : GetBgTile(at);
        }

        public char GetFgTile(Vector2 at) {
            Vector2 p = (at - Position * 8) / 8;
            return fgTileMap[(int)p.X, (int)p.Y];
        }

        public char GetBgTile(Vector2 at) {
            Vector2 p = (at - Position * 8) / 8;
            return bgTileMap[(int)p.X, (int)p.Y];
        }

        public bool SetFgTile(int x, int y, char tile) {
            char orig = fgTileMap[x, y];
            if (orig != tile) {
                fgTileMap[x, y] = tile;
                return true;
            }
            return false;
        }

        public bool SetBgTile(int x, int y, char tile) {
            char orig = bgTileMap[x, y];
            if (orig != tile) {
                bgTileMap[x, y] = tile;
                return true;
            }
            return false;
        }

        public void Autotile() {
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

        internal void CalculateScissorRect(Editor.BufferCamera camera) {
            Vector2 offset = Position * 8;

            Vector2 zero = Calc.Round(Vector2.Transform(offset, camera.Matrix));
            Vector2 size = Calc.Round(Vector2.Transform(offset + new Vector2(Width * 8, Height * 8), camera.Matrix) - zero);
            ScissorRect = new Rectangle(
                (int)zero.X, (int)zero.Y,
                (int)size.X, (int)size.Y);
        }

        internal void Render(Rectangle viewRect) {
            Vector2 offset = Position * 8;

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
            foreach (Decal decal in BgDecals)
                decal.Render(offset);

            // Entities
            foreach (Entity entity in Entities) {
                Calc.PushRandom(entity.GetHashCode());
                entity.RenderBefore();
                Calc.PopRandom();
            }

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
            foreach (Decal decal in FgDecals)
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

            DirtyTrackedEntities.Clear();
        }

        internal void HQRender(Matrix m) {
            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = ScissorRect;

            // Entities
            foreach (Entity entity in Entities)
                entity.HQRender();
            // Triggers
            foreach (Entity trigger in Triggers)
                trigger.HQRender();
        }

        public void UpdateBounds() {
            var newFgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
            for (int x = 0; x < fgTileMap.Columns; x++)
                for (int y = 0; y < fgTileMap.Rows; y++)
                    newFgTiles[x, y] = fgTileMap[x, y];
            fgTileMap = newFgTiles;

            var newBgTiles = new VirtualMap<char>(Bounds.Width, Bounds.Height, '0');
            for (int x = 0; x < bgTileMap.Columns; x++)
                for (int y = 0; y < bgTileMap.Rows; y++)
                    newBgTiles[x, y] = bgTileMap[x, y];
            bgTileMap = newBgTiles;

            Autotile();
        }

        public Element CreateLevelData() {
			Element ret = new Element {
				Attributes = new Dictionary<string, object> {
					["name"] = "lvl_" + Name,
					["x"] = X * 8,
					["y"] = Y * 8,
					["width"] = Width * 8,
					["height"] = Height * 8,

					["music"] = Music,
					["alt_music"] = AltMusic,
					["ambience"] = Ambience,
					["musicLayer1"] = MusicLayers[0],
					["musicLayer2"] = MusicLayers[1],
					["musicLayer3"] = MusicLayers[2],
					["musicLayer4"] = MusicLayers[3],

					["musicProgress"] = MusicProgress,
					["ambienceProgress"] = AmbienceProgress,

					["dark"] = Dark,
					["underwater"] = Underwater,
					["space"] = Space,
					["windPattern"] = WindPattern.ToString(),

					["cameraOffsetX"] = CameraOffset.X,
					["cameraOffsetY"] = CameraOffset.Y
				}
			};

			Element entitiesElement = new Element {
				Attributes = new Dictionary<string, object>(),
				Name = "entities",
				Children = new List<Element>()
			};
			ret.Children = new List<Element> {
				entitiesElement
			};

			foreach (var entity in Entities) {
                try {
                    Element entityElem = new Element {
                        Name = entity.Name,
                        Children = new List<Element>(),
                        Attributes = new Dictionary<string, object> {
                            ["id"] = entity.EntityID,
                            ["x"] = entity.X - X * 8,
                            ["y"] = entity.Y - Y * 8,
                            ["width"] = entity.Width,
                            ["height"] = entity.Height,
                            ["originX"] = entity.Origin.X,
                            ["originY"] = entity.Origin.Y
                        }
                    };

                    foreach(var opt in entity.Info.Options.Keys) {
                        var val = entity.Get(opt);
                        // check that we don't overwrite any of the above (e.g. from a LuaEntity)
                        if(val != null && !(opt.Equals("id") || opt.Equals("x") || opt.Equals("y") || opt.Equals("width") || opt.Equals("height") || opt.Equals("originX") || opt.Equals("originY")))
                            entityElem.Attributes[opt] = val;
                    }

                    foreach(var node in entity.Nodes) {
                        Element n = new Element {
                            Attributes = new Dictionary<string, object> {
                                ["x"] = node.X - X * 8,
                                ["y"] = node.Y - Y * 8
                            }
                        };
                        entityElem.Children.Add(n);
                    }

                    entitiesElement.Children.Add(entityElem);
                } catch(Exception e) {
                    Snowberry.Log(LogLevel.Error, $"Could not create instance of entity {entity.Name}: {e}");
                }
            }

			Element triggersElement = new Element {
				Attributes = new Dictionary<string, object>(),
				Name = "triggers",
				Children = new List<Element>()
			};
			ret.Children.Add(triggersElement);

            foreach (var tigger in Triggers) {
				Element triggersElem = new Element {
					Name = tigger.Name,
					Children = new List<Element>(),
					Attributes = new Dictionary<string, object> {
						["x"] = tigger.X - X * 8,
						["y"] = tigger.Y - Y * 8,
						["width"] = tigger.Width,
						["height"] = tigger.Height,
						["originX"] = tigger.Origin.X,
						["originY"] = tigger.Origin.Y
					}
				};

				foreach (var opt in tigger.Info.Options.Keys) {
                    var val = tigger.Get(opt);
                    if (val != null)
                        triggersElem.Attributes[opt] = val;
                }

                foreach (var node in tigger.Nodes) {
					Element n = new Element {
						Attributes = new Dictionary<string, object> {
							["x"] = node.X - X * 8,
							["y"] = node.Y - Y * 8
						}
					};
					triggersElem.Children.Add(n);
                }

                triggersElement.Children.Add(triggersElem);
            }

            Element fgDecalsElem = new Element();
            fgDecalsElem.Name = "fgdecals";
            fgDecalsElem.Children = new List<Element>();
            ret.Children.Add(fgDecalsElem);
            foreach (var decal in FgDecals) {
				Element decalElem = new Element {
					Attributes = new Dictionary<string, object> {
						["x"] = decal.Position.X,
						["y"] = decal.Position.Y,
						["scaleX"] = decal.Scale.X,
						["scaleY"] = decal.Scale.Y,
						["texture"] = decal.Texture
					}
				};
				fgDecalsElem.Children.Add(decalElem);
            }

            Element bgDecalsElem = new Element();
            bgDecalsElem.Name = "bgdecals";
            bgDecalsElem.Children = new List<Element>();
            ret.Children.Add(bgDecalsElem);
            foreach (var decal in BgDecals) {
				Element decalElem = new Element {
					Attributes = new Dictionary<string, object> {
						["x"] = decal.Position.X,
						["y"] = decal.Position.Y,
						["scaleX"] = decal.Scale.X,
						["scaleY"] = decal.Scale.Y,
						["texture"] = decal.Texture
					}
				};
				bgDecalsElem.Children.Add(decalElem);
            }

            StringBuilder fgTiles = new StringBuilder();
            for (int y = 0; y < fgTileMap.Rows; y++) {
                for (int x = 0; x < fgTileMap.Columns; x++) {
                    fgTiles.Append(fgTileMap[x, y]);
                }
                fgTiles.Append("\n");
            }
            StringBuilder bgTiles = new StringBuilder();
            for (int y = 0; y < bgTileMap.Rows; y++) {
                for (int x = 0; x < bgTileMap.Columns; x++) {
                    bgTiles.Append(bgTileMap[x, y]);
                }
                bgTiles.Append("\n");
            }

			Element fgSolidsElem = new Element {
				Name = "solids",
				Attributes = new Dictionary<string, object> {
					["innerText"] = fgTiles.ToString()
				}
			};
			ret.Children.Add(fgSolidsElem);

			Element bgSolidsElem = new Element {
				Name = "bg",
				Attributes = new Dictionary<string, object> {
					["innerText"] = bgTiles.ToString()
				}
			};
			ret.Children.Add(bgSolidsElem);

            return ret;
        }

        public void AddEntity(Entity e) {
            AllEntities.Add(e);
            if (e is Plugin_Trigger)
                Triggers.Add(e);
            else
                Entities.Add(e);
            if (e.Tracked) {
                Type tracking = e.GetType();
                if (!TrackedEntities.ContainsKey(tracking))
                    TrackedEntities[tracking] = new List<Entity>();
                TrackedEntities[tracking].Add(e);
            }
        }

        public void RemoveEntity(Entity e) {
            AllEntities.Remove(e);
            Entities.Remove(e);
            Triggers.Remove(e);
            Type tracking = e.GetType();
            if (e.Tracked && TrackedEntities.ContainsKey(tracking)) {
                TrackedEntities[tracking].Remove(e);
                if (TrackedEntities[tracking].Count == 0)
                    TrackedEntities.Remove(tracking);
            }
        }

        public void MarkTrackedEntityDirty(Entity e) {
            if (e.Tracked) {
                DirtyTrackedEntities[e.GetType()] = true;
            }
        }
    }
}
