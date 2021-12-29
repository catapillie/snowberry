using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.TrackSpinner;

namespace Snowberry.Editor.Entities {
    [Plugin("spinner")]
    public class Plugin_Spinner : Entity {
        [Option("attachToSolid")] public bool Attached = false;
        [Option("color")] public CrystalColor SpinnerColor = CrystalColor.Blue;
        [Option("dust")] public bool Dust = false;

        private List<Entity> connectTo;

        public Plugin_Spinner() {
            Tracked = true;
        }

        public override void InitializeAfter() {
            base.InitializeAfter();
            // Handle vanilla stuff
            Dust |= IsVanillaDust();
            SpinnerColor = GetColorForVanillaMap() ?? SpinnerColor;
        }

        public override void RenderBefore() {
            base.RenderBefore();
            if (Editor.FancyRender && !Dust) {
                CrystalColor color = SpinnerColor;

                var colourString = color switch {
                    CrystalColor.Blue => "blue",
                    CrystalColor.Red => "red",
                    CrystalColor.Purple => "purple",
                    _ => "white"
                };

                Color c = Color.White;
                if (color == CrystalColor.Rainbow) {
                    c = Calc.HsvToColor(0.4f + Calc.YoYo(Position.Length() % 280 / 280) * 0.4f, 0.4f, 0.9f);
                }

                UpdateConnections();
                MTexture bg = GFX.Game[$"danger/crystal/bg_{colourString}00"];
                foreach (var item in connectTo)
                    bg.DrawCentered(Position + (item.Position - Position) / 2, c);
            }
        }

        public override void Render() {
            base.Render();

            CrystalColor color = SpinnerColor;

            if (Dust) {
                GFX.Game["danger/dustcreature/base00"].DrawCentered(Position);
                GFX.Game["danger/dustcreature/center00"].DrawCentered(Position);
            } else {
                var colourString = color switch {
                    CrystalColor.Blue => "blue",
                    CrystalColor.Red => "red",
                    CrystalColor.Purple => "purple",
                    _ => "white"
                };
                Color c = Color.White;
                if (color == CrystalColor.Rainbow)
                    c = Calc.HsvToColor(0.4f + Calc.YoYo(Position.Length() % 280 / 280) * 0.4f, 0.4f, 0.9f);
                MTexture spinner = GFX.Game[$"danger/crystal/fg_{colourString}03"];
                spinner.DrawCentered(Position, c);
            }
        }

        private void UpdateConnections() {
            if (connectTo == null || Room.DirtyTrackedEntities.ContainsKey(typeof(Plugin_Spinner)) && Room.DirtyTrackedEntities[typeof(Plugin_Spinner)]) {
                connectTo = new List<Entity>();
                foreach (var item in Room.TrackedEntities[typeof(Plugin_Spinner)]) {
                    if ((item.Position - Position).LengthSquared() < 24 * 24) {
                        connectTo.Add(item);
                    }
                }
            }
        }

        public static void AddPlacements() {
            string[] types = new string[] { "Blue", "Red", "Purple", "Rainbow" };
            foreach (var type in types)
                Placements.Create($"Spinner ({type})", "spinner", new Dictionary<string, object>() { { "color", type } });
        }

        public CrystalColor? GetColorForVanillaMap() {
            return Editor.VanillaLevelID switch {
                5 => CrystalColor.Red,
                6 => CrystalColor.Purple,
                10 => CrystalColor.Rainbow,
                _ => null
            };
        }

        public bool IsVanillaDust() {
            int id = Editor.VanillaLevelID;
            return id == 3 || (id == 7 && ((Room ?? Editor.SelectedRoom)?.Name?.StartsWith("d-") ?? false));
        }
    }

    public class Plugin_MovingSpinner : Entity {
        [Option("dust")] public bool Dust = false;
        [Option("star")] public bool Star = false;

        public override void InitializeAfter() {
            base.InitializeAfter();
            Dust |= IsVanillaDust();
            Star |= IsVanillaStar();
        }

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

        public override void ApplyDefaults() {
            base.ChangeDefault();
            ResetNodes();
            AddNode(Position + new Vector2(16, 0));
        }

        public bool IsVanillaDust() {
            int id = Editor.VanillaLevelID;
            return id == 3 || (id == 7 && ((Room ?? Editor.SelectedRoom)?.Name?.StartsWith("d-") ?? false));
        }

        public bool IsVanillaStar() {
            int id = Editor.VanillaLevelID;
            return id == 10;
        }
    }

    [Plugin("trackSpinner")]
    public class Plugin_TrackSpinner : Plugin_MovingSpinner {
        [Option("speed")] public Speeds Speed = Speeds.Normal;
        [Option("startCenter")] public bool StartAtCenter = false;

        public override void HQRender() {
            DrawUtil.DottedLine(Position, Nodes[0], Color.White * 0.5f, 8, 4);
            base.HQRender();
        }
    }

    [Plugin("rotateSpinner")]
    public class Plugin_RotateSpinner : Plugin_MovingSpinner {
        [Option("clockwise")] public bool Clockwise = false;

        public override void HQRender() {
            Draw.Circle(Position, Vector2.Distance(Position, Nodes[0]), Color.White * 0.5f, 20);
            base.HQRender();
        }
    }
}