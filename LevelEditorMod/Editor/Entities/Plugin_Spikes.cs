using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("spikesUp")]
    [EntityPlugin("spikesDown")]
    [EntityPlugin("spikesLeft")]
    [EntityPlugin("spikesRight")]
    public class Plugin_Spikes : EntityPlugin {
        [EntityOption("type")] public string Type = "default";

        internal override void Render() {
            base.Render();
            
            switch (Name) {
                case "spikesUp":
                default:
                    Draw.Rect(Position - Vector2.UnitY * 5, Width, 5, Color.White * 0.25f);
                    break;

                case "spikesDown":
                    Draw.Rect(Position, Width, 5, Color.White * 0.25f);
                    break;

                case "spikesLeft":
                    Draw.Rect(Position - Vector2.UnitX * 5, 5, Height, Color.White * 0.25f);
                    break;

                case "spikesRight":
                    Draw.Rect(Position, 5, Height, Color.White * 0.25f);
                    break;
            }
        }
    }
}
