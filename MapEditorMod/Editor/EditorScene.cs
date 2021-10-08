using Celeste.Mod;
using Monocle;

namespace MapEditorMod.Editor {
    public class EditorScene : Scene {
        internal static void Open() {
            Module.Log(LogLevel.Info, "Opening level editor");
            Engine.Scene = new EditorScene();
        }
    }
}
