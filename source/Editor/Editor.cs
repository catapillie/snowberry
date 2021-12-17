using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.UI;
using System;
using System.Collections.Generic;

namespace Snowberry.Editor {
    public class Editor : Scene {
        public class BufferCamera {
            private bool changedView = true;

            private Vector2 pos;
            public Vector2 Position {
                get => pos;
                set {
                    pos = value;
                    changedView = true;
                }
            }
            public int X => (int)Position.X;
            public int Y => (int)Position.Y;

            private float scale = 1f;
            public float Zoom {
                get => scale;
                set {
                    scale = value;
                    if (scale < 1f)
                        Buffer = null;
                    else {
                        Vector2 size = new Vector2(Engine.Width, Engine.Height) / scale;
                        Buffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, (int)size.X + (Engine.Width % scale == 0 ? 0 : 1), (int)size.Y + (Engine.Height % scale == 0 ? 0 : 1));
                    }
                    changedView = true;
                }
            }

            private Matrix matrix, inverse, screenview;
            public Matrix Matrix {
                get {
                    if (changedView)
                        UpdateMatrices();
                    return matrix;
                }
            }
            public Matrix Inverse {
                get {
                    if (changedView)
                        UpdateMatrices();
                    return inverse;
                }
            }
            public Matrix ScreenView {
                get {
                    if (changedView)
                        UpdateMatrices();
                    return screenview;
                }
            }

            public Rectangle ViewRect { get; private set; }

            public RenderTarget2D Buffer { get; private set; }

            public BufferCamera() {
                Buffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.Width, Engine.Height);
            }

            private void UpdateMatrices() {
                Matrix m = Matrix.CreateTranslation((int)-Position.X, (int)-Position.Y, 0f) * Matrix.CreateScale(Math.Min(1f, Zoom));

                if (Buffer != null) {
                    m *= Matrix.CreateTranslation(Buffer.Width / 2, Buffer.Height / 2, 0f);
                    ViewRect = new Rectangle((int)Position.X - Buffer.Width / 2, (int)Position.Y - Buffer.Height / 2, Buffer.Width, Buffer.Height);
                    screenview = m * Matrix.CreateScale(Zoom);
                } else {
                    m *= Engine.ScreenMatrix * Matrix.CreateTranslation(Engine.ViewWidth / 2, Engine.ViewHeight / 2, 0f);
                    int w = (int)(Engine.Width / Zoom);
                    int h = (int)(Engine.Height / Zoom);
                    ViewRect = new Rectangle((int)Position.X - w / 2, (int)Position.Y - h / 2, w, h);
                    screenview = m;
                }
                inverse = Matrix.Invert(matrix = m);

                changedView = false;
            }
        }

        public static class Mouse {
            public static Vector2 Screen { get; internal set; }
            public static Vector2 ScreenLast { get; internal set; }

            public static Vector2 World { get; internal set; }
            public static Vector2 WorldLast { get; internal set; }
        }

        public static Editor Instance { get; private set; }

        public static bool FancyRender = true;

        private static readonly Color bg = Calc.HexToColor("060607");

        public BufferCamera Camera { get; private set; }

        public Vector2 mousePos, lastMousePos;
        public Vector2 worldClick;
        public static bool MouseClicked = false;

        public Map Map { get; private set; }

        private readonly UIElement ui = new UIElement();
        private RenderTarget2D uiBuffer;

        internal static Rectangle? Selection;
        internal static Room SelectedRoom;
        internal static int SelectedFillerIndex = -1;
        internal static List<EntitySelection> SelectedEntities;

        public UIToolbar Toolbar;
        public UIElement ToolPanel;

        // TODO: potentially replace with just setting the MapData of Playtest
        private static bool generatePlaytestMapData = false;
        internal static Session PlaytestSession;
        internal static MapData PlaytestMapData;

        public static int VanillaLevelID { get; private set; }
        public static AreaKey? From;

        private Editor(Map map) {
            Engine.Instance.IsMouseVisible = true;
            Map = map;

            SelectedRoom = null;
            SelectedFillerIndex = -1;
            Instance = this;
        }

        internal static void Open(MapData data) {
            Audio.Stop(Audio.CurrentAmbienceEventInstance);
            Audio.Stop(Audio.CurrentMusicEventInstance);

            Map map = null;
            if (data != null) {
                Snowberry.Log(LogLevel.Info, $"Opening level editor using map {data.Area.GetSID()}");
                // Also copies the target's metadata into Playtest's metadata.
                From = data.Area;
                map = new Map(data);
                map.Rooms.ForEach(r => r.AllEntities.ForEach(e => e.InitializeAfter()));
            } else
                From = null;

            Engine.Scene = new Editor(map);
        }

        internal static void OpenNew() {
            Audio.Stop(Audio.CurrentAmbienceEventInstance);
            Audio.Stop(Audio.CurrentMusicEventInstance);

            Map map = null;

            Snowberry.Log(LogLevel.Info, $"Opening new map in level editor");
            // Also empties the target's metadata.
            map = new Map("snowberry map");
            map.Rooms.ForEach(r => r.AllEntities.ForEach(e => e.InitializeAfter()));
            From = null;

            Engine.Scene = new Editor(map);
        }

        private void MenuUI() {
            ui.Add(new UIMainMenu(uiBuffer.Width, uiBuffer.Height));
        }

        private void MappingUI() {
            Toolbar = new UIToolbar(this);
            ui.Add(Toolbar);
            Toolbar.Width = uiBuffer.Width;

            var nameLabel = new UILabel($"Map: {From?.SID ?? "(new map)"} (ID: {From?.ID ?? -1}, Mode: {From?.Mode ?? AreaMode.Normal})");
            ui.AddBelow(nameLabel);
            nameLabel.Position += new Vector2(10, 10);

            var roomLabel = new UILabel(() => $"Room: {SelectedRoom?.Name ?? (SelectedFillerIndex > -1 ? $"(filler: {SelectedFillerIndex})" : "(none)")}");
            ui.AddBelow(roomLabel);
            roomLabel.Position += new Vector2(10, 10);

            string editorreturn = Dialog.Clean("SNOWBERRY_EDITOR_RETURN");
            string editorplaytest = Dialog.Clean("SNOWBERRY_EDITOR_PLAYTEST");
            string editorexport = Dialog.Clean("SNOWBERRY_EDITOR_EXPORT");

            if (From.HasValue) {
                UIButton rtm = new UIButton(editorreturn, Fonts.Regular, 6, 6) {
                    OnPress = () => {
                        Audio.SetMusic(null);
                        Audio.SetAmbience(null);

                        SaveData.InitializeDebugMode();

                        LevelEnter.Go(new Session(From.Value), true);
                    }
                };
                ui.AddBelow(rtm);
            }

            UIButton test = new UIButton(editorplaytest, Fonts.Regular, 6, 6) {
                OnPress = () => {
                    Audio.SetMusic(null);
                    Audio.SetAmbience(null);

                    SaveData.InitializeDebugMode();

                    generatePlaytestMapData = true;
                    PlaytestMapData = new MapData(Map.From);
                    PlaytestSession = new Session(Map.From);
                    LevelEnter.Go(PlaytestSession, true);
                    generatePlaytestMapData = false;
                },
            };
            ui.AddBelow(test);

            UIButton export = new UIButton(editorexport, Fonts.Regular, 6, 6) {
                OnPress = () => {
                    BinaryExporter.ExportMap(Map);
                }
            };
            ui.AddBelow(export);

            SwitchTool(0);
        }

        public override void Begin() {
            base.Begin();
            Camera = new BufferCamera();
            uiBuffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.ViewWidth / 2, Engine.ViewHeight / 2);

            if (Map == null)
                MenuUI();
            else
                MappingUI();
        }

        public override void End() {
            base.End();
            Camera.Buffer?.Dispose();
            uiBuffer.Dispose();
            ui.Destroy();
        }

        public override void Update() {
            base.Update();

            Mouse.WorldLast = Mouse.World;
            Mouse.ScreenLast = Mouse.Screen;

            lastMousePos = mousePos;
            mousePos = MInput.Mouse.Position;

            // zooming
            bool canZoom = ui.CanScrollThrough();
            int wheel = Math.Sign(MInput.Mouse.WheelDelta);
            float scale = Camera.Zoom;
            if (canZoom) {
                if (wheel > 0)
                    scale = scale >= 1 ? scale + 1 : scale * 2f;
                else if (wheel < 0)
                    scale = scale > 1 ? scale - 1 : scale / 2f;
            }
            scale = Calc.Clamp(scale, 0.0625f, 24f);
            if (scale != Camera.Zoom)
                Camera.Zoom = scale;

            if (Camera.Buffer != null)
                mousePos /= Camera.Zoom;

            // controls
            bool canClick = ui.CanClickThrough();

			// panning
			bool middlePan = Snowberry.Settings.MiddleClickPan;
			if ((middlePan && MInput.Mouse.CheckMiddleButton || !middlePan && MInput.Mouse.CheckRightButton) && canClick) {
                Vector2 move = lastMousePos - mousePos;
                if (move != Vector2.Zero)
                    Camera.Position += move / (Camera.Buffer == null ? Camera.Zoom : 1f);
            }

            MouseState m = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 mouseVec = new Vector2(m.X, m.Y);
            Mouse.Screen = mouseVec / 2;
            Mouse.World = Calc.Round(Vector2.Transform(Camera.Buffer == null ? mouseVec : mousePos, Camera.Inverse));

            MouseClicked = false;
            ui.Update();

            // room & filler select
            if (Map != null) {
                if ((MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton) && canClick) {
                    if (MInput.Mouse.PressedLeftButton || MInput.Mouse.PressedRightButton) {
                        Point mouse = new Point((int)Mouse.World.X, (int)Mouse.World.Y);

                        worldClick = Mouse.World;
                        var before = SelectedRoom;
                        SelectedRoom = Map.GetRoomAt(mouse);
                        SelectedFillerIndex = Map.GetFillerIndexAt(mouse);
                        // don't let tools click when clicking onto new rooms
                        if (SelectedRoom != before)
                            canClick = false;
                    }
                }

                // tool updating
                var tool = Tool.Tools[Toolbar.CurrentTool];
                tool.Update(canClick);

                // keybinds
                if (MInput.Keyboard.Pressed(Keys.F)) {
                    FancyRender = !FancyRender;
                }
            }
        }

        public void SwitchTool(int toolIdx) {
            ToolPanel?.Destroy();
            ui.Remove(ToolPanel);

            Toolbar.CurrentTool = toolIdx;
            var tool = Tool.Tools[toolIdx];
            ToolPanel = tool.CreatePanel();
            ToolPanel.Position = new Vector2(uiBuffer.Width - ToolPanel.Width, Toolbar.Height);
            ToolPanel.Height = uiBuffer.Height - Toolbar.Height;
            ui.Add(ToolPanel);

            SelectedEntities = null;
        }

        public override void Render() {
            var tool = Map == null ? null : Tool.Tools[Toolbar.CurrentTool];

            #region UI Rendering

            Engine.Instance.GraphicsDevice.SetRenderTarget(uiBuffer);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            ui.Render();

            // Tooltip rendering
            var tooltip = ui.HoveredTooltip();
			if(tooltip != null) {
                var tooltipArea = Fonts.Regular.Measure(tooltip);
                var at = Mouse.Screen.Round() - new Vector2(0, tooltipArea.Y + 6);
                Draw.Rect(at, tooltipArea.X + 8, tooltipArea.Y + 6, Color.Black * 0.4f);
                Fonts.Regular.Draw(tooltip, at + new Vector2(4, 3), Vector2.One, Color.White);
			}

            Draw.SpriteBatch.End();

            #endregion

            #region Tool Rendering

            if (Map != null) {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                tool.RenderScreenSpace();
                Draw.SpriteBatch.End();
            }

            #endregion

            #region Map Rendering

            if (Camera.Buffer != null)
                Engine.Instance.GraphicsDevice.SetRenderTarget(Camera.Buffer);
            else
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);

            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            if (Map != null) {
                Map.Render(Camera);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix);
                tool.RenderWorldSpace();
                Draw.SpriteBatch.End();
            }

            #endregion

            #region Displaying on Backbuffer + HQRender

            if (Camera.Buffer != null) {
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);
                Engine.Instance.GraphicsDevice.Clear(bg);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                Draw.SpriteBatch.Draw(Camera.Buffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Camera.Zoom, SpriteEffects.None, 0f);
                Draw.SpriteBatch.End();
            }

            // HQRender
            if (Map != null)
                Map.HQRender(Camera);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            Draw.SpriteBatch.Draw(uiBuffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();

            #endregion
        }

        private static void CreatePlaytestMapDataHook(Action<MapData> orig_Load, MapData self) {
            if (!generatePlaytestMapData)
                orig_Load(self);
            else {
                if (Engine.Scene is Editor editor) {
                    editor.Map.GenerateMapData(self);
                } else orig_Load(self);
            }
        }

        private static MapData HookSessionGetAreaData(Func<Session, MapData> orig, Session self) {
            if (self.Area.SID == "Snowberry/Playtest") {
                return PlaytestMapData;
            }

            return orig(self);
        }

        internal static void CopyMapMeta(AreaData from, AreaData to) {
            to.ASideAreaDataBackup = from.ASideAreaDataBackup;
            to.BloomBase = from.BloomBase;
            to.BloomStrength = from.BloomStrength;
            to.CanFullClear = from.CanFullClear;
            to.CassetteSong = from.CassetteSong;
            to.CobwebColor = from.CobwebColor;
            to.ColorGrade = from.ColorGrade;
            to.CompleteScreenName = from.CompleteScreenName;
            to.CoreMode = from.CoreMode;
            to.CrumbleBlock = from.CrumbleBlock;
            to.DarknessAlpha = from.DarknessAlpha;
            to.Dreaming = from.Dreaming;
            to.Icon = from.Icon;
            to.Interlude = from.Interlude;
            to.IntroType = from.IntroType;
            to.IsFinal = from.IsFinal;
            to.Jumpthru = from.Jumpthru;
            to.Meta = from.Meta;
            to.Mode = from.Mode;
            to.Name = from.Name;
            to.Spike = from.Spike;
            // mountain meta?
            to.TitleAccentColor = from.TitleAccentColor;
            to.TitleBaseColor = from.TitleBaseColor;
            to.TitleTextColor = from.TitleTextColor;
            to.Wipe = from.Wipe;
            to.WoodPlatform = from.WoodPlatform;

            // hold onto info about vanilla's hardcoded stuff
            VanillaLevelID = from.IsOfficialLevelSet() ? from.ID : -1;
        }

        internal static void EmptyMapMeta(AreaData of) {
            CopyMapMeta(new AreaData(), of);
        }
    }
}
