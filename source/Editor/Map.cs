using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Snowberry.Editor {

    using Element = BinaryPacker.Element;

    public class Map {

        public readonly string Name;
        public readonly AreaKey From;

        public readonly List<Room> Rooms = new List<Room>();
        public readonly List<Rectangle> Fillers = new List<Rectangle>();

        public readonly List<Styleground> FGStylegrounds = new();
        public readonly List<Styleground> BGStylegrounds = new();

        internal Map(string name) {
            Name = name;

            var playtestData = AreaData.Get("Snowberry/Playtest");
            //Editor.EmptyMapMeta(playtestData);
            AreaKey playtestKey = playtestData.ToKey();
            From = playtestKey;
        }

        internal Map(MapData data)
            : this(data.Filename) {
            foreach (LevelData roomData in data.Levels)
                Rooms.Add(new Room(roomData, this));
            foreach (Rectangle filler in data.Filler)
                Fillers.Add(filler);
            var playtestData = AreaData.Get("Snowberry/Playtest");
            var targetData = AreaData.Get(data.Area);
            AreaKey playtestKey = playtestData.ToKey();
            From = playtestKey;
            Editor.CopyMapMeta(targetData, playtestData);

			// load stylegrounds
			if(data.Foreground != null && data.Foreground.Children != null) {
				foreach(var item in data.Foreground.Children) {
                    string name = item.Name.ToLowerInvariant();

                    if(name.Equals("apply")) {
						if(item.Children != null) {
							foreach(var child in item.Children) {
                                Styleground styleground = Styleground.Create(child.Name.ToLower(), this, child, item);
								if(styleground != null)
                                    FGStylegrounds.Add(styleground);
                                else
                                    Snowberry.Log(LogLevel.Info, $"Missing styleground plugin for: {name}.");
                            }
                        }
                    } else {
                        Styleground styleground = Styleground.Create(name, this, item);
                        if(styleground != null)
                            FGStylegrounds.Add(styleground);
                        else
                            Snowberry.Log(LogLevel.Info, $"Missing styleground plugin for: {name}.");
                    }
				}
			}

            if(data.Background != null && data.Background.Children != null) {
                foreach(var item in data.Background.Children) {
                    string name = item.Name.ToLowerInvariant();

                    if(name.Equals("apply")) {
                        if(item.Children != null) {
                            foreach(var child in item.Children) {
                                Styleground styleground = Styleground.Create(child.Name.ToLower(), this, child, item);
                                if(styleground != null)
                                    BGStylegrounds.Add(styleground);
                                else
                                    Snowberry.Log(LogLevel.Info, $"Missing styleground plugin for: {name}.");
                            }
                        }
                    } else {
                        Styleground styleground = Styleground.Create(name, this, item);
                        if(styleground != null)
                            BGStylegrounds.Add(styleground);
                        else
                            Snowberry.Log(LogLevel.Info, $"Missing styleground plugin for: {name}.");
                    }
                }
            }

            Snowberry.Log(LogLevel.Info, $"Loaded {FGStylegrounds.Count} foreground stylegrounds and {BGStylegrounds.Count} background stylegrounds.");
        }

        internal Room GetRoomAt(Point at) {
            foreach (Room room in Rooms)
                if (new Rectangle(room.X * 8, room.Y * 8, room.Width * 8, room.Height * 8).Contains(at))
                    return room;
            return null;
        }

        internal int GetFillerIndexAt(Point at) {
			for(int i = 0; i < Fillers.Count; i++) {
				Rectangle filler = Fillers[i];
				if(new Rectangle(filler.X * 8, filler.Y * 8, filler.Width * 8, filler.Height * 8).Contains(at))
                    return i;
			}

			return -1;
        }

        internal void Render(Editor.BufferCamera camera) {
            Rectangle viewRect = camera.ViewRect;

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
            foreach (var styleground in BGStylegrounds)
                if (styleground.IsVisible(Editor.SelectedRoom))
                    styleground.Render();
            Draw.SpriteBatch.End();

            foreach (Room room in Rooms) {
				Rectangle rect = new Rectangle(room.Bounds.X * 8, room.Bounds.Y * 8, room.Bounds.Width * 8, room.Bounds.Height * 8);
				if (!viewRect.Intersects(rect))
					continue;

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
                room.Render(viewRect, camera);
                Draw.SpriteBatch.End();
            }

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
            foreach (var styleground in FGStylegrounds)
                if (styleground.IsVisible(Editor.SelectedRoom))
                    styleground.Render();
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
			for(int i = 0; i < Fillers.Count; i++) {
				Rectangle filler = Fillers[i];
				Rectangle rect = new Rectangle(filler.X * 8, filler.Y * 8, filler.Width * 8, filler.Height * 8);
            	Draw.Rect(rect, Color.White * (Editor.SelectedFillerIndex == i ? 0.14f : 0.1f));
            }
            Draw.SpriteBatch.End();
        }

        internal void HQRender(Editor.BufferCamera camera) {
            Rectangle viewRect = camera.ViewRect;

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;

            foreach (Room room in Rooms) {
                Rectangle rect = new Rectangle(room.Bounds.X * 8, room.Bounds.Y * 8, room.Bounds.Width * 8, room.Bounds.Height * 8);
                if (!viewRect.Intersects(rect))
                    continue;

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.ScreenView);
                room.HQRender(camera.ScreenView);
                Draw.SpriteBatch.End();
            }

            Draw.SpriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
        }

        public void GenerateMapData(MapData data){
			foreach(var room in Rooms)
                data.Levels.Add(new LevelData(room.CreateLevelData()));
			foreach(var filler in Fillers)
                data.Filler.Add(filler);
            Snowberry.Log(LogLevel.Info, "meta: " + data.Meta);
            // TODO: set stylegrounds

            // bounds
            int num = int.MaxValue;
            int num2 = int.MaxValue;
            int num3 = int.MinValue;
            int num4 = int.MinValue;
            foreach(LevelData level2 in data.Levels) {
                if(level2.Bounds.Left < num) {
                    num = level2.Bounds.Left;
                }
                
                if(level2.Bounds.Top < num2) {
                    num2 = level2.Bounds.Top;
                }

                if(level2.Bounds.Right > num3) {
                    num3 = level2.Bounds.Right;
                }

                if(level2.Bounds.Bottom > num4) {
                    num4 = level2.Bounds.Bottom;
                }
            }

            foreach(Rectangle item in data.Filler) {
                if(item.Left < num) {
                    num = item.Left;
                }

                if(item.Top < num2) {
                    num2 = item.Top;
                }

                if(item.Right > num3) {
                    num3 = item.Right;
                }

                if(item.Bottom > num4) {
                    num4 = item.Bottom;
                }
            }

            int num5 = 64;
            data.Bounds = new Rectangle(num - num5, num2 - num5, num3 - num + num5 * 2, num4 - num2 + num5 * 2);
        }

        public Element Export() {
            Element map = new Element();
            map.Children = new List<Element>();

            // children:
            //   levels w/ levels as children
            Element levels = new Element();
            levels.Name = "levels";
            levels.Children = new List<Element>();
            foreach(var room in Rooms)
                levels.Children.Add(room.CreateLevelData());
            map.Children.Add(levels);

            //   Filler w/ children w/ x,y,w,h
            Element fillers = new Element();
            fillers.Name = "Filler";
            fillers.Children = new List<Element>();
            foreach(var filler in Fillers) {
                Element fill = new Element();
                fill.Attributes = new Dictionary<string, object>() {
                    { "x", filler.X },
                    { "y", filler.Y },
                    { "w", filler.Width },
                    { "h", filler.Height }
                };
                fillers.Children.Add(fill);
			}
            map.Children.Add(fillers);

            //   style: w/ optional color, Backgrounds child & Foregrounds child
            Element style = new Element();
            style.Name = "Style";
            style.Children = new List<Element>() { /*bgStylegrounds ?? new Element(), fgStylegrounds ?? new Element()*/ };
			return map;
        }
    }
}
