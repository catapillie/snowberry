using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.IO;

namespace LevelEditorMod.Editor {
    public class Decal {
        private readonly Room room;

        private MTexture texture;
        private Vector2 position;
        private Vector2 scale;

        public Decal(Room room, string texture) {
            this.room = room;
            this.texture = GFX.Game[texture];
        }

        public Decal(Room room, DecalData data) {
            this.room = room;

            // messy, see Celeste.Decal.orig_ctor
            texture = GFX.Game[Path.Combine("decals", data.Texture.Replace(Path.GetExtension(data.Texture), "")).Replace('\\', '/')];
            position = data.Position;
            scale = data.Scale;
        }
    }
}
