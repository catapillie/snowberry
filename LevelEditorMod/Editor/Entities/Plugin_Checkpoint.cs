using Celeste;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("checkpoint")]
    public class Plugin_Checkpoint : Entity {
        [Option("bg")] public string Background = "";

        public override void Render() {
            base.Render();

            //GFX.Game[$"objects/booster/{(Red ? "boosterRed" : "booster")}00"].DrawOutlineCentered(Position);
            int id = Room.Map.From.ID;
			string text = !string.IsNullOrWhiteSpace(Background) ? "objects/checkpoint/bg/" + Background : "objects/checkpoint/bg/" + id;
			if(GFX.Game.Has(text)) {
                GFX.Game[text].DrawJustified(Position, new Microsoft.Xna.Framework.Vector2(0.5f, 1));
                //Add(image = new Image(GFX.Game[text]));
                //image.JustifyOrigin(0.5f, 1f);
            }
        }
    }
}
