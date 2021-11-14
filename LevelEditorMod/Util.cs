using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod {
    public static class Util {
        public static class Colors {
            public static readonly Color White = Calc.HexToColor("f0f0f0");
            public static readonly Color Cyan = Calc.HexToColor("1fc4bc");
            public static readonly Color DarkCyan = Calc.HexToColor("1da4ad");
            public static readonly Color Red = Calc.HexToColor("db2323");
            public static readonly Color DarkRed = Calc.HexToColor("b02020");
            public static readonly Color Blue = Calc.HexToColor("2877de");
            public static readonly Color DarkBlue = Calc.HexToColor("2365cf");
            public static readonly Color DarkGray = Calc.HexToColor("060607");
            public static readonly Color CloudGray = Calc.HexToColor("293036");
            public static readonly Color CloudLightGray = Calc.HexToColor("5c646b");
        }

        public static int Bit(this bool b) => b ? 1 : 0;
    }
}
