using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("jumpThru")]
    public class Plugin_JumpThru : Entity {
        [Option("texture")] public string Texture = "default";
        [Option("surfaceIndex")] public int SurfaceIndex = -1;

		public override int MinWidth => 8;

		public override void Render() {
            base.Render();

            string name = Texture == "default" ? "wood" : Texture;
            MTexture tex = GFX.Game[$"objects/jumpthru/{name}"];

            int w = Width / 8;
            for (int i = 0; i < w; i++) {
                int tx = 1, ty = 0;

                if (i == 0) {
                    tx = 0;
                    ty = Room.GetFgTile(Position - Vector2.UnitX) == '0' ? 1 : 0;
                } else if (i == w - 1) {
                    tx = 2;
                    ty = Room.GetFgTile(Position + Vector2.UnitX * Width) == '0' ? 1 : 0;
                }

                tex.GetSubtexture(tx * 8, ty * 8, 8, 8).Draw(Position + Vector2.UnitX * i * 8);
            }
        }

        public static void AddPlacements() {
            string[] types = new string[] { "Wood", "Cliffside", "Core", "Dream", "Moon", "Reflection", "Temple" };
            foreach(var type in types)
                Placements.Create($"Jump-thru ({type})", "jumpThru", new Dictionary<string, object>() { { "texture", type.ToLower() } });
            // they all follow a nice pattern except this one
            Placements.Create($"Jump-thru (Temple B)", "jumpThru", new Dictionary<string, object>() { { "texture", "templeB" } });
        }
    }
}
