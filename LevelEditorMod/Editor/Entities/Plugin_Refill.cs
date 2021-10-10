using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("refill")]
    public class Plugin_Refill : EntityPlugin {
        [EntityOption("twoDash")] public bool TwoDash = false;
        [EntityOption("oneUse")] public bool OneUse = false;

        internal override void Render() {
            base.Render();

            GFX.Game[$"objects/{(TwoDash ? "refillTwo" : "refill")}/idle00"].DrawOutlineCentered(Position);
        }
    }
}
