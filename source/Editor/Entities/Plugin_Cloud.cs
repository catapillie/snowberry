using Celeste;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {

    [Plugin("cloud")]
    public class Plugin_Cloud : Entity {

        [Option("fragile")] public bool Fragile = false;

        public override void Render() {
            base.Render();

            string type = Fragile ? "fragile" : "cloud";
            string suffix = Room.Map.From.Mode == AreaMode.Normal ? "" : "Remix";
            GFX.Game[$"objects/clouds/{type}{suffix}00"].DrawCentered(Position);
        }

        public static void AddPlacements(){
            Placements.Create("Cloud", "cloud");
            Placements.Create("Cloud (Fragile)", "cloud", new Dictionary<string, object>() { { "fragile", true } });
        }
    }
}
