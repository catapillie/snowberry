using LevelEditorMod.Editor;
using Monocle;

namespace LevelEditorMod {
    internal class Commands {
        [Command("editor", "opens the level editor")]
        internal static void EditorCommand() => EditorScene.Open();
    }
}
