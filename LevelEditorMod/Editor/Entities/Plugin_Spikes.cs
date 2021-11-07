using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Spikes;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("spikesUp")]
    [Plugin("spikesDown")]
    [Plugin("spikesLeft")]
    [Plugin("spikesRight")]
    public class Plugin_Spikes : Entity {
        [Option("type")] public string Type = "default";

        private Directions dir;
        bool initialized = false;

        public override void Initialize() {
            base.Initialize();

            dir = Name switch {
                "spikesRight" => Directions.Right,
                "spikesLeft" => Directions.Left,
                "spikesDown" => Directions.Down,
                _ => Directions.Up,
            };
            initialized = true;
        }

        public override void Render() {
            base.Render();
            
            if (Type == "tentacles") {
                // TODO: this part
            } else {
                MTexture spikes = GFX.Game[$"danger/spikes/{Type}_{dir.ToString().ToLower()}00"];

                switch (dir) {
                    default:
                    case Directions.Up:
                        for (int x = 0; x < Width / 8; x++)
                            spikes.DrawJustified(Position + new Vector2(x * 8, 1), new Vector2(0.0f, 1.0f));
                        break;

                    case Directions.Down:
                        for (int x = 0; x < Width / 8; x++)
                            spikes.Draw(Position + new Vector2(x * 8, -1));
                        break;

                    case Directions.Left:
                        for (int y = 0; y < Height / 8; y++)
                            spikes.DrawJustified(Position + new Vector2(1, y * 8), new Vector2(1.0f, 0.0f));
                        break;

                    case Directions.Right:
                        for (int y = 0; y < Height / 8; y++)
                            spikes.Draw(Position + new Vector2(-1, y * 8));
                        break;
                }
            }
        }

		public override void ApplyDefaults() {
			base.ApplyDefaults();
            if(initialized)
			    if(dir == Directions.Up || dir == Directions.Down) {
                    SetWidth(8);
			    }else if(dir == Directions.Left || dir == Directions.Right) {
                    SetHeight(8);
                }
        }
	}
}
