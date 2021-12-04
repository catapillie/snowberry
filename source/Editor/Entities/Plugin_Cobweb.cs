using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities {
    [Plugin("cobweb")]
    public class Plugin_Cobweb : Entity {
        [Option("color")] public Color Color = Calc.HexToColor("696A6A");

        public override int MinNodes => 1;
        public override int MaxNodes => 1;

        public override void Render() {
            base.Render();

            Vector2 mid = new SimpleCurve(Position, Nodes[0], (Position + Nodes[0]) / 2f + Vector2.UnitY * 4).GetPoint(0.5f);

            new SimpleCurve(mid, Position, (mid + Position) / 2f + Vector2.UnitY * 4).Render(Color, 20);
            foreach (Vector2 node in Nodes)
                new SimpleCurve(mid, node, (mid + node) / 2f + Vector2.UnitY * 4).Render(Color, 20);
        }

        public static void AddPlacements() {
            Placements.Create("Cobweb", "cobweb");
        }
    }
}
