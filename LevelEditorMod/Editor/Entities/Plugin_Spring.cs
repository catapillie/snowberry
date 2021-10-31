using Celeste;
using Microsoft.Xna.Framework;
using System;

namespace LevelEditorMod.Editor.Entities {

    [Plugin("spring")]
    [Plugin("wallSpringLeft")]
    [Plugin("wallSpringRight")]
    public class Plugin_Spring : Entity {

        public Spring.Orientations Dir = Spring.Orientations.Floor;

        public override void Initialize() {
            base.Initialize();

            Dir = Name switch {
                "spring" => Spring.Orientations.Floor,
                "wallSpringLeft" => Spring.Orientations.WallLeft,
                "wallSpringRight" => Spring.Orientations.WallRight,
                _ => Spring.Orientations.Floor,
            };
        }

        public override void Render() {
            base.Render();
            
            GFX.Game[$"objects/spring/00"].DrawJustified(Position, new Vector2(0.5f, 1), Color.White, 1, Dir switch{
                Spring.Orientations.Floor => 0,
                Spring.Orientations.WallLeft => (float)Math.PI / 2f,
                Spring.Orientations.WallRight => -(float)Math.PI / 2f,
                _ => 0
            });
        }
    }
}
