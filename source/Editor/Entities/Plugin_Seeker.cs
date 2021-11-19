using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities {
    [Plugin("seeker")]
    public class Plugin_Seeker : Entity {

		public override int MaxNodes => -1;

		public override void Render() {
            base.Render();

            MTexture seeker = GFX.Game["characters/monsters/predator73"];
            seeker.DrawCentered(Position);

            Vector2 prev = Position;
            foreach (Vector2 node in Nodes) {
                seeker.DrawCentered(node);
                Draw.Line(prev, node, Color.White * 0.5f);
                prev = node;
            }
        }

        public static void AddPlacements() {
            Placements.Create("Seeker", "seeker");
        }
    }

    [Plugin("playerSeeker")]
    public class Plugin_PlayerSeeker : Entity {
        public override void Render() {
            base.Render();
            GFX.Game["decals/5-temple/statue_e"].DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Player Seeker", "playerSeeker");
        }
    }
}
