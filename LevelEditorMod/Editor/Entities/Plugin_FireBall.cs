using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("fireBall")]
    public class Plugin_FireBall : Entity {
        [Option("amount")] public int Amount = 3;
        [Option("offset")] public float Offset = 0.0f;
        [Option("speed")] public float Speed = 1.0f;
        [Option("notCoreMode")] public bool NotCoreMode = false;

        public override void Render() {
            base.Render();

            Vector2 start = Position;
            Vector2 end = Nodes[0];

            MTexture orb = GFX.Game["objects/fireball/fireball01"];

            if (Amount == 0 || start == end) {
                orb.DrawCentered(Position);
            } else {
                Draw.Line(start, end, Color.Teal);
                Vector2 d = end - start;
                float step = 1f / Amount;
                for (float f = 0f; f < 1f; f += step)
                    orb.DrawCentered(Position + d * ((f + Offset) % 1f));
            }
        }

        public override void ApplyDefaults() {
            base.ChangeDefault();
            ResetNodes();
            AddNode(Position + new Vector2(16, 0));
        }
    }
}
