using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities {
    [Plugin("cassette")]
    public class Plugin_Cassette : Entity {

		public override int MinNodes => 2;
		public override int MaxNodes => 2;

		public override void Render() {
            base.Render();
            GFX.Game["collectables/cassette/idle00"].DrawCentered(Position);
            new SimpleCurve(Position, Nodes[1], Nodes[0]).Render(Color.DarkCyan * 0.75f, 32, 2);
        }

        public static void AddPlacements() {
            Placements.Create("Cassette", "cassette");
        }
    }
}
