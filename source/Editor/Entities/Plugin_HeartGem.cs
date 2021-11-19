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
                AreaMode.Normal => "collectables/heartGem/0/00",
                AreaMode.BSide => "collectables/heartGem/1/00",
                AreaMode.CSide => "collectables/heartGem/2/00",
                _ => "collectables/heartGem/3/00",
            };
            GFX.Game[Fake ? "collectables/heartGem/3/00" : texture].DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Crystal Heart", "blackGem");
        }
    }
}
