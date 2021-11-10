using Celeste;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("strawberry")]
    public class Plugin_Strawberry : Entity {
        [Option("winged")] public bool Winged = false;
        [Option("moon")] public bool Moon = false;
        [Option("order")] public int Order = -1;
        [Option("checkpointID")] public int CheckpointID = -1;

		public override int MaxNodes => -1;

		public override void Render() {
            base.Render();

            bool seeded = Nodes.Length != 0;
            if (Moon) {
                string anim = seeded || Winged ? "ghost" : "normal";
                GFX.Game[$"collectables/moonBerry/{anim}00"].DrawCentered(Position);
            } else {
                string dir = seeded ? "ghostberry" : "strawberry";
                string anim = Winged ? "wings01" : (seeded ? "idle00" : "normal00");
                GFX.Game[$"collectables/{dir}/{anim}"].DrawCentered(Position);
            }

            if (seeded)
                foreach (Vector2 node in Nodes)
                    GFX.Game["collectables/strawberry/seed00"].DrawCentered(node);
        }

        public static void AddPlacements() {
            Placements.Create("Strawberry", "strawberry");
            Placements.Create("Strawberry (Winged)", "strawberry", new Dictionary<string, object>() { { "winged", true } });
            Placements.Create("Moon Berry", "strawberry", new Dictionary<string, object>() { { "moon", true } });
        }
    }
}
