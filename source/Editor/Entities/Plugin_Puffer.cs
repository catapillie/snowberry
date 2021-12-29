using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities {
    [Plugin("eyebomb")]
    public class Plugin_Puffer : Entity {
        [Option("right")] public bool Right;

        public override void Render() {
            base.Render();

            FromSprite("pufferFish", "idle")?.DrawOutlineCentered(Position, Color.White, new Vector2(Right ? 1 : -1, 1));
        }

        public static void AddPlacements() {
            Placements.Create("Puffer", "eyebomb");
        }
    }
}