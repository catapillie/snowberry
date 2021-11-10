using Celeste;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("booster")]
    public class Plugin_Booster : Entity {
        [Option("red")] public bool Red;
        [Option("ch9_hub_booster")] public bool Ch9Hub;

        public override void Render() {
            base.Render();

            GFX.Game[$"objects/booster/{(Red ? "boosterRed" : "booster")}00"].DrawOutlineCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Booster (Green)", "booster", new Dictionary<string, object>() { { "red", false } });
            Placements.Create("Booster (Red)", "booster", new Dictionary<string, object>() { { "red", true } });
        }
    }
}
