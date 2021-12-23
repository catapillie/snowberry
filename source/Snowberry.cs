using Celeste;
using Celeste.Mod;
using Snowberry.Editor;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snowberry {
    public sealed class Snowberry : EverestModule {
        private static Hook hook_MapData_orig_Load, hook_Session_get_MapData;

        public static Snowberry Instance {
            get;
            private set;
        }

        public Snowberry() {
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

			On.Celeste.Editor.MapEditor.ctor += UsePlaytestMap;
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
                foreach (Type type in asm.GetTypesSafe().Where(t => !t.IsAbstract && typeof(SnowberryModule).IsAssignableFrom(t))) {
                    ConstructorInfo ctor = type.GetConstructor(new Type[] { });
                    if (ctor != null) {
                        SnowberryModule editorModule = (SnowberryModule) ctor.Invoke(new object[] { });
                        LevelEditor.EditorModules.Add(editorModule);

                        PluginInfo.GenerateFromAssembly(asm, editorModule);

                        Log(LogLevel.Info, $"Successfully loaded Snowberry Module '{editorModule.Name}'");
                    }
                }
            }
        }

        public override void Unload() {
            hook_MapData_orig_Load?.Dispose();
            hook_Session_get_MapData?.Dispose();
            On.Celeste.Editor.MapEditor.ctor -= UsePlaytestMap;
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
            => Logger.Log(level, "Snowberry", message);

        private void UsePlaytestMap(On.Celeste.Editor.MapEditor.orig_ctor orig, Celeste.Editor.MapEditor self, AreaKey area, bool reloadMapData) {
            orig(self, area, reloadMapData);
            var selfData = new DynamicData(self);
            if(selfData.Get<Session>("CurrentSession") == Editor.Editor.PlaytestSession) {
                var templates = selfData.Get<List<Celeste.Editor.LevelTemplate>>("levels");
                templates.Clear();
                foreach(LevelData level in Editor.Editor.PlaytestMapData.Levels) {
                    templates.Add(new Celeste.Editor.LevelTemplate(level));
                }
                foreach(Microsoft.Xna.Framework.Rectangle item in Editor.Editor.PlaytestMapData.Filler) {
                    templates.Add(new Celeste.Editor.LevelTemplate(item.X, item.Y, item.Width, item.Height));
                }
            }
        }
    }
}
