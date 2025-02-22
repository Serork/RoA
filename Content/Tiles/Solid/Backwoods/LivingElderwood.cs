using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class LivingElderwood3 : LivingElderwood {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
}

sealed class LivingElderwood2 : LivingElderwood {
    public override string Texture => base.Texture[..^1];
}

class LivingElderwood : ModTile {
    public const int MINTILEREQUIRED = 65;

    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGrass>());
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsStone>());
        TileHelper.MergeWith(Type, TileID.Dirt);
        TileHelper.MergeWith(Type, TileID.Grass);
        TileHelper.MergeWith(Type, TileID.JungleGrass);
        TileHelper.MergeWith(Type, TileID.Mud);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGreenMoss>());

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

        DustType = (ushort)ModContent.DustType<WoodTrash>();
		AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());

        MineResist = 1.5f;
        MinPick = MINTILEREQUIRED;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Crafting.Elderwood>());
    }

    #region Merge with Living Elderwood Leaves (Vanilla adaptation)
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile = Main.tile[i, j];
        Tile tile2 = Main.tile[i, j - 1];
        Tile tile3 = Main.tile[i, j + 1];
        Tile tile4 = Main.tile[i - 1, j];
        Tile tile5 = Main.tile[i + 1, j];
        Tile tile6 = Main.tile[i - 1, j + 1];
        Tile tile7 = Main.tile[i + 1, j + 1];
        Tile tile8 = Main.tile[i - 1, j - 1];
        Tile tile9 = Main.tile[i + 1, j - 1];
        int upLeft = -1;
        int up = -1;
        int upRight = -1;
        int left = -1;
        int right = -1;
        int downLeft = -1;
        int down = -1;
        int downRight = -1;
        if (tile4 != null && tile4.HasTile) {
            left = (Main.tileStone[tile4.TileType] ? 1 : tile4.TileType);
            if (tile4.Slope == (SlopeType)1 || tile4.Slope == (SlopeType)3)
                left = -1;
        }

        if (tile5 != null && tile5.HasTile) {
            right = (Main.tileStone[tile5.TileType] ? 1 : tile5.TileType);
            if (tile5.Slope == (SlopeType)2 || tile5.Slope == (SlopeType)4)
                right = -1;
        }

        if (tile2 != null && tile2.HasTile) {
            up = (Main.tileStone[tile2.TileType] ? 1 : tile2.TileType);
            if (tile2.Slope == (SlopeType)3 || tile2.Slope == (SlopeType)4)
                up = -1;
        }

        if (tile3 != null && tile3.HasTile) {
            down = (Main.tileStone[tile3.TileType] ? 1 : tile3.TileType);
            if (tile3.Slope == (SlopeType)1 || tile3.Slope == (SlopeType)2)
                down = -1;
        }

        if (tile8 != null && tile8.HasTile)
            upLeft = (Main.tileStone[tile8.TileType] ? 1 : tile8.TileType);

        if (tile9 != null && tile9.HasTile)
            upRight = (Main.tileStone[tile9.TileType] ? 1 : tile9.TileType);

        if (tile6 != null && tile6.HasTile)
            downLeft = (Main.tileStone[tile6.TileType] ? 1 : tile6.TileType);

        if (tile7 != null && tile7.HasTile)
            downRight = (Main.tileStone[tile7.TileType] ? 1 : tile7.TileType);

        if (tile.Slope == (SlopeType)2) {
            up = -1;
            left = -1;
        }

        if (tile.Slope == (SlopeType)1) {
            up = -1;
            right = -1;
        }

        if (tile.Slope == (SlopeType)4) {
            down = -1;
            left = -1;
        }

        if (tile.Slope == (SlopeType)3) {
            down = -1;
            right = -1;
        }
        int num = Type;
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<LivingElderwoodlLeaves>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        int num27 = tile.TileFrameNumber;
        bool mergeDown = false, mergeUp = false, mergeRight = false, mergeLeft = false;
        Rectangle rectangle = new Rectangle(-1, -1, 0, 0);
        WorldGen.TileMergeAttempt(num, Main.tileMerge[num], ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        if (up == -2 || down == -2 || left == -2 || right == -2 || upLeft == -2 || upRight == -2 || downLeft == -2 || downRight == -2) {
            if (up > -1 && up != num)
                up = -1;

            if (down > -1 && down != num)
                down = -1;

            if (left > -1 && left != num)
                left = -1;

            if (right > -1 && right != num)
                right = -1;
            if (up != -1 && down != -1 && left != -1 && right != -1) {
                if (up == -2 && down == num && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 144;
                            rectangle.Y = 108;
                            break;
                        case 1:
                            rectangle.X = 162;
                            rectangle.Y = 108;
                            break;
                        default:
                            rectangle.X = 180;
                            rectangle.Y = 108;
                            break;
                    }

                    mergeUp = true;
                }
                else if (up == num && down == -2 && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 144;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 162;
                            rectangle.Y = 90;
                            break;
                        default:
                            rectangle.X = 180;
                            rectangle.Y = 90;
                            break;
                    }

                    mergeDown = true;
                }
                else if (up == num && down == num && left == -2 && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 162;
                            rectangle.Y = 126;
                            break;
                        case 1:
                            rectangle.X = 162;
                            rectangle.Y = 144;
                            break;
                        default:
                            rectangle.X = 162;
                            rectangle.Y = 162;
                            break;
                    }

                    mergeLeft = true;
                }
                else if (up == num && down == num && left == num && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 144;
                            rectangle.Y = 126;
                            break;
                        case 1:
                            rectangle.X = 144;
                            rectangle.Y = 144;
                            break;
                        default:
                            rectangle.X = 144;
                            rectangle.Y = 162;
                            break;
                    }

                    mergeRight = true;
                }
                else if (up == -2 && down == num && left == -2 && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 36;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 126;
                            break;
                        default:
                            rectangle.X = 36;
                            rectangle.Y = 162;
                            break;
                    }

                    mergeUp = true;
                    mergeLeft = true;
                }
                else if (up == -2 && down == num && left == num && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 54;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 54;
                            rectangle.Y = 126;
                            break;
                        default:
                            rectangle.X = 54;
                            rectangle.Y = 162;
                            break;
                    }

                    mergeUp = true;
                    mergeRight = true;
                }
                else if (up == num && down == -2 && left == -2 && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 36;
                            rectangle.Y = 108;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 144;
                            break;
                        default:
                            rectangle.X = 36;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeDown = true;
                    mergeLeft = true;
                }
                else if (up == num && down == -2 && left == num && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 54;
                            rectangle.Y = 108;
                            break;
                        case 1:
                            rectangle.X = 54;
                            rectangle.Y = 144;
                            break;
                        default:
                            rectangle.X = 54;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeDown = true;
                    mergeRight = true;
                }
                else if (up == num && down == num && left == -2 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 180;
                            rectangle.Y = 126;
                            break;
                        case 1:
                            rectangle.X = 180;
                            rectangle.Y = 144;
                            break;
                        default:
                            rectangle.X = 180;
                            rectangle.Y = 162;
                            break;
                    }

                    mergeLeft = true;
                    mergeRight = true;
                }
                else if (up == -2 && down == -2 && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 144;
                            rectangle.Y = 180;
                            break;
                        case 1:
                            rectangle.X = 162;
                            rectangle.Y = 180;
                            break;
                        default:
                            rectangle.X = 180;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeUp = true;
                    mergeDown = true;
                }
                else if (up == -2 && down == num && left == -2 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 198;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 198;
                            rectangle.Y = 108;
                            break;
                        default:
                            rectangle.X = 198;
                            rectangle.Y = 126;
                            break;
                    }

                    mergeUp = true;
                    mergeLeft = true;
                    mergeRight = true;
                }
                else if (up == num && down == -2 && left == -2 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 198;
                            rectangle.Y = 144;
                            break;
                        case 1:
                            rectangle.X = 198;
                            rectangle.Y = 162;
                            break;
                        default:
                            rectangle.X = 198;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                }
                else if (up == -2 && down == -2 && left == num && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 216;
                            rectangle.Y = 144;
                            break;
                        case 1:
                            rectangle.X = 216;
                            rectangle.Y = 162;
                            break;
                        default:
                            rectangle.X = 216;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeUp = true;
                    mergeDown = true;
                    mergeRight = true;
                }
                else if (up == -2 && down == -2 && left == -2 && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 216;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 216;
                            rectangle.Y = 108;
                            break;
                        default:
                            rectangle.X = 216;
                            rectangle.Y = 126;
                            break;
                    }

                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                }
                else if (up == -2 && down == -2 && left == -2 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 198;
                            break;
                        case 1:
                            rectangle.X = 126;
                            rectangle.Y = 198;
                            break;
                        default:
                            rectangle.X = 144;
                            rectangle.Y = 198;
                            break;
                    }

                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                }
                else if (up == num && down == num && left == num && right == num) {
                    if (upLeft == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 18;
                                rectangle.Y = 108;
                                break;
                            case 1:
                                rectangle.X = 18;
                                rectangle.Y = 144;
                                break;
                            default:
                                rectangle.X = 18;
                                rectangle.Y = 180;
                                break;
                        }
                    }

                    if (upRight == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 0;
                                rectangle.Y = 108;
                                break;
                            case 1:
                                rectangle.X = 0;
                                rectangle.Y = 144;
                                break;
                            default:
                                rectangle.X = 0;
                                rectangle.Y = 180;
                                break;
                        }
                    }

                    if (downLeft == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 18;
                                rectangle.Y = 90;
                                break;
                            case 1:
                                rectangle.X = 18;
                                rectangle.Y = 126;
                                break;
                            default:
                                rectangle.X = 18;
                                rectangle.Y = 162;
                                break;
                        }
                    }

                    if (downRight == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 0;
                                rectangle.Y = 90;
                                break;
                            case 1:
                                rectangle.X = 0;
                                rectangle.Y = 126;
                                break;
                            default:
                                rectangle.X = 0;
                                rectangle.Y = 162;
                                break;
                        }
                    }
                }
            }
            else {
                if (up == -1 && down == -2 && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 234;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 252;
                            rectangle.Y = 0;
                            break;
                        default:
                            rectangle.X = 270;
                            rectangle.Y = 0;
                            break;
                    }

                    mergeDown = true;
                }
                else if (up == -2 && down == -1 && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 234;
                            rectangle.Y = 18;
                            break;
                        case 1:
                            rectangle.X = 252;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 270;
                            rectangle.Y = 18;
                            break;
                    }

                    mergeUp = true;
                }
                else if (up == num && down == num && left == -1 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 234;
                            rectangle.Y = 36;
                            break;
                        case 1:
                            rectangle.X = 252;
                            rectangle.Y = 36;
                            break;
                        default:
                            rectangle.X = 270;
                            rectangle.Y = 36;
                            break;
                    }

                    mergeRight = true;
                }
                else if (up == num && down == num && left == -2 && right == -1) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 234;
                            rectangle.Y = 54;
                            break;
                        case 1:
                            rectangle.X = 252;
                            rectangle.Y = 54;
                            break;
                        default:
                            rectangle.X = 270;
                            rectangle.Y = 54;
                            break;
                    }

                    mergeLeft = true;
                }

                if (up != -1 && down != -1 && left == -1 && right == num) {
                    if (up == -2 && down == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 72;
                                rectangle.Y = 144;
                                break;
                            case 1:
                                rectangle.X = 72;
                                rectangle.Y = 162;
                                break;
                            default:
                                rectangle.X = 72;
                                rectangle.Y = 180;
                                break;
                        }

                        mergeUp = true;
                    }
                    else if (down == -2 && up == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 72;
                                rectangle.Y = 90;
                                break;
                            case 1:
                                rectangle.X = 72;
                                rectangle.Y = 108;
                                break;
                            default:
                                rectangle.X = 72;
                                rectangle.Y = 126;
                                break;
                        }

                        mergeDown = true;
                    }
                }
                else if (up != -1 && down != -1 && left == num && right == -1) {
                    if (up == -2 && down == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 90;
                                rectangle.Y = 144;
                                break;
                            case 1:
                                rectangle.X = 90;
                                rectangle.Y = 162;
                                break;
                            default:
                                rectangle.X = 90;
                                rectangle.Y = 180;
                                break;
                        }

                        mergeUp = true;
                    }
                    else if (down == -2 && up == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 90;
                                rectangle.Y = 90;
                                break;
                            case 1:
                                rectangle.X = 90;
                                rectangle.Y = 108;
                                break;
                            default:
                                rectangle.X = 90;
                                rectangle.Y = 126;
                                break;
                        }

                        mergeDown = true;
                    }
                }
                else if (up == -1 && down == num && left != -1 && right != -1) {
                    if (left == -2 && right == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 0;
                                rectangle.Y = 198;
                                break;
                            case 1:
                                rectangle.X = 18;
                                rectangle.Y = 198;
                                break;
                            default:
                                rectangle.X = 36;
                                rectangle.Y = 198;
                                break;
                        }

                        mergeLeft = true;
                    }
                    else if (right == -2 && left == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 54;
                                rectangle.Y = 198;
                                break;
                            case 1:
                                rectangle.X = 72;
                                rectangle.Y = 198;
                                break;
                            default:
                                rectangle.X = 90;
                                rectangle.Y = 198;
                                break;
                        }

                        mergeRight = true;
                    }
                }
                else if (up == num && down == -1 && left != -1 && right != -1) {
                    if (left == -2 && right == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 0;
                                rectangle.Y = 216;
                                break;
                            case 1:
                                rectangle.X = 18;
                                rectangle.Y = 216;
                                break;
                            default:
                                rectangle.X = 36;
                                rectangle.Y = 216;
                                break;
                        }

                        mergeLeft = true;
                    }
                    else if (right == -2 && left == num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 54;
                                rectangle.Y = 216;
                                break;
                            case 1:
                                rectangle.X = 72;
                                rectangle.Y = 216;
                                break;
                            default:
                                rectangle.X = 90;
                                rectangle.Y = 216;
                                break;
                        }

                        mergeRight = true;
                    }
                }
                else if (up != -1 && down != -1 && left == -1 && right == -1) {
                    if (up == -2 && down == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 108;
                                rectangle.Y = 216;
                                break;
                            case 1:
                                rectangle.X = 108;
                                rectangle.Y = 234;
                                break;
                            default:
                                rectangle.X = 108;
                                rectangle.Y = 252;
                                break;
                        }

                        mergeUp = true;
                        mergeDown = true;
                    }
                    else if (up == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 126;
                                rectangle.Y = 144;
                                break;
                            case 1:
                                rectangle.X = 126;
                                rectangle.Y = 162;
                                break;
                            default:
                                rectangle.X = 126;
                                rectangle.Y = 180;
                                break;
                        }

                        mergeUp = true;
                    }
                    else if (down == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 126;
                                rectangle.Y = 90;
                                break;
                            case 1:
                                rectangle.X = 126;
                                rectangle.Y = 108;
                                break;
                            default:
                                rectangle.X = 126;
                                rectangle.Y = 126;
                                break;
                        }

                        mergeDown = true;
                    }
                }
                else if (up == -1 && down == -1 && left != -1 && right != -1) {
                    if (left == -2 && right == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 162;
                                rectangle.Y = 198;
                                break;
                            case 1:
                                rectangle.X = 180;
                                rectangle.Y = 198;
                                break;
                            default:
                                rectangle.X = 198;
                                rectangle.Y = 198;
                                break;
                        }

                        mergeLeft = true;
                        mergeRight = true;
                    }
                    else if (left == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 0;
                                rectangle.Y = 252;
                                break;
                            case 1:
                                rectangle.X = 18;
                                rectangle.Y = 252;
                                break;
                            default:
                                rectangle.X = 36;
                                rectangle.Y = 252;
                                break;
                        }

                        mergeLeft = true;
                    }
                    else if (right == -2) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 54;
                                rectangle.Y = 252;
                                break;
                            case 1:
                                rectangle.X = 72;
                                rectangle.Y = 252;
                                break;
                            default:
                                rectangle.X = 90;
                                rectangle.Y = 252;
                                break;
                        }

                        mergeRight = true;
                    }
                }
                else if (up == -2 && down == -1 && left == -1 && right == -1) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 144;
                            break;
                        case 1:
                            rectangle.X = 108;
                            rectangle.Y = 162;
                            break;
                        default:
                            rectangle.X = 108;
                            rectangle.Y = 180;
                            break;
                    }

                    mergeUp = true;
                }
                else if (up == -1 && down == -2 && left == -1 && right == -1) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 90;
                            break;
                        case 1:
                            rectangle.X = 108;
                            rectangle.Y = 108;
                            break;
                        default:
                            rectangle.X = 108;
                            rectangle.Y = 126;
                            break;
                    }

                    mergeDown = true;
                }
                else if (up == -1 && down == -1 && left == -2 && right == -1) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 0;
                            rectangle.Y = 234;
                            break;
                        case 1:
                            rectangle.X = 18;
                            rectangle.Y = 234;
                            break;
                        default:
                            rectangle.X = 36;
                            rectangle.Y = 234;
                            break;
                    }

                    mergeLeft = true;
                }
                else if (up == -1 && down == -1 && left == -1 && right == -2) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 54;
                            rectangle.Y = 234;
                            break;
                        case 1:
                            rectangle.X = 72;
                            rectangle.Y = 234;
                            break;
                        default:
                            rectangle.X = 90;
                            rectangle.Y = 234;
                            break;
                    }

                    mergeRight = true;
                }
            }
            if (rectangle.X < 0 || rectangle.Y < 0) {
                if (up == num && down == num && left == num && right == num) {
                    if (upLeft != num && upRight != num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 108;
                                rectangle.Y = 18;
                                break;
                            case 1:
                                rectangle.X = 126;
                                rectangle.Y = 18;
                                break;
                            default:
                                rectangle.X = 144;
                                rectangle.Y = 18;
                                break;
                        }
                    }
                    else if (downLeft != num && downRight != num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 108;
                                rectangle.Y = 36;
                                break;
                            case 1:
                                rectangle.X = 126;
                                rectangle.Y = 36;
                                break;
                            default:
                                rectangle.X = 144;
                                rectangle.Y = 36;
                                break;
                        }
                    }
                    else if (upLeft != num && downLeft != num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 180;
                                rectangle.Y = 0;
                                break;
                            case 1:
                                rectangle.X = 180;
                                rectangle.Y = 18;
                                break;
                            default:
                                rectangle.X = 180;
                                rectangle.Y = 36;
                                break;
                        }
                    }
                    else if (upRight != num && downRight != num) {
                        switch (num27) {
                            case 0:
                                rectangle.X = 198;
                                rectangle.Y = 0;
                                break;
                            case 1:
                                rectangle.X = 198;
                                rectangle.Y = 18;
                                break;
                            default:
                                rectangle.X = 198;
                                rectangle.Y = 36;
                                break;
                        }
                    }
                    else {
                        switch (num27) {
                            case 0:
                                rectangle.X = 18;
                                rectangle.Y = 18;
                                break;
                            case 1:
                                rectangle.X = 36;
                                rectangle.Y = 18;
                                break;
                            default:
                                rectangle.X = 54;
                                rectangle.Y = 18;
                                break;
                        }
                    }
                }
                else if (up != num && down == num && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 18;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 0;
                            break;
                        default:
                            rectangle.X = 54;
                            rectangle.Y = 0;
                            break;
                    }
                }
                else if (up == num && down != num && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 18;
                            rectangle.Y = 36;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 36;
                            break;
                        default:
                            rectangle.X = 54;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up == num && down == num && left != num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 0;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 0;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 0;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up == num && down == num && left == num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 72;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 72;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 72;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up != num && down == num && left != num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 0;
                            rectangle.Y = 54;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 54;
                            break;
                        default:
                            rectangle.X = 72;
                            rectangle.Y = 54;
                            break;
                    }
                }
                else if (up != num && down == num && left == num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 18;
                            rectangle.Y = 54;
                            break;
                        case 1:
                            rectangle.X = 54;
                            rectangle.Y = 54;
                            break;
                        default:
                            rectangle.X = 90;
                            rectangle.Y = 54;
                            break;
                    }
                }
                else if (up == num && down != num && left != num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 0;
                            rectangle.Y = 72;
                            break;
                        case 1:
                            rectangle.X = 36;
                            rectangle.Y = 72;
                            break;
                        default:
                            rectangle.X = 72;
                            rectangle.Y = 72;
                            break;
                    }
                }
                else if (up == num && down != num && left == num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 18;
                            rectangle.Y = 72;
                            break;
                        case 1:
                            rectangle.X = 54;
                            rectangle.Y = 72;
                            break;
                        default:
                            rectangle.X = 90;
                            rectangle.Y = 72;
                            break;
                    }
                }
                else if (up == num && down == num && left != num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 90;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 90;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 90;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up != num && down != num && left == num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 72;
                            break;
                        case 1:
                            rectangle.X = 126;
                            rectangle.Y = 72;
                            break;
                        default:
                            rectangle.X = 144;
                            rectangle.Y = 72;
                            break;
                    }
                }
                else if (up != num && down == num && left != num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 126;
                            rectangle.Y = 0;
                            break;
                        default:
                            rectangle.X = 144;
                            rectangle.Y = 0;
                            break;
                    }
                }
                else if (up == num && down != num && left != num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 108;
                            rectangle.Y = 54;
                            break;
                        case 1:
                            rectangle.X = 126;
                            rectangle.Y = 54;
                            break;
                        default:
                            rectangle.X = 144;
                            rectangle.Y = 54;
                            break;
                    }
                }
                else if (up != num && down != num && left != num && right == num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 162;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 162;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 162;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up != num && down != num && left == num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 216;
                            rectangle.Y = 0;
                            break;
                        case 1:
                            rectangle.X = 216;
                            rectangle.Y = 18;
                            break;
                        default:
                            rectangle.X = 216;
                            rectangle.Y = 36;
                            break;
                    }
                }
                else if (up != num && down != num && left != num && right != num) {
                    switch (num27) {
                        case 0:
                            rectangle.X = 162;
                            rectangle.Y = 54;
                            break;
                        case 1:
                            rectangle.X = 180;
                            rectangle.Y = 54;
                            break;
                        default:
                            rectangle.X = 198;
                            rectangle.Y = 54;
                            break;
                    }
                }
            }

            if (rectangle.X <= -1 || rectangle.Y <= -1) {
                if (num27 <= 0) {
                    rectangle.X = 18;
                    rectangle.Y = 18;
                }
                else if (num27 == 1) {
                    rectangle.X = 36;
                    rectangle.Y = 18;
                }

                if (num27 >= 2) {
                    rectangle.X = 54;
                    rectangle.Y = 18;
                }
            }
            tile.TileFrameX = (short)rectangle.X;
            tile.TileFrameY = (short)rectangle.Y;
            return false;
        }
        return true;
    }
    #endregion
}