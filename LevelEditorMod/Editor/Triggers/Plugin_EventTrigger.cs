namespace LevelEditorMod.Editor.Triggers {
    [Plugin("eventTrigger")]
    [Plugin("creditsTrigger")]
    public class Plugin_EventTrigger : Plugin_Trigger {
        [Option("event")] public string Event = "";

        public static new void AddPlacements() {
            Placements.Create("Event Trigger", "eventTrigger");
            Placements.Create("Credits Trigger", "creditsTrigger");
        }
    }
}
