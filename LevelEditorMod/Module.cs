using Celeste;
using Celeste.Mod;
using LevelEditorMod.Editor;
using Microsoft.Xna.Framework;
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
            Everest.Events.MainMenu.OnCreateButtons += MainMenu_OnCreateButtons;
            
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
            Everest.Events.MainMenu.OnCreateButtons -= MainMenu_OnCreateButtons;
        }

        
        
        private void MainMenu_OnCreateButtons(OuiMainMenu menu, System.Collections.Generic.List<MenuButton> buttons)
        {
            MainMenuSmallButton btn = new MainMenuSmallButton("EDITOR_MAINMENU", "menu/editor", menu, Vector2.Zero, Vector2.Zero, () => {
                Editor.Editor.OpenFancy(null); //uwu
            });
            int c = 2;
            if (Celeste.Celeste.PlayMode == Celeste.Celeste.PlayModes.Debug) c++;
            buttons.Insert(c, btn);
        }


        //Currently unused but necessary to determine if Randomizer or other things create a button
        public static bool TryGetModule(EverestModuleMetadata meta, out EverestModule module)
        {
            foreach (EverestModule other in Everest.Modules)
            {
                EverestModuleMetadata otherData = other.Metadata;
                if (otherData.Name != meta.Name)
                    continue;

                Version version = otherData.Version;
                if (Everest.Loader.VersionSatisfiesDependency(meta.Version, version))
                {
                    module = other;
                    return true;
                }
            }

            module = null;
            return false;
        }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);
    }
}
