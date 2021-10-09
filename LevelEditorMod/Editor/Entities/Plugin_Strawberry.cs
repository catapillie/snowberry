using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("strawberry")]
    public class Plugin_Strawberry : EntityPlugin {
        [EntityOption] private bool winged;
        [EntityOption] private bool moon;
        [EntityOption] private int order;
        [EntityOption] private int checkpointID;

        internal override void Render() {
            base.Render();

            Vector2[] nodes = GetNodes();

            bool seeded = nodes.Length != 0;
            if (moon) {
                string anim = seeded || winged ? "ghost" : "normal";
                GFX.Game[$"collectables/moonBerry/{anim}00"].DrawCentered(Position);
            } else {
                string dir = seeded ? "ghostberry" : "strawberry";
                string anim = winged ? "wings01" : (seeded ? "idle00" : "normal00");
                GFX.Game[$"collectables/{dir}/{anim}"].DrawCentered(Position);
            }

            if (seeded)
                foreach (Vector2 node in nodes)
                    GFX.Game["collectables/strawberry/seed00"].DrawCentered(node);
        }
    }
}
