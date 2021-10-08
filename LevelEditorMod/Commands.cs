using Celeste;
using LevelEditorMod.Editor;
using Monocle;

namespace LevelEditorMod {
    internal class Commands {
        [Command("editor", "opens the level editor")]
        internal static void EditorCommand() {
            if (Engine.Scene is Level level)
                LevelEditor.Open(level.Session.MapData);
            else
                Engine.Commands.Log("Open the level editor by loading a level and running the command 'editor'");
        }
    }
}
