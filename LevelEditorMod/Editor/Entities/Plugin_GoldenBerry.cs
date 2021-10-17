using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("goldenBerry")]
    public class Plugin_GoldenBerry : Entity {
        [Option("winged")] public bool Winged = false;

        public override void Render() {
            base.Render();

            bool seeded = Nodes.Length != 0;
            string dir = seeded ? "ghostgoldberry" : "goldberry";
            string anim = Winged ? "wings01" : "idle00";
            GFX.Game[$"collectables/{dir}/{anim}"].DrawCentered(Position);

            if (seeded)
                foreach (Vector2 node in Nodes)
                    GFX.Game["collectables/strawberry/seed00"].DrawCentered(node);
        }
    }
}
