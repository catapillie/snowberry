using LevelEditorMod.Editor;
using System.Collections.Generic;
using System.Linq;

namespace LevelEditorMod {
    public static class LevelEditor {
        internal static readonly HashSet<EditorModule> EditorModules = new HashSet<EditorModule>();
        public static EditorModule[] Modules => EditorModules.ToArray();
    }
}
