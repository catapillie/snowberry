using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor {
    public abstract class TriggerPlugin : EntityPlugin {
        internal override void Render() {
            base.Render();
            Draw.Rect(Position, Width, Height, Color.Red * 0.2f);
            Draw.HollowRect(Position, Width, Height, Color.Red);
        }
    }
}
