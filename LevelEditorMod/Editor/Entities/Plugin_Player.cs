using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("player")]
    public class Plugin_Player : Entity {
        public override void Render() {
            base.Render();

            GFX.Game["characters/player/sitDown00"].DrawCentered(Position - Vector2.UnitY * 16);
        }
    }
}
