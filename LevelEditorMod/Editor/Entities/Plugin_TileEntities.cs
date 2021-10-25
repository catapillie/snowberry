using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    public abstract class Plugin_TileEntityBase : Entity {
        protected virtual float Alpha => 1.0f;
        protected VirtualMap<MTexture> Tiles;

        public override void Render() {
            base.Render();

            if (Tiles != null) {
                Color c = Color.White * Alpha;
                for (int x = 0; x < Tiles.Columns; x++)
                    for (int y = 0; y < Tiles.Rows; y++)
                        Tiles[x, y]?.Draw(Position + new Vector2(x, y) * 8, Vector2.Zero, c);
            }
        }
    }

    public abstract class Plugin_TileEntity : Plugin_TileEntityBase {
        [Option("tiletype")] public char TileType = '3';

        public override void Initialize() {
            base.Initialize();
            Tiles = GFX.FGAutotiler.GenerateBox(TileType, Width / 8, Height / 8).TileGrid.Tiles;
        }
    }

    [Plugin("introCrusher")]
    public class Plugin_IntroCrusher : Plugin_TileEntity {
        [Option("flags")] public string Flags = "1,0b";

        public override void Render() {
            base.Render();

            if (Tiles != null && Nodes.Length > 0) {
                Color c = Color.White * 0.25f;
                for (int x = 0; x < Tiles.Columns; x++)
                    for (int y = 0; y < Tiles.Rows; y++)
                        Tiles[x, y]?.Draw(Nodes[0] + new Vector2(x, y) * 8, Vector2.Zero, c);
            }
            DrawUtil.DottedLine(Center, Nodes[0] + new Vector2(Width, Height) / 2, Color.White, 8, 4);
        }
    }

    [Plugin("finalBossFallingBlock")]
    public class Plugin_BadelineBossFallingBlock : Plugin_TileEntityBase {
        public override void Initialize() {
            base.Initialize();
            Tiles = GFX.FGAutotiler.GenerateBox('g', Width / 8, Height / 8).TileGrid.Tiles;
        }
    }

    [Plugin("coverupWall")]
    [Plugin("fakeWall")]
    public class Plugin_FakeWall : Plugin_TileEntity {
        protected override float Alpha => 0.7f;
    }

    [Plugin("fakeBlock")]
    [Plugin("exitBlock")] 
    public class Plugin_FakeBlock : Plugin_FakeWall {
        [Option("playTransitionReveal")] public bool PlayTransitionReveal = false;
    }

    [Plugin("conditionBlock")]
    public class Plugin_ConditionBlock : Plugin_FakeWall {
        [Option("condition")] public string Condition = "Key";
        [Option("conditionID")] public string ConditionID = "1:1";
    }

    [Plugin("floatySpaceBlock")]
    public class Plugin_FloatySpaceBlock : Plugin_TileEntity {
        [Option("disableSpawnOffset")] public bool DisableOffset = false;
    }

    [Plugin("crumbleWallOnRumble")]
    public class Plugin_CrumbleWallOnRumble : Plugin_TileEntity {
        [Option("blendin")] public bool BlendIn = true;
        [Option("persistent")] public bool Persistent = false;

        public override void ChangeDefault() {
            base.ChangeDefault();
            TileType = 'm';
        }
    }

    [Plugin("dashBlock")]
    public class Plugin_DashBlock : Plugin_TileEntity {
        [Option("blendin")] public bool BlendIn = true;
        [Option("canDash")] public bool CanDash = true;
        [Option("permanent")] public bool Permanent = false;
    }

    [Plugin("fallingBlock")]
    public class Plugin_FallingBlock : Plugin_TileEntity {
        [Option("climbFall")] public bool ClimbFall = true;
        [Option("behind")] public bool Behind = false;
    }
}
