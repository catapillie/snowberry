using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("infiniteStar")]
    public class Plugin_Feather : Entity {
        [Option("shielded")] public bool Shielded;
        [Option("singleUse")] public bool OneUse;

        public override void Render() {
            base.Render();
            GFX.Game["objects/flyFeather/idle00"].DrawCentered(Position);
            if (Shielded)
                Draw.Circle(Position, 12f, Color.White, 5);
        }
    }
}
