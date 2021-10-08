using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.IO;

namespace LevelEditorMod.Editor {
    public class Decal {
        public readonly Room Room;

        public MTexture Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Scale { get; private set; }

        public Decal(Room room, string texture) {
            Room = room;
            Texture = GFX.Game[texture];
        }

        public Decal(Room room, DecalData data) {
            Room = room;

            // messy, see Celeste.Decal.orig_ctor
            Texture = GFX.Game[Path.Combine("decals", data.Texture.Replace(Path.GetExtension(data.Texture), "")).Replace('\\', '/')];
            Position = data.Position;
            Scale = data.Scale;
        }
    }
}
