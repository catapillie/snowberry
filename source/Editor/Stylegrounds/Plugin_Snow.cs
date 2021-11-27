namespace Snowberry.Editor.Stylegrounds {

	[Plugin("snowfg")]
	[Plugin("snowbg")]
	internal class Plugin_Snow : Styleground {

		public bool Fg => Name == "snowfg";

		public override void Render() {
			base.Render();
		}
	}
}