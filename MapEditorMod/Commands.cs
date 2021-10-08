using MapEditorMod.Editor;
using Monocle;

namespace MapEditorMod {
    internal class Commands {
        [Command("editor", "opens the level editor")]
        internal static void EditorCommand() => EditorScene.Open();
    }
}
