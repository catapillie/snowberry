using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities {
    [Plugin("payphone")]
    public class Plugin_Payphone : Entity {
        public override void Render() {
            base.Render();
            GFX.Game["scenery/payphone"].DrawJustified(Position, new Vector2(0.5f, 1.0f));
        }

        public static void AddPlacements() {
            Placements.Create("Payphone", "payphone");
        }
    }
}
