using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Tiles;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsLilypad : ModTile, TileHooks.IGetTileDrawData {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;

        Main.tileLavaDeath[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        //TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Microsoft.Xna.Framework.Color(19, 82, 44));
    }

    public override void Load() {
        //On_Main.DrawTileInWater += On_Main_DrawTileInWater;
        On_Liquid.DelWater += On_Liquid_DelWater;
        On_TileDrawing.DrawSingleTile += On_TileDrawing_DrawSingleTile;
        On_Main.DrawTileInWater += On_Main_DrawTileInWater;
    }

    private void On_TileDrawing_DrawSingleTile(On_TileDrawing.orig_DrawSingleTile orig, TileDrawing self, Terraria.DataStructures.TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY) {
        if (drawData.tileCache.TileType == ModContent.TileType<BackwoodsLilypad>() && drawData.tileCache.LiquidAmount > 0) {
            return;
        }

        orig(self, drawData, solidLayer, waterStyleOverride, screenPosition, screenOffset, tileX, tileY);
    }

    private void On_Liquid_DelWater(On_Liquid.orig_DelWater orig, int l) {
        int num = Main.liquid[l].x;
        int num2 = Main.liquid[l].y;
        Tile tile = Main.tile[num - 1, num2];
        Tile tile2 = Main.tile[num + 1, num2];
        Tile tile3 = Main.tile[num, num2 + 1];
        Tile tile4 = Main.tile[num, num2];
        byte b = 2;
        if (tile4.LiquidAmount < b) {
            tile4.LiquidAmount = 0;
            if (tile.LiquidAmount < b)
                tile.LiquidAmount = 0;
            else
                Liquid.AddWater(num - 1, num2);

            if (tile2.LiquidAmount < b)
                tile2.LiquidAmount = 0;
            else
                Liquid.AddWater(num + 1, num2);
        }
        else if (tile4.LiquidAmount < 20) {
            if ((tile.LiquidAmount < tile4.LiquidAmount && (!tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])) || (tile2.LiquidAmount < tile4.LiquidAmount && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType])) || (tile3.LiquidAmount < byte.MaxValue && (!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType])))
                tile4.LiquidAmount = 0;
        }
        else if (tile3.LiquidAmount < byte.MaxValue && (!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) && !Liquid.stuck && (!Main.tile[num, num2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num, num2].TileType] || Main.tileSolidTop[Main.tile[num, num2].TileType])) {
            Main.liquid[l].kill = 0;
            return;
        }

        if (tile4.LiquidAmount < 250 && Main.tile[num, num2 - 1].LiquidAmount > 0)
            Liquid.AddWater(num, num2 - 1);

        if (tile4.LiquidAmount == 0) {
            tile4.LiquidType = 0;
        }
        else {
            if (tile2.LiquidAmount > 0 && tile2.LiquidAmount < 250 && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType]) && tile4.LiquidAmount != tile2.LiquidAmount)
                Liquid.AddWater(num + 1, num2);

            if (tile.LiquidAmount > 0 && tile.LiquidAmount < 250 && (!tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile4.LiquidAmount != tile.LiquidAmount)
                Liquid.AddWater(num - 1, num2);

            if (tile4.LiquidType == LiquidID.Lava) {
                Liquid.LavaCheck(num, num2);
                for (int i = num - 1; i <= num + 1; i++) {
                    for (int j = num2 - 1; j <= num2 + 1; j++) {
                        Tile tile5 = Main.tile[i, j];
                        if (!tile5.HasTile)
                            continue;

                        bool flag = false;
                        bool flag2 = false;
                        if (tile5.TileType == ModContent.TileType<BackwoodsGrass>()) {
                            flag = true;
                        }
                        if (flag) {
                            for (int i2 = num - 1; i2 <= num + 1; i2++) {
                                if (flag2) {
                                    break;
                                }
                                for (int j2 = num2 - 1; j2 <= num2 + 1; j2++) {
                                    if ((Main.tile[i2, j2 - 1].TileType == TileID.Trees && !Main.hardMode) ||
                                        Main.tile[i2, j2 - 1].TileType == ModContent.TileType<BackwoodsBigTree>()) {
                                        flag2 = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag2) {
                            flag = false;
                        }
                        if (flag ||
                            tile5.TileType == 2 || tile5.TileType == 23 || tile5.TileType == 109 || tile5.TileType == 199 || tile5.TileType == 477 || tile5.TileType == 492) {
                            tile5.TileType = 0;
                            WorldGen.SquareTileFrame(i, j);
                            if (Main.netMode == 2)
                                NetMessage.SendTileSquare(-1, num, num2, 3);
                        }
                        else if (tile5.TileType == 60 || tile5.TileType == 70 || tile5.TileType == 661 || tile5.TileType == 662) {
                            tile5.TileType = 59;
                            WorldGen.SquareTileFrame(i, j);
                            if (Main.netMode == 2)
                                NetMessage.SendTileSquare(-1, num, num2, 3);
                        }
                    }
                }
            }
            else if (tile4.LiquidType == LiquidID.Honey) {
                Liquid.HoneyCheck(num, num2);
            }
            else if (tile4.LiquidType == LiquidID.Shimmer) {
                Liquid.ShimmerCheck(num, num2);
            }
        }

        if (Main.netMode == 2)
            Liquid.NetSendLiquid(num, num2);

        Liquid.numLiquid--;
        tile = Main.tile[Main.liquid[l].x, Main.liquid[l].y];
        tile.CheckingLiquid = false;
        Main.liquid[l].x = Main.liquid[Liquid.numLiquid].x;
        Main.liquid[l].y = Main.liquid[Liquid.numLiquid].y;
        Main.liquid[l].kill = Main.liquid[Liquid.numLiquid].kill;
        if (Main.tileAlch[tile4.TileType]) {
            WorldGen.CheckAlch(num, num2);
        }
        else if (tile4.TileType == 518 || tile4.TileType == ModContent.TileType<BackwoodsLilypad>()) {
            if (Liquid.quickFall) {
                if (tile4.TileType == 518) {
                    WorldGen.CheckLilyPad(num, num2);
                }
                else {
                    CheckTileFrame(num, num2);
                }
            }
            else if (Main.tile[num, num2 + 1].LiquidAmount < byte.MaxValue || Main.tile[num, num2 - 1].LiquidAmount > 0)
                WorldGen.SquareTileFrame(num, num2);
            else {
                if (tile4.TileType == 518) {
                    WorldGen.CheckLilyPad(num, num2);
                }
                else {
                    CheckTileFrame(num, num2);
                }
            }
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGen.gen) {
            CheckTileFrame(i, j);
        }

        return true;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        int num19 = (int)WorldGenHelper.GetTileSafely(i, j).LiquidAmount / 16;
        num19 -= 3;
        if (WorldGen.SolidTile(i, j - 1) && num19 > 8)
            num19 = 8;

        Dust.NewDust(new Vector2(i * 16, j * 16 - num19), 16, 16, DustType);

        return false;
    }

    private void CheckTileFrame(int i, int j) {
        //if (Main.netMode == NetmodeID.MultiplayerClient)
        //    return;

        int x = i, y = j;
        if (Main.tile[x, y].LiquidType != LiquidID.Water) {
            WorldGen.KillTile(x, y);
            if (Main.netMode == 2)
                NetMessage.SendData(17, -1, -1, null, 0, x, y);

            return;
        }

        int num = y;
        while ((!Main.tile[x, num].HasTile || !Main.tileSolid[Main.tile[x, num].TileType] || Main.tileSolidTop[Main.tile[x, num].TileType]) && num < Main.maxTilesY - 50) {
            num++;
        }

        int type = Main.tile[x, num].TileType;
        int num2 = -1;

        if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)type))
            num2 = 0;

        if (num2 >= 0) {
            if (num2 != Main.tile[x, y].TileFrameY) {
                Main.tile[x, y].TileFrameY = (short)num2;
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, x, y);
            }

            if (Main.tile[x, y - 1].LiquidAmount > 0 && !Main.tile[x, y - 1].HasTile) {
                Tile tile = Main.tile[x, y];
                short tileFrameX = tile.TileFrameX, tileFrameY = tile.TileFrameY;
                tile.ClearTile();
                tile.HasTile = false;

                tile = Main.tile[x, y - 1];
                tile.ClearTile();
                tile.HasTile = true;
                tile.TileType = Type;
                tile.TileFrameX = tileFrameX;
                tile.TileFrameY = tileFrameY;
                tile.IsHalfBlock = false;
                tile.Slope = 0;

                WorldGen.SquareTileFrame(x, y - 1, resetFrame: false);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, x, y - 1, 1, 2);
            }
            else {
                if (Main.tile[x, y].LiquidAmount != 0)
                    return;

                Tile tileSafely = Framing.GetTileSafely(x, y + 1);
                if (!tileSafely.HasTile) {
                    Tile tile = Main.tile[x, y];
                    short tileFrameX = tile.TileFrameX, tileFrameY = tile.TileFrameY;
                    tile.ClearTile();
                    tile.HasTile = false;

                    tile = Main.tile[x, y + 1];
                    tile.ClearTile();
                    tile.HasTile = true;
                    tile.TileType = Type;
                    tile.TileFrameX = tileFrameX;
                    tile.TileFrameY = tileFrameY;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;

                    WorldGen.SquareTileFrame(x, y + 1, resetFrame: false);
                    if (Main.netMode == 2)
                        NetMessage.SendTileSquare(-1, x, y + 1, 1, 2);
                }
                else if (tileSafely.HasTile && !TileID.Sets.Platforms[tileSafely.TileType] && (!Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType])) {
                    WorldGen.KillTile(x, y);
                    if (Main.netMode == 2)
                        NetMessage.SendData(17, -1, -1, null, 0, x, y);
                }
            }
        }
        else {
            WorldGen.KillTile(x, y);
            if (Main.netMode == 2)
                NetMessage.SendData(17, -1, -1, null, 0, x, y);
        }
    }

    private void On_Main_DrawTileInWater(On_Main.orig_DrawTileInWater orig, Vector2 drawOffset, int x, int y) {
        orig(drawOffset, x, y);

        if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<BackwoodsLilypad>()) {
            //Main.instance.LoadTiles(Main.tile[x, y].TileType);
            Tile tile = Main.tile[x, y];
            int num = (int)tile.LiquidAmount / 16;
            num -= 3;
            if (WorldGen.SolidTile(x, y - 1) && num > 8)
                num = 8;

            Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 18);
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) {
                zero = Vector2.Zero;
            }
            Main.spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, new Vector2(x * 16, y * 16 - num - 2) - Main.screenPosition + zero, value, Lighting.GetColor(x, y), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        int x = i, y = j;
        //if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == Type) {
        //Main.instance.LoadTiles(Main.tile[x, y].TileType);
        Tile tile = Main.tile[x, y];
        int num = (int)tile.LiquidAmount / 16;
        num -= 3;
        if (WorldGen.SolidTile(x, y - 1) && num > 8)
            num = 8;

        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }

        Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 18);
        Main.spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, new Vector2(x * 16, y * 16 - num - 2) - Main.screenPosition + zero, value, Lighting.GetColor(x, y), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
        //}

        return false;
    }

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        //int num27 = (int)tileCache.LiquidAmount / 16;
        //num27 -= 3;
        //if (WorldGen.SolidTile(x, y - 1) && num27 > 8)
        //    num27 = 8;

        //if (tileCache.LiquidAmount == 0) {
        //    Tile tileSafely = Framing.GetTileSafely(x, y + 1);
        //    if (tileSafely.HasUnactuatedTile) {
        //        switch (tileSafely.BlockType) {
        //            case (BlockType)1:
        //                num27 = -16 + Math.Max(8, (int)tileSafely.LiquidAmount / 16);
        //                break;
        //            case (BlockType)2:
        //            case (BlockType)3:
        //                num27 -= 4;
        //                break;
        //        }
        //    }
        //}

        //tileTop -= num27;
    }

    //public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
    //    int x = i, y = j;
    //    if (Main.tile[x, y].LiquidType != 0) {
    //        WorldGen.KillTile(x, y);
    //        if (Main.netMode == 2)
    //            NetMessage.SendData(17, -1, -1, null, 0, x, y);

    //        return false;
    //    }

    //    int num = y;
    //    while ((!Main.tile[x, num].HasTile|| !Main.tileSolid[Main.tile[x, num].TileType] || Main.tileSolidTop[Main.tile[x, num].TileType]) && num < Main.maxTilesY - 50) {
    //        num++;
    //        if (Main.tile[x, num] == null)
    //            return false;
    //    }

    //    int type = Main.tile[x, num].TileType;
    //    int num2 = -1;
    //    if (type == 2 || type == 477)
    //        num2 = 0;

    //    if (type == 109 || type == 109 || type == 116)
    //        num2 = 18;

    //    if (type == 60)
    //        num2 = 36;

    //    if (num2 >= 0) {
    //        if (num2 != Main.tile[x, y].TileFrameY) {
    //            Main.tile[x, y].TileFrameY = (short)num2;
    //            if (Main.netMode == 2)
    //                NetMessage.SendTileSquare(-1, x, y);
    //        }

    //        if (Main.tile[x, y - 1].AnyLiquid() && !Main.tile[x, y - 1].HasTile) {
    //            Tile tile = Main.tile[x, y - 1];
    //            tile.HasTile = true;
    //            Main.tile[x, y - 1].TileType = Type;
    //            Main.tile[x, y - 1].TileFrameX = Main.tile[x, y].TileFrameX;
    //            Main.tile[x, y - 1].TileFrameY = Main.tile[x, y].TileFrameY;
    //            tile.IsHalfBlock = false;
    //            tile.Slope = 0;
    //            tile = Main.tile[x, y];
    //            tile.HasTile = false;
    //            Main.tile[x, y].TileType = 0;
    //            WorldGen.SquareTileFrame(x, y - 1, resetFrame: false);
    //            if (Main.netMode == 2)
    //                NetMessage.SendTileSquare(-1, x, y - 1, 1, 2);
    //        }
    //        else {
    //            if (Main.tile[x, y].LiquidAmount != 0)
    //                return false;

    //            Tile tileSafely = Framing.GetTileSafely(x, y + 1);
    //            if (!tileSafely.HasTile) {
    //                Tile tile = Main.tile[x, y + 1];
    //                tile.HasTile = true;
    //                Main.tile[x, y + 1].TileType = Type;
    //                Main.tile[x, y + 1].TileFrameX = Main.tile[x, y].TileFrameX;
    //                Main.tile[x, y + 1].TileFrameY = Main.tile[x, y].TileFrameY;
    //                tile.IsHalfBlock = false;
    //                tile.Slope = 0;
    //                tile = Main.tile[x, y];
    //                tile.HasTile = false;
    //                Main.tile[x, y].TileType = 0;
    //                WorldGen.SquareTileFrame(x, y + 1, resetFrame: false);
    //                if (Main.netMode == 2)
    //                    NetMessage.SendTileSquare(-1, x, y, 1, 2);
    //            }
    //            else if (tileSafely.HasTile && !TileID.Sets.Platforms[tileSafely.TileType] && (!Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType])) {
    //                WorldGen.KillTile(x, y);
    //                if (Main.netMode == 2)
    //                    NetMessage.SendData(17, -1, -1, null, 0, x, y);
    //            }
    //        }
    //    }
    //    else {
    //        WorldGen.KillTile(x, y);
    //        if (Main.netMode == 2)
    //            NetMessage.SendData(17, -1, -1, null, 0, x, y);
    //    }

    //    return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    //}
}