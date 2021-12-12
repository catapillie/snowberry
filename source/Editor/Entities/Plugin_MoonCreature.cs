namespace Snowberry.Editor.Entities {
    [Plugin("moonCreature")]
    public class Plugin_MoonCreature : Entity {
        [Option("number")] public int Number = -1;

        public override void Render() {
            base.Render();
            FromSprite("moonCreatureTiny", "idle")?.DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Moon Creature", "moonCreature");
        }
    }
}
