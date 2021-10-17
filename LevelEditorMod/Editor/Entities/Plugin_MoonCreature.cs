using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("moonCreature")]
    public class Plugin_MoonCreature : Entity {
        [Option("number")] public int Number = -1;

        public override void Render() {
            base.Render();
            GFX.Game["scenery/moon_creatures/tiny05"].DrawCentered(Position);
        }
    }
}
