using Celeste;
using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("bonfire")]
    public class Plugin_Bonfire : EntityPlugin {
        [EntityOption("mode")] public string Mode = "Unlit";

        internal override void Render() {
            base.Render();

            GFX.Game[Mode switch {
                "lit" => "objects/campfire/fire08",
                "smoking" => "objects/campfire/smoking04",
                _ => "objects/campfire/fire00",
            }].DrawJustified(Position, new Vector2(0.5f, 1.0f));
        }
    }
}
