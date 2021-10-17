using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor.Triggers {
    [Plugin("cameraOffsetTrigger")]
    public class Plugin_CameraOffsetTrigger : Plugin_Trigger {
        [Option("cameraX")] public float CameraX = 0.0f;
        [Option("cameraY")] public float CameraY = 0.0f;

        public override void Render() {
            base.Render();
            Fonts.Pico8.Draw($"(x: {CameraX} y: {CameraY})", Center + Vector2.UnitY * 6, Vector2.One, new Vector2(0.5f, 0.5f), Color.Black);
        }
    }
}
