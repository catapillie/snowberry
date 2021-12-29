using Celeste;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor.Entities {
    [Plugin("npc")]
    public class Plugin_NPC : Entity {
        [Option("npc")] public string NPC = "";

        public static string GrannySprite = "characters/oldlady/idle00";
        public static string TheoSprite = "characters/theo/theo00";
        public static string OshiroSprite = "characters/oshiro/oshiro24";
        public static string BadelineSprite = "characters/badeline/sleep00";

        public override void Render() {
            base.Render();

            GFX.Game[NPC switch {
                "granny_00_house" => GrannySprite,
                "granny_04_cliffside" => GrannySprite,
                "granny_06_intro" => GrannySprite,
                "granny_06_ending" => GrannySprite,
                "granny_07x" => GrannySprite,
                "granny_08_inside" => GrannySprite,
                "granny_09_outside" => GrannySprite,
                "granny_09_inside" => GrannySprite,
                "granny_10_never" => GrannySprite,

                "theo_01_campfire" => TheoSprite,
                "theo_02_campfire" => TheoSprite,
                "theo_03_escaping" => TheoSprite,
                "theo_03_vents" => "characters/theo/theo64",
                "theo_04_cliffside" => TheoSprite,
                "theo_05_entrance" => TheoSprite,
                "theo_05_inmirror" => TheoSprite,
                "theo_06_plateau" => TheoSprite,
                "theo_06_ending" => TheoSprite,
                "theo_08_inside" => TheoSprite,

                "oshiro_03_lobby" => OshiroSprite,
                "oshiro_03_hallway" => OshiroSprite,
                "oshiro_03_hallway2" => OshiroSprite,
                "oshiro_03_bigroom" => OshiroSprite,
                "oshiro_03_breakdown" => OshiroSprite,
                "oshiro_03_suite" => "characters/oshiro/oshiro80",
                "oshiro_03_rooftop" => OshiroSprite,

                "evil_05" => BadelineSprite,
                "badeline_06_crying" => BadelineSprite, // not technically accurate but hair

                "gravestone_10" => "decals/10-farewell/grave",
                _ => "",
            }].DrawJustified(Position, new Vector2(0.5f, 1f), Color.White * 0.75f);
        }

        public static void AddPlacements() {
            Placements.Create("NPC", "npc");
        }
    }
}