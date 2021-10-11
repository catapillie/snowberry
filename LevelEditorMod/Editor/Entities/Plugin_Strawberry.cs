using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("strawberry")]
    public class Plugin_Strawberry : EntityPlugin {
        [EntityOption("winged")] public bool Winged = false;
        [EntityOption("moon")] public bool Moon = false;
        [EntityOption("order")] public int Order = -1;
        [EntityOption("checkpointID")] public int CheckpointID = -1;

        public override void Render() {
            base.Render();

            Vector2[] nodes = GetNodes();

            bool seeded = nodes.Length != 0;
            if (Moon) {
                string anim = seeded || Winged ? "ghost" : "normal";
                GFX.Game[$"collectables/moonBerry/{anim}00"].DrawCentered(Position);
            } else {
                string dir = seeded ? "ghostberry" : "strawberry";
                string anim = Winged ? "wings01" : (seeded ? "idle00" : "normal00");
                GFX.Game[$"collectables/{dir}/{anim}"].DrawCentered(Position);
            }

            if (seeded)
                foreach (Vector2 node in nodes)
                    GFX.Game["collectables/strawberry/seed00"].DrawCentered(node);
        }
    }
}
