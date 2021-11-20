using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities {
    [Plugin("wire")]
    public class Plugin_Wire : Entity {
        [Option("above")] public bool Above = false;
        [Option("color")] public Color Color = Calc.HexToColor("595866");

		public override int MinNodes => 1;
		public override int MaxNodes => 1;

		public override void Render() {
            base.Render();

            Vector2 start = Position;
            Vector2 end = Nodes[0];
            Vector2 control = (start + end) / 2f + Vector2.UnitY * 24f;

            SimpleCurve curve = new SimpleCurve(start, end, control);
            curve.Render(Color, 20);
        }

        public static void AddPlacements() {
            Placements.Create("Wire", "wire");
        }
    }
}
