using Celeste;

namespace Snowberry.Editor.Entities {
    [Plugin("summitgem")]
    public class Plugin_SummitGem : Entity {
        [Option("gem")] public int Gem = 0;

        public override void Render() {
            base.Render();
            GFX.Game[$"collectables/summitgems/{Gem}/gem00"].DrawCentered(Position);
        }
    }
}