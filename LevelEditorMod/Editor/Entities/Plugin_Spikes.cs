using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("spikesUp")]
    [EntityPlugin("spikesDown")]
    [EntityPlugin("spikesLeft")]
    [EntityPlugin("spikesRight")]
    public class Plugin_Spikes : EntityPlugin {
        [EntityOption("type")] public string Type = "default";

        private Spikes.Directions dir;

        public override void Initialize() {
            base.Initialize();

            dir = Name switch {
                "spikesRight" => Spikes.Directions.Right,
                "spikesLeft" => Spikes.Directions.Left,
                "spikesDown" => Spikes.Directions.Down,
                _ => Spikes.Directions.Up,
            };
        }

        public override void Render() {
            base.Render();

            if (Type == "tentacles") {
                // TODO: this part
            } else {
                MTexture spikes = GFX.Game[$"danger/spikes/{Type}_{dir.ToString().ToLower()}00"];

                switch (dir) {
                    default:
                    case Spikes.Directions.Up:
                        for (int x = 0; x < Width / 8; x++)
                            spikes.DrawJustified(Position + new Vector2(x * 8, 1), new Vector2(0.0f, 1.0f));
                        break;

                    case Spikes.Directions.Down:
                        for (int x = 0; x < Width / 8; x++)
                            spikes.Draw(Position + new Vector2(x * 8, -1));
                        break;

                    case Spikes.Directions.Left:
                        for (int y = 0; y < Height / 8; y++)
                            spikes.DrawJustified(Position + new Vector2(1, y * 8), new Vector2(1.0f, 0.0f));
                        break;

                    case Spikes.Directions.Right:
                        for (int y = 0; y < Height / 8; y++)
                            spikes.Draw(Position + new Vector2(-1, y * 8));
                        break;
                }
            }
        }
    }
}
