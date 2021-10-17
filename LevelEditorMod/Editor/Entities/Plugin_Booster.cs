using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("booster")]
    public class Plugin_Booster : Entity {
        [Option("red")] public bool Red;
        [Option("ch9_hub_booster")] public bool Ch9Hub;

        public override void Render() {
            base.Render();

            GFX.Game[$"objects/booster/{(Red ? "boosterRed" : "booster")}00"].DrawOutlineCentered(Position);
        }
    }
}
