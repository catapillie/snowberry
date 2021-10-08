using Celeste.Mod;

namespace MapEditorMod {
    public class Module : EverestModule {
        public static Module Instance {
            get;
            private set;
        }

        public Module() {
            Instance = this;
        }

        public override void Load() { }

        public override void Unload() { }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Map Maker Mod", message);
    }
}
