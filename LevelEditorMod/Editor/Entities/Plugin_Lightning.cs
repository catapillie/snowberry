using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("lightning")]
    public class Plugin_Lightning : Entity {

        [Option("moveTime")] public float MoveTime = 1;

        public static Color[] ElectricityColors = new Color[2]{
            Calc.HexToColor("8cf7e2"),
            Calc.HexToColor("fcf579")
        };

        public override int MinWidth => 8;
        public override int MinHeight => 8;
        public override int MaxNodes => 1;

        public Plugin_Lightning() {
            Tracked = true;
        }

		public override void Render() {
            base.Render();

            Draw.Rect(Position, Width, Height, ElectricityColors[0] * 0.15f);

            if (Editor.FancyRender) {
                int prev0 = 0, prev1 = 0, prev3 = 0, prev4 = 0;
                int slice = 4;
                for(int j = 0; j < Width / (slice * 2); j++) {
                    bool drawUp = !IsLightningAt(Position.X + j * slice * 2 + 2, Position.Y - 4);
                    bool drawDown = !IsLightningAt(Position.X + j * slice * 2 + 2, Position.Y + 4 + Height);
                    for(int k = 0; k < 2; k++) {
                        int i = j * 2 + k;
                        bool last = i + 1 >= Width / slice;
                        if(drawUp) {
                            Draw.Line(Position + new Vector2(i * slice, prev0), Position + new Vector2(i * slice + slice, prev0 = (last ? 0 : Calc.Random.Next(-2, 3))), ElectricityColors[0]);
                            Draw.Line(Position + new Vector2(i * slice, prev1), Position + new Vector2(i * slice + slice, prev1 = (last ? 0 : Calc.Random.Next(-2, 3))), ElectricityColors[1]);
                        }
                        if(drawDown) {
                            Draw.Line(Position + new Vector2(i * slice, Height + prev3), Position + new Vector2(i * slice + slice, Height + (prev3 = (last ? 0 : Calc.Random.Next(-2, 3)))), ElectricityColors[0]);
                            Draw.Line(Position + new Vector2(i * slice, Height + prev4), Position + new Vector2(i * slice + slice, Height + (prev4 = (last ? 0 : Calc.Random.Next(-2, 3)))), ElectricityColors[1]);
                        }
                    }
                }
                prev0 = 0; prev1 = 0; prev3 = 0; prev4 = 0;
                for(int j = 0; j < Height / (slice * 2); j++) {
                    bool drawLeft = !IsLightningAt(Position.X - 4, Position.Y + j * slice * 2 + 2);
                    bool drawRight = !IsLightningAt(Position.X + 4 + Width, Position.Y + j * slice * 2 + 2);
                    for(int k = 0; k < 2; k++) {
                        int i = j * 2 + k;
                        bool last = i + 1 >= Height / slice;
                        if(drawLeft) {
                            Draw.Line(Position + new Vector2(prev0, i * slice), Position + new Vector2(prev0 = (last ? 0 : Calc.Random.Next(-2, 3)), i * slice + slice), ElectricityColors[0]);
                            Draw.Line(Position + new Vector2(prev1, i * slice), Position + new Vector2(prev1 = (last ? 0 : Calc.Random.Next(-2, 3)), i * slice + slice), ElectricityColors[1]);
                        }
                        if(drawRight) {
                            Draw.Line(Position + new Vector2(Width + prev3, i * slice), Position + new Vector2(Width + (prev3 = (last ? 0 : Calc.Random.Next(-2, 3))), i * slice + slice), ElectricityColors[0]);
                            Draw.Line(Position + new Vector2(Width + prev4, i * slice), Position + new Vector2(Width + (prev4 = (last ? 0 : Calc.Random.Next(-2, 3))), i * slice + slice), ElectricityColors[1]);
                        }
                    }
                }
            } else
                Draw.HollowRect(Position, Width, Height, ElectricityColors[1]);

            if (Nodes.Length != 0)
                DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2f, Color.White, 4, 2);
        }

        protected bool IsLightningAt(float x, float y) {
			/*if(Room == null)
                return false;
            
			for(int i = 0; i < Room.TrackedEntities[typeof(Plugin_Lightning)].Count; i++)
                if(Room.TrackedEntities[typeof(Plugin_Lightning)][i] is Plugin_Lightning e && e.Bounds.Contains((int)x, (int)y))
                    return true;
			
            return false;*/

            // this is faster
            return Room != null && Room.TrackedEntities[typeof(Plugin_Lightning)].Exists(e => e is Plugin_Lightning && e.Bounds.Contains((int)x, (int)y));
        }

        protected override Rectangle[] Select() {
            if (Nodes.Length != 0) {
                Vector2 node = Nodes[0];
                return new Rectangle[] {
                    Bounds, new Rectangle((int)node.X, (int)node.Y, Width, Height)
                };
            } else {
                return new Rectangle[] { Bounds };
            }
        }

        public static void AddPlacements() {
            Placements.Create("Lightning", "lightning");
        }
    }
}
