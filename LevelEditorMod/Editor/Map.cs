using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    public class Map {
        private readonly List<Room> rooms = new List<Room>();
        private readonly List<Rectangle> fillers = new List<Rectangle>();

        public readonly string Name;
        public readonly AreaMode Mode;

        internal Map(string name) {
            Name = name;
        }

        internal Map(MapData data)
            : this(data.Filename) {
            foreach (LevelData roomData in data.Levels)
                rooms.Add(new Room(roomData, this));
            foreach (Rectangle filler in data.Filler)
                fillers.Add(filler);
            Mode = data.Area.Mode;
        }

        internal Room GetRoomAt(Point at) {
            foreach (Room room in rooms)
                if (new Rectangle(room.X * 8, room.Y * 8, room.Width * 8, room.Height * 8).Contains(at))
                    return room;
            return null;
        }

        internal void Render(Editor.Camera camera) {
            Rectangle viewRect = camera.ViewRect;

            Rectangle scissor = Draw.SpriteBatch.GraphicsDevice.ScissorRectangle;
            Engine.Instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;

            foreach (Room room in rooms) {
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
            foreach (Rectangle filler in fillers) {
            	Rectangle rect = new Rectangle(filler.X * 8, filler.Y * 8, filler.Width * 8, filler.Height * 8);
            	Draw.Rect(rect, Color.White * 0.08f);
            }
            Draw.SpriteBatch.End();
        }
    }
}
