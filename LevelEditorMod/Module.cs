using Celeste;
using Celeste.Mod;
using LevelEditorMod.Editor;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
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

            LoadModules();

            Fonts.Load();
        }

        private void LoadModules() {
            LevelEditor.EditorModules.Clear();
            foreach (EverestModule module in Everest.Modules) {
                Assembly asm = module.GetType().Assembly;
                foreach (Type type in asm.GetTypesSafe().Where(t => !t.IsAbstract && typeof(EditorModule).IsAssignableFrom(t))) {
                    ConstructorInfo ctor = type.GetConstructor(new Type[] { });
                    if (ctor != null) {
                        EditorModule editorModule = (EditorModule) ctor.Invoke(new object[] { });
                        LevelEditor.EditorModules.Add(editorModule);

                        PluginInfo.GenerateFromAssembly(asm, editorModule);

                        Log(LogLevel.Info, $"Successfully loaded Level Editor Module '{editorModule.Name}'");
                    }
                }
            }
        }

        public override void Unload() {
            hook_MapData_orig_Load?.Dispose();
            hook_Session_get_MapData?.Dispose();
        }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);
    }
}
