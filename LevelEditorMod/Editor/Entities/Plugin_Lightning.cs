using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("lightning")]
    public class Plugin_Lightning : Entity {
        [Option("moveTime")] public float MoveTime = 1;

        public static Color[] ElectricityColors = new Color[2]{
            Calc.HexToColor("fcf579"),
            Calc.HexToColor("8cf7e2")
        };

        public override int MinWidth => 8;
        public override int MinHeight => 8;
        public override int MaxNodes => 1;

        public override void Render() {
            base.Render();

            Draw.Rect(Position, Width, Height, ElectricityColors[0] * 0.25f);
            Draw.HollowRect(Position, Width, Height, ElectricityColors[1]);
            if (Nodes.Length != 0)
                DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White, 4, 2);
        }

        protected override Rectangle[] Select() {
            if (Nodes.Length != 0) {
                Vector2 node = Nodes[0];
                return new Rectangle[] {
                    Bounds, new Rectangle((int)node.X, (int)node.Y, Width, Height)
                };
            } else {
                return new Rectangle[] { Bounds };
            }
        }

        public static void AddPlacements() {
            Placements.Create("Lightning", "lightning");
        }
    }
}
