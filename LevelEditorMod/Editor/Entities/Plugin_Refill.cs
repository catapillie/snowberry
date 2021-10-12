using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("refill")]
    public class Plugin_Refill : Entity {
        [Option("twoDash")] public bool TwoDash = false;
        [Option("oneUse")] public bool OneUse = false;

        public override void Render() {
            base.Render();

            GFX.Game[$"objects/{(TwoDash ? "refillTwo" : "refill")}/idle00"].DrawOutlineCentered(Position);
        }
    }
}
