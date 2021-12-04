namespace Snowberry.Editor.Stylegrounds {

	[Plugin("planets")]
	internal class Plugin_Planets : Styleground {

		// Stored as a float and rounded when loading
		[Option("count")] public float Count = 32;
		[Option("size")] public string Size = "small";

		public override void Render() {
			base.Render();
		}
	}
}