using Celeste;
using Monocle;

namespace Snowberry {
    internal class Commands {
        [Command("editor", "opens the snowberry level editor")]
        internal static void EditorCommand() {
            Editor.Editor.Open(Engine.Scene is Level level ? level.Session.MapData : null);
        }
    }
}
