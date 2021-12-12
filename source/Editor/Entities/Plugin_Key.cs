namespace Snowberry.Editor.Entities {
    [Plugin("key")]
    public class Plugin_Key : Entity {
        public override void Render() {
            base.Render();
            FromSprite("key", "idle")?.DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Key", "key");
        }
    }
}
