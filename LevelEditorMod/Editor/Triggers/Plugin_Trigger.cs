using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Triggers {
    public abstract class Plugin_Trigger : Entity {
        protected virtual Color Color { get; } = Calc.HexToColor("0c5f7a");

        public override void Render() {
            base.Render();

            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Draw.Rect(rect, Color * 0.3f);
            Draw.HollowRect(rect, Color);

            Fonts.Pico8.Draw(Name, new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f), Vector2.One, Vector2.One * 0.5f, Color.Black); ;
        }
    }
}
