using Microsoft.Xna.Framework;

namespace LevelEditorMod.Editor {
    public struct Selection {
        internal Rectangle? Main;
        internal Rectangle[] Nodes;

        public Selection(Rectangle main) {
            Main = main;
            Nodes = null;
        }

        public Selection(Rectangle main, params Rectangle[] nodes) {
            Main = main;
            Nodes = nodes;
        }
    }
}
