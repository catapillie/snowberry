using Celeste;
using Celeste.Mod;
using LevelEditorMod.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    public class Editor : Scene {
        internal class Camera {
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

            private Matrix matrix, inverse;
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

            public Rectangle ViewRect { get; private set; }

            public RenderTarget2D Buffer { get; private set; }

            public Camera() {
                Buffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.Width, Engine.Height);
            }

            private void UpdateMatrices() {
                Matrix m = Matrix.CreateTranslation((int)-Position.X, (int)-Position.Y, 0f) * Matrix.CreateScale(Math.Min(1f, Zoom));
                if (Buffer != null) {
                    m *= Matrix.CreateTranslation(Buffer.Width / 2, Buffer.Height / 2, 0f);
                    ViewRect = new Rectangle((int)Position.X - Buffer.Width / 2, (int)Position.Y - Buffer.Height / 2, Buffer.Width, Buffer.Height);
                } else {
                    m *= Engine.ScreenMatrix * Matrix.CreateTranslation(Engine.ViewWidth / 2, Engine.ViewHeight / 2, 0f);
                    int w = (int)(Engine.Width / Zoom);
                    int h = (int)(Engine.Height / Zoom);
                    ViewRect = new Rectangle((int)Position.X - w / 2, (int)Position.Y - h / 2, w, h);
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

        private static readonly Color bg = Calc.HexToColor("060607");

        private Camera camera;

        public Vector2 mousePos, lastMousePos;
        public Vector2 worldClick;

        public Map Map { get; private set; }

        private readonly UIElement ui = new UIElement();
        private RenderTarget2D uiBuffer;

        internal static Rectangle? Selection;
        internal static Room SelectedRoom;
        internal static List<EntitySelection> SelectedEntities;

        public UIToolbar Toolbar;
        public UIElement ToolPanel;

        private static bool generatePlaytestMapData = false;
        private static Session playtestSession;
        private static MapData playtestMapData;

        private Editor(Map map) {
            Engine.Instance.IsMouseVisible = true;

            Map = map;
        }

        internal static void Open(MapData data) {
            Map map = new Map(data);

            Module.Log(LogLevel.Info, $"Opening level editor using map {data.Area.GetSID()}");

            Audio.Stop(Audio.CurrentAmbienceEventInstance);
            Audio.Stop(Audio.CurrentMusicEventInstance);

            Engine.Scene = new Editor(map);
        }

        public override void Begin() {
            base.Begin();
            camera = new Camera();
            uiBuffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.ViewWidth / 2, Engine.ViewHeight / 2);

            Toolbar = new UIToolbar(this);
            ui.Add(Toolbar);
            Toolbar.Width = uiBuffer.Width;

            var nameLabel = new UILabel($"Map: {Map.From.SID} ({Map.From.Mode})");
            ui.AddBelow(nameLabel);
            nameLabel.Position += new Vector2(10, 0);

            var roomLabel = new UILabel(() => $"Room: {SelectedRoom?.Name ?? "none"}");
            ui.AddBelow(roomLabel);
            roomLabel.Position += new Vector2(10, 10);

            UIButton rtm = new UIButton("Return to Map", Fonts.Regular, 6, 6) {
				OnPress = () => {
                    Audio.SetMusic(null);
                    Audio.SetAmbience(null);

                    LevelEnter.Go(new Session(Map.From), true);
				}
			};
			ui.AddBelow(rtm);

            UIButton test = new UIButton("Playtest", Fonts.Regular, 6, 6) {
                OnPress = () => {
                    Audio.SetMusic(null);
                    Audio.SetAmbience(null);

                    generatePlaytestMapData = true;
                    playtestMapData = new MapData(Map.From);
                    playtestSession = new Session(Map.From);
                    LevelEnter.Go(playtestSession, true);
                    generatePlaytestMapData = false;
                },
            };
            ui.AddBelow(test);

            SwitchTool(0);
        }

        public override void End() {
            base.End();
            camera.Buffer?.Dispose();
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
            int wheel = Math.Sign(MInput.Mouse.WheelDelta);
            float scale = camera.Zoom;
            if (wheel > 0)
                scale = scale >= 1 ? scale + 1 : scale * 2f;
            else if (wheel < 0)
                scale = scale > 1 ? scale - 1 : scale / 2f;
            scale = Calc.Clamp(scale, 0.0625f, 24f);
            if (scale != camera.Zoom)
                camera.Zoom = scale;

            if (camera.Buffer != null)
                mousePos /= camera.Zoom;

            // panning
            if (MInput.Mouse.CheckRightButton) {
                Vector2 move = lastMousePos - mousePos;
                if (move != Vector2.Zero)
                    camera.Position += move / (camera.Buffer == null ? camera.Zoom : 1f);
            }

            MouseState m = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 mouseVec = new Vector2(m.X, m.Y);
            Mouse.Screen = mouseVec / 2;
            Mouse.World = Calc.Round(Vector2.Transform(camera.Buffer == null ? mouseVec : mousePos, camera.Inverse));

            ui.Update();

            // room select
            bool shift = MInput.Keyboard.CurrentState[Keys.LeftShift] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightShift] == KeyState.Down;
            if(MInput.Mouse.CheckLeftButton && shift) {
                if(MInput.Mouse.PressedLeftButton) {
                    Point mouse = new Point((int)Mouse.World.X, (int)Mouse.World.Y);

                    worldClick = Mouse.World;
                    SelectedRoom = Map.GetRoomAt(mouse);
                }
            }

            // tool updating
            var tool = Tool.Tools[Toolbar.CurrentTool];
            tool.Update();
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

        public static Editor GetCurrent() {
            if(Engine.Scene is Editor editor)
                return editor;
            return null;
        }

        public override void Render() {
            var tool = Tool.Tools[Toolbar.CurrentTool];

            #region UI Rendering

            Engine.Instance.GraphicsDevice.SetRenderTarget(uiBuffer);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            ui.Render();
            
            Draw.SpriteBatch.End();

            #endregion

            #region Tool Rendering

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            tool.RenderScreenSpace();
            Draw.SpriteBatch.End();

            #endregion

            #region Map Rendering

            if(camera.Buffer != null)
                Engine.Instance.GraphicsDevice.SetRenderTarget(camera.Buffer);
            else
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);

            Engine.Instance.GraphicsDevice.Clear(bg);
            Map.Render(camera);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
            tool.RenderWorldSpace();
            Draw.SpriteBatch.End();

            #endregion

            #region Displaying on Backbuffer

            if (camera.Buffer != null) {
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                Draw.SpriteBatch.Draw(camera.Buffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0f);
                Draw.SpriteBatch.End();
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            Draw.SpriteBatch.Draw(uiBuffer, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();

            #endregion
        }

        private static void CreatePlaytestMapDataHook(Action<MapData> orig_Load, MapData self) {
            if(!generatePlaytestMapData)
                orig_Load(self);
            else {
                //CreatePlaytestMapData(self);
                if(Engine.Scene is Editor editor) {
                    editor.Map.GenerateMapData(self);
                } else orig_Load(self);
            }
		}

        private static MapData HookSessionGetAreaData(Func<Session, MapData> orig, Session self) {
            if(self == playtestSession) {
                return playtestMapData;
			}

			return orig(self);
        }
    }
}
