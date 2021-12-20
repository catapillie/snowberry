using Microsoft.Xna.Framework;

using Snowberry.Editor;

namespace Snowberry.Modules {

	public class LoennSupport : SnowberryModule {

		public static SnowberryModule INSTANCE { get; private set; }

		public LoennSupport()
			: base("Loenn Plugins", Color.Pink) {
			INSTANCE = this;
		}
	}
}
