using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Stylegrounds {

	[Plugin("starfield")]
	internal class Plugin_Starfield : Styleground {

		[Option("color")] public string StarfieldColor = "ffffff";
		[Option("speed")] public float StarfieldSpeed = 1f;

		public override void Render() {
			base.Render();
		}
	}
}