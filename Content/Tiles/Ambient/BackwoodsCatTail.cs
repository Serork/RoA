using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Tiles;
using RoA.Common.Utilities.Extensions;
using RoA.Content.WorldGenerations;
using RoA.Core.Utility;

using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsCatTail : ModTile, TileHooks.IGetTileDrawData {
    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        tileTop = 2;
        if (x % 2 == 0)
            tileSpriteEffect = SpriteEffects.FlipHorizontally;
    }

    public override void Load() {
        On_TileDrawing.DrawMultiTileGrass += On_TileDrawing_DrawMultiTileGrass;
        On_NPC.FindCattailTop += On_NPC_FindCattailTop;
        On_WorldGen.UpdateWorld_OvergroundTile += On_WorldGen_UpdateWorld_OvergroundTile;
        On_WorldGen.UpdateWorld_UndergroundTile += On_WorldGen_UpdateWorld_UndergroundTile; ;
        //IL_NPC.SpawnNPC += IL_NPC_SpawnNPC;
    }

    private void On_WorldGen_UpdateWorld_UndergroundTile(On_WorldGen.orig_UpdateWorld_UndergroundTile orig, int i, int j, bool checkNPCSpawns, int wallDist) {
        orig(i, j, checkNPCSpawns, wallDist);

        if (Main.tile[i, j].LiquidAmount > 32) {
            if (Main.tile[i, j].HasTile) {
            }
            else if (WorldGen.genRand.Next(600) == 0) {
                int right = BackwoodsVars.BackwoodsCenterX + BackwoodsVars.BackwoodsHalfSizeX + 100;
                int left = BackwoodsVars.BackwoodsCenterX - BackwoodsVars.BackwoodsHalfSizeX - 100;
                BackwoodsBiomePass.PlaceBackwoodsLilypad(i, j, right, left);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, i, j);
            }
            else if (WorldGen.genRand.Next(600) == 0) {
                BackwoodsBiomePass.PlaceBackwoodsCattail(i, j);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, i, j);
            }
        }
    }

    private void On_WorldGen_UpdateWorld_OvergroundTile(On_WorldGen.orig_UpdateWorld_OvergroundTile orig, int i, int j, bool checkNPCSpawns, int wallDist) {
        orig(i, j, checkNPCSpawns, wallDist);

        if (Main.tile[i, j].LiquidAmount > 32) {
            if (Main.tile[i, j].HasTile) {
            }
            else if (WorldGen.genRand.Next(600) == 0) {
                int right = BackwoodsVars.BackwoodsCenterX + BackwoodsVars.BackwoodsHalfSizeX + 100;
                int left = BackwoodsVars.BackwoodsCenterX - BackwoodsVars.BackwoodsHalfSizeX - 100;
                BackwoodsBiomePass.PlaceBackwoodsLilypad(i, j, right, left);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, i, j);
            }
            else if (WorldGen.genRand.Next(600) == 0) {
                BackwoodsBiomePass.PlaceBackwoodsCattail(i, j);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, i, j);
            }
        }
    }

    private bool On_NPC_FindCattailTop(On_NPC.orig_FindCattailTop orig, int landX, int landY, out int cattailX, out int cattailY) {
        cattailX = landX;
        cattailY = landY;
        if (!WorldGen.InWorld(landX, landY, 31))
            return false;

        int num = 1;
        for (int i = landX - 30; i <= landX + 30; i++) {
            for (int j = landY - 20; j <= landY + 20; j++) {
                Tile tile = Main.tile[i, j];
                if (tile != null && tile.HasTile && (tile.TileType == 519 || tile.TileType == ModContent.TileType<BackwoodsCatTail>()) && tile.TileFrameX >= 180 && Main.rand.Next(num) == 0) {
                    cattailX = i;
                    cattailY = j;
                    num++;
                }
            }
        }

        if (cattailX != landX || cattailY != landY)
            return true;

        return false;
    }

    //private void IL_NPC_SpawnNPC(ILContext il) {
    //    ILCursor cursor = new(il);
    //    //int index = 0;
    //    //if (!cursor.TryGotoNext(MoveType.After,
    //    //    i => i.MatchCall(typeof(NPC), "get_" + nameof(NPC.TooWindyForButterflies)),
    //    //    i => i.MatchStloc(index))) {
    //    //    return;
    //    //}
    //    //cursor.Emit(OpCodes.Ldloc_0);
    //    //cursor.EmitDelegate(() => {
    //    //    return true;
    //    //});
    //    //cursor.Emit(OpCodes.Stloc_0);

    //    //MonoModHooks.DumpIL(ModContent.GetInstance<RoA>(), il);


    //    ILLabel label = null;
    //    int num = 0;
    //    cursor.GotoNext(
    //        MoveType.After,
    //        i => i.MatchLdloc(out _),
    //        i => i.MatchLdcI4(2),
    //        i => i.MatchBeq(out _),
    //        i => i.MatchLdloc(out _),
    //        i => i.MatchLdcI4(477),
    //        i => i.MatchBeq(out _),
    //        i => i.MatchLdloc(out num),
    //        i => i.MatchLdcI4(53),
    //        i => i.MatchBneUn(out label)
    //    );
    //    ILLabel next = il.DefineLabel(cursor.Next);
    //    cursor.Index -= 3;
    //    cursor.Emit(OpCodes.Ldloc, num);
    //    cursor.EmitDelegate((int type) => {
    //        return true;
    //    });
    //    cursor.Emit(OpCodes.Brtrue, next);

    //    MonoModHooks.DumpIL(ModContent.GetInstance<RoA>(), il);
    //}

    private void On_TileDrawing_DrawMultiTileGrass(On_TileDrawing.orig_DrawMultiTileGrass orig, TileDrawing self) {
        Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
        Vector2 zero = Vector2.Zero;
        int num = 4;
        int[] specialsCount = typeof(TileDrawing).GetFieldValue<int[]>("_specialsCount", Main.instance.TilesRenderer);
        Point[][] specialPositions = typeof(TileDrawing).GetFieldValue<Point[][]>("_specialPositions", Main.instance.TilesRenderer);
        int num2 = specialsCount[num];
        for (int i = 0; i < num2; i++) {
            Point point = specialPositions[num][i];
            int x = point.X;
            int num3 = point.Y;
            int sizeX = 1;
            int num4 = 1;
            Tile tile = Main.tile[x, num3];
            if (tile != null && tile.HasTile) {
                switch (Main.tile[x, num3].TileType) {
                    case 27:
                        sizeX = 2;
                        num4 = 5;
                        break;
                    case 236:
                    case 238:
                        sizeX = (num4 = 2);
                        break;
                    case 233:
                        sizeX = ((Main.tile[x, num3].TileFrameY != 0) ? 2 : 3);
                        num4 = 2;
                        break;
                    case 530:
                    case 651:
                        sizeX = 3;
                        num4 = 2;
                        break;
                    case 485:
                    case 490:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                    case 525:
                    case 526:
                    case 527:
                    case 652:
                        sizeX = 2;
                        num4 = 2;
                        break;
                    case 489:
                        sizeX = 2;
                        num4 = 3;
                        break;
                    case 493:
                        sizeX = 1;
                        num4 = 2;
                        break;
                    case 519:
                        sizeX = 1;
                        num4 = ClimbCatTail(x, num3);
                        num3 -= num4 - 1;
                        break;
                }
                if (Main.tile[x, num3].TileType == ModContent.TileType<BackwoodsCatTail>()) {
                    sizeX = 1;
                    num4 = ClimbCatTail(x, num3);
                    num3 -= num4 - 1;
                }
                typeof(TileDrawing).GetMethod("DrawMultiTileGrassInWind", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Invoke(Main.instance.TilesRenderer, [unscaledPosition, zero, x, num3, sizeX, num4]);
            }
        }
    }

    private int ClimbCatTail(int originx, int originy) {
        int num = 0;
        int num2 = originy;
        while (num2 > 10) {
            Tile tile = Main.tile[originx, num2];
            if (!(tile.HasTile && (tile.TileType == ModContent.TileType<BackwoodsCatTail>() || tile.TileType == 519)))
                break;

            if (tile.TileFrameX >= 180) {
                num++;
                break;
            }

            num2--;
            num++;
        }

        return num;
    }

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

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        if (Main.tile[i, j].TileFrameX / 18 <= 4) {
            TileHelper.AddSpecialPoint(i, j, 4);
        }

        return false;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGen.gen) {
            CheckCatTail(i, j);
        }

        return true;
    }

    private static int RollDragonflyType(int tileType = 2) {
        return Main.rand.NextFromList(new short[3] {
            596,
            597,
            599
        });
    }

    public override void RandomUpdate(int i, int j) {
        if (!Helper.OnScreenWorld(new Point(i, j).ToWorldCoordinates())) {
            return;
        }

        if (NPC.TooWindyForButterflies) {
            return;
        }

        for (int k = 0; k < 255; k++) {
            if (!Main.player[k].active || Main.player[k].dead)
                continue;

            if (Main.player[k].isNearNPC(398, NPC.MoonLordFightingDistance))
                continue;

            if (!Main.player[k].ZoneGraveyard && Main.rand.Next(2) == 0 && !Main.raining && Main.dayTime && NPC.FindCattailTop(i, j, out int cattailX, out int cattailY)) {
                if (WorldGen.SolidTile(cattailX, cattailY)) {
                    continue;
                }
                IEntitySource entitySource = new EntitySource_SpawnNPC();
                if (Main.player[k].RollLuck(NPC.goldCritterChance) == 0)
                    NPC.NewNPC(entitySource, cattailX * 16 + 8, cattailY * 16, 601);
                else
                    NPC.NewNPC(entitySource, cattailX * 16 + 8, cattailY * 16, RollDragonflyType(Type));

                if (Main.rand.Next(3) == 0)
                    NPC.NewNPC(entitySource, cattailX * 16 + 8 - 16, cattailY * 16, RollDragonflyType(Type));

                if (Main.rand.Next(3) == 0)
                    NPC.NewNPC(entitySource, cattailX * 16 + 8 + 16, cattailY * 16, RollDragonflyType(Type));
            }
        }
    }

    private void CheckCatTail(int x, int j) {
        int num = j;
        bool flag = false;
        int num2 = num;
        while ((!Main.tile[x, num2].HasTile || !Main.tileSolid[Main.tile[x, num2].TileType] || Main.tileSolidTop[Main.tile[x, num2].TileType]) && num2 < Main.maxTilesY - 50) {
            if (Main.tile[x, num2].HasTile && Main.tile[x, num2].TileType != Type)
                flag = true;

            if (!Main.tile[x, num2].HasTile)
                break;

            num2++;
            if (Main.tile[x, num2] == null)
                return;
        }

        num = num2 - 1;
        if (Main.tile[x, num] == null)
            return;

        while (Main.tile[x, num] != null && Main.tile[x, num].LiquidAmount > 0 && num > 50) {
            if ((Main.tile[x, num].HasTile && Main.tile[x, num].TileType != Type) || Main.tile[x, num].LiquidType != 0)
                flag = true;

            num--;
            if (Main.tile[x, num] == null)
                return;
        }

        num++;
        if (Main.tile[x, num] == null)
            return;

        int num3 = num;
        int num4 = 8;
        if (num2 - num3 > num4)
            flag = true;

        int type = Main.tile[x, num2].TileType;
        int num5 = -1;

        if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)type)) {
            num5 = 0;
        }
        //switch (type) {
        //    case 2:
        //    case 477:
        //        num5 = 0;
        //        break;
        //    case 53:
        //        num5 = 18;
        //        break;
        //    case 199:
        //    case 234:
        //    case 662:
        //        num5 = 54;
        //        break;
        //    case 23:
        //    case 112:
        //    case 661:
        //        num5 = 72;
        //        break;
        //    case 70:
        //        num5 = 90;
        //        break;
        //}

        if (!Main.tile[x, num2].HasUnactuatedTile)
            flag = true;

        if (num5 < 0)
            flag = true;

        num = num2 - 1;
        if (Main.tile[x, num] != null && !Main.tile[x, num].HasTile) {
            for (int num6 = num; num6 >= num3; num6--) {
                if (Main.tile[x, num6] == null)
                    return;

                if (Main.tile[x, num6].HasTile && Main.tile[x, num6].TileType == ModContent.TileType<BackwoodsCatTail>()) {
                    num = num6;
                    break;
                }
            }
        }

        while (Main.tile[x, num] != null && Main.tile[x, num].HasTile && Main.tile[x, num].TileType == Type) {
            num--;
        }

        num++;
        if (Main.tile[x, num2 - 1] != null && Main.tile[x, num2 - 1].LiquidAmount < 127 && WorldGen.genRand.Next(4) == 0)
            flag = true;

        if (Main.tile[x, num] != null && Main.tile[x, num].TileFrameX >= 180 && Main.tile[x, num].LiquidAmount > 127 && WorldGen.genRand.Next(4) == 0)
            flag = true;

        if (Main.tile[x, num] != null && Main.tile[x, num2 - 1] != null && Main.tile[x, num].TileFrameX > 18) {
            if (Main.tile[x, num2 - 1].TileFrameX < 36 || Main.tile[x, num2 - 1].TileFrameX > 72)
                flag = true;
            else if (Main.tile[x, num].TileFrameX < 90)
                flag = true;
            else if (Main.tile[x, num].TileFrameX >= 108 && Main.tile[x, num].TileFrameX <= 162)
                Main.tile[x, num].TileFrameX = 90;
        }

        if (num2 > num + 4 && Main.tile[x, num + 4] != null && Main.tile[x, num + 3] != null && Main.tile[x, num + 4].LiquidAmount == 0 && Main.tile[x, num + 3].TileType == 519)
            flag = true;

        if (flag) {
            int num7 = num3;
            if (num < num3)
                num7 = num;

            num7 -= 4;
            for (int i = num7; i <= num2; i++) {
                if (Main.tile[x, i] != null && Main.tile[x, i].HasTile && Main.tile[x, i].TileType == ModContent.TileType<BackwoodsCatTail>()) {
                    WorldGen.KillTile(x, i);
                    if (Main.netMode == 2)
                        NetMessage.SendData(17, -1, -1, null, 0, x, i);

                    WorldGen.SquareTileFrame(x, i);
                }
            }
        }
        else {
            if (num5 == Main.tile[x, num].TileFrameY)
                return;

            for (int k = num; k < num2; k++) {
                if (Main.tile[x, k] != null && Main.tile[x, k].HasTile && Main.tile[x, k].TileType == ModContent.TileType<BackwoodsCatTail>()) {
                    Main.tile[x, k].TileFrameY = (short)num5;
                    if (Main.netMode == 2)
                        NetMessage.SendTileSquare(-1, x, num);
                }
            }
        }
    }
}