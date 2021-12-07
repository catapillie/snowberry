using Celeste;
using Celeste.Mod;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Snowberry.Editor;
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

        public static SnowberryModule[] Modules { get; private set; }

        public Snowberry() {
            Instance = this;
        }

		public override Type SettingsType => typeof(SnowberrySettings);
        public static SnowberrySettings Settings => (SnowberrySettings)Instance._Settings;

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
            On.Celeste.MapData.StartLevel += DontCrashOnEmptyPlaytestLevel;
            On.Celeste.LevelEnter.Routine += DontEnterPlaytestMap;
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            LoadModules();

            Fonts.Load();
        }

        public override void Unload() {
            hook_MapData_orig_Load?.Dispose();
            hook_Session_get_MapData?.Dispose();

            On.Celeste.Editor.MapEditor.ctor -= UsePlaytestMap;
            On.Celeste.MapData.StartLevel -= DontCrashOnEmptyPlaytestLevel;
            On.Celeste.LevelEnter.Routine -= DontEnterPlaytestMap;
        }

        private static void LoadModules() {
            List<SnowberryModule> modules = new List<SnowberryModule>();

            foreach (EverestModule module in Everest.Modules) {
                Assembly asm = module.GetType().Assembly;
                foreach (Type type in asm.GetTypesSafe().Where(t => !t.IsAbstract && typeof(SnowberryModule).IsAssignableFrom(t))) {
                    ConstructorInfo ctor = type.GetConstructor(new Type[] { });
                    if (ctor != null) {
                        SnowberryModule editorModule = (SnowberryModule)ctor.Invoke(new object[] { });

                        PluginInfo.GenerateFromAssembly(asm, editorModule);

                        modules.Add(editorModule);
                        Log(LogLevel.Info, $"Successfully loaded Snowberry Module '{editorModule.Name}'");
                    }
                }
            }

            Modules = modules.ToArray();
        }


        public static void Log(LogLevel level, string message) {
            Logger.Log(level, "Snowberry", message);
        }

        private void UsePlaytestMap(On.Celeste.Editor.MapEditor.orig_ctor orig, Celeste.Editor.MapEditor self, AreaKey area, bool reloadMapData) {
            orig(self, area, reloadMapData);
            var selfData = new DynamicData(self);
            if (selfData.Get<Session>("CurrentSession") == Editor.Editor.PlaytestSession) {
                var templates = selfData.Get<List<Celeste.Editor.LevelTemplate>>("levels");
                templates.Clear();
                foreach (LevelData level in Editor.Editor.PlaytestMapData.Levels) {
                    templates.Add(new Celeste.Editor.LevelTemplate(level));
                }
                foreach (Microsoft.Xna.Framework.Rectangle item in Editor.Editor.PlaytestMapData.Filler) {
                    templates.Add(new Celeste.Editor.LevelTemplate(item.X, item.Y, item.Width, item.Height));
                }
            }
        }

        private LevelData DontCrashOnEmptyPlaytestLevel(On.Celeste.MapData.orig_StartLevel orig, MapData self) {
            // TODO: just add an empty room
            if (self.Area.SID == "Snowberry/Playtest" && self.Levels.Count == 0) {
                var empty = new BinaryPacker.Element();
                empty.Children = new List<BinaryPacker.Element>();
                empty.Attributes = new Dictionary<string, object>();
                empty.Attributes["name"] = "lvl_empty_map";
                return new LevelData(empty);
            } else
                return orig(self);
        }

        private System.Collections.IEnumerator DontEnterPlaytestMap(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
            var session = new DynamicData(self).Get<Session>("session");
            if (session.Area.SID == "Snowberry/Playtest" && session != Editor.Editor.PlaytestSession && string.IsNullOrEmpty(LevelEnter.ErrorMessage)) {
                return CantEnterRoutine(self);
            } else
                return orig(self);
        }

        private System.Collections.IEnumerator CantEnterRoutine(LevelEnter self) {
            yield return 1f;
            Postcard postcard;
            self.Add(postcard = new Postcard(Dialog.Get("SNOWBERRY_PLAYTEST_MAP_POSTCARD"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
            new DynamicData(self).Set("postcard", postcard);
            yield return postcard.DisplayRoutine();
            SaveData.Instance.CurrentSession_Safe = new Session(AreaKey.Default);
            SaveData.Instance.LastArea_Safe = AreaKey.Default;

            Monocle.Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaQuit);
        }
    }
}
