using Celeste;
using Celeste.Mod;
using LevelEditorMod.Editor;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
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

			On.Celeste.Editor.MapEditor.ctor += UsePlaytestMap;
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
            On.Celeste.Editor.MapEditor.ctor -= UsePlaytestMap;
        }

        public static void Log(LogLevel level, string message)
            => Logger.Log(level, "Level Editor Mod", message);

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
