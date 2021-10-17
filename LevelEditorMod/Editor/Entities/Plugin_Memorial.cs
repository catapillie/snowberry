using Celeste;
using Microsoft.Xna.Framework;
using System.Linq;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("memorial")]
    [Plugin("everest/memorial")]
    public class Plugin_Memorial : Entity {
        [Option("dialog")] public string Dialog = "MEMORIAL";
        [Option("sprite")] public string Sprite = "scenery/memorial/memorial";
        [Option("spacing")] public int Spacing = 16;

        public override void Render() {
            base.Render();

            GFX.Game[Sprite].DrawJustified(Position, new Vector2(0.5f, 1.0f));

            int y = 0;
            foreach (string str in Celeste.Dialog.Clean(Dialog).Split('\n').Reverse()) {
                Fonts.Pico8.Draw(str, Position - Vector2.UnitY * (64 + y), Vector2.One, new Vector2(0.5f, 1.0f), Color.White);
                y += 5;
            }
        }
    }
}
