using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.BackwoodsSystems;
using RoA.Common.Cache;
using RoA.Common.Sets;
using RoA.Common.Tiles;
using RoA.Common.Utilities.Extensions;
﻿using RoA.Common.World;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Gores;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Solid;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Trees;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

using static RoA.Common.Tiles.TileHooks;

namespace RoA.Content.Tiles.Ambient.LargeTrees;

sealed class BackwoodsBigTree : ModTile, IPostDraw, IRequireMinAxePower, IResistToAxe {
    public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight) {
        //if (IsStart(i, j) && !WorldGenHelper.GetTileSafely(i, j + 1).ActiveTile(ModContent.TileType<BackwoodsGrass>())) {
        //    WorldGen.KillTile(i, j);
        //}
    }

    int IRequireMinAxePower.MinAxe => PrimordialTree.MINAXEREQUIRED;

    bool IResistToAxe.CanBeApplied(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && (IsTrunk(i, j) || IsStart(i, j));
    float IResistToAxe.ResistToPick => 0.25f;

    public override void Load() {
        On_TileDrawing.DrawTrees += On_TileDrawing_DrawTrees;
        On_WorldGen.AttemptToGrowTreeFromSapling += On_WorldGen_AttemptToGrowTreeFromSapling;
        On_WorldGen.CanKillTile_int_int_refBoolean += On_WorldGen_CanKillTile_int_int_refBoolean;
        On_WorldGen.smCallBack += On_WorldGen_smCallBack;
        On_Player.TryReplantingTree += On_Player_TryReplantingTree;
        On_Player.IsBottomOfTreeTrunkNoRoots += On_Player_IsBottomOfTreeTrunkNoRoots;
    }

    private bool On_Player_IsBottomOfTreeTrunkNoRoots(On_Player.orig_IsBottomOfTreeTrunkNoRoots orig, Player self, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (!tile.HasTile)
            return false;

        int type = ModContent.TileType<BackwoodsBigTree>();
        if (tile.TileType == type && IsStart(x, y) && Main.tile[x + 1, y].TileType == type && Main.tile[x - 1, y].TileType == type) {
            return true;
        }

        return orig(self, x, y);
    }

    private void On_Player_TryReplantingTree(On_Player.orig_TryReplantingTree orig, Player self, int x, int y) {
        int type = 20;
        int style = 0;

        PlantLoader.CheckAndInjectModSapling(x, y, ref type, ref style);

        if (TileObject.CanPlace(Player.tileTargetX, Player.tileTargetY, type, style, self.direction, out var objectData)) {
            bool num = TileObject.Place(objectData);
            WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);
            if (num) {
                TileObjectData.CallPostPlacementPlayerHook(Player.tileTargetX, Player.tileTargetY, type, style, self.direction, objectData.alternate, objectData);
                if (Main.netMode == 1)
                    NetMessage.SendObjectPlacement(-1, Player.tileTargetX, Player.tileTargetY, objectData.type, objectData.style, objectData.alternate, objectData.random, self.direction);
            }
        }
        type = 20;
        if (WorldGenHelper.GetTileSafely(Player.tileTargetX + 1, Player.tileTargetY).TileType == ModContent.TileType<BackwoodsBigTree>() && WorldGenHelper.GetTileSafely(Player.tileTargetX + 2, Player.tileTargetY).TileType == ModContent.TileType<BackwoodsBigTree>()) {
            PlantLoader.CheckAndInjectModSapling(x + 1, y, ref type, ref style);
            if (TileObject.CanPlace(Player.tileTargetX + 1, Player.tileTargetY, type, style, self.direction, out objectData)) {
                bool num = TileObject.Place(objectData);
                WorldGen.SquareTileFrame(Player.tileTargetX + 1, Player.tileTargetY);
                if (num) {
                    TileObjectData.CallPostPlacementPlayerHook(Player.tileTargetX + 1, Player.tileTargetY, type, style, self.direction, objectData.alternate, objectData);
                    if (Main.netMode == 1)
                        NetMessage.SendObjectPlacement(-1, Player.tileTargetX + 1, Player.tileTargetY, objectData.type, objectData.style, objectData.alternate, objectData.random, self.direction);
                }
            }
        }
        type = 20;
        if (WorldGenHelper.GetTileSafely(Player.tileTargetX - 1, Player.tileTargetY).TileType == ModContent.TileType<BackwoodsBigTree>() && WorldGenHelper.GetTileSafely(Player.tileTargetX - 2, Player.tileTargetY).TileType == ModContent.TileType<BackwoodsBigTree>()) {
            PlantLoader.CheckAndInjectModSapling(x - 1, y, ref type, ref style);
            if (TileObject.CanPlace(Player.tileTargetX - 1, Player.tileTargetY, type, style, self.direction, out objectData)) {
                bool num = TileObject.Place(objectData);
                WorldGen.SquareTileFrame(Player.tileTargetX - 1, Player.tileTargetY);
                if (num) {
                    TileObjectData.CallPostPlacementPlayerHook(Player.tileTargetX - 1, Player.tileTargetY, type, style, self.direction, objectData.alternate, objectData);
                    if (Main.netMode == 1)
                        NetMessage.SendObjectPlacement(-1, Player.tileTargetX - 1, Player.tileTargetY, objectData.type, objectData.style, objectData.alternate, objectData.random, self.direction);
                }
            }
        }
    }

    private void On_WorldGen_smCallBack(On_WorldGen.orig_smCallBack orig, object threadContext) {
        orig(threadContext);

        foreach (Point16 position in BackwoodsVars.AllTreesWorldPositions.ToList()) {
            bool flag = false;
            for (int checkX = -10; checkX < 11; checkX++) {
                if (WorldGenHelper.GetTileSafely(position.X + checkX, position.Y).TileType == GetSelfType()) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                TryGrowBigTree(position.X, position.Y + 1, placeRand: WorldGen.genRand, ignoreAcorns: true, ignoreTrees: true, gen: true);
            }
        }
        foreach (Point16 position in BackwoodsVars.AllTreesWorldPositions.ToList()) {
            BackwoodsVars.AllTreesWorldPositions.Remove(new Point16(position.X, position.Y));
            BackwoodsVars.BackwoodsTreeCountInWorld--;
        }

        BackwoodsVars.AllTreesWorldPositions.Clear();
        BackwoodsVars.BackwoodsTreeCountInWorld = 0;
    }

    private bool On_WorldGen_CanKillTile_int_int_refBoolean(On_WorldGen.orig_CanKillTile_int_int_refBoolean orig, int i, int j, out bool blockDamaged) {
        blockDamaged = false;
        if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
            return false;

        Tile tile = Main.tile[i, j];
        Tile tile2 = default;

        if (!tile.HasTile)
            return false;

        if (!TileLoader.CanKillTile(i, j, tile.TileType, ref blockDamaged))
            return false;

        if (j >= 1)
            tile2 = Main.tile[i, j - 1];

        if (tile2.HasTile) {
            int type = tile2.TileType;
            if (type == GetSelfType() && tile.TileType != type) {
                return false;
            }

        }

        return orig(i, j, out blockDamaged);
    }

    private bool On_WorldGen_AttemptToGrowTreeFromSapling(On_WorldGen.orig_AttemptToGrowTreeFromSapling orig, int x, int y, bool underground) {
        if (!underground && Main.tile[x, y].TileType == TileID.Saplings) {
            if (TryGrowBigTree(x, y, placeRand: WorldGen.genRand)) {
                return true;
            }
        }

        return orig(x, y, underground);
    }

    public static bool TryGrowBigTree(int i, int j, int height = -1, UnifiedRandom placeRand = null, bool shouldCheckExtraOneTile = true, bool ignoreAcorns = false, bool ignoreTrees = false, bool shouldMainCheck = true, bool gen = false) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return false;
        }

        if (!WorldGen.InWorld(i, j, 2)) {
            return false;
        }

        placeRand ??= Main.rand;
        if (height == -1) {
            height = placeRand.Next(17, 24);
        }

        for (; TileID.Sets.TreeSapling[Main.tile[i, j].TileType]; j++) {
        }

        if (!TileID.Sets.TreeSapling[Main.tile[i + 1, j - 1].TileType] && !ignoreAcorns) {
            return false;
        }

        //if ((Main.tile[i - 1, j - 1].LiquidAmount != 0 || Main.tile[i, j - 1].LiquidAmount != 0 || Main.tile[i + 1, j - 1].LiquidAmount != 0) && !WorldGen.notTheBees) {
        //    return false;
        //}

        int i2 = i;
        int grassTileType = ModContent.TileType<BackwoodsGrass>();
        if (shouldMainCheck) {
            if (!(Main.tile[i2, j].HasUnactuatedTile && !Main.tile[i2, j].IsHalfBlock && Main.tile[i2, j].Slope == 0 &&
                Main.tile[i2 - 1, j].TileType == grassTileType && Main.tile[i2, j].TileType == grassTileType && Main.tile[i2 + 1, j].TileType == grassTileType && Main.tile[i2 + 2, j].TileType == grassTileType &&
                ((Main.remixWorld && (double)j > Main.worldSurface) || Main.tile[i2, j - 1].WallType == 0 || WorldGen.DefaultTreeWallTest(Main.tile[i2, j - 1].WallType)))) {
                return false;
            }
        }

        int num = !shouldCheckExtraOneTile ? 1 : 2;
        int num2 = height;
        int num3 = num2 + 4;
        bool flag = false;
        List<int> ignore = [];
        ignore.Add(20);
        if (ignoreTrees) {
            ignore.Add(TileID.Trees);
            ignore.Add(ModContent.TileType<TreeBranch>());
        }

        if (!gen) {
            if (WorldGenHelper.CustomEmptyTileCheck(i - num, i + num + 1, j - num3, j - 1, ignore.ToArray()) && WorldGenHelper.CustomEmptyTileCheck(i - 1, i + 2, j - 2, j - 1, ignore.ToArray()))
                flag = true;
        }
        else {
            if (WorldGenHelper.CustomEmptyTileCheck2(i - num, i + num + 1, j - num3, j - 1, ignore.ToArray()) && WorldGenHelper.CustomEmptyTileCheck2(i - 1, i + 2, j - 2, j - 1, ignore.ToArray()))
                flag = true;
        }

        if (flag && !shouldCheckExtraOneTile) {
            if (WorldGenHelper.GetTileSafely(i - 2, j).TileType == GetSelfType() || WorldGenHelper.GetTileSafely(i + 3, j).TileType == GetSelfType()) {
                flag = false;
            }
        }

        if (!flag) {
            return false;
        }

        j -= 1;
        PlaceBegin(i, j, height, placeRand, out Point pointToStartPlacingTrunk, gen);
        PlaceTrunk(pointToStartPlacingTrunk, height, placeRand, gen);

        return true;
    }

    public override void SetStaticDefaults() {
        LocalizedText name = CreateMapEntryName();

        DustType = ModContent.DustType<WoodTrash>();
        HitSound = SoundID.Dig;

        TileID.Sets.IsATreeTrunk[Type] = true;

        TileID.Sets.GetsCheckedForLeaves[Type] = true;

        Main.tileMergeDirt[Type] = false;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;

        Main.tileLavaDeath[Type] = true;

        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

        TileSets.ShouldKillTileBelow[Type] = false;
        TileSets.PreventsSlopesBelow[Type] = true;
        CantBeSlopedTileSystem.Included[Type] = true;

        AddMapEntry(new Color(114, 81, 57), name);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (Main.rand.NextBool(7)) {
            yield return new Item(ItemID.Acorn, Main.rand.Next(1, 3));
        }

        yield return new Item(ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
    }

    public static bool IsStart(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && !IsBranch(i, j) && !WorldGenHelper.ActiveTile(i, j + 1, GetSelfType());

    public static bool IsTrunk(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && !IsStart(i, j) && !IsNormalBranch(i, j) && !IsBigBranch(i, j);

    private static bool IsBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && tile.TileFrameY >= 108 && tile.TileFrameY < 180;
    }

    private static bool IsBranch2(int i, int j) => IsNormalBranch(i, j) || IsBigBranch(i, j);

    private static bool IsBigBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && IsBranch(i, j) && tile.TileFrameX == 144;
    }

    private static bool IsNormalBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && IsBranch(i, j) && tile.TileFrameX == 108;
    }

    private static bool IsTop(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && tile.TileFrameX == 54 && tile.TileFrameY == 0;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (fail) {
            return;
        }

        if (IsStart(i, j + 1)) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j + 1);
            tile.TileFrameX += 54;
        }

        UnifiedRandom placeRandom = WorldGen.genRand;
        bool left = !WorldGenHelper.ActiveTile(i - 1, j, Type);
        int direction = -left.ToDirectionInt();
        if (IsTop(i, j)) {
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int count = placeRandom.Next(3, 6) * 10;
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 100f) * direction, placeRandom.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i, j - 15) * 16) + offset;
                Gore.NewGore(null,
                    position,
                    new Vector2(),
                    leafGoreType,
                    placeRandom.Next(90, 130) * 0.01f);
            }

            count = placeRandom.Next(3, 6) * 40;
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 100f) * direction, placeRandom.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i, j - 12) * 16) + offset;
                Dust.NewDustPerfect(position, DustType);
            }
        }

        if (IsBigBranch(i, j)) {
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int count = placeRandom.Next(3, 6);
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 20f) * direction, placeRandom.NextFloat(0f, 40f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i + 1 * direction, j - 2) * 16) + offset;
                Gore.NewGore(null,
                    position,
                    new Vector2(),
                    leafGoreType,
                    placeRandom.Next(90, 130) * 0.01f);
                Dust.NewDustPerfect(position, DustType);
            }
        }

        if (IsStart(i, j) && !noItem) {
            for (int checkX = i; checkX < i + 3; checkX++) {
                for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(checkX, checkJ).ActiveTile(Type); checkJ--) {
                    WorldGen.KillTile(checkX, checkJ, false, false, false);
                }
            }
            for (int checkX = i; checkX > i - 3; checkX--) {
                for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(checkX, checkJ).ActiveTile(Type); checkJ--) {
                    WorldGen.KillTile(checkX, checkJ, false, false, false);
                }
            }
            for (int checkX = i; checkX < i + 4; checkX++) {
                if (checkX != i && !WorldGenHelper.GetTileSafely(checkX, j).HasTile) {
                    break;
                }
                if (IsStart(checkX, j)) {
                    WorldGenHelper.GetTileSafely(checkX, j).HasTile = false;
                    for (int k = 0; k < 5; k++) {
                        Dust.NewDustDirect(new Vector2(checkX, j).ToWorldCoordinates(), 16, 16, DustType);
                    }
                    int itemWhoAmI = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(checkX, j), checkX * 16, j * 16, 16, 16, ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
                    if (Main.netMode == NetmodeID.MultiplayerClient && itemWhoAmI >= 0) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
            for (int checkX = i; checkX > i - 4; checkX--) {
                if (checkX != i && !WorldGenHelper.GetTileSafely(checkX, j).HasTile) {
                    break;
                }
                if (IsStart(checkX, j)) {
                    WorldGenHelper.GetTileSafely(checkX, j).HasTile = false;
                    for (int k = 0; k < 5; k++) {
                        Dust.NewDustDirect(new Vector2(checkX, j).ToWorldCoordinates(), 16, 16, DustType);
                    }
                    int itemWhoAmI = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(checkX, j), checkX * 16, j * 16, 16, 16, ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
                    if (Main.netMode == NetmodeID.MultiplayerClient && itemWhoAmI >= 0) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f, 0f, 0f, 0, 0, 0);
                    }
                }
            }

            return;
        }
        if (IsBranch2(i, j) && noItem) {
            return;
        }

        if (IsNormalBranch(i, j)) {
            if (IsBranch(i + 1, j)) {
                WorldGenHelper.GetTileSafely(i + 1, j).TileFrameY -= 72;
            }
            if (IsBranch(i - 1, j)) {
                WorldGenHelper.GetTileSafely(i - 1, j).TileFrameY -= 72;
            }
        }
        if (IsBigBranch(i, j)) {
            if (IsBranch(i + 1, j)) {
                Tile tile = WorldGenHelper.GetTileSafely(i + 1, j);
                tile.TileFrameX -= 54;
                tile.TileFrameY -= 72;
            }
            if (IsBranch(i - 1, j)) {
                Tile tile = WorldGenHelper.GetTileSafely(i - 1, j);
                tile.TileFrameX -= 54;
                tile.TileFrameY -= 72;
            }
        }

        if (IsTrunk(i, j)) {
            if (!noItem) {
                if (IsTrunk(i + 1, j)) {
                    WorldGen.KillTile(i + 1, j, false, false, true);
                }
                if (IsTrunk(i - 1, j)) {
                    WorldGen.KillTile(i - 1, j, false, false, true);
                }
                if (IsBranch(i - 1, j)) {
                    WorldGen.KillTile(i - 1, j, false, false, false);
                }
                if (IsBranch(i + 1, j)) {
                    WorldGen.KillTile(i + 1, j, false, false, false);
                }
                if (IsBranch(i - 2, j)) {
                    WorldGen.KillTile(i - 2, j, false, false, false);
                }
                if (IsBranch(i + 2, j)) {
                    WorldGen.KillTile(i + 2, j, false, false, false);
                }
                for (int destroyExtraX = 1; destroyExtraX < 3; destroyExtraX++) {
                    if (IsBranch2(i + destroyExtraX, j + 1)) {
                        WorldGen.KillTile(i + destroyExtraX, j + 1, false, false, true);
                    }
                    if (IsBranch2(i - destroyExtraX, j + 1)) {
                        WorldGen.KillTile(i - destroyExtraX, j + 1, false, false, true);
                    }
                }
            }
            for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(i, checkJ).ActiveTile(Type); checkJ--) {
                WorldGen.KillTile(i, checkJ, false, false, false);
                if (IsBranch(i - 1, checkJ)) {
                    WorldGen.KillTile(i - 1, checkJ, false, false, false);
                }
                if (IsBranch(i + 1, checkJ)) {
                    WorldGen.KillTile(i + 1, checkJ, false, false, false);
                }
                if (IsTrunk(i + 1, checkJ)) {
                    WorldGen.KillTile(i + 1, checkJ, false, false, false);
                    if (IsBranch(i + 2, checkJ)) {
                        WorldGen.KillTile(i + 2, checkJ, false, false, false);
                    }
                }
                if (IsTrunk(i - 1, checkJ)) {
                    WorldGen.KillTile(i - 1, checkJ, false, false, false);
                    if (IsBranch(i - 2, checkJ)) {
                        WorldGen.KillTile(i - 2, checkJ, false, false, false);
                    }
                }
            }
            SetFramingForCutTrees(i, j + 1, placeRandom);
            SetFramingForCutTrees(i + 1, j + 1, placeRandom);
            SetFramingForCutTrees(i - 1, j + 1, placeRandom);
        }
    }

    private static void SetFramingForCutTrees(int i, int j, UnifiedRandom placeRandom) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (IsTrunk(i, j)) {
            tile.TileFrameX = (short)(18 + (placeRandom.NextBool() ? 18 : 0));
            tile.TileFrameY = (short)(placeRandom.NextBool() ? 18 : 0);
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY += 2;

    private static ushort GetSelfType() => (ushort)ModContent.TileType<BackwoodsBigTree>();

    private static void PlaceTrunk(Point pointToPlaceTrunk, int height, UnifiedRandom placeRand, bool gen = false) {
        int i = pointToPlaceTrunk.X, j = pointToPlaceTrunk.Y;
        bool hasProperStart() {
            for (int checkX = i - 1; checkX < i + 3; checkX++) {
                if (!WorldGenHelper.ActiveTile(checkX, j + 1, GetSelfType())) {
                    return false;
                }
            }
            return true;
        }
        if (!hasProperStart()) {
            return;
        }
        short tileFrameX, tileFrameY;
        for (int placeY = j; placeY > j - height; placeY--) {
            bool canPlaceBranch = placeY != j;
            bool canPlaceBigBranch = WorldGenHelper.GetTileSafely(i, placeY - 1).TileFrameX < 72 && WorldGenHelper.GetTileSafely(i, placeY + 1).TileFrameX < 72;
            GetFramingForTrunk(canPlaceBranch, canPlaceBigBranch, placeRand, out tileFrameX, out tileFrameY, out bool shouldPlaceBranch, out bool shouldPlaceBigBranch);
            short frameXForBranch = (short)(shouldPlaceBigBranch ? 144 : 108);
            if (shouldPlaceBranch || shouldPlaceBigBranch) {
                PlaceTileInternal(i, placeY, tileFrameX, tileFrameY, placeRand, gen);
                PlaceTileInternal(i - 1, placeY, frameXForBranch, tileFrameY, placeRand, gen);
            }
            else {
                PlaceTileInternal(i, placeY, tileFrameX, tileFrameY, placeRand, gen);
            }
            GetFramingForTrunk(canPlaceBranch, canPlaceBigBranch, placeRand, out tileFrameX, out tileFrameY, out shouldPlaceBranch, out shouldPlaceBigBranch, true);
            frameXForBranch = (short)(shouldPlaceBigBranch ? 144 : 108);
            if (shouldPlaceBranch || shouldPlaceBigBranch) {
                PlaceTileInternal(i + 1, placeY, tileFrameX, tileFrameY, placeRand, gen);
                PlaceTileInternal(i + 2, placeY, frameXForBranch, tileFrameY, placeRand, gen);
            }
            else {
                PlaceTileInternal(i + 1, placeY, tileFrameX, tileFrameY, placeRand, gen);
            }
        }
        int topPlaceY = j - height;
        GetFramingForTop(placeRand, out tileFrameX, out tileFrameY);
        PlaceTileInternal(i, topPlaceY, tileFrameX, tileFrameY, placeRand, gen);
        PlaceTileInternal(i + 1, topPlaceY, tileFrameX, tileFrameY, placeRand, gen);
    }

    private static void GetFramingForTop(UnifiedRandom placeRand, out short tileFrameX, out short tileFrameY) {
        tileFrameX = 54;
        tileFrameY = 0;
    }

    private static void GetFramingForTrunk(bool canPlaceBranch, bool canPlaceBigBranch, UnifiedRandom placeRand, out short tileFrameX, out short tileFrameY, out bool shouldPlaceBranch, out bool shouldPlaceBigBranch, bool second = false) {
        shouldPlaceBranch = canPlaceBranch && placeRand.NextBool(7);
        shouldPlaceBigBranch = canPlaceBigBranch && canPlaceBranch && !shouldPlaceBranch && placeRand.NextBool(7);
        if (shouldPlaceBigBranch) {
            shouldPlaceBranch = true;
        }
        short frameY = (short)(shouldPlaceBranch ? 108 : 36);
        tileFrameY = (short)(frameY + placeRand.Next(4) * 18);
        tileFrameX = (short)((shouldPlaceBigBranch ? 72 : 18) + (second ? 18 : 0));
    }

    public override bool CanExplode(int i, int j) {
        if (!Main.hardMode) {
            return false;
        }

        return base.CanExplode(i, j);
    }

    private static void PlaceBegin(int i, int j, int height, UnifiedRandom placeRand, out Point pointToStartPlacingTrunk, bool gen = false) {
        short getFrameYForStart() => (short)(180 + (placeRand.NextBool() ? 18 : 0));
        for (int checkY = j - (int)(height * 2f); checkY < j + 1; checkY++) {
            for (int checkX = i - 1; checkX < i + 3; checkX++) {
                Tile tile = WorldGenHelper.GetTileSafely(checkX, checkY);
                tile.HasTile = false;
            }
        }
        for (int checkY = j - (int)(height * 2f); checkY < j + 1; checkY++) {
            for (int checkX = i - 4; checkX < i + 6; checkX++) {
                Tile tile2 = WorldGenHelper.GetTileSafely(checkX, checkY);
                if (tile2.TileType == TileID.Trees || tile2.TileType == ModContent.TileType<TreeBranch>()) {
                    bool flag3 = true;
                    for (int x = -1; x < 2; x++) {
                        for (int y = -1; y < 2; y++) {
                            if (Math.Abs(x) != Math.Abs(y)) {
                                Tile tile3 = WorldGenHelper.GetTileSafely(checkX + x, checkY + y);
                                if (tile3.HasTile && (tile3.TileType == TileID.Trees || tile3.TileType == ModContent.TileType<TreeBranch>())) {
                                    flag3 = false;
                                }
                            }
                        }
                    }
                    if (flag3) {
                        tile2.HasTile = false;
                    }
                }
            }
        }
        PlaceTileInternal(i - 1, j, 0, getFrameYForStart(), placeRand, gen);
        PlaceTileInternal(i, j, 18, getFrameYForStart(), placeRand, gen);
        PlaceTileInternal(i + 1, j, 36, getFrameYForStart(), placeRand, gen);
        PlaceTileInternal(i + 2, j, 54, getFrameYForStart(), placeRand, gen);
        for (int checkX = i - 1; checkX < i + 3; checkX++) {
            Tile tile2 = WorldGenHelper.GetTileSafely(checkX, j + 1);
            tile2.IsHalfBlock = false;
            tile2.Slope = 0;
        }
        pointToStartPlacingTrunk = new Point(i, j - 1);
    }

    private static void PlaceTileInternal(int i, int j, short tileFrameX, short tileFrameY, UnifiedRandom placeRand = null, bool gen = false) {
        WorldGen.PlaceTile(i, j, GetSelfType(), true, false, -1, 0);
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        tile.TileFrameX = tileFrameX;
        tile.TileFrameY = tileFrameY;

        placeRand ??= Main.rand;

        float num2 = 10f;

        int chance = (int)(BackwoodsVars.AllTreesWorldPositions.Count / 100f);
        if (!WorldGen.gen) {
            if (!Main.dedServ) {
                if ((gen && placeRand.NextBool(chance)) || !gen)
                    Gore.NewGore(null, new Vector2(i - 1, j).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
                if ((gen && placeRand.NextBool(chance)) || !gen)
                    Gore.NewGore(null, new Vector2(i, j).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
                if ((gen && placeRand.NextBool(chance)) || !gen)
                    Gore.NewGore(null, new Vector2(i + 1, j).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
            }

            if (IsTop(i, j)) {
                if (!Main.dedServ) {
                    for (int k = 0; k < 5; k++) {
                        if ((gen && placeRand.NextBool(chance)) || !gen)
                            Gore.NewGore(null, new Vector2(i - 1, j - k).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
                        if ((gen && placeRand.NextBool(chance)) || !gen)
                            Gore.NewGore(null, new Vector2(i, j - k).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
                        if ((gen && placeRand.NextBool(chance)) || !gen)
                            Gore.NewGore(null, new Vector2(i + 1, j - k).ToWorldCoordinates() + new Vector2(8, 8), Utils.RandomVector2(placeRand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + placeRand.NextFloat() * 0.6f);
                    }

                }

                ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
                int count = placeRand.Next(3, 6) * 10;
                for (int k = 0; k < count; k++) {
                    Vector2 offset = new Vector2(placeRand.NextFloat(-150f, 150f), placeRand.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
                    Vector2 position = (new Vector2(i + 5, j - 15) * 16) + offset;
                    if ((gen && placeRand.NextBool(chance)) || !gen)
                        if (!Main.dedServ) {
                            Gore.NewGore(null,
                                        position,
                                        Utils.RandomVector2(placeRand, 0f - num2, num2),
                                        leafGoreType,
                                        0.7f + placeRand.NextFloat() * 0.6f);
                        }
                }
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        //DrawTop(i, j, spriteBatch);
        //DrawItselfParts(i, j, spriteBatch, Texture, Type);
    }

    private void DrawTop(int i, int j, SpriteBatch spriteBatch) {
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        TileHelper.AddPostSolidTileDrawPoint(this, i, j);

        if (IsNormalBranch(i, j) || IsBigBranch(i, j) || IsTop(i, j)) {
            return false;
        }

        //return true;
        return true;
    }

    private void On_TileDrawing_DrawTrees(On_TileDrawing.orig_DrawTrees orig, TileDrawing self) {
        orig(self);

        foreach ((ModTile modTile, Point16 position) in TileHelper.SolidTileDrawPoints.OrderBy(x => x.Item2.X + x.Item2.Y)) {
            if (modTile is IPostDraw && modTile is not null && modTile is BackwoodsBigTree) {
                int i = position.X, j = position.Y;
                if (!IsTop(i, j)) {
                    DrawItselfParts(i, j, Main.spriteBatch, ResourceManager.TileTextures + "Ambient/LargeTrees/BackwoodsBigTree", ModContent.TileType<BackwoodsBigTree>());
                }
            }
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_treeWindCounter")]
    public extern static ref double TileDrawing_treeWindCounter(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetWindCycle")]
    public extern static float TileDrawing_GetWindCycle(TileDrawing self, int x, int y, double windCounter);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rand")]
    public extern static ref UnifiedRandom TileDrawing_rand(TileDrawing self);

    void IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X, j = pos.Y;
        Tile tile = Main.tile[i, j];
        //Vector2 zero = Vector2.Zero;
        //Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
        //if (tile.TileType == GetSelfType()) {
        //    Main.spriteBatch.DrawSelf(TextureAssets.Tile[tile.TileType].Value, new Vector2(i * 16, j * 16) - Main.screenPosition + zero, value, Lighting.GetColor(i, j), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
        //}

        if (IsTop(i, j) && TileDrawing.IsVisible(tile)) {
            DrawItselfParts(i, j, Main.spriteBatch, ResourceManager.TileTextures + "Ambient/LargeTrees/BackwoodsBigTree", ModContent.TileType<BackwoodsBigTree>());
        }

        tile = WorldGenHelper.GetTileSafely(i, j);
        Vector2 drawPosition = new(i * 16 - (int)Main.screenPosition.X - 18,
                                   j * 16 - (int)Main.screenPosition.Y);
        Color color = Lighting.GetColor(i, j);
        bool left = !IsTrunk(i - 1, j);
        SpriteEffects effects = SpriteEffects.FlipHorizontally;
        if (IsTrunk(i, j) && !IsTop(i, j)) {
            Texture2D extraTexture = PaintsRenderer.TryGetPaintedTexture(i, j, TileLoader.GetTile(GetSelfType()).Texture + "_Extra");
            ulong seed = (ulong)(i * j % 192372);
            if (Utils.RandomInt(ref seed, 10) < 3) {
                int height = 18;
                bool flag = Utils.RandomInt(ref seed, 2) == 0;
                int usedFrame;
                if (flag) {
                    usedFrame = 2 + Utils.RandomInt(ref seed, 3);
                }
                else {
                    usedFrame = Utils.RandomInt(ref seed, 2);
                }
                spriteBatch.Draw(extraTexture, drawPosition + Vector2.UnitX * 14f + new Vector2(left ? 0f : 3f, 3f),
                    new Rectangle(left ? 21 : 0, usedFrame * height, 21, height), color, 0f, Vector2.Zero, 1f, effects, 0);

                if (flag && BackwoodsFogHandler.Opacity > 0f && TileDrawing.IsVisible(tile)) {
                    SpriteBatchSnapshot snapshot = spriteBatch.CaptureSnapshot();
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);
                    ulong speed = (((ulong)j << 32) | (ulong)i);
                    float posX = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                    float posY = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                    int directionX = Utils.RandomInt(ref speed, 2) == 0 ? 1 : -1;
                    int directionY = Utils.RandomInt(ref speed, 2) != 0 ? 1 : -1;
                    float opacity = BackwoodsFogHandler.Opacity > 0f ? BackwoodsFogHandler.Opacity : 1f;
                    spriteBatch.Draw(extraTexture, drawPosition + Vector2.UnitX * 14f + new Vector2(left ? 0f : 3f, 3f) -
                        new Vector2(Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionX * posX,
                        Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionY * posY),
                        new Rectangle(left ? 21 : 0, usedFrame * height, 21, height), Color.Lerp(Color.White, color, 0.8f) * opacity, 0f, Vector2.Zero, 1f, effects, 0);
                    spriteBatch.End();
                    spriteBatch.Begin(in snapshot);
                }

                if (flag) {
                    if (Main.rand.NextBool(1050)) {
                        Dust dust = Dust.NewDustPerfect(drawPosition + Main.rand.Random2(0, tile.TileFrameX, 0, tile.TileFrameY), ModContent.DustType<TreeDust>());
                        dust.velocity *= 0.5f + Main.rand.NextFloat() * 0.25f;
                        dust.scale *= 1.1f;
                    }
                }
            }
        }
    }

    private static void DrawItselfParts(int i, int j, SpriteBatch spriteBatch, string texture, int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (!TileDrawing.IsVisible(tile)) {
            return;
        }
        bool shouldDrawBranch = IsNormalBranch(i, j);
        Vector2 drawPosition = new(i * 16 - (int)Main.screenPosition.X - 18,
                                   j * 16 - (int)Main.screenPosition.Y);
        bool shouldDrawBigBranch = IsBigBranch(i, j);
        bool left = !WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(GetSelfType());
        SpriteFrame spriteFrame;
        byte variant;
        int offsetX, offsetY;
        Color color = Lighting.GetColor(i, j);
        SpriteEffects effects = SpriteEffects.None;
        UnifiedRandom random = TileDrawing_rand(Main.instance.TilesRenderer);
        if (!left) {
            effects = SpriteEffects.FlipHorizontally;
        }
        bool flag = tile.WallType > 0;
        if (shouldDrawBigBranch) {
            Texture2D bigBranchTexture = PaintsRenderer.TryGetPaintedTexture(i, j, texture + "_BigBranches");
            Vector2 textureSize = bigBranchTexture.Size();
            spriteFrame = new(1, 3);
            ulong seed = (ulong)(i * j % 192372);
            variant = (byte)Utils.RandomInt(ref seed, 3);
            spriteFrame = spriteFrame.With(0, variant);
            Rectangle sourceRectangle = spriteFrame.GetSourceRectangle(bigBranchTexture);
            int direction = -left.ToDirectionInt();
            offsetX = -left.ToDirectionInt() * (bigBranchTexture.Width - 12);
            offsetX -= bigBranchTexture.Width / 2 * direction;
            offsetX -= 4 * direction;
            if (left) {
                offsetX -= 8;
            }
            offsetY = -sourceRectangle.Height / 2;
            drawPosition.X += offsetX;
            drawPosition.Y += offsetY;
            float num8 = 0f;
            float num4 = 0.06f;
            if (!flag)
                num8 = TileDrawing_GetWindCycle(Main.instance.TilesRenderer, i, j, TileDrawing_treeWindCounter(Main.instance.TilesRenderer));
            if (num8 < 0f)
                drawPosition.X += num8 / 10f;
            drawPosition.X -= Math.Abs(num8 / 10f) * 2f;
            //float num = Main.WindForVisuals;
            //if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            //    num = Math.Max(Math.Abs(Main.WindForVisuals), 401 * 0.001f);
            //    drawPosition.X -= 3f;
            //}

            Vector2 origin = new(!left ? 0f : textureSize.X, textureSize.Y / 3f);
            spriteBatch.Draw(bigBranchTexture, drawPosition - Vector2.UnitX * 10f + origin, sourceRectangle, color, num8 * num4, origin, 1f, effects, 0f);
        }
        if (shouldDrawBranch) {
            Texture2D branchTexture = PaintsRenderer.TryGetPaintedTexture(i, j, texture + "_Branches");
            offsetY = 0;
            if (left) {
                offsetX = 10;
            }
            else {
                offsetX = 26;
            }
            drawPosition.X += offsetX;
            drawPosition.Y += offsetY;
            variant = (byte)((tile.TileFrameY - 108) / 18);
            spriteFrame = new(1, 4);
            spriteFrame = spriteFrame.With(0, variant);
            spriteBatch.Draw(branchTexture, drawPosition - Vector2.UnitX * 10f, spriteFrame.GetSourceRectangle(branchTexture), color, 0f, Vector2.Zero, 1f, effects, 0f);
        }

        bool shouldDrawTop = IsTop(i, j);
        if (shouldDrawTop) {
            if (WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(type)) {
                return;
            }
            Texture2D topTexture = ModContent.Request<Texture2D>(texture + "_Top").Value;
            if ((Main.tile[i + 1, j].TileType == type && Main.tile[i + 1, j].TileColor == Main.tile[i, j].TileColor) ||
                (Main.tile[i - 1, j].TileType == type && Main.tile[i - 1, j].TileColor == Main.tile[i, j].TileColor)) {
                topTexture = PaintsRenderer.TryGetPaintedTexture(i, j, texture + "_Top");
            }
            Vector2 textureSize = topTexture.Size();
            Vector2 offset = -textureSize;
            offset.X += textureSize.X / 2f;
            offset += new Vector2(36f, 18f);
            effects = SpriteEffects.None;
            offset.X += effects == SpriteEffects.None ? 4f : -10f;

            float num3 = 0.02f;
            float num15 = 0f;
            if (!flag)
                num15 = TileDrawing_GetWindCycle(Main.instance.TilesRenderer, i, j, TileDrawing_treeWindCounter(Main.instance.TilesRenderer));
            drawPosition.X += num15 * 2f;
            drawPosition.Y += Math.Abs(num15) * 2f;
            Vector2 origin = new(textureSize.X / 2f, textureSize.Y);
            spriteBatch.Draw(topTexture, drawPosition + offset + origin, null, color, num15 * num3, origin, 1f, effects, 0f);

            Vector2 position = (new Vector2(i, j - 15) * 16) + new Vector2(random.NextFloat(-100f, 100f), random.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
            spawnLeafs(position, true);
        }

        void spawnLeafs(Vector2? position = null, bool increasedSpawnRate = false) {
            IEntitySource entitySource = new EntitySource_TileUpdate(i, j);
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int x = i, y = j;
            int chance = typeof(TileDrawing).GetFieldValue<int>("_leafFrequency", Main.instance.TilesRenderer);
            if (increasedSpawnRate) {
                chance /= 40;
            }
            else {
                chance *= 4;
            }
            chance *= 3;
            if (Main.rand.NextBool(chance)) {
                tile = Main.tile[x, y + 1];
                if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                    float windForVisuals = Main.WindForVisuals;
                    if (!Main.dedServ) {
                        if ((!(windForVisuals < -0.2f) || (!WorldGen.SolidTile(Main.tile[x - 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x - 2, y + 1]))) && (!(windForVisuals > 0.2f) || (!WorldGen.SolidTile(Main.tile[x + 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x + 2, y + 1]))))
                            Gore.NewGorePerfect(entitySource, position != null ? position.Value : new Vector2(x * 16, y * 16 + 16), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
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
                            Gore.NewGorePerfect(entitySource, position != null ? new Vector2(position.Value.X + 8 + 4 * num + num2, position.Value.Y * 16 + 8) : new Vector2(x * 16 + 8 + 4 * num + num2, y * 16 + 8), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                        }
                    }
                }
            }
        }
        spawnLeafs();
    }
}
