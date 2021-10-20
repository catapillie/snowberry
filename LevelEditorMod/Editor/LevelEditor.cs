using Celeste;
using Celeste.Mod;
using LevelEditorMod.Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace LevelEditorMod.Editor {
    public class LevelEditor : Scene {
        internal class Camera {
            private bool changedView;

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

            public Camera() {
                Buffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.Width, Engine.Height);
            }
        }

        private static readonly Color bg = Calc.HexToColor("060607");

        private Camera camera;
        private Vector2 mousePos, lastMousePos;
        private Vector2 mouseWorldPos, lastMouseWorldPos;

        private Map map;

        private readonly UIElement ui = new UIElement();
        private RenderTarget2D uiBuffer;

        //private readonly static FormattedText infoText = FormattedText.Parse("Currently editing : {#00dce8}{map}{#<<}...\n{#cfa51d}Camera : [{#b343bf}{camx}{#<<}, {#b343bf}{camy}{#<<}] ×{#b343bf}{zoom}");

        private LevelEditor(Map map) {
            Engine.Instance.IsMouseVisible = true;

            ui.Add(new UIButton("◀- back to map", Fonts.Regular) {
                FG = Calc.HexToColor("f0f0f0"),
                BG = Calc.HexToColor("db2323"),
                PressedBG = Calc.HexToColor("f0f0f0"),
                PressedFG = Calc.HexToColor("db2323"),
            });

            this.map = map;
        }

        internal static void Open(MapData data) {
            Map map = new Map(data);

            Module.Log(LogLevel.Info, $"Opening level editor using map {data.Area.GetSID()}");

            Audio.Stop(Audio.CurrentAmbienceEventInstance);
            Audio.Stop(Audio.CurrentMusicEventInstance);

            Engine.Scene = new LevelEditor(map);
        }

        public override void Update() {
            base.Update();

            lastMouseWorldPos = mouseWorldPos;
            lastMousePos = mousePos;
            mousePos = MInput.Mouse.Position;
            
            // zooming
            int wheel = Math.Sign(MInput.Mouse.WheelDelta);
            float s = camera.Zoom;
            if (wheel > 0)
                s = s >= 1 ? s + 1 : s * 2f;
            else if (wheel < 0)
                s = s > 1 ? s - 1 : s / 2f;
            s = Calc.Clamp(s, 0.0625f, 24f);
            if (s != camera.Zoom)
                camera.Zoom = s;

            if (camera.Buffer != null)
                mousePos /= camera.Zoom;

            // panning
            if (MInput.Mouse.CheckRightButton) {
                Vector2 move = lastMousePos - mousePos;
                if (move != Vector2.Zero)
                    camera.Position += move / (camera.Buffer == null ? camera.Zoom : 1f);
            }

            mouseWorldPos = Vector2.Transform(mousePos, camera.Inverse);

            ui.Update();
        }

        public override void Begin() {
            base.Begin();
            camera = new Camera();
            uiBuffer = new RenderTarget2D(Engine.Instance.GraphicsDevice, Engine.Width / 2, Engine.Height / 2);
        }

        public override void End() {
            base.End();
            camera.Buffer?.Dispose();
            uiBuffer.Dispose();
        }

        public override void Render() {
            #region UI Rendering

            Engine.Instance.GraphicsDevice.SetRenderTarget(uiBuffer);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            ui.Render();
            //Fonts.Regular.Draw(infoText, Vector2.Zero, Vector2.One, Vector2.Zero, map.Name, camera.X, camera.Y, camera.Zoom);
            Draw.SpriteBatch.End();

            #endregion

            #region Map Rendering

            if (camera.Buffer != null)
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
    }
}
