using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace LevelEditorMod.Editor {
    public class LevelEditor : Scene {
        private static readonly Color bg = Calc.HexToColor("121212");

        private Vector2 mousePos, lastMousePos;
        private Vector2 mouseWorldPos, lastMouseWorldPos;
        private readonly Camera camera;

        private Map map;

        private LevelEditor(Map map) {
            Engine.Instance.IsMouseVisible = true;
            camera = new Camera();
            camera.CenterOrigin();

            this.map = map;
        }

        internal static void Open(MapData data) {
            Map map = new Map(data);

            Module.Log(LogLevel.Info, $"Opening level editor using map {data.Area.GetSID()}/{data.Filename}");

            Audio.Stop(Audio.CurrentAmbienceEventInstance);
            Audio.Stop(Audio.CurrentMusicEventInstance);

            Engine.Scene = new LevelEditor(map);
        }

        public override void Update() {
            base.Update();
            mousePos = MInput.Mouse.Position;

            // zooming
            int wheel = Math.Sign(MInput.Mouse.WheelDelta);
            if (wheel != 0) {
                if ((wheel > 0 && camera.Zoom >= 1f) || camera.Zoom > 1f) {
                    camera.Zoom += wheel;
                } else {
                    camera.Zoom += wheel * 0.1f;
                }
                camera.Zoom = Math.Max(0.1f, Math.Min(24f, camera.Zoom));
            }

            // panning
            if (MInput.Mouse.CheckRightButton)
                camera.Position += (lastMousePos - mousePos) / camera.Zoom;

            mouseWorldPos = Vector2.Transform(mousePos, camera.Inverse);

            lastMousePos = mousePos;
            lastMouseWorldPos = mouseWorldPos;
        }

        public override void Render() {
            Engine.Instance.GraphicsDevice.Clear(bg);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix * Engine.ScreenMatrix);
            
            int left = (int)camera.Left;
            int right = (int)camera.Right;
            int top = (int)camera.Top;
            int bottom = (int)camera.Bottom;
            Rectangle viewRect = new Rectangle(left, top, right - left, bottom - top);

            map.Render(viewRect);

            Draw.SpriteBatch.End();


            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null);
            Draw.Text(Draw.DefaultFont, $"Currently editing : {map.Name}...\nCamera : {camera.Position}, Zoom : {camera.Zoom}", Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}
