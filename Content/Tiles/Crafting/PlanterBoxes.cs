using RoA.Content.Dusts.Backwoods;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class PlanterBoxes : ModTile, IPostSetupContent {
    public override void Load() {
        On_Player.PlaceThing_Tiles_BlockPlacementForAssortedThings += On_Player_PlaceThing_Tiles_BlockPlacementForAssortedThings;
        On_WorldGen.PlaceAlch += On_WorldGen_PlaceAlch;
        On_WorldGen.CheckAlch += On_WorldGen_CheckAlch;
    }

    private void On_WorldGen_CheckAlch(On_WorldGen.orig_CheckAlch orig, int x, int y) {
        //if (Main.tile[x, y] == null)
        //    Main.tile[x, y] = new Tile();

        //if (Main.tile[x, y + 1] == null)
        //    Main.tile[x, y + 1] = new Tile();

        bool flag = false;
        if (!Main.tile[x, y + 1].HasUnactuatedTile)
            flag = true;

        if (Main.tile[x, y + 1].IsHalfBlock)
            flag = true;

        int num = Main.tile[x, y].TileFrameX / 18;
        Main.tile[x, y].TileFrameY = 0;
        if (!flag) {
            int planterBox = ModContent.TileType<PlanterBoxes>();
            switch (num) {
                case 0:
                    if (Main.tile[x, y + 1].TileType != 109 && Main.tile[x, y + 1].TileType != 2 && Main.tile[x, y + 1].TileType != 477 && Main.tile[x, y + 1].TileType != 492 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 1:
                    if (Main.tile[x, y + 1].TileType != 60 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 2:
                    if (Main.tile[x, y + 1].TileType != 0 && Main.tile[x, y + 1].TileType != 59 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 3:
                    if (Main.tile[x, y + 1].TileType != 661 && Main.tile[x, y + 1].TileType != 662 && Main.tile[x, y + 1].TileType != 199 && Main.tile[x, y + 1].TileType != 203 && Main.tile[x, y + 1].TileType != 23 && Main.tile[x, y + 1].TileType != 25 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 4:
                    if (Main.tile[x, y + 1].TileType != 53 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox && Main.tile[x, y + 1].TileType != 116)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 5:
                    if (Main.tile[x, y + 1].TileType != 57 && Main.tile[x, y + 1].TileType != 633 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].TileType == 82 || Main.tile[x, y].LiquidType != LiquidID.Lava || Main.netMode == 1)
                        break;
                    if (Main.tile[x, y].LiquidAmount > 16) {
                        if (Main.tile[x, y].TileType == 83) {
                            Main.tile[x, y].TileType = 84;
                            if (Main.netMode == 2)
                                NetMessage.SendTileSquare(-1, x, y);
                        }
                    }
                    else if (Main.tile[x, y].TileType == 84) {
                        Main.tile[x, y].TileType = 83;
                        if (Main.netMode == 2)
                            NetMessage.SendTileSquare(-1, x, y);
                    }
                    break;
                case 6:
                    if (Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox && Main.tile[x, y + 1].TileType != 147 && Main.tile[x, y + 1].TileType != 161 && Main.tile[x, y + 1].TileType != 163 && Main.tile[x, y + 1].TileType != 164 && Main.tile[x, y + 1].TileType != 200)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
            }
        }

        if (flag)
            WorldGen.KillTile(x, y);
    }

    private bool On_WorldGen_PlaceAlch(On_WorldGen.orig_PlaceAlch orig, int x, int y, int style) {
        //if (Main.tile[x, y] == null)
        //    Main.tile[x, y] = new Tile();

        //if (Main.tile[x, y + 1] == null)
        //    Main.tile[x, y + 1] = new Tile();

        if (!Main.tile[x, y].HasTile && Main.tile[x, y + 1].HasUnactuatedTile && !Main.tile[x, y + 1].IsHalfBlock && Main.tile[x, y + 1].Slope == 0) {
            bool flag = false;
            int planterBox = ModContent.TileType<PlanterBoxes>();
            switch (style) {
                case 0:
                    if (Main.tile[x, y + 1].TileType != 2 && Main.tile[x, y + 1].TileType != 477 && Main.tile[x, y + 1].TileType != 492 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox && Main.tile[x, y + 1].TileType != 109)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0)
                        flag = true;
                    break;
                case 1:
                    if (Main.tile[x, y + 1].TileType != 60 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0)
                        flag = true;
                    break;
                case 2:
                    if (Main.tile[x, y + 1].TileType != 0 && Main.tile[x, y + 1].TileType != 59 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0)
                        flag = true;
                    break;
                case 3:
                    if (Main.tile[x, y + 1].TileType != 661 && Main.tile[x, y + 1].TileType != 662 && Main.tile[x, y + 1].TileType != 203 && Main.tile[x, y + 1].TileType != 199 && Main.tile[x, y + 1].TileType != 23 && Main.tile[x, y + 1].TileType != 25 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0)
                        flag = true;
                    break;
                case 4:
                    if (Main.tile[x, y + 1].TileType != 53 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox && Main.tile[x, y + 1].TileType != 116)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
                case 5:
                    if (Main.tile[x, y + 1].TileType != 57 && Main.tile[x, y + 1].TileType != 633 && Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType != LiquidID.Lava)
                        flag = true;
                    break;
                case 6:
                    if (Main.tile[x, y + 1].TileType != 78 && Main.tile[x, y + 1].TileType != 380 && Main.tile[x, y + 1].TileType != planterBox && Main.tile[x, y + 1].TileType != 147 && Main.tile[x, y + 1].TileType != 161 && Main.tile[x, y + 1].TileType != 163 && Main.tile[x, y + 1].TileType != 164 && Main.tile[x, y + 1].TileType != 200)
                        flag = true;
                    if (Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Lava)
                        flag = true;
                    break;
            }

            if (!flag) {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = 82;
                tile.TileFrameX = (short)(18 * style);
                tile.TileFrameY = 0;
                return true;
            }
        }

        return false;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGen.InWorld(i, j, 5)) {
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }

        Tile left = Main.tile[i - 1, j];
        Tile right = Main.tile[i + 1, j];
        Tile tile = Main.tile[i, j];
        bool[] merge = Main.tileMerge[Type];
        bool mergeLeft = false;
        bool mergeRight = false;

        if (left.HasTile) {
            mergeLeft = merge[left.TileType];
        }
        if (right.HasTile) {
            mergeRight = merge[right.TileType];
        }

        if (mergeLeft && mergeRight) {
            tile.TileFrameX = 18;
        }
        else if (mergeLeft) {
            tile.TileFrameX = 36;
        }
        else if (mergeRight) {
            tile.TileFrameX = 0;
        }
        else {
            tile.TileFrameX = 54;
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 8;
    }

    public override bool Slope(int i, int j) {
        return false;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;

        Main.tileMerge[Type][Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        AddMapEntry(new Microsoft.Xna.Framework.Color(191, 142, 111));

        DustType = ModContent.DustType<Dusts.Backwoods.Furniture>();
        AdjTiles = [TileID.PlanterBox];
    }

    void IPostSetupContent.PostSetupContent() {
        AutomaticallyMakeHerbsAnchorToAequusPlanterBoxes();
    }

    // aequus
    private void AutomaticallyMakeHerbsAnchorToAequusPlanterBoxes() {
        for (int i = 0; i < TileLoader.TileCount; i++) {
            TileObjectData objData = TileObjectData.GetTileData(i, 0);
            if (objData == null || objData.AnchorAlternateTiles == null || objData.AnchorAlternateTiles.Length == 0) {
                continue;
            }

            // Check if this tile anchors to Planter Boxes
            if (objData.AnchorAlternateTiles.Any(tileId => tileId == TileID.PlanterBox || tileId == TileID.ClayPot)) {
                lock (objData) {
                    // If so, add Aequus' planter box automatically.
                    int[] anchorAlternates = objData.AnchorAlternateTiles;
                    Array.Resize(ref anchorAlternates, anchorAlternates.Length + 1);
                    anchorAlternates[^1] = ModContent.TileType<PlanterBoxes>();
                    objData.AnchorAlternateTiles = anchorAlternates;
                }
            }
        }

        for (int i = TileID.Count; i < TileLoader.TileCount; i++) {
            ModTile modTile = TileLoader.GetTile(i);

            if (modTile == null || modTile.AdjTiles == null || modTile.AdjTiles.Length == 0 || !Main.tileTable[i] || !Main.tileSolidTop[i]) {
                continue;
            }

            // Other mods which populate their AdjTile array with TileID.PlanterBox will be automatically merged with
            // AdjTiles is a very convenient way to detect if a tile is a counterpart of a vanilla type,
            // even if planter boxes are not used for crafting.
            if (modTile.AdjTiles.Any(tileId => tileId == TileID.PlanterBox || tileId == TileID.ClayPot)) {
                Main.tileMerge[ModContent.TileType<PlanterBoxes>()][i] = true;
            }
        }
    }

    private bool On_Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(On_Player.orig_PlaceThing_Tiles_BlockPlacementForAssortedThings orig, Player self, bool canPlace) {
        TileObjectData objData = TileObjectData.GetTileData(self.GetSelectedItem().createTile, 0);
        bool flag = self.GetSelectedItem().type == 213 || self.GetSelectedItem().type == 5295;
        if (flag) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 0 || Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 1 || Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 38)
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 2 || self.GetSelectedItem().createTile == 109) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasUnactuatedTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 0)
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 23 || self.GetSelectedItem().createTile == 199) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasUnactuatedTile) {
                if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 0)
                    canPlace = true;
                else if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 59)
                    canPlace = true;
            }
        }
        else if (self.GetSelectedItem().createTile == 227) {
            canPlace = true;
        }
        else if (self.GetSelectedItem().createTile >= 373 && self.GetSelectedItem().createTile <= 375) {
            int num = Player.tileTargetX;
            int num2 = Player.tileTargetY - 1;
            if (Main.tile[num, num2].HasUnactuatedTile && Main.tileSolid[Main.tile[num, num2].TileType] && !Main.tileSolidTop[Main.tile[num, num2].TileType])
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 461) {
            int num3 = Player.tileTargetX;
            int num4 = Player.tileTargetY - 1;
            if (Main.tile[num3, num4].HasUnactuatedTile && Main.tileSolid[Main.tile[num3, num4].TileType] && !Main.tileSolidTop[Main.tile[num3, num4].TileType])
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 60 || self.GetSelectedItem().createTile == 70 || self.GetSelectedItem().createTile == 661 || self.GetSelectedItem().createTile == 662) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasUnactuatedTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 59)
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 4 || self.GetSelectedItem().createTile == 136) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].WallType > 0) {
                canPlace = true;
            }
            else {
                if (!WorldGen.SolidTileNoAttach(Player.tileTargetX, Player.tileTargetY + 1) && !WorldGen.SolidTileNoAttach(Player.tileTargetX - 1, Player.tileTargetY) && !WorldGen.SolidTileNoAttach(Player.tileTargetX + 1, Player.tileTargetY)) {
                    if (!WorldGen.SolidTileNoAttach(Player.tileTargetX, Player.tileTargetY + 1) && (Main.tile[Player.tileTargetX, Player.tileTargetY + 1].IsHalfBlock || Main.tile[Player.tileTargetX, Player.tileTargetY + 1].Slope != 0)) {
                        if (!TileID.Sets.Platforms[Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType]) {
                            WorldGen.SlopeTile(Player.tileTargetX, Player.tileTargetY + 1);
                            if (Main.netMode == 1)
                                NetMessage.SendData(17, -1, -1, null, 14, Player.tileTargetX, Player.tileTargetY + 1);
                        }
                    }
                    else if (!WorldGen.SolidTileNoAttach(Player.tileTargetX, Player.tileTargetY + 1) && !WorldGen.SolidTileNoAttach(Player.tileTargetX - 1, Player.tileTargetY) && (Main.tile[Player.tileTargetX - 1, Player.tileTargetY].IsHalfBlock || Main.tile[Player.tileTargetX - 1, Player.tileTargetY].Slope != 0)) {
                        if (!TileID.Sets.Platforms[Main.tile[Player.tileTargetX - 1, Player.tileTargetY].TileType]) {
                            WorldGen.SlopeTile(Player.tileTargetX - 1, Player.tileTargetY);
                            if (Main.netMode == 1)
                                NetMessage.SendData(17, -1, -1, null, 14, Player.tileTargetX - 1, Player.tileTargetY);
                        }
                    }
                    else if (!WorldGen.SolidTileNoAttach(Player.tileTargetX, Player.tileTargetY + 1) && !WorldGen.SolidTileNoAttach(Player.tileTargetX + 1, Player.tileTargetY) && (Main.tile[Player.tileTargetX + 1, Player.tileTargetY].IsHalfBlock || Main.tile[Player.tileTargetX + 1, Player.tileTargetY].Slope != 0) && !TileID.Sets.Platforms[Main.tile[Player.tileTargetX + 1, Player.tileTargetY].TileType]) {
                        WorldGen.SlopeTile(Player.tileTargetX + 1, Player.tileTargetY);
                        if (Main.netMode == 1)
                            NetMessage.SendData(17, -1, -1, null, 14, Player.tileTargetX + 1, Player.tileTargetY);
                    }
                }

                int num5 = Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType;
                if (Main.tile[Player.tileTargetX, Player.tileTargetY].IsHalfBlock)
                    num5 = -1;

                int num6 = Main.tile[Player.tileTargetX - 1, Player.tileTargetY].TileType;
                int num7 = Main.tile[Player.tileTargetX + 1, Player.tileTargetY].TileType;
                int tree = Main.tile[Player.tileTargetX - 1, Player.tileTargetY - 1].TileType;
                int tree2 = Main.tile[Player.tileTargetX + 1, Player.tileTargetY - 1].TileType;
                int tree3 = Main.tile[Player.tileTargetX - 1, Player.tileTargetY - 1].TileType;
                int tree4 = Main.tile[Player.tileTargetX + 1, Player.tileTargetY + 1].TileType;
                if (!Main.tile[Player.tileTargetX, Player.tileTargetY + 1].HasUnactuatedTile)
                    num5 = -1;

                if (!Main.tile[Player.tileTargetX - 1, Player.tileTargetY].HasUnactuatedTile)
                    num6 = -1;

                if (!Main.tile[Player.tileTargetX + 1, Player.tileTargetY].HasUnactuatedTile)
                    num7 = -1;

                if (!Main.tile[Player.tileTargetX - 1, Player.tileTargetY - 1].HasUnactuatedTile)
                    tree = -1;

                if (!Main.tile[Player.tileTargetX + 1, Player.tileTargetY - 1].HasUnactuatedTile)
                    tree2 = -1;

                if (!Main.tile[Player.tileTargetX - 1, Player.tileTargetY + 1].HasUnactuatedTile)
                    tree3 = -1;

                if (!Main.tile[Player.tileTargetX + 1, Player.tileTargetY + 1].HasUnactuatedTile)
                    tree4 = -1;

                if (num5 >= 0 && Main.tileSolid[num5] && (!Main.tileNoAttach[num5] || (num5 >= 0 && TileID.Sets.Platforms[num5])))
                    canPlace = true;
                else if ((num6 >= 0 && Main.tileSolid[num6] && !Main.tileNoAttach[num6]) || (WorldGen.IsTreeType(num6) && WorldGen.IsTreeType(tree) && WorldGen.IsTreeType(tree3)) || (num6 >= 0 && TileID.Sets.IsBeam[num6]))
                    canPlace = true;
                else if ((num7 >= 0 && Main.tileSolid[num7] && !Main.tileNoAttach[num7]) || (WorldGen.IsTreeType(num7) && WorldGen.IsTreeType(tree2) && WorldGen.IsTreeType(tree4)) || (num7 >= 0 && TileID.Sets.IsBeam[num7]))
                    canPlace = true;
            }
        }
        else if (self.GetSelectedItem().createTile == 78 || self.GetSelectedItem().createTile == 98 || self.GetSelectedItem().createTile == 100 || self.GetSelectedItem().createTile == 173 || self.GetSelectedItem().createTile == 174 || self.GetSelectedItem().createTile == 324) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY + 1].HasUnactuatedTile && (Main.tileSolid[Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType] || Main.tileTable[Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType]))
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 419) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY + 1].HasTile && (Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType == 419 || (self.GetSelectedItem().placeStyle != 2 && Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType == 420)))
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 13 || self.GetSelectedItem().createTile == 29 || self.GetSelectedItem().createTile == 33 || self.GetSelectedItem().createTile == 49 || self.GetSelectedItem().createTile == 50 || self.GetSelectedItem().createTile == 103) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY + 1].HasUnactuatedTile && Main.tileTable[Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType])
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 275 || self.GetSelectedItem().createTile == 276 || self.GetSelectedItem().createTile == 277) {
            canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 51 || self.GetSelectedItem().createTile == 330 || self.GetSelectedItem().createTile == 331 || self.GetSelectedItem().createTile == 332 || self.GetSelectedItem().createTile == 333 || self.GetSelectedItem().createTile == 336 || self.GetSelectedItem().createTile == 340 || self.GetSelectedItem().createTile == 342 || self.GetSelectedItem().createTile == 341 || self.GetSelectedItem().createTile == 343 || self.GetSelectedItem().createTile == 344 || self.GetSelectedItem().createTile == 379 || self.GetSelectedItem().createTile == 351) {
            if (Main.tile[Player.tileTargetX + 1, Player.tileTargetY].HasTile || Main.tile[Player.tileTargetX + 1, Player.tileTargetY].WallType > 0 || Main.tile[Player.tileTargetX - 1, Player.tileTargetY].HasTile || Main.tile[Player.tileTargetX - 1, Player.tileTargetY].WallType > 0 || Main.tile[Player.tileTargetX, Player.tileTargetY + 1].HasTile || Main.tile[Player.tileTargetX, Player.tileTargetY + 1].WallType > 0 || Main.tile[Player.tileTargetX, Player.tileTargetY - 1].HasTile || Main.tile[Player.tileTargetX, Player.tileTargetY - 1].WallType > 0)
                canPlace = true;
        }
        else if (self.GetSelectedItem().createTile == 314) {
            for (int i = Player.tileTargetX - 1; i <= Player.tileTargetX + 1; i++) {
                for (int j = Player.tileTargetY - 1; j <= Player.tileTargetY + 1; j++) {
                    Tile tile = Main.tile[i, j];
                    if (tile.HasTile || tile.WallType > 0) {
                        canPlace = true;
                        break;
                    }
                }
            }
        }
        else {
            Tile tile2 = Main.tile[Player.tileTargetX - 1, Player.tileTargetY];
            Tile tile3 = Main.tile[Player.tileTargetX + 1, Player.tileTargetY];
            Tile tile4 = Main.tile[Player.tileTargetX, Player.tileTargetY - 1];
            Tile tile5 = Main.tile[Player.tileTargetX, Player.tileTargetY + 1];
            if ((tile3.HasTile && (Main.tileSolid[tile3.TileType] || TileID.Sets.IsBeam[tile3.TileType] || Main.tileRope[tile3.TileType] || tile3.TileType == 314)) || tile3.WallType > 0 || (tile2.HasTile && (Main.tileSolid[tile2.TileType] || TileID.Sets.IsBeam[tile2.TileType] || Main.tileRope[tile2.TileType] || tile2.TileType == 314)) || tile2.WallType > 0 || (tile5.HasTile && (Main.tileSolid[tile5.TileType] || TileID.Sets.IsBeam[tile5.TileType] || Main.tileRope[tile5.TileType] || tile5.TileType == 314)) || tile5.WallType > 0 || (tile4.HasTile && (Main.tileSolid[tile4.TileType] || TileID.Sets.IsBeam[tile4.TileType] || Main.tileRope[tile4.TileType] || tile4.TileType == 314)) || tile4.WallType > 0)
                canPlace = true;
            else if (Main.tile[Player.tileTargetX, Player.tileTargetY].WallType > 0)
                canPlace = true;
        }

        if (flag && Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile) {
            int num8 = Player.tileTargetX;
            int num9 = Player.tileTargetY;
            if (Main.tile[num8, num9].TileType == 3 || Main.tile[num8, num9].TileType == 73 || Main.tile[num8, num9].TileType == 84) {
                WorldGen.KillTile(Player.tileTargetX, Player.tileTargetY);
                if (!Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.netMode == 1)
                    NetMessage.SendData(17, -1, -1, null, 0, Player.tileTargetX, Player.tileTargetY);
            }
            else if (Main.tile[num8, num9].TileType == 83) {
                bool flag2 = false;
                int num10 = Main.tile[num8, num9].TileFrameX / 18;
                if (num10 == 0 && Main.dayTime)
                    flag2 = true;

                if (num10 == 1 && !Main.dayTime)
                    flag2 = true;

                if (num10 == 3 && !Main.dayTime && (Main.bloodMoon || Main.moonPhase == 0))
                    flag2 = true;

                if (num10 == 4 && (Main.raining || Main.cloudAlpha > 0f))
                    flag2 = true;

                if (num10 == 5 && !Main.raining && Main.dayTime && Main.time > 40500.0)
                    flag2 = true;

                if (flag2) {
                    WorldGen.KillTile(Player.tileTargetX, Player.tileTargetY);
                    NetMessage.SendData(17, -1, -1, null, 0, Player.tileTargetX, Player.tileTargetY);
                }
            }
        }

        if (Main.tileAlch[self.GetSelectedItem().createTile])
            canPlace = true;

        if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && (Main.tileCut[Main.tile[Player.tileTargetX, Player.tileTargetY].TileType] || TileID.Sets.BreakableWhenPlacing[Main.tile[Player.tileTargetX, Player.tileTargetY].TileType] || (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType >= 373 && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType <= 375) || Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 461)) {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType != self.GetSelectedItem().createTile) {
                bool num11 = Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType != ModContent.TileType<PlanterBoxes>() && Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType != 78 && Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType != 380 && Main.tile[Player.tileTargetX, Player.tileTargetY + 1].TileType != 579;
                bool flag3 = Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 3 || Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == 73;
                bool flag4 = Main.tileAlch[Main.tile[Player.tileTargetX, Player.tileTargetY].TileType] && WorldGen.IsHarvestableHerbWithSeed(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType, Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameX / 18);
                bool flag5 = Main.tileAlch[self.GetSelectedItem().createTile];
                if (num11 || ((flag3 || flag4) && flag5)) {
                    WorldGen.KillTile(Player.tileTargetX, Player.tileTargetY);
                    if (!Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.netMode == 1)
                        NetMessage.SendData(17, -1, -1, null, 0, Player.tileTargetX, Player.tileTargetY);
                }
                else {
                    canPlace = false;
                }
            }
            else {
                canPlace = false;
            }
        }

        if (!canPlace && self.GetSelectedItem().createTile >= 0 && TileID.Sets.Platforms[self.GetSelectedItem().createTile]) {
            for (int k = Player.tileTargetX - 1; k <= Player.tileTargetX + 1; k++) {
                for (int l = Player.tileTargetY - 1; l <= Player.tileTargetY + 1; l++) {
                    if (Main.tile[k, l].HasTile) {
                        canPlace = true;
                        break;
                    }
                }
            }
        }

        if (self.GetSelectedItem().createTile == 3) {
            canPlace = WorldGen.IsFitToPlaceFlowerIn(Player.tileTargetX, Player.tileTargetY, 3);
            if (canPlace) {
                WorldGen.KillTile(Player.tileTargetX, Player.tileTargetY);
                if (Main.netMode == 1 && !Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile)
                    NetMessage.SendData(17, -1, -1, null, 0, Player.tileTargetX, Player.tileTargetY);
            }
        }

        return canPlace;
    }
}