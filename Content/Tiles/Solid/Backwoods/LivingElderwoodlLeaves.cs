using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Utilities.Extensions;
using RoA.Content.Gores;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class LivingElderwoodlLeaves : ModTile {
    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false, brick: false);
        TileHelper.MergeWith(Type, TileID.Dirt);

        Main.tileMerge[Type][(ushort)ModContent.TileType<BackwoodsGrass>()] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

        TileID.Sets.Leaves[Type] = true;
        TileID.Sets.ForcedDirtMerging[Type] = true;

        HitSound = SoundID.Grass;
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(0, 128, 0));

        MineResist = 0.01f;
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttemptFrametest(i, j, ModContent.TileType<LivingElderwoodlLeaves>(), ModContent.TileType<LivingElderwood>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }

    public override bool CanDrop(int i, int j) => false;

    public override void RandomUpdate(int i, int j) {
        if (Main.tile[i, j].HasUnactuatedTile) {
            int num34 = 10;
            if (Main.rand.Next(num34) == 0 && WorldGen.GrowMoreVines(i, j) && !Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].LiquidType != LiquidID.Lava) {
                bool flag5 = false;
                ushort type7 = (ushort)ModContent.TileType<BackwoodsVines>();
                if (Main.tile[i, j].WallType == 68 || Main.tile[i, j].WallType == ModContent.WallType<BackwoodsGrassWall>() || Main.tile[i, j].WallType == ModContent.WallType<BackwoodsFlowerGrassWall>() || Main.tile[i, j].WallType == 65 || Main.tile[i, j].WallType == 66 || Main.tile[i, j].WallType == 63)
                    type7 = (ushort)ModContent.TileType<BackwoodsVinesFlower>();
                else if (Main.tile[i, j + 1].WallType == 68 || Main.tile[i, j + 1].WallType == ModContent.WallType<BackwoodsGrassWall>() || Main.tile[i, j + 1].WallType == ModContent.WallType<BackwoodsFlowerGrassWall>() || Main.tile[i, j + 1].WallType == 65 || Main.tile[i, j + 1].WallType == 66 || Main.tile[i, j + 1].WallType == 63)
                    type7 = (ushort)ModContent.TileType<BackwoodsVinesFlower>();

                //if (Main.remixWorld && genRand.Next(5) == 0)
                //    type7 = 382;

                for (int num35 = j; num35 > j - 10; num35--) {
                    if (Main.tile[i, num35].BottomSlope) {
                        flag5 = false;
                        break;
                    }

                    if (Main.tile[i, num35].HasTile && Main.tile[i, num35].TileType == Type && !Main.tile[i, num35].BottomSlope) {
                        flag5 = true;
                        break;
                    }
                }

                if (flag5) {
                    int num36 = j + 1;
                    Tile tile = Main.tile[i, num36];
                    Main.tile[i, num36].TileType = type7;
                    tile.HasTile = true;
                    Main.tile[i, num36].CopyPaintAndCoating(Main.tile[i, j]);
                    WorldGen.SquareTileFrame(i, num36);
                    if (Main.netMode == 2)
                        NetMessage.SendTileSquare(-1, i, num36);
                }
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        // adapted vanilla
        IEntitySource entitySource = new EntitySource_TileUpdate(i, j);
        ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
        int x = i, y = j;
        if (Main.rand.Next(typeof(TileDrawing).GetFieldValue<int>("_leafFrequency", Main.instance.TilesRenderer)) == 0) {
            Tile tile = Main.tile[x, y + 1];
            if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                float windForVisuals = Main.WindForVisuals;
                if (!Main.dedServ) {
                    if ((!(windForVisuals < -0.2f) || (!WorldGen.SolidTile(Main.tile[x - 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x - 2, y + 1]))) && (!(windForVisuals > 0.2f) || (!WorldGen.SolidTile(Main.tile[x + 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x + 2, y + 1]))))
                        Gore.NewGorePerfect(entitySource, new Vector2(x * 16, y * 16 + 16), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                }
            }
            if (Main.rand.NextBool()) {
                int num = 0;
                if (Main.WindForVisuals > 0.2f)
                    num = 1;
                else if (Main.WindForVisuals < -0.2f)
                    num = -1;

                tile = Main.tile[x + num, y];
                if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                    int num2 = 0;
                    if (num == -1)
                        num2 = -10;

                    if (!Main.dedServ) {
                        Gore.NewGorePerfect(entitySource, new Vector2(x * 16 + 8 + 4 * num + num2, y * 16 + 8), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                    }
                }
            }
        }
    }
}