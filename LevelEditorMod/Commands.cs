using Celeste;
using Monocle;

namespace LevelEditorMod {
    internal class Commands {
        [Command("editor", "opens the level editor")]
        internal static void EditorCommand() {
            Editor.Editor.Open(Engine.Scene is Level level ? level.Session.MapData : null);
        }
    }
}
