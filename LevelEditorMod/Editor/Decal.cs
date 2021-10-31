using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.IO;

namespace LevelEditorMod.Editor {
    public class Decal {
        private readonly Room room;

        private MTexture texture;
        public Vector2 position;
        public Vector2 scale;
        public string Texture { get; private set; }

        internal Decal(Room room, string texture) {
            this.room = room;
            this.texture = GFX.Game[texture];
            //this.Texture = texture;
        }

        internal Decal(Room room, DecalData data) {
            this.room = room;

            // messy, see Celeste.Decal.orig_ctor
            var ext = Path.GetExtension(data.Texture);
            texture = GFX.Game[Path.Combine("decals", Texture = ext.Length > 0 ? data.Texture.Replace(Path.GetExtension(data.Texture), "") : data.Texture).Replace('\\', '/')];
            position = data.Position;
            scale = data.Scale;
        }

        internal void Render(Vector2 offset) {
            texture.DrawCentered(offset + position, Color.White, scale);
        }
    }
}
