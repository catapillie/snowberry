using Celeste.Mod;

namespace LevelEditorMod {
    public class Module : EverestModule {
        public static Module Instance {
            get;
            private set;
        }

        public Module() {
            Instance = this;
        }

        public override void Load() {
            Plugins.Register(GetType().Assembly);
        }

        public override void Unload() { }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);
    }
}
