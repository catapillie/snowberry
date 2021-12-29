using Celeste;

namespace Snowberry.Editor.Entities {
    [Plugin("touchSwitch")]
    public class Plugin_TouchSwitch : Entity {
        public override void Render() {
            base.Render();
            GFX.Game["objects/touchswitch/container"].DrawCentered(Position);
            GFX.Game["objects/touchswitch/icon00"].DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Touch Switch", "touchSwitch");
        }
    }
}