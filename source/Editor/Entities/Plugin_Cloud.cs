using Celeste;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {
    [Plugin("cloud")]
    public class Plugin_Cloud : Entity {
        [Option("fragile")] public bool Fragile = false;

        public override void Render() {
            base.Render();

            string type = Fragile ? "cloudFragile" : "cloud";
            string suffix = (Editor.From?.Mode ?? AreaMode.Normal) == AreaMode.Normal ? "" : "Remix";
            FromSprite(type + suffix, "idle")?.DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Cloud", "cloud");
            Placements.Create("Cloud (Fragile)", "cloud", new Dictionary<string, object>() { { "fragile", true } });
        }
    }
}