using Microsoft.Xna.Framework;

using Mono.Cecil.Cil;

using MonoMod.Cil;

using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Crafting;
using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

sealed class PlanterBoxHooks : ILoadable {
    public void Load(Mod mod) {
        //On_WorldGen.CheckAlch += CheckAlchPlanterBoxHack;
        On_WorldGen.CheckBanner += CheckBannerPlanterBoxHack;
        On_WorldGen.Check1x2Top += On_WorldGen_Check1x2Top;
        //IL_WorldGen.PlantCheck += IL_WorldGen_PlantCheck;
        //// Terraria only uses this in PlaceTile, but this detour may be useful if other mods use it?
        On_WorldGen.IsFitToPlaceFlowerIn += IsFitToPlaceFlowerInPlanterBoxHack;
        //IL_WorldGen.PlaceTile += PlaceTileILEdit_AbsolutelyNotScary;

        On_Player.FigureOutWhatToPlace += On_Player_FigureOutWhatToPlace;
        IL_WorldGen.CanCutTile += IL_WorldGen_CanCutTile;
        IL_WorldGen.TryKillingReplaceableTile += IL_WorldGen_TryKillingReplaceableTile;
        On_TileObject.CanPlace += On_TileObject_CanPlace;
        On_WorldGen.PlantCheck += On_WorldGen_PlantCheck;

        On_WorldGen.PlaceTile += On_WorldGen_PlaceTile;
    }

    private void On_WorldGen_Check1x2Top(On_WorldGen.orig_Check1x2Top orig, int x, int j, ushort type) {
        foreach (var m in RoA.Instance.GetContent<PlanterBoxes>()) {
            TileID.Sets.Platforms[m.Type] = true;
        }

        orig(x, j, type);

        foreach (var m in RoA.Instance.GetContent<PlanterBoxes>()) {
            TileID.Sets.Platforms[m.Type] = false;
        }
    }

    private void On_WorldGen_PlantCheck(On_WorldGen.orig_PlantCheck orig, int x, int y) {
        x = Utils.Clamp(x, 1, Main.maxTilesX - 2);
        y = Utils.Clamp(y, 1, Main.maxTilesY - 2);
        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                if (Main.tile[i, j] == null)
                    return;
            }
        }

        int num = -1;
        int num2 = Main.tile[x, y].TileType;
        _ = x - 1;
        _ = 0;
        _ = x + 1;
        _ = Main.maxTilesX;
        _ = y - 1;
        _ = 0;
        if (y + 1 >= Main.maxTilesY)
            num = num2;

        if (x - 1 >= 0 && Main.tile[x - 1, y] != null && Main.tile[x - 1, y].HasUnactuatedTile)
            _ = Main.tile[x - 1, y].TileType;

        if (x + 1 < Main.maxTilesX && Main.tile[x + 1, y] != null && Main.tile[x + 1, y].HasUnactuatedTile)
            _ = Main.tile[x + 1, y].TileType;

        if (y - 1 >= 0 && Main.tile[x, y - 1] != null && Main.tile[x, y - 1].HasUnactuatedTile)
            _ = Main.tile[x, y - 1].TileType;

        if (y + 1 < Main.maxTilesY && Main.tile[x, y + 1] != null && Main.tile[x, y + 1].HasUnactuatedTile && !Main.tile[x, y + 1].IsHalfBlock && Main.tile[x, y + 1].Slope == 0)
            num = Main.tile[x, y + 1].TileType;

        if (x - 1 >= 0 && y - 1 >= 0 && Main.tile[x - 1, y - 1] != null && Main.tile[x - 1, y - 1].HasUnactuatedTile)
            _ = Main.tile[x - 1, y - 1].TileType;

        if (x + 1 < Main.maxTilesX && y - 1 >= 0 && Main.tile[x + 1, y - 1] != null && Main.tile[x + 1, y - 1].HasUnactuatedTile)
            _ = Main.tile[x + 1, y - 1].TileType;

        if (x - 1 >= 0 && y + 1 < Main.maxTilesY && Main.tile[x - 1, y + 1] != null && Main.tile[x - 1, y + 1].HasUnactuatedTile)
            _ = Main.tile[x - 1, y + 1].TileType;

        if (x + 1 < Main.maxTilesX && y + 1 < Main.maxTilesY && Main.tile[x + 1, y + 1] != null && Main.tile[x + 1, y + 1].HasUnactuatedTile)
            _ = Main.tile[x + 1, y + 1].TileType;

        if ((num2 != 3 || num == 2 || num == 477 || num == 78 || (num == 380 || num == ModContent.TileType<PlanterBoxes>()) || num == 579) &&
            ((num2 != ModContent.TileType<BackwoodsPlants>() && num2 != ModContent.TileType<BackwoodsBush>()) || num == ModContent.TileType<BackwoodsGrass>() || num == 78 || (num == 380 || num == ModContent.TileType<PlanterBoxes>())) && (num2 != 73 || num == 2 || num == 477 || num == 78 || (num == 380 || num == ModContent.TileType<PlanterBoxes>()) || num == 579) && (num2 != 24 || num == 23 || num == 661) && (num2 != 61 || num == 60) && (num2 != 74 || num == 60) && (num2 != 71 || num == 70) && (num2 != 110 || num == 109 || num == 492) && (num2 != 113 || num == 109 || num == 492) && (num2 != 201 || num == 199 || num == 662) && (num2 != 637 || num == 633))
            return;

        bool flag = false;
        if (num2 == 3 || num2 == 110 || num2 == 24)
            flag = Main.tile[x, y].TileFrameX == 144;

        if (num2 == 201)
            flag = Main.tile[x, y].TileFrameX == 270;

        if ((num2 == 3 || num2 == 73) && num != 2 && num != 477 && Main.tile[x, y].TileFrameX >= 162)
            Main.tile[x, y].TileFrameX = 126;

        if (num2 == 74 && num != 60 && Main.tile[x, y].TileFrameX >= 162)
            Main.tile[x, y].TileFrameX = 126;

        switch (num) {
            case 23:
            case 661:
                num2 = 24;
                if (Main.tile[x, y].TileFrameX >= 162)
                    Main.tile[x, y].TileFrameX = 126;
                break;
            case 2:
            case 477:
                num2 = ((num2 != 113) ? 3 : 73);
                break;
            case 109:
            case 492:
                num2 = ((num2 != 73) ? 110 : 113);
                break;
            case 199:
            case 662:
                num2 = 201;
                break;
            case 60:
                num2 = 61;
                while (Main.tile[x, y].TileFrameX > 126) {
                    Main.tile[x, y].TileFrameX -= 126;
                }
                break;
            case 70:
                num2 = 71;
                while (Main.tile[x, y].TileFrameX > 72) {
                    Main.tile[x, y].TileFrameX -= 72;
                }
                break;
        }

        if (num2 != Main.tile[x, y].TileType) {
            Main.tile[x, y].TileType = (ushort)num2;
            if (flag) {
                Main.tile[x, y].TileFrameX = 144;
                if (num2 == 201)
                    Main.tile[x, y].TileFrameX = 270;
            }
        }
        else {
            WorldGen.KillTile(x, y);
        }
    }

    private bool On_WorldGen_PlaceTile(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style) {
        int num = Type;
        if (i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY) {
            Tile tile = Main.tile[i, j];

            if (forced || Collision.EmptyTile(i, j) || !Main.tileSolid[num] || (num == 23 && tile.TileType == 0 && tile.HasTile) || (num == 199 && tile.TileType == 0 && tile.HasTile) || (num == 2 && tile.TileType == 0 && tile.HasTile) || (num == 109 && tile.TileType == 0 && tile.HasTile) || (num == 60 && tile.TileType == 59 && tile.HasTile) || (num == 661 && tile.TileType == 59 && tile.HasTile) || (num == 662 && tile.TileType == 59 && tile.HasTile) || (num == 70 && tile.TileType == 59 && tile.HasTile) || (num == 633 && tile.TileType == 57 && tile.HasTile) || (Main.tileMoss[num] && (tile.TileType == 1 || tile.TileType == 38) && tile.HasTile)) {
                if (num == 3 || num == 24 || num == 110 || num == 201 || num == 637) {
                    Tile tile2 = Main.tile[i, j + 1];
                    if (!(j < 1 || j > Main.maxTilesY - 1) && tile2.HasTile && tile2.Slope == 0 && !tile2.IsHalfBlock && num == 3) {
                        if (tile2.TileType == ModContent.TileType<PlanterBoxes>()/*Main.tile[i, j + 1].TileType == 78 || Main.tile[i, j + 1].TileType == 380 || Main.tile[i, j + 1].TileType == 579*/) {
                            tile.HasTile = true;
                            if (WorldGen.genRand.NextBool()) {
                                tile.TileFrameY = 0;
                                tile.TileType = (ushort)ModContent.TileType<BackwoodsPlants>();
                                tile.TileFrameX = (short)(18 * WorldGen.genRand.Next(6, 20));
                            }
                            else {
                                tile.TileType = (ushort)num;
                                int num2 = WorldGen.genRand.NextFromList<int>(6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 27, 30, 33, 36, 39, 42);
                                switch (num2) {
                                    case 21:
                                    case 24:
                                    case 27:
                                    case 30:
                                    case 33:
                                    case 36:
                                    case 39:
                                    case 42:
                                        num2 += WorldGen.genRand.Next(3);
                                        break;
                                }

                                tile.TileFrameX = (short)(num2 * 18);
                            }

                            return true;
                        }
                    }
                }
            }
        }

        return orig(i, j, Type, mute, forced, plr, style);
    }

    public void Unload() { }

    static void On_Player_FigureOutWhatToPlace(On_Player.orig_FigureOutWhatToPlace orig, Player self, Tile targetTile, Item sItem, out int tileToCreate, out int previewPlaceStyle, out bool? overrideCanPlace, out int? forcedRandom) {
        orig(self, targetTile, sItem, out tileToCreate, out previewPlaceStyle, out overrideCanPlace, out forcedRandom);

        if (targetTile.HasTile && TileLoader.GetTile(targetTile.TileType) is PlantBase modHerb && !modHerb.IsGrown(Player.tileTargetX, Player.tileTargetY)) {
            overrideCanPlace = false;
        }
    }

    void IL_WorldGen_TryKillingReplaceableTile(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        // The first time we load the 2nd argument is right before the check for planter boxes.
        if (!cursor.TryGotoNext(i => i.MatchLdarg2())) {
            //Mod.Logger.Error("Error locating ldarg2 in WorldGen.TryKillingReplaceableTile."); return;
        }

        // Grab the label for the branch statement which is actually right after ldarg2 
        ILLabel branchLabel = null;
        if (!cursor.TryGotoNext(i => i.MatchBeq(out branchLabel)) || branchLabel == null) {
            //Mod.Logger.Error("Error locating beq in WorldGen.TryKillingReplaceableTile."); return;
        }

        // Increment the index, we have now fallen into the chain of type checks
        cursor.Index++;

        // Insert a check for our modded planter box in the chain of type checks.
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate((int x, int y) => TileLoader.GetTile(Main.tile[x, y + 1].TileType) is not PlanterBoxes);
        cursor.EmitBrfalse(branchLabel);
    }

    void IL_WorldGen_CanCutTile(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        // First ldsfld is for the null check, the 2nd one is a type check.
        if (!cursor.TryGotoNext(i => i.MatchLdsflda(typeof(Main), nameof(Main.tile))) || !cursor.TryGotoNext(i => i.MatchLdsflda(typeof(Main), nameof(Main.tile)))) {
            //Mod.Logger.Error("Error locating the 2nd ldsflda of Main.tile in WorldGen.CanCutTile."); return;
        }

        // Keep track of the index where the type check chain starts so we can return to it later.
        int index = cursor.Index;

        // Go forward a bit to capture the label which is used in the type checks.
        ILLabel branchLabel = null;
        if (!cursor.TryGotoNext(i => i.MatchBeq(out branchLabel)) || branchLabel == null) {
            //Mod.Logger.Error("Error locating the tile check's beq in WorldGen.CanCutTile."); return;
        }

        // Set the index back now that we have the label.
        cursor.Index = index;

        // Insert a check for our modded planter box in the chain of type checks.
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate((int x, int y) => TileLoader.GetTile(Main.tile[x, y + 1].TileType) is not PlanterBoxes);
        cursor.EmitBrfalse(branchLabel);
    }

    private void IL_WorldGen_PlantCheck(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        while (cursor.TryGotoNext(i => i.MatchLdcI4(TileID.PlanterBox))) {
            // Move down to branch statement
            cursor.Index++;

            // Push TileID.PlanterBox if type equals our modded planter box.
            cursor.EmitDelegate((int type, int checkAgainstType) => type == ModContent.TileType<PlanterBoxes>() ? TileID.PlanterBox : checkAgainstType);
        }
    }

    private void PlaceTileILEdit_AbsolutelyNotScary(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(i => i.MatchCall(typeof(WorldGen), nameof(WorldGen.IsFitToPlaceFlowerIn)))) {
            //Mod.Logger.Error("Error locating WorldGen.IsFitToPlaceFlowerIn in WorldGen.PlaceTile for planter boxes.");
            return;
        }

        cursor.Index++; // Move index onto the branch operation

        // a boolean is already on the stack from the previous IsFitToPlaceFlowerIn call.
        cursor.Emit(OpCodes.Ldarg_0); // Push i
        cursor.Emit(OpCodes.Ldarg_1); // Push j
        cursor.Emit(OpCodes.Ldloc_0); // Push num (Type)

        // Check if the return value was already true, or if the type is equal to our modded planter box.
        cursor.EmitDelegate((bool returnValue, int i, int j, int type) => returnValue || type == ModContent.TileType<PlanterBoxes>());

        // It will now fall into the flower planting code if the type is equal to our modded planter box.

        // We now fall down into the check for the planter box type
        if (!cursor.TryGotoNext(i => i.MatchLdcI4(TileID.PlanterBox))) {
            //Mod.Logger.Error("Error locating instruction for pushing TileID.PlanterBox onto the stack in WorldGen.PlaceTile.");
            return;
        }

        // Move down to branch statement
        cursor.Index++;

        // Push TileID.PlanterBox if type equals our modded planter box.
        cursor.EmitDelegate((int type, int checkAgainstType) => type == ModContent.TileType<PlanterBoxes>() ? TileID.PlanterBox : type);
    }

    private bool IsFitToPlaceFlowerInPlanterBoxHack(On_WorldGen.orig_IsFitToPlaceFlowerIn orig, int x, int y, int typeAttemptedToPlace) {
        if (y < 1 || y > Main.maxTilesY - 1)
            return false;

        Tile tile = Main.tile[x, y + 1];
        if (tile.HasTile && tile.Slope == 0 && !tile.IsHalfBlock) {
            if (((tile.TileType != 2 && tile.TileType != 78 && tile.TileType != 380 && tile.TileType != ModContent.TileType<PlanterBoxes>() && tile.TileType != 477 && tile.TileType != 579) || typeAttemptedToPlace != 3) && ((tile.TileType != 23 && tile.TileType != 661) || typeAttemptedToPlace != 24) && ((tile.TileType != 109 && tile.TileType != 492) || typeAttemptedToPlace != 110) && ((tile.TileType != 199 && tile.TileType != 662) || typeAttemptedToPlace != 201)) {
                if (tile.TileType == 633)
                    return typeAttemptedToPlace == 637;

                return false;
            }

            return true;
        }

        return false;
    }

    private void CheckBannerPlanterBoxHack(On_WorldGen.orig_CheckBanner orig, int x, int j, byte type) {
        foreach (var m in RoA.Instance.GetContent<PlanterBoxes>()) {
            TileID.Sets.Platforms[m.Type] = true;
        }

        orig(x, j, type);

        foreach (var m in RoA.Instance.GetContent<PlanterBoxes>()) {
            TileID.Sets.Platforms[m.Type] = false;
        }
    }

    private void CheckAlchPlanterBoxHack(On_WorldGen.orig_CheckAlch orig, int x, int y) {
        Tile soil = Main.tile[x, y + 1];
        ushort soilType = soil.TileType;
        bool customPlanter = TileLoader.GetTile(soilType) is PlanterBoxes;
        if (customPlanter) {
            soil.TileType = TileID.PlanterBox;
        }

        orig(x, y);

        if (customPlanter) {
            soil.TileType = soilType;
        }
    }

    private bool On_TileObject_CanPlace(On_TileObject.orig_CanPlace orig, int x, int y, int type, int style, int dir, out TileObject objectData, bool onlyCheck, int? forcedRandom, bool checkStay) {
        TileObjectData tileData = TileObjectData.GetTileData(type, style);
        objectData = TileObject.Empty;
        if (tileData == null)
            return false;

        int num = x - tileData.Origin.X;
        int num2 = y - tileData.Origin.Y;
        if (num < 0 || num + tileData.Width >= Main.maxTilesX || num2 < 0 || num2 + tileData.Height >= Main.maxTilesY)
            return false;

        bool flag = tileData.RandomStyleRange > 0;
        if (TileObjectPreviewData.placementCache == null)
            TileObjectPreviewData.placementCache = new TileObjectPreviewData();

        TileObjectPreviewData.placementCache.Reset();
        int num3 = 0;
        if (tileData.AlternatesCount != 0)
            num3 = tileData.AlternatesCount;

        float num4 = -1f;
        float num5 = -1f;
        int num6 = 0;
        TileObjectData tileObjectData = null;
        int num7 = -1;
        while (num7 < num3) {
            num7++;
            TileObjectData tileData2 = TileObjectData.GetTileData(type, style, num7);
            if (tileData2.Direction != 0 && ((tileData2.Direction == TileObjectDirection.PlaceLeft && dir == 1) || (tileData2.Direction == TileObjectDirection.PlaceRight && dir == -1)))
                continue;

            int num8 = x - tileData2.Origin.X;
            int num9 = y - tileData2.Origin.Y;
            if (num8 < 5 || num8 + tileData2.Width > Main.maxTilesX - 5 || num9 < 5 || num9 + tileData2.Height > Main.maxTilesY - 5)
                return false;

            Rectangle rectangle = new Rectangle(0, 0, tileData2.Width, tileData2.Height);
            int num10 = 0;
            int num11 = 0;
            if (tileData2.AnchorTop.tileCount != 0) {
                if (rectangle.Y == 0) {
                    rectangle.Y = -1;
                    rectangle.Height++;
                    num11++;
                }

                int checkStart = tileData2.AnchorTop.checkStart;
                if (checkStart < rectangle.X) {
                    rectangle.Width += rectangle.X - checkStart;
                    num10 += rectangle.X - checkStart;
                    rectangle.X = checkStart;
                }

                int num12 = checkStart + tileData2.AnchorTop.tileCount - 1;
                int num13 = rectangle.X + rectangle.Width - 1;
                if (num12 > num13)
                    rectangle.Width += num12 - num13;
            }

            if (tileData2.AnchorBottom.tileCount != 0) {
                if (rectangle.Y + rectangle.Height == tileData2.Height)
                    rectangle.Height++;

                int checkStart2 = tileData2.AnchorBottom.checkStart;
                if (checkStart2 < rectangle.X) {
                    rectangle.Width += rectangle.X - checkStart2;
                    num10 += rectangle.X - checkStart2;
                    rectangle.X = checkStart2;
                }

                int num14 = checkStart2 + tileData2.AnchorBottom.tileCount - 1;
                int num15 = rectangle.X + rectangle.Width - 1;
                if (num14 > num15)
                    rectangle.Width += num14 - num15;
            }

            if (tileData2.AnchorLeft.tileCount != 0) {
                if (rectangle.X == 0) {
                    rectangle.X = -1;
                    rectangle.Width++;
                    num10++;
                }

                int num16 = tileData2.AnchorLeft.checkStart;
                if ((tileData2.AnchorLeft.type & AnchorType.Tree) == AnchorType.Tree)
                    num16--;

                if (num16 < rectangle.Y) {
                    rectangle.Width += rectangle.Y - num16;
                    num11 += rectangle.Y - num16;
                    rectangle.Y = num16;
                }

                int num17 = num16 + tileData2.AnchorLeft.tileCount - 1;
                if ((tileData2.AnchorLeft.type & AnchorType.Tree) == AnchorType.Tree)
                    num17 += 2;

                int num18 = rectangle.Y + rectangle.Height - 1;
                if (num17 > num18)
                    rectangle.Height += num17 - num18;
            }

            if (tileData2.AnchorRight.tileCount != 0) {
                if (rectangle.X + rectangle.Width == tileData2.Width)
                    rectangle.Width++;

                int num19 = tileData2.AnchorLeft.checkStart;
                if ((tileData2.AnchorRight.type & AnchorType.Tree) == AnchorType.Tree)
                    num19--;

                if (num19 < rectangle.Y) {
                    rectangle.Width += rectangle.Y - num19;
                    num11 += rectangle.Y - num19;
                    rectangle.Y = num19;
                }

                int num20 = num19 + tileData2.AnchorRight.tileCount - 1;
                if ((tileData2.AnchorRight.type & AnchorType.Tree) == AnchorType.Tree)
                    num20 += 2;

                int num21 = rectangle.Y + rectangle.Height - 1;
                if (num20 > num21)
                    rectangle.Height += num20 - num21;
            }

            if (onlyCheck) {
                TileObject.objectPreview.Reset();
                TileObject.objectPreview.Active = true;
                TileObject.objectPreview.Type = (ushort)type;
                TileObject.objectPreview.Style = (short)style;
                TileObject.objectPreview.Alternate = num7;
                TileObject.objectPreview.Size = new Point16(rectangle.Width, rectangle.Height);
                TileObject.objectPreview.ObjectStart = new Point16(num10, num11);
                TileObject.objectPreview.Coordinates = new Point16(num8 - num10, num9 - num11);
            }

            float num22 = 0f;
            float num23 = tileData2.Width * tileData2.Height;
            float num24 = 0f;
            float num25 = 0f;
            for (int i = 0; i < tileData2.Width; i++) {
                for (int j = 0; j < tileData2.Height; j++) {
                    Tile tileSafely = Framing.GetTileSafely(num8 + i, num9 + j);
                    bool flag2 = !tileData2.LiquidPlace(tileSafely, checkStay);
                    bool flag3 = false;
                    if (tileData2.AnchorWall) {
                        num25 += 1f;
                        if (!tileData2.isValidWallAnchor(tileSafely.WallType))
                            flag3 = true;
                        else
                            num24 += 1f;
                    }

                    bool flag4 = false;
                    if (tileSafely.HasTile && (!Main.tileCut[tileSafely.TileType] || tileSafely.TileType == 484 || tileSafely.TileType == 654) && !TileID.Sets.BreakableWhenPlacing[tileSafely.TileType] && !checkStay)
                        flag4 = true;

                    if (flag4 || flag2 || flag3) {
                        if (onlyCheck)
                            TileObject.objectPreview[i + num10, j + num11] = 2;

                        continue;
                    }

                    if (onlyCheck)
                        TileObject.objectPreview[i + num10, j + num11] = 1;

                    num22 += 1f;
                }
            }

            AnchorData anchorBottom = tileData2.AnchorBottom;
            if (anchorBottom.tileCount != 0) {
                num25 += (float)anchorBottom.tileCount;
                int height = tileData2.Height;
                for (int k = 0; k < anchorBottom.tileCount; k++) {
                    int num26 = anchorBottom.checkStart + k;
                    Tile tileSafely = Framing.GetTileSafely(num8 + num26, num9 + height);
                    bool flag5 = false;
                    if (tileSafely.HasUnactuatedTile) {
                        if ((anchorBottom.type & AnchorType.SolidTile) == AnchorType.SolidTile && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType] && !Main.tileNoAttach[tileSafely.TileType] && (tileData2.FlattenAnchors || tileSafely.BlockType == 0))
                            flag5 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag5 && ((anchorBottom.type & AnchorType.SolidWithTop) == AnchorType.SolidWithTop || (anchorBottom.type & AnchorType.Table) == AnchorType.Table)) {
                            if (TileID.Sets.Platforms[tileSafely.TileType]) {
                                _ = tileSafely.TileFrameX / TileObjectData.PlatformFrameWidth();
                                if (!tileSafely.IsHalfBlock && WorldGen.PlatformProperTopFrame(tileSafely.TileFrameX))
                                    flag5 = true;
                            }
                            else if (Main.tileSolid[tileSafely.TileType] && Main.tileSolidTop[tileSafely.TileType]) {
                                flag5 = true;
                            }
                        }

                        if (!flag5 && (anchorBottom.type & AnchorType.Table) == AnchorType.Table && !TileID.Sets.Platforms[tileSafely.TileType] && Main.tileTable[tileSafely.TileType] && tileSafely.BlockType == 0)
                            flag5 = true;

                        if (!flag5 && (anchorBottom.type & AnchorType.SolidSide) == AnchorType.SolidSide && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType]) {
                            int num27 = (int)tileSafely.BlockType;
                            if ((uint)(num27 - 4) <= 1u)
                                flag5 = tileData2.isValidTileAnchor(tileSafely.TileType);
                        }

                        if (!flag5 && (anchorBottom.type & AnchorType.AlternateTile) == AnchorType.AlternateTile && tileData2.isValidAlternateAnchor(tileSafely.TileType))
                            flag5 = true;
                    }
                    else if (!flag5 && (anchorBottom.type & AnchorType.EmptyTile) == AnchorType.EmptyTile) {
                        flag5 = true;
                    }

                    if (!flag5) {
                        if (onlyCheck)
                            TileObject.objectPreview[num26 + num10, height + num11] = 2;

                        continue;
                    }

                    if (onlyCheck)
                        TileObject.objectPreview[num26 + num10, height + num11] = 1;

                    num24 += 1f;
                }
            }

            anchorBottom = tileData2.AnchorTop;
            if (anchorBottom.tileCount != 0) {
                num25 += (float)anchorBottom.tileCount;
                int num28 = -1;
                for (int l = 0; l < anchorBottom.tileCount; l++) {
                    int num29 = anchorBottom.checkStart + l;
                    Tile tileSafely = Framing.GetTileSafely(num8 + num29, num9 + num28);
                    bool flag6 = false;
                    if (tileSafely.HasUnactuatedTile) {
                        if ((anchorBottom.type & AnchorType.SolidTile) == AnchorType.SolidTile && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType] && !Main.tileNoAttach[tileSafely.TileType] && (tileData2.FlattenAnchors || tileSafely.BlockType == 0)) // AnchorTop
                            flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag6 && (anchorBottom.type & AnchorType.SolidBottom) == AnchorType.SolidBottom && ((Main.tileSolid[tileSafely.TileType] && (!Main.tileSolidTop[tileSafely.TileType] || (TileID.Sets.Platforms[tileSafely.TileType] && (tileSafely.IsHalfBlock || tileSafely.TopSlope)))) || tileSafely.IsHalfBlock || tileSafely.TopSlope) && !TileID.Sets.NotReallySolid[tileSafely.TileType] && !tileSafely.BottomSlope)
                            flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag6 && (anchorBottom.type & AnchorType.Platform) == AnchorType.Platform && TileID.Sets.Platforms[tileSafely.TileType])
                            flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag6 && (anchorBottom.type & AnchorType.PlatformNonHammered) == AnchorType.PlatformNonHammered && TileID.Sets.Platforms[tileSafely.TileType] && tileSafely.Slope == 0 && !tileSafely.IsHalfBlock)
                            flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag6 && (anchorBottom.type & AnchorType.PlanterBox) == AnchorType.PlanterBox && (tileSafely.TileType == 380 ||
                            tileSafely.TileType == ModContent.TileType<PlanterBoxes>()))
                            flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag6 && (anchorBottom.type & AnchorType.SolidSide) == AnchorType.SolidSide && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType]) {
                            int num27 = (int)tileSafely.BlockType;
                            if ((uint)(num27 - 2) <= 1u)
                                flag6 = tileData2.isValidTileAnchor(tileSafely.TileType);
                        }

                        if (!flag6 && (anchorBottom.type & AnchorType.AlternateTile) == AnchorType.AlternateTile && tileData2.isValidAlternateAnchor(tileSafely.TileType))
                            flag6 = true;
                    }
                    else if (!flag6 && (anchorBottom.type & AnchorType.EmptyTile) == AnchorType.EmptyTile) {
                        flag6 = true;
                    }

                    if (!flag6) {
                        if (onlyCheck)
                            TileObject.objectPreview[num29 + num10, num28 + num11] = 2;

                        continue;
                    }

                    if (onlyCheck)
                        TileObject.objectPreview[num29 + num10, num28 + num11] = 1;

                    num24 += 1f;
                }
            }

            anchorBottom = tileData2.AnchorRight;
            if (anchorBottom.tileCount != 0) {
                num25 += (float)anchorBottom.tileCount;
                int width = tileData2.Width;
                for (int m = 0; m < anchorBottom.tileCount; m++) {
                    int num30 = anchorBottom.checkStart + m;
                    Tile tileSafely = Framing.GetTileSafely(num8 + width, num9 + num30);
                    bool flag7 = false;
                    if (tileSafely.HasUnactuatedTile) {
                        if ((anchorBottom.type & AnchorType.SolidTile) == AnchorType.SolidTile && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType] && !Main.tileNoAttach[tileSafely.TileType] && (tileData2.FlattenAnchors || tileSafely.BlockType == 0)) // AnchorRight
                            flag7 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag7 && (anchorBottom.type & AnchorType.SolidSide) == AnchorType.SolidSide && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType]) {
                            int num27 = (int)tileSafely.BlockType;
                            if (num27 == 2 || num27 == 4)
                                flag7 = tileData2.isValidTileAnchor(tileSafely.TileType);
                        }

                        if (!flag7 && (anchorBottom.type & AnchorType.Tree) == AnchorType.Tree && TileID.Sets.IsATreeTrunk[tileSafely.TileType]) {
                            flag7 = true;
                            if (m == 0) {
                                num25 += 1f;
                                Tile tileSafely2 = Framing.GetTileSafely(num8 + width, num9 + num30 - 1);
                                if (tileSafely2.HasUnactuatedTile && TileID.Sets.IsATreeTrunk[tileSafely2.TileType]) {
                                    num24 += 1f;
                                    if (onlyCheck)
                                        TileObject.objectPreview[width + num10, num30 + num11 - 1] = 1;
                                }
                                else if (onlyCheck) {
                                    TileObject.objectPreview[width + num10, num30 + num11 - 1] = 2;
                                }
                            }

                            if (m == anchorBottom.tileCount - 1) {
                                num25 += 1f;
                                Tile tileSafely3 = Framing.GetTileSafely(num8 + width, num9 + num30 + 1);
                                if (tileSafely3.HasUnactuatedTile && TileID.Sets.IsATreeTrunk[tileSafely3.TileType]) {
                                    num24 += 1f;
                                    if (onlyCheck)
                                        TileObject.objectPreview[width + num10, num30 + num11 + 1] = 1;
                                }
                                else if (onlyCheck) {
                                    TileObject.objectPreview[width + num10, num30 + num11 + 1] = 2;
                                }
                            }
                        }

                        if (!flag7 && (anchorBottom.type & AnchorType.AlternateTile) == AnchorType.AlternateTile && tileData2.isValidAlternateAnchor(tileSafely.TileType))
                            flag7 = true;
                    }
                    else if (!flag7 && (anchorBottom.type & AnchorType.EmptyTile) == AnchorType.EmptyTile) {
                        flag7 = true;
                    }

                    if (!flag7) {
                        if (onlyCheck)
                            TileObject.objectPreview[width + num10, num30 + num11] = 2;

                        continue;
                    }

                    if (onlyCheck)
                        TileObject.objectPreview[width + num10, num30 + num11] = 1;

                    num24 += 1f;
                }
            }

            anchorBottom = tileData2.AnchorLeft;
            if (anchorBottom.tileCount != 0) {
                num25 += (float)anchorBottom.tileCount;
                int num31 = -1;
                for (int n = 0; n < anchorBottom.tileCount; n++) {
                    int num32 = anchorBottom.checkStart + n;
                    Tile tileSafely = Framing.GetTileSafely(num8 + num31, num9 + num32);
                    bool flag8 = false;
                    if (tileSafely.HasUnactuatedTile) {
                        if ((anchorBottom.type & AnchorType.SolidTile) == AnchorType.SolidTile && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType] && !Main.tileNoAttach[tileSafely.TileType] && (tileData2.FlattenAnchors || tileSafely.BlockType == 0)) // AnchorLeft
                            flag8 = tileData2.isValidTileAnchor(tileSafely.TileType);

                        if (!flag8 && (anchorBottom.type & AnchorType.SolidSide) == AnchorType.SolidSide && Main.tileSolid[tileSafely.TileType] && !Main.tileSolidTop[tileSafely.TileType]) {
                            int num27 = (int)tileSafely.BlockType;
                            if (num27 == 3 || num27 == 5)
                                flag8 = tileData2.isValidTileAnchor(tileSafely.TileType);
                        }

                        if (!flag8 && (anchorBottom.type & AnchorType.Tree) == AnchorType.Tree && TileID.Sets.IsATreeTrunk[tileSafely.TileType]) {
                            flag8 = true;
                            if (n == 0) {
                                num25 += 1f;
                                Tile tileSafely4 = Framing.GetTileSafely(num8 + num31, num9 + num32 - 1);
                                if (tileSafely4.HasUnactuatedTile && TileID.Sets.IsATreeTrunk[tileSafely4.TileType]) {
                                    num24 += 1f;
                                    if (onlyCheck)
                                        TileObject.objectPreview[num31 + num10, num32 + num11 - 1] = 1;
                                }
                                else if (onlyCheck) {
                                    TileObject.objectPreview[num31 + num10, num32 + num11 - 1] = 2;
                                }
                            }

                            if (n == anchorBottom.tileCount - 1) {
                                num25 += 1f;
                                Tile tileSafely5 = Framing.GetTileSafely(num8 + num31, num9 + num32 + 1);
                                if (tileSafely5.HasUnactuatedTile && TileID.Sets.IsATreeTrunk[tileSafely5.TileType]) {
                                    num24 += 1f;
                                    if (onlyCheck)
                                        TileObject.objectPreview[num31 + num10, num32 + num11 + 1] = 1;
                                }
                                else if (onlyCheck) {
                                    TileObject.objectPreview[num31 + num10, num32 + num11 + 1] = 2;
                                }
                            }
                        }

                        if (!flag8 && (anchorBottom.type & AnchorType.AlternateTile) == AnchorType.AlternateTile && tileData2.isValidAlternateAnchor(tileSafely.TileType))
                            flag8 = true;
                    }
                    else if (!flag8 && (anchorBottom.type & AnchorType.EmptyTile) == AnchorType.EmptyTile) {
                        flag8 = true;
                    }

                    if (!flag8) {
                        if (onlyCheck)
                            TileObject.objectPreview[num31 + num10, num32 + num11] = 2;

                        continue;
                    }

                    if (onlyCheck)
                        TileObject.objectPreview[num31 + num10, num32 + num11] = 1;

                    num24 += 1f;
                }
            }

            if (tileData2.HookCheckIfCanPlace.hook != null) {
                if (tileData2.HookCheckIfCanPlace.processedCoordinates) {
                    _ = tileData2.Origin;
                    _ = tileData2.Origin;
                }

                if (tileData2.HookCheckIfCanPlace.hook(x, y, type, style, dir, num7) == tileData2.HookCheckIfCanPlace.badReturn && tileData2.HookCheckIfCanPlace.badResponse == 0) {
                    num24 = 0f;
                    num22 = 0f;
                    TileObject.objectPreview.AllInvalid();
                }
            }

            float num33 = num24 / num25;
            // Backport a fix for tiles with no anchors: if (totalAnchorCount == 0) anchorPercent = 1;
            if (num25 == 0)
                num33 = 1;
            float num34 = num22 / num23;
            if (num34 == 1f && num25 == 0f) {
                num23 = 1f;
                num25 = 1f;
                num33 = 1f;
                num34 = 1f;
            }

            if (num33 == 1f && num34 == 1f) {
                num4 = 1f;
                num5 = 1f;
                num6 = num7;
                tileObjectData = tileData2;
                break;
            }

            if (num33 > num4 || (num33 == num4 && num34 > num5)) {
                TileObjectPreviewData.placementCache.CopyFrom(TileObject.objectPreview);
                num4 = num33;
                num5 = num34;
                tileObjectData = tileData2;
                num6 = num7;
            }
        }

        int num35 = -1;
        if (flag) {
            if (TileObjectPreviewData.randomCache == null)
                TileObjectPreviewData.randomCache = new TileObjectPreviewData();

            bool flag9 = false;
            if (TileObjectPreviewData.randomCache.Type == type) {
                Point16 coordinates = TileObjectPreviewData.randomCache.Coordinates;
                Point16 objectStart = TileObjectPreviewData.randomCache.ObjectStart;
                int num36 = coordinates.X + objectStart.X;
                int num37 = coordinates.Y + objectStart.Y;
                int num38 = x - tileObjectData.Origin.X;
                int num39 = y - tileObjectData.Origin.Y;
                /* Fix random cache not working with alternates with different origins (mistakenly cycles new random each update), it should use the alternates TOD, not the tile styles TOD 
				int num38 = x - tileData.Origin.X;
				int num39 = y - tileData.Origin.Y;
				*/

                if (num36 != num38 || num37 != num39)
                    flag9 = true;
            }
            else {
                flag9 = true;
            }

            int randomStyleRange = tileData.RandomStyleRange;
            int num40 = Main.rand.Next(tileData.RandomStyleRange);
            if (forcedRandom.HasValue)
                num40 = (forcedRandom.Value % randomStyleRange + randomStyleRange) % randomStyleRange;

            num35 = ((!flag9 && !forcedRandom.HasValue) ? TileObjectPreviewData.randomCache.Random : num40);
        }

        if (tileData.SpecificRandomStyles != null) {
            if (TileObjectPreviewData.randomCache == null)
                TileObjectPreviewData.randomCache = new TileObjectPreviewData();

            bool flag10 = false;
            if (TileObjectPreviewData.randomCache.Type == type) {
                Point16 coordinates2 = TileObjectPreviewData.randomCache.Coordinates;
                Point16 objectStart2 = TileObjectPreviewData.randomCache.ObjectStart;
                int num41 = coordinates2.X + objectStart2.X;
                int num42 = coordinates2.Y + objectStart2.Y;
                int num43 = x - tileData.Origin.X;
                int num44 = y - tileData.Origin.Y;
                if (num41 != num43 || num42 != num44)
                    flag10 = true;
            }
            else {
                flag10 = true;
            }

            int num45 = tileData.SpecificRandomStyles.Length;
            int num46 = Main.rand.Next(num45);
            if (forcedRandom.HasValue)
                num46 = (forcedRandom.Value % num45 + num45) % num45;

            num35 = ((!flag10 && !forcedRandom.HasValue) ? TileObjectPreviewData.randomCache.Random : (tileData.SpecificRandomStyles[num46] - style));
        }

        if (onlyCheck) {
            if (num4 != 1f || num5 != 1f) {
                TileObject.objectPreview.CopyFrom(TileObjectPreviewData.placementCache);
                num7 = num6;
            }

            TileObject.objectPreview.Random = num35;
            if (tileData.RandomStyleRange > 0 || tileData.SpecificRandomStyles != null)
                TileObjectPreviewData.randomCache.CopyFrom(TileObject.objectPreview);
        }

        if (!onlyCheck) {
            objectData.xCoord = x - tileObjectData.Origin.X;
            objectData.yCoord = y - tileObjectData.Origin.Y;
            objectData.type = type;
            objectData.style = style;
            objectData.alternate = num7;
            objectData.random = num35;
        }

        if (num4 == 1f)
            return num5 == 1f;

        return false;
    }
}
