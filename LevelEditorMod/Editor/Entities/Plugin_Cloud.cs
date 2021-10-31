using Celeste;

namespace LevelEditorMod.Editor.Entities {

    [Plugin("cloud")]
    public class Plugin_Cloud : Entity {

        [Option("fragile")] public bool Fragile = false;

        public override void Render() {
            base.Render();
            
            string type = Fragile ? "fragile" : "cloud";
            string suffix = Room.Map.Mode == AreaMode.Normal ? "" : "Remix";
            GFX.Game[$"objects/clouds/{type}{suffix}00"].DrawCentered(Position);
        }
    }
}
