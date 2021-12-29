using Microsoft.Xna.Framework;
using Monocle;

namespace Snowberry.Editor.Entities {
    [Plugin("badelineBoost")]
    public class Plugin_BadelineBoost : Entity {
        [Option("lockCamera")] public bool LockCamera = true;
        [Option("canSkip")] public bool CanSkip = false;
        [Option("finalCh9Boost")] public bool FinalCh9Boost = false;
        [Option("finalCh9GoldenBoost")] public bool FinalCh9GoldenBoost = false;
        [Option("finalCh9Dialog")] public bool FinalCh9Dialog = false;

        public override void Render() {
            base.Render();

            MTexture orb = FromSprite("badelineBoost", "idle");
            orb?.DrawCentered(Position);

            foreach (Vector2 node in Nodes)
                orb?.DrawCentered(node);
        }

        public override void HQRender() {
            base.HQRender();

            Vector2 prev = Position;
            foreach (Vector2 node in Nodes) {
                DrawUtil.DottedLine(prev, node, Color.Red * 0.5f, 8, 4);
                prev = node;
            }
        }

        public static void AddPlacements() {
            Placements.Create("Badeline Boost", "badelineBoost");
        }
    }
}