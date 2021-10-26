using Celeste.Mod;
using LevelEditorMod.Editor;
using Monocle;
using System.IO;

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
            PluginInfo.GenerateFromAssembly(GetType().Assembly);
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            Fonts.Load();
        }

        public override void Unload() { }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);
    }
}
