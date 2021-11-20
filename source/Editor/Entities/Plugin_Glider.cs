using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Snowberry.Editor.Entities {
    [Plugin("glider")]
    public class Plugin_Glider : Entity {

        [Option("bubble")] public bool Bubble = false;
        [Option("tutorial")] public bool Tutorial = false;

        public override void Render() {
            base.Render();
            GFX.Game["objects/glider/idle0"].DrawOutlineCentered(Position);
            if(Bubble) {
                for(int i = 0; i < 24; i++) {
                    Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
                }
            }
        }

        private Vector2 PlatformAdd(int num) {
            return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(3 + num * 0.2f) * 1.8));
        }

        private Color PlatformColor(int num) {
            if(num <= 1 || num >= 22) {
                return Color.White * 0.4f;
            }

            return Color.White * 0.8f;
        }

        public static void AddPlacements() {
            Placements.Create("Jellyfish", "glider");
        }
    }
}
