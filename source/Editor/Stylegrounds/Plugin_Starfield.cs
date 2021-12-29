using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Stylegrounds {
    [Plugin("starfield")]
    internal class Plugin_Starfield : Styleground {
        [Option("color")] public Color StarfieldColor = Color.White;
        [Option("speed")] public float StarfieldSpeed = 1f;

        public override void Render() {
            base.Render();
        }
    }
}