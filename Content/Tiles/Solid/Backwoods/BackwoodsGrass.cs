using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGrass : ModTile, IPostSetupContent {
    void IPostSetupContent.PostSetupContent() {
        for (int i = 0; i < TileLoader.TileCount; i++) {
            TileObjectData objData = TileObjectData.GetTileData(i, 0);
            if (objData == null || objData.AnchorValidTiles == null || objData.AnchorValidTiles.Length == 0) {
                continue;
            }

            if (i != TileID.AshPlants && i != TileID.AshVines && i != TileID.TreeAsh) {
                if (objData.AnchorValidTiles.Any(tileId => tileId == TileID.AshGrass)) {
                    lock (objData) {
                        int[] anchorAlternates = objData.AnchorValidTiles;
                        Array.Resize(ref anchorAlternates, anchorAlternates.Length + 1);
                        anchorAlternates[^1] = ModContent.TileType<BackwoodsGrass>();
                        objData.AnchorValidTiles = anchorAlternates;
                    }
                }
            }
        }
    }

    public override void Load() {
        On_Player.DoBootsEffect_PlaceFlowersOnTile += On_Player_DoBootsEffect_PlaceFlowersOnTile;
        On_WorldGen.PlaceSunflower += On_WorldGen_PlaceSunflower;
    }

    private void On_WorldGen_PlaceSunflower(On_WorldGen.orig_PlaceSunflower orig, int x, int y, ushort type) {
        if ((double)y > Main.worldSurface - 1.0 && !Main.remixWorld)
            return;

        bool flag = true;
        for (int i = x; i < x + 2; i++) {
            for (int j = y - 3; j < y + 1; j++) {
                //if (Main.tile[i, j] == null)
                //    Main.tile[i, j] = new Tile();

                if (Main.tile[i, j].HasTile || Main.tile[i, j].WallType > 0)
                    flag = false;
            }

            //if (Main.tile[i, y + 1] == null)
            //    Main.tile[i, y + 1] = new Tile();

            if (!Main.tile[i, y + 1].HasUnactuatedTile || Main.tile[i, y + 1].IsHalfBlock || Main.tile[i, y + 1].Slope != 0 || (Main.tile[i, y + 1].TileType != ModContent.TileType<BackwoodsGrass>() && Main.tile[i, y + 1].TileType != 2 && Main.tile[i, y + 1].TileType != 109))
                flag = false;
        }

        if (!flag)
            return;

        int num = WorldGen.genRand.Next(3);
        for (int k = 0; k < 2; k++) {
            for (int l = -3; l < 1; l++) {
                int num2 = k * 18 + WorldGen.genRand.Next(3) * 36;
                if (l <= -2)
                    num2 = k * 18 + num * 36;

                int num3 = (l + 3) * 18;
                Tile tile = Main.tile[x + k, y + l];
                tile.HasTile = true;
                tile.TileFrameX = (short)num2;
                tile.TileFrameY = (short)num3;
                tile.TileType = type;
            }
        }
    }

    private bool On_Player_DoBootsEffect_PlaceFlowersOnTile(On_Player.orig_DoBootsEffect_PlaceFlowersOnTile orig, Player self, int X, int Y) {
        Tile tile = WorldGenHelper.GetTileSafely(X, Y);
        if (!tile.HasTile && tile.LiquidAmount == 0 && Main.tile[X, Y + 1] != null && WorldGen.SolidTile(X, Y + 1)) {
            tile.TileFrameY = 0;
            tile.Slope = 0;
            tile.IsHalfBlock = false;

            if (Main.tile[X, Y + 1].TileType == ModContent.TileType<BackwoodsGrass>()) {
                tile.HasTile = true;
                tile.TileType = (ushort)ModContent.TileType<BackwoodsPlants>();
                tile.TileFrameX = (short)(18 * Main.rand.Next(20));
                tile.TileFrameY = 0;
                tile.CopyPaintAndCoating(Main.tile[X, Y + 1]);
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendTileSquare(-1, X, Y);
                }

                return true;
            }
        }

        return orig(self, X, Y);
    }

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileID.Sets.Conversion.MergesWithDirtInASpecialWay[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        TransformTileSystem.ReplaceToTypeOnKill[Type] = TileID.Dirt;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(38, 107, 57));
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => EmitDusts(i, j);

    public override bool CanDrop(int i, int j) => false;

    public static void EmitDusts(int i, int j) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        if (Main.rand.NextBool(300)) {
            Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<BackwoodsDust>());
        }
    }

    public override void RandomUpdate(int i, int j) {
        int x = i, y = j;
        if (Main.tile[i, j].LiquidAmount <= 32 && !Framing.GetTileSafely(x, y - 1).HasTile && Main.tile[i, j].HasUnactuatedTile && Main.tile[i, j].Slope == 0 && !Main.tile[i, j].IsHalfBlock &&
            (j > Main.worldSurface - 1 || Main.rand.NextBool(2))) {
            if (Main.rand.NextChance(0.045)) {
                //int mintType = ModContent.TileType<MiracleMint>();
                //int style = Main.rand.Next(2);
                //if (WorldGen.PlaceTile(x, y - 1, mintType, true, style: style)) {
                //    Main.tile[x, y - 1].CopyPaintAndCoating(Main.tile[i, j]);
                //}
                //if (Main.netMode == NetmodeID.Server && Main.tile[i, y - 1].HasTile) {
                //    NetMessage.SendTileSquare(-1, i, y - 1);
                //}
                //ModContent.GetInstance<MiracleMintTE>().Place(x, y - 1);
                //if (Main.netMode != NetmodeID.SinglePlayer) {
                //    MultiplayerSystem.SendPacket(new PlaceMiracleMintPacket(x, y - 1));
                //}
            }
            else {
                if (Main.rand.NextChance(0.15)) {
                    if (WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<BackwoodsBush>(), true, style: Main.rand.Next(4))) {
                        Main.tile[x, y - 1].CopyPaintAndCoating(Main.tile[i, j]);
                    }
                    if (Main.netMode == NetmodeID.Server && Main.tile[i, y - 1].HasTile) {
                        NetMessage.SendTileSquare(-1, i, y - 1);
                    }

                    return;
                }
                if (WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<BackwoodsPlants>(), true, style: Main.rand.Next(20))) {
                    Main.tile[x, y - 1].CopyPaintAndCoating(Main.tile[i, j]);
                }
                if (Main.netMode == NetmodeID.Server && Main.tile[i, y - 1].HasTile) {
                    NetMessage.SendTileSquare(-1, i, y - 1);
                }
            }
        }

        if (Main.tile[i, j].HasUnactuatedTile) {
            int num34 = 1;
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

        int minI = i - 1;
        int maxI = i + 2;
        int minJ = j - 1;
        int maxJ = j + 2;
        if (WorldGen.InWorld(i, j, 10)) {
            int num2 = Main.tile[i, j].TileType;
            TileColorCache color2 = Main.tile[i, j].BlockColorAndCoating();
            bool flag6 = false;
            for (int num3 = minI; num3 < maxI; num3++) {
                for (int num4 = minJ; num4 < maxJ; num4++) {
                    if ((i != num3 || j != num4) && Main.tile[num3, num4].HasTile && Main.tile[num3, num4].TileType == TileID.Dirt) {
                        WorldGen.SpreadGrass(num3, num4, TileID.Dirt, num2, repeat: false, color2);
                        if (Main.tile[num3, num4].TileType == num2) {
                            WorldGen.SquareTileFrame(num3, num4);
                            flag6 = true;
                        }
                    }
                }
            }
            if (Main.netMode == NetmodeID.Server && flag6) {
                NetMessage.SendTileSquare(-1, i, j, 3);
            }
        }
    }
}