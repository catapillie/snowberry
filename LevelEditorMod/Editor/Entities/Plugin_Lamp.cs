using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("lamp")]
    public class Plugin_Lamp : EntityPlugin {
        [EntityOption("broken")] public bool Broken = false;

        internal override void Render() {
            base.Render();

            GFX.Game["scenery/lamp"].GetSubtexture(Broken ? 16 : 0, 0, 16, 80).DrawJustified(Position, new Vector2(0.5f, 1.0f));
        }
    }
}
