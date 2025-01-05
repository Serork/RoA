using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

//sealed class BeaconTE : ModTileEntity {
//    public static readonly short[] Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];

//    public enum BeaconVariant : byte {
//        None,
//        Amethyst,
//        Topaz,
//        Sapphire,
//        Emerald,
//        Ruby,
//        Diamond,
//        Amber,
//        Length
//    }

//    private BeaconVariant _variant = BeaconVariant.None;

//    public bool HasGemInIt => GetItemID(num3: 0) != -1;

//    public void RemoveGem() => _variant = BeaconVariant.None;

//    public void DropGem(Player player, int x, int y) {
//        if (!HasGemInIt) {
//            return;
//        }

//        int num = Item.NewItem(player.GetSource_Misc("frombeacon"), x, y, 18, 18, GetItemID());
//        if (Main.netMode == NetmodeID.MultiplayerClient) {
//            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num);
//        }
//    }

//    public void InsertGem(short gemType) {
//        for (int i = 0; i < Gems.Length; i++) {
//            if (Gems[i] == gemType) {
//                _variant = (BeaconVariant)(i + 1);
//            }
//        }
//    }

//    public byte GetVariant(bool forDrawing = true, int num = 0) {
//        bool flag = forDrawing && _variant == BeaconVariant.None;
//        return Math.Max((byte)((byte)(flag ? BeaconVariant.Length : _variant) + (flag ? -1 : 0) + num), (byte)0);
//    }

//    public int GetItemID(bool forDrawing = true, int num2 = -1, int num3 = -1) {
//        int num = num2 != -1 ? num2 : GetVariant(forDrawing, num3);
//        if (_variant != BeaconVariant.None && num == 7) {
//            num = 6;
//        }
//        return num switch {
//            >= 0 and <= 6 => Gems[num],
//            _ => -1,
//        };
//    }

//    public override void SaveData(TagCompound tag) {
//        tag[nameof(BeaconTE)] = (byte)_variant;
//    }

//    public override void LoadData(TagCompound tag) {
//        _variant = (BeaconVariant)tag.Get<byte>(nameof(BeaconTE));
//    }

//    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
//        if (Main.netMode == NetmodeID.MultiplayerClient) {
//            NetMessage.SendTileSquare(Main.myPlayer, i, j);
//            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

//            return -1;
//        }

//        return Place(i, j);
//    }

//    public override void OnKill() => DropGem(Main.LocalPlayer, (int)(Position.X * 16f), (int)((Position.Y - 2) * 16f + 8f));

//    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

//    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<Beacon>());
//}

sealed class Beacon : ModTile {
    private static int _variantToShow;

    public override void SetStaticDefaults() {
        Main.tileTable[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.newTile.DrawYOffset = 2;
        //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<BeaconTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
        height = 2;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    //public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<BeaconTE>().Place(i, j);

    //public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<BeaconTE>().Kill(i, j);

    //private static BeaconTE GetTE(int i, int j) {
    //    BeaconTE foundTE = null;
    //    int j2 = 0;
    //    TileObjectData tileObjectData = TileObjectData.GetTileData(WorldGenHelper.GetTileSafely(i, j));
    //    if (tileObjectData == null) {
    //        return null;
    //    }
    //    while (j2 < tileObjectData.CoordinateHeights.Length) {
    //        BeaconTE desiredTE = TileHelper.GetTE<BeaconTE>(i, j + j2);
    //        if (desiredTE is null) {
    //            j2++;
    //            continue;
    //        }

    //        foundTE = desiredTE;
    //        break;
    //    }

    //    return foundTE;
    //}

    private bool IsTileValidToBeHovered(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type) {
            return false;
        }

        return true;
    }

    //public static int VariantToShow => _variantToShow;

    //public static void UpdateVariants() {
    //    double time = 50.0;
    //    if (Main.timeForVisualEffects % time == 0.0) {
    //        _variantToShow++;
    //        if (_variantToShow >= Enum.GetNames(typeof(BeaconTE.BeaconVariant)).Length - 2) {
    //            _variantToShow = 0;
    //        }
    //    }
    //}

    //public static int GetGemItemID(int i, int j) {
    //    BeaconTE te = GetTE(i, j);
    //    if (te is null) {
    //        return -1;
    //    }

    //    bool flag2 = te.HasGemInIt;
    //    bool flag = !flag2 && ModContent.GetInstance<BeaconInterface>().Active;
    //    if (flag) {
    //        UpdateVariants();
    //        return te.GetItemID(false, VariantToShow);
    //    }
    //    else {
    //        return flag2 ? te.GetItemID() : ModContent.ItemType<Items.Placeable.Crafting.Beacon>();
    //    }
    //}

    //public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
    //    BeaconTE te = GetTE(i, j);
    //    if (te is null) {
    //        return;
    //    }

    //    tileFrameY += (short)(54 * te.GetVariant(false));
    //}

    public override void MouseOver(int i, int j) {
        //BeaconTE te = GetTE(i, j);
        //if (te is null) {
        //    return;
        //}

        //Player player = Main.LocalPlayer;
        //player.cursorItemIconID = !BeaconInterface.HasOpened(te) ? ModContent.ItemType<Items.Placeable.Crafting.Beacon>() : GetGemItemID(i, j);
        //if (player.cursorItemIconID != -1) {
        //    player.noThrow = 2;
        //    player.cursorItemIconEnabled = true;
        //}
    }

    public override bool RightClick(int i, int j) {
        //BeaconTE te = GetTE(i, j);
        //if (te is null) {
        //    return false;
        //}

        if (IsTileValidToBeHovered(i, j)) {
            Player player = Main.LocalPlayer;
            short[] gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];
            Item item = player.GetSelectedItem();
            if (gems.Contains((short)item.type)) {
                int variant = 0;
                for (int k = 0; k < gems.Length; k++) {
                    if (gems[k] == (short)item.type) {
                        variant = k + 1;
                    }
                }
                int num3 = 0;
                bool flag2 = WorldGenHelper.GetTileSafely(i, j).TileFrameY < 54;
                bool flag = false;
                for (int l = j - 2; l < j + 2; l++) {
                    Tile tile2 = WorldGenHelper.GetTileSafely(i, l);
                    if (tile2.ActiveTile(Type)) {
                        short getTileFrameY(int usedVariant) {
                            return (short)(num3 * 18 + 54 * usedVariant);
                        }
                        void setFrame(int usedVariant) {
                            tile2.TileFrameY = getTileFrameY(usedVariant);
                            WorldGen.SquareTileFrame(i, l);
                            NetMessage.SendTileSquare(-1, i, l, 1, 1);
                            num3++;
                        }
                        if (flag || (tile2.TileFrameY >= getTileFrameY(variant) &&
                            tile2.TileFrameY < getTileFrameY(variant + 1))) {
                            flag = true;
                            setFrame(0);
                        }
                        if (!flag) {
                            setFrame(variant);
                        }
                        int gemType = gems[variant - 1];
                        bool flag3 = !WorldGenHelper.GetTileSafely(i, l - 1).ActiveTile(Type);
                        if (flag3) {
                            if (!flag2) {
                                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32,
                                    gemType);
                            }
                            if (flag2) {
                                player.ConsumeItem(gemType);
                            }
                            SoundEngine.PlaySound(SoundID.MenuTick, new Point(i, l).ToWorldCoordinates());
                        }
                    }
                }
            }
        }

        //while (WorldGenHelper.ActiveTile(i, j, Type)) {
        //    j--;
        //}
        //BeaconInterface.ToggleUI(i, j + 1, te);

        return true;
    }
}