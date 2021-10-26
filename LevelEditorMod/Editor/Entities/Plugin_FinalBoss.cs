using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("finalBoss")]
    public class Plugin_FinalBoss : Entity {
        [Option("patternIndex")] public int PatternIndex = 1;
        [Option("startHit")] public bool StartHit = false;
        [Option("cameraPastY")] public float cameraPastY = 120.0f;
        [Option("cameraLockY")] public bool CameraLockY = true;
        [Option("canChangeMusic")] public bool CanChangeMusic = true;

        public override void Render() {
            base.Render();

            MTexture baddy = GFX.Game["characters/badelineBoss/charge00"];
            baddy.DrawCentered(Position);

            Vector2 prev = Position;
            foreach (Vector2 node in Nodes) {
                baddy.DrawCentered(node);
                DrawUtil.DottedLine(prev, node, Color.Red * 0.5f, 8, 4);
                prev = node;
            }
        }
    }
}
