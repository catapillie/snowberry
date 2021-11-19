using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("seekerBarrier")]
    public class Plugin_SeekerBarrier : Entity {

        [Option("moveTime")] public float MoveTime = 1;

        private List<bool> upperEdges;
        private List<bool> lowerEdges;
        private List<bool> leftEdges;
        private List<bool> rightEdges;

        public override int MinWidth => 8;
        public override int MinHeight => 8;

        public Plugin_SeekerBarrier() {
            Tracked = true;
        }

		public override void Render() {
            base.Render();

            Color c = Color.White * 0.15f;
            Draw.Rect(Position, Width, Height, c);
            
            if (Editor.FancyRender) {
                int slice = 4;
                int horizLimit = Width / (slice * 2);
                int vertLimit = Height / (slice * 2);

                bool dirty = upperEdges == null || (Room != null && Room.DirtyTrackedEntities.ContainsKey(typeof(Plugin_SeekerBarrier)) && Room.DirtyTrackedEntities[typeof(Plugin_SeekerBarrier)]);
                if(dirty) {
                    upperEdges = new List<bool>(horizLimit);
                    lowerEdges = new List<bool>(horizLimit);
                    leftEdges = new List<bool>(vertLimit);
                    rightEdges = new List<bool>(vertLimit);
                }

                for(int j = 0; j < horizLimit; j++) {
					if(upperEdges.Count <= j) {
                        upperEdges.Add(false);
                        lowerEdges.Add(false);
                    }
                    bool drawUp = dirty ? upperEdges[j] = !IsBarrierAt(Position.X + j * slice * 2 + 2, Position.Y - 4) : upperEdges[j];
                    bool drawDown = dirty ? lowerEdges[j] = !IsBarrierAt(Position.X + j * slice * 2 + 2, Position.Y + 4 + Height) : lowerEdges[j];
                    for(int k = 0; k < 2; k++) {
                        int i = j * 2 + k;
                        if(drawUp) {
                            Vector2 sliceStart = Position + new Vector2(i * slice, 0);
                            for(int across = 0; (float)across < slice; across++) {
                                Vector2 loc = sliceStart + Vector2.UnitX * across;
                                Draw.Line(loc, loc + Vector2.UnitY * ((float)Math.Sin(loc.X / 3f) - 1.5f) * 1.5f, c);
                            }
                        }
                        if(drawDown) {
                            Vector2 sliceStart = Position + new Vector2(i * slice, Height);
                            for(int across = 0; (float)across < slice; across++) {
                                Vector2 loc = sliceStart + Vector2.UnitX * (across + 1);
                                Draw.Line(loc, loc + Vector2.UnitY * ((float)Math.Sin(loc.X / 3f) + 1.5f) * 1.5f, c);
                            }
                        }
                    }
                }

                for(int j = 0; j < vertLimit; j++) {
                    if(leftEdges.Count <= j) {
                        leftEdges.Add(false);
                        rightEdges.Add(false);
                    }
                    bool drawLeft = dirty ? leftEdges[j] = !IsBarrierAt(Position.X - 4, Position.Y + j * slice * 2 + 2) : leftEdges[j];
                    bool drawRight = dirty ? rightEdges[j] = !IsBarrierAt(Position.X + 4 + Width, Position.Y + j * slice * 2 + 2) : rightEdges[j];
                    for(int k = 0; k < 2; k++) {
                        int i = j * 2 + k;
                        if(drawLeft) {
                            Vector2 sliceStart = Position + new Vector2(0, i * slice);
                            for(int across = 0; (float)across < slice; across++) {
                                Vector2 loc = sliceStart + Vector2.UnitY * across;
                                Draw.Line(loc, loc + Vector2.UnitX * ((float)Math.Sin(loc.Y / 3f) - 1.5f) * 1.5f, c);
                            }
                        }
                        if(drawRight) {
                            Vector2 sliceStart = Position + new Vector2(Width, i * slice);
                            for(int across = 0; (float)across < slice; across++) {
                                Vector2 loc = sliceStart + Vector2.UnitY * (across + 1);
                                Draw.Line(loc, loc + Vector2.UnitX * ((float)Math.Sin(loc.Y / 3f) + 1.5f) * 1.5f, c);
                            }
                        }
                    }
                }
                
                for(int i = 0; i < Width * Height / 32f; i++) {
                    var point = new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f));
                    Draw.Pixel.Draw(Position + point, Vector2.Zero, Color.White * 0.5f);
                }
            }
        }

        protected bool IsBarrierAt(float x, float y) {
            return Room != null && Room.TrackedEntities.ContainsKey(typeof(Plugin_SeekerBarrier)) && Room.TrackedEntities[typeof(Plugin_SeekerBarrier)].Exists(e => e is Plugin_SeekerBarrier && e.Bounds.Contains((int)x, (int)y));
        }

        public static void AddPlacements() {
            Placements.Create("Seeker Barrier", "seekerBarrier");
        }
    }
}
