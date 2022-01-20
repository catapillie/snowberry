using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Snowberry.Editor.Stylegrounds {
    [Plugin("parallax")]
    internal class Plugin_Parallax : Styleground {
        [Option("texture")] public string Texture = "";
        [Option("atlas")] public string Atlas = "game";
        [Option("blendmode")] public string BlendMode = "alphablend";
        [Option("fadeIn")] public bool FadeIn = false;

        public sealed override bool Additive => BlendMode == "additive";

        // image: "bgs/blah" in "game"
        public override string Title() {
            return $"{Dialog.Clean("SNOWBERRY_STYLEGROUNDS_IMAGE")}: \"{Texture}\" {(Atlas != "game" ? $"in {Atlas}" : "")}";
        }

        public override void Render(Room room) {
            base.Render(room);

            Editor editor = Editor.Instance;
            if (editor == null)
                return;

            MTexture mtex = (Atlas == "game" && GFX.Game.Has(Texture)) ? GFX.Game[Texture] : (Atlas == "gui" && GFX.Gui.Has(Texture) ? GFX.Gui[Texture] : GFX.Misc[Texture]);
            Vector2 cameraPos = editor.Camera.Position.Floor() - new Vector2(160, 90);
            Vector2 pos = (Position + cameraPos * (Vector2.One - Scroll)).Floor();

            if (Color.A <= 1)
                return;

            SpriteEffects flip = SpriteEffects.None;
            if (FlipX && FlipY) {
                flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            } else if (FlipX) {
                flip = SpriteEffects.FlipHorizontally;
            } else if (FlipY) {
                flip = SpriteEffects.FlipVertically;
            }

            int left = room.Bounds.Left * 8;
            int right = room.Bounds.Right * 8;
            int top = room.Bounds.Top * 8;
            int bottom = room.Bounds.Bottom * 8;

            if (LoopX) {
                while (pos.X < left)
                    pos.X += mtex.Width;
                while (pos.X > left)
                    pos.X -= mtex.Width;
            }

            if (LoopY) {
                while (pos.Y < top)
                    pos.Y += mtex.Height;
                while (pos.Y > top)
                    pos.Y -= mtex.Height;
            }

            Vector2 drawPos = pos;
            do {
                do {
                    mtex.Draw(drawPos, Vector2.Zero, Color, 1f, 0f, flip);
                    if (!LoopY)
                        break;
                    drawPos.Y += mtex.Height;
                } while (drawPos.Y < bottom);

                if (!LoopX)
                    break;
                drawPos.X += mtex.Width;
                drawPos.Y = pos.Y;
            } while (drawPos.X < right);
        }
    }
}