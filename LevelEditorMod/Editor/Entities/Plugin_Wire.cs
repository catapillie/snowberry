using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("wire")]
    public class Plugin_Wire : Entity {
        [Option("above")] public bool Above = false;
        [Option("color")] public string Hex = "595866";

        private Color Color => Calc.HexToColor(Hex);

        public override void Render() {
            base.Render();

            Vector2 start = Position;
            Vector2 end = Nodes[0];
            Vector2 control = (start + end) / 2f + Vector2.UnitY * 24f;

            SimpleCurve curve = new SimpleCurve(start, end, control);
            curve.Render(Color, 16);
        }
    }
}
