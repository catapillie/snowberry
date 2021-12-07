using Celeste.Mod;

namespace Snowberry {

	public class SnowberrySettings : EverestModuleSettings {

		[SettingName("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN")]
		[SettingSubText("SNOWBERRY_SETTINGS_MIDDLE_CLICK_PAN_SUB")]
		public bool MiddleClickPan { get; set; } = true;
	}
}
