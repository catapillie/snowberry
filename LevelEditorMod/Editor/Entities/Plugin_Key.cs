using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("key")]
    public class Plugin_Key : Entity {
        public override void Render() {
            base.Render();
            GFX.Game["collectables/key/idle00"].DrawCentered(Position);
        }

        public static void AddPlacements() {
            Placements.Create("Key", "key");
        }
    }
}
