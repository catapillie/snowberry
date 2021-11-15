using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    public class Map {

        // TODO: represent stylegrounds in-editor
        private readonly BinaryPacker.Element bgStylegounds, fgStylegrounds;

        public readonly string Name;
        public readonly AreaKey From;

        public readonly List<Room> Rooms = new List<Room>();
        public readonly List<Rectangle> Fillers = new List<Rectangle>();

        internal Map(string name) {
            Name = name;
        }

        internal Map(MapData data)
            : this(data.Filename) {
            foreach (LevelData roomData in data.Levels)
                Rooms.Add(new Room(roomData, this));
            foreach (Rectangle filler in data.Filler)
                Fillers.Add(filler);
            From = data.Area;
            bgStylegounds = data.Background;
            fgStylegrounds = data.Foreground;
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

        internal void Render(Editor.Camera camera) {
            Rectangle viewRect = camera.ViewRect;

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;

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
			for(int i = 0; i < Fillers.Count; i++) {
				Rectangle filler = Fillers[i];
				Rectangle rect = new Rectangle(filler.X * 8, filler.Y * 8, filler.Width * 8, filler.Height * 8);
            	Draw.Rect(rect, Color.White * (Editor.SelectedFillerIndex == i ? 0.14f : 0.1f));
            }
            Draw.SpriteBatch.End();
        }

        public void GenerateMapData(MapData data){
			foreach(var room in Rooms)
                data.Levels.Add(new LevelData(room.CreateLevelData()));
			foreach(var filler in Fillers)
                data.Filler.Add(filler);
            Module.Log(LogLevel.Info, "meta: " + data.Meta);
            // ...
            data.Foreground = fgStylegrounds;
            data.Background = bgStylegounds;
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
    }
}
