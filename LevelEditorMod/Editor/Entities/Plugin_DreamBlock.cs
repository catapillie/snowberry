using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("dreamBlock")]
    public class Plugin_DreamBlock : Entity {
        [Option("fastMoving")] public bool Fast = false;
        [Option("oneUse")] public bool OneUse = false;
        [Option("below")] public bool Below = false;

        public override void Render() {
            base.Render();

            Draw.Rect(Position, Width, Height, Color.Black * 0.25f);
            Draw.HollowRect(Position, Width, Height, Color.White);
            if (Nodes.Length != 0)
                Draw.Line(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White);
        }
    }
}
