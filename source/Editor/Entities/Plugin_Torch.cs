using Celeste;
using System.Collections.Generic;

namespace Snowberry.Editor.Entities {
    [Plugin("torch")]
    public class Plugin_Torch : Entity {
        [Option("startLit")] public bool Lit;

        public override void Render() {
            base.Render();
            
            GFX.Game[$"objects/temple/{(Lit ? "litTorch" : "torch")}03"].DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Torch", "torch", new Dictionary<string, object>() { { "red", false } });
            Placements.Create("Torch (Lit)", "torch", new Dictionary<string, object>() { { "startLit", true } });
        }
    }
}
