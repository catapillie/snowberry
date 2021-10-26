using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("seekerStatue")]
    public class Plugin_SeekerStatue : Entity {
        public override void Render() {
            base.Render();

            GFX.Game["decals/5-temple/statue_e"].DrawCentered(Position);

            MTexture seeker = GFX.Game["characters/monsters/predator73"];
            Vector2 prev = Position;
            foreach (Vector2 node in Nodes) {
                seeker.DrawCentered(node);
                Draw.Line(prev, node, Color.White * 0.5f);
                prev = node;
            }
        }
    }
}
