using Celeste;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("foregroundDebris")]
    public class Plugin_ForegroundDebris : Entity {
        private MTexture[] debris;

        public override void Initialize() {
            base.Initialize();
            if (Calc.Random.Next(2) == 1)
                debris = new MTexture[] {
                    GFX.Game["scenery/fgdebris/rock_a00"],
                    GFX.Game["scenery/fgdebris/rock_a01"],
                    GFX.Game["scenery/fgdebris/rock_a02"],
                };
            else
                debris = new MTexture[] {
                    GFX.Game["scenery/fgdebris/rock_b00"],
                    GFX.Game["scenery/fgdebris/rock_b01"],
                };
        }

        public override void Render() {
            base.Render();
            foreach (MTexture t in debris)
                t.DrawCentered(Position);
        }
    }
}
