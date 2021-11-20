using Snowberry.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry {
    public static class LevelEditor {
        internal static readonly HashSet<SnowberryModule> EditorModules = new HashSet<SnowberryModule>();
        public static SnowberryModule[] Modules => EditorModules.ToArray();
    }
}
