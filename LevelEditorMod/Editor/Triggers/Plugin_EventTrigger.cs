namespace LevelEditorMod.Editor.Triggers {
    [EntityPlugin("eventTrigger")]
    public class Plugin_EventTrigger : TriggerPlugin {
        [EntityOption("event")] private string eventName;
    }
}
