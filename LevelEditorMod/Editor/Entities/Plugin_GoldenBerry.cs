using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("goldenBerry")]
    [Plugin("memorialTextController")]
    public class Plugin_GoldenBerry : Entity {
        [Option("winged")] public bool Winged = false;

        private bool noDash;

		public override int MaxNodes => -1;

		public override void Initialize() {
            base.Initialize();
            noDash = Name == "memorialTextController";
        }

        public override void Render() {
            base.Render();

            bool seeded = Nodes.Length != 0;
            string dir = seeded && !noDash ? "ghostgoldberry" : "goldberry";
            string anim = Winged || noDash ? "wings01" : "idle00";
            GFX.Game[$"collectables/{dir}/{anim}"].DrawCentered(Position);

            if (seeded)
                foreach (Vector2 node in Nodes)
                    GFX.Game["collectables/strawberry/seed00"].DrawCentered(node);
        }

        public static void AddPlacements() {
            Placements.Create("Golden Berry", "goldenBerry");
            Placements.Create("Dashless Golden Berry", "memorialTextController");
        }
    }
}
