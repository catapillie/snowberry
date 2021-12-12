using Celeste;

namespace Snowberry.Editor.Entities {
    [Plugin("blackGem")]
    public class Plugin_HeartGem : Entity {
        [Option("fake")] public bool Fake = false;
        [Option("removeCameraTriggers")] public bool RemoveCameraTriggers = false;
        [Option("fakeHeartDialog")] public string FakeHeartDialog = "CH9_FAKE_HEART";
        [Option("keepGoingDialog")] public string KeepGoingDialog = "CH9_KEEP_GOING";

        public override void Render() {
            base.Render();

            string texture = Room.Map.From.Mode switch {
                AreaMode.Normal => "heartgem0",
                AreaMode.BSide => "heartgem1",
                AreaMode.CSide => "heartgem2",
                _ => "heartgem3",
            };
            FromSprite(Fake ? "heartgem3" : texture, "idle")?.DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Crystal Heart", "blackGem");
        }
    }
}
