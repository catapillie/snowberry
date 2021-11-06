using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("cassette")]
    public class Plugin_Cassette : Entity {
        public override void Render() {
            base.Render();
            GFX.Game["collectables/cassette/idle00"].DrawCentered(Position);
            new SimpleCurve(Position, Nodes[1], Nodes[0]).Render(Color.DarkCyan * 0.75f, 32, 2);
        }

		public override void ApplyDefaults() {
			base.ChangeDefault();
            ResetNodes();
            AddNode(Position + new Vector2(30, 20));
            AddNode(Position + new Vector2(60, 0));
        }
	}
}
