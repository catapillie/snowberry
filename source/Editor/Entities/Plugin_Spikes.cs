using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.Spikes;

namespace Snowberry.Editor.Entities {
    [Plugin("spikesUp")]
    [Plugin("spikesDown")]
    [Plugin("spikesLeft")]
    [Plugin("spikesRight")]
    public class Plugin_Spikes : Entity {
        [Option("type")] public string Type = "default";

        private Directions dir;
        bool initialized = false;

        public override int MinWidth => (!initialized || dir == Directions.Left || dir == Directions.Right) ? -1 : 8;
        public override int MinHeight => (!initialized || dir == Directions.Up || dir == Directions.Down) ? -1 : 8;

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

        public static void AddPlacements() {
            string[] directions = new string[] { "Up", "Down", "Left", "Right" };
            string[] types = new string[] { "Default", "Outline", "Cliffside", "Reflection" };
            foreach (var type in types)
                foreach (var dir in directions)
                    Placements.Create($"Spikes ({dir}, {type})", "spikes" + dir, new Dictionary<string, object>() { { "type", type.ToLower() } });
        }
    }
}