using Celeste;
using Celeste.Mod;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace LevelEditorMod {
    public class Module : EverestModule {

        private static Hook hook_MapData_orig_Load, hook_Session_get_MapData;

        public static Module Instance {
            get;
            private set;
        }

        public Module() {
            Instance = this;
        }

        public override void Load() {
            PluginInfo.GenerateFromAssembly(GetType().Assembly);

            hook_MapData_orig_Load = new Hook(
                typeof(MapData).GetMethod("orig_Load", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(Editor.Editor).GetMethod("CreatePlaytestMapDataHook", BindingFlags.Static | BindingFlags.NonPublic)
            );

            hook_Session_get_MapData = new Hook(
                typeof(Session).GetProperty("MapData", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                typeof(Editor.Editor).GetMethod("HookSessionGetAreaData", BindingFlags.Static | BindingFlags.NonPublic)
            );
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            Fonts.Load();
        }

        public override void Unload() {
            hook_MapData_orig_Load?.Dispose();
            hook_Session_get_MapData?.Dispose();
        }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);
    }
}
