using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("wire")]
    public class Plugin_Wire : Entity {
        [Option("above")] public bool Above = false;
        [Option("color")] public Color Color = Calc.HexToColor("595866");

        public override void Render() {
            base.Render();

            Vector2 start = Position;
            Vector2 end = Nodes[0];
            Vector2 control = (start + end) / 2f + Vector2.UnitY * 24f;

            SimpleCurve curve = new SimpleCurve(start, end, control);
            curve.Render(Color, 20);
        }

        public override void ApplyDefaults() {
            base.ChangeDefault();
            ResetNodes();
            AddNode(Position + new Vector2(16, 0));
        }
    }
}
