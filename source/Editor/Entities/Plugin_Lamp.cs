using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities {
    [Plugin("lamp")]
    public class Plugin_Lamp : Entity {
        [Option("broken")] public bool Broken = false;

        public override void Render() {
            base.Render();

            GFX.Game["scenery/lamp"].GetSubtexture(Broken ? 16 : 0, 0, 16, 80).DrawJustified(Position, new Vector2(0.5f, 1.0f));
        }

        public static void AddPlacements() {
            Placements.Create("Lamp", "lamp");
        }
    }
}