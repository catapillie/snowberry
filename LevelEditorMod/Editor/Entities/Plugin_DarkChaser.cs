using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("darkChaser")]
    public class Plugin_DarkChaser : Entity {
        [Option("canChangeMusic")] public bool CanChangeMusic = true;

        public override void Render() {
            base.Render();
            GFX.Game["characters/badeline/sleep00"].DrawJustified(Position + Vector2.UnitX * 4, new Vector2(0.5f, 1.0f));
        }

        public static void AddPlacements() {
            Placements.Create("Badeline Chaser", "darkChaser");
        }
    }

    [Plugin("darkChaserEnd")]
    public class Plugin_DarkChaserEnd : Entity {
        private static readonly Color fill = Color.Magenta * 0.2f;
        private static readonly Color border = new Color(0.4f, 0.0f, 0.4f);

		public override int MinWidth => 8;
		public override int MinHeight => 8;

		public override void Render() {
            base.Render();
            Draw.Rect(Position, Width, Height, fill);
            Draw.HollowRect(Position, Width, Height, border);
            Draw.HollowRect(Position + Vector2.One, Width - 2, Height - 2, border);
        }

        public static void AddPlacements() {
            Placements.Create("Chaser Barrier", "darkChaserEnd");
        }
    }
}
