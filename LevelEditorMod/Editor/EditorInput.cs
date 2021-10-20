using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor {
    public static class EditorInput {
        public static class Mouse {
            public static Vector2 Screen { get; internal set; }
            public static Vector2 ScreenLast { get; internal set; }

            public static Vector2 World { get; internal set; }
            public static Vector2 WorldLast { get; internal set; }
        }
    }
}
