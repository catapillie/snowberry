using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("player")]
    public class Plugin_Player : EntityPlugin {
        public override void Render() {
            base.Render();

            GFX.Game["characters/player/sitDown00"].DrawCentered(Position - Vector2.UnitY * 16);
        }
    }
}
