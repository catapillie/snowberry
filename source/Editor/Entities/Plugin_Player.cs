using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities {
    [Plugin("player")]
    public class Plugin_Player : Entity {
        public override void Render() {
            base.Render();

            FromSprite("player", "sitDown")?.DrawCentered(Position - Vector2.UnitY * 16);
        }

        public static void AddPlacements() {
            Placements.Create("Spawn Point", "player");
        }
    }
}
