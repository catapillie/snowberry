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
        private Vector2 mousePos, lastMousePos;
        private Vector2 worldClick;

        private Map map;

        private readonly UIElement ui = new UIElement();
        private RenderTarget2D uiBuffer;

        internal static Rectangle? Selection;
        internal static Room SelectedRoom;
        internal static List<EntitySelection> SelectedEntities;
        private bool canSelect;

        private static bool generatePlaytestMapData = false;
        private static Session playtestSession;
        private static MapData playtestMapData;

        private Editor(Map map) {
            Engine.Instance.IsMouseVisible = true;

            this.map = map;
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

            var nameLabel = new UILabel($"Map: {map.From.SID} ({map.From.Mode})");
            ui.AddBelow(nameLabel);
            nameLabel.Position += new Vector2(10, 0);

            var roomLabel = new UILabel(() => $"Room: {SelectedRoom?.Name ?? "none"}");
            ui.AddBelow(roomLabel);
            roomLabel.Position += new Vector2(10, 10);

            UIButton rtm = new UIButton("Return to Map", Fonts.Regular, 6, 6) {
				OnPress = () => LevelEnter.Go(new Session(map.From), true)
			};
			ui.AddBelow(rtm);

            UIButton test = new UIButton("Playtest", Fonts.Regular, 6, 6) {
                OnPress = () => {
                    generatePlaytestMapData = true;
                    playtestMapData = new MapData(map.From);
                    playtestSession = new Session(map.From);
                    LevelEnter.Go(playtestSession, true);
                    generatePlaytestMapData = false;
                },
            };
            ui.AddBelow(test);
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

            // controls
            if (MInput.Mouse.CheckLeftButton) {
                if (MInput.Mouse.PressedLeftButton) {
                    Point mouse = new Point((int)Mouse.World.X, (int)Mouse.World.Y);

                    worldClick = Mouse.World;
                    SelectedRoom = map.GetRoomAt(mouse);

                    canSelect = true;
                    if (SelectedEntities != null) {
                        foreach (EntitySelection s in SelectedEntities) {
                            if (s.Contains(mouse)) {
                                canSelect = false;
                                break;
                            }
                        }
                    }
                }

                if (canSelect && SelectedRoom != null) {
                    int ax = (int)Math.Min(Mouse.World.X, worldClick.X);
                    int ay = (int)Math.Min(Mouse.World.Y, worldClick.Y);
                    int bx = (int)Math.Max(Mouse.World.X, worldClick.X);
                    int by = (int)Math.Max(Mouse.World.Y, worldClick.Y);
                    Selection = new Rectangle(ax, ay, bx - ax, by - ay);

                    SelectedEntities = SelectedRoom.GetSelectedEntities(Selection.Value);
                } else if (SelectedEntities != null) {
                    foreach (EntitySelection s in SelectedEntities)
                        s.Move(Mouse.World - Mouse.WorldLast);
                }
            } else
                Selection = null;
        }

        public override void Render() {
            #region UI Rendering

            Engine.Instance.GraphicsDevice.SetRenderTarget(uiBuffer);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            ui.Render();
            
            Draw.SpriteBatch.End();

            #endregion

			#region Map Rendering

			if(camera.Buffer != null)
                Engine.Instance.GraphicsDevice.SetRenderTarget(camera.Buffer);
            else
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);

            Engine.Instance.GraphicsDevice.Clear(bg);
            map.Render(camera);

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
                    editor.map.GenerateMapData(self);
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
