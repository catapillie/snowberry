using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("darkChaser")]
    public class Plugin_DarkChaser : Entity {
        [Option("canChangeMusic")] public bool CanChangeMusic = true;

        public override void Render() {
            base.Render();
            GFX.Game["characters/badeline/sleep00"].DrawJustified(Position + Vector2.UnitX * 4, new Vector2(0.5f, 1.0f));
        }
    }
}
