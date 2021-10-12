using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Triggers {
    public abstract class Plugin_Trigger : Entity {
        public override void Render() {
            base.Render();
            Draw.Rect(Position, Width, Height, Color.Red * 0.2f);
            Draw.HollowRect(Position, Width, Height, Color.Red);
        }
    }
}
