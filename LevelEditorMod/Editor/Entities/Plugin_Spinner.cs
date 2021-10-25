using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.TrackSpinner;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("spinner")]
    public class Plugin_Spinner : Entity {
        [Option("attachToSolid")] public bool Attached = false;
        [Option("color")] public CrystalColor SpinnerColor = CrystalColor.Blue;
        [Option("dust")] public bool Dust = false;

        public override void Render() {
            base.Render();

            if (Dust) {
                GFX.Game["danger/dustcreature/base00"].DrawCentered(Position);
                GFX.Game["danger/dustcreature/center00"].DrawCentered(Position);
            } else {
                MTexture spinner = GFX.Game[SpinnerColor switch {
                    CrystalColor.Blue => "danger/crystal/fg_blue03",
                    CrystalColor.Red => "danger/crystal/fg_red03",
                    CrystalColor.Purple => "danger/crystal/fg_purple03",
                    _ => "danger/crystal/fg_white03"
                }];

                Color c = Color.White;
                if (SpinnerColor == CrystalColor.Rainbow) {
                    c = Calc.HsvToColor(0.4f + Calc.YoYo(Position.Length() % 280 / 280) * 0.4f, 0.4f, 0.9f);
                }
                //spinner.DrawCentered(Position + Vector2.UnitX, Color.Black);
                //spinner.DrawCentered(Position - Vector2.UnitX, Color.Black);
                //spinner.DrawCentered(Position + Vector2.UnitY, Color.Black);
                //spinner.DrawCentered(Position - Vector2.UnitY, Color.Black);
                spinner.DrawCentered(Position, c);
            }
        }
    }

    public class Plugin_MovingSpinner : Entity {
        [Option("dust")] public bool Dust = false;
        [Option("star")] public bool Star = false;

        public override void Render() {
            base.Render();

            Vector2 stop = Nodes[0];

            if (Star) {
                MTexture star = GFX.Game["danger/starfish13"];
                star.DrawCentered(Position);
                star.DrawCentered(stop);
            } else if (Dust) {
                MTexture dustbase = GFX.Game["danger/dustcreature/base00"],
                    dustcenter = GFX.Game["danger/dustcreature/center00"];
                dustbase.DrawCentered(Position);
                dustcenter.DrawCentered(Position);
                dustbase.DrawCentered(stop);
                dustcenter.DrawCentered(stop);
            } else {
                MTexture blade = GFX.Game["danger/blade00"];
                blade.DrawCentered(Position);
                blade.DrawCentered(stop);
            }
        }
    }

    [Plugin("trackSpinner")]
    public class Plugin_TrackSpinner : Plugin_MovingSpinner {
        [Option("speed")] public Speeds Speed = Speeds.Normal;
        [Option("startCenter")] public bool StartAtCenter = false;

        public override void Render() {
            DrawUtil.DottedLine(Position, Nodes[0], Color.White * 0.5f, 8, 4);
            base.Render();
        }
    }

    [Plugin("rotateSpinner")]
    public class Plugin_RotateSpinner : Plugin_MovingSpinner {
        [Option("clockwise")] public bool Clockwise = false;

        public override void Render() {
            Draw.Circle(Position, Vector2.Distance(Position, Nodes[0]), Color.White * 0.5f, 20);
            base.Render();
        }
    }
}
