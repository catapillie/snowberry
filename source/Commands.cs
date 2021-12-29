using Celeste;
using Monocle;

namespace Snowberry {
    internal class Commands {
        [Command("editor", "opens the snowberry level editor")]
        internal static void EditorCommand() {
            Editor.Editor.Open(Engine.Scene is Level level ? level.Session.MapData : null);
        }

        [Command("editor_new", "opens the snowberry level editor on an empty map")]
        internal static void NewMapCommand() {
            Editor.Editor.OpenNew();
        }
    }
}