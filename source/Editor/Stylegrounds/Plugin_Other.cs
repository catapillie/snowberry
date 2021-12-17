using System.Collections.Generic;

namespace Snowberry.Editor.Stylegrounds {

	// Any styleground that doesn't have it's own plugin.
	public class Plugin_Other : Styleground{

		public readonly Dictionary<string, object> Attrs = new();

		public override void Set(string option, object value) {
			Attrs[option] = value;
		}

		public override object Get(string option) {
			return Attrs[option];
		}
	}
}