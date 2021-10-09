using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("strawberry")]
    public class Plugin_Strawberry : EntityPlugin {
        internal override void Render() {
            base.Render();
            GFX.Game["collectables/strawberry/normal00"].DrawCentered(Position);
        }
    }
}
