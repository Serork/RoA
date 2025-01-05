using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.UI;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class BeaconTE : ModTileEntity {
    public static readonly short[] Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];

    public enum BeaconVariant : byte {
        None,
        Amethyst,
        Topaz,
        Sapphire,
        Emerald,
        Ruby,
        Diamond,
        Amber,
        Length
    }

    private BeaconVariant _variant = BeaconVariant.None;

    public bool HasGemInIt => GetItemID(num3: 0) != -1;

    public void RemoveGem() => _variant = BeaconVariant.None;

    public void DropGem(Player player, int x, int y) {
        if (!HasGemInIt) {
            return;
        }

        int num = Item.NewItem(player.GetSource_Misc("frombeacon"), x, y, 18, 18, GetItemID());
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num);
        }
    }

    public void InsertGem(short gemType) {
        for (int i = 0; i < Gems.Length; i++) {
            if (Gems[i] == gemType) {
                _variant = (BeaconVariant)(i + 1);
            }
        }
    }

    public byte GetVariant(bool forDrawing = true, int num = 0) {
        bool flag = forDrawing && _variant == BeaconVariant.None;
        return Math.Max((byte)((byte)(flag ? BeaconVariant.Length : _variant) + (flag ? -1 : 0) + num), (byte)0);
    }

    public int GetItemID(bool forDrawing = true, int num2 = -1, int num3 = -1) {
        int num = num2 != -1 ? num2 : GetVariant(forDrawing, num3);
        if (_variant != BeaconVariant.None && num == 7) {
            num = 6;
        }
        return num switch {
            >= 0 and <= 6 => Gems[num],
            _ => -1,
        };
    }

    public override void SaveData(TagCompound tag) {
        tag[nameof(BeaconTE)] = (byte)_variant;
    }

    public override void LoadData(TagCompound tag) {
        _variant = (BeaconVariant)tag.Get<byte>(nameof(BeaconTE));
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        return Place(i, j);
    }

    public override void OnKill() => DropGem(Main.LocalPlayer, (int)(Position.X * 16f), (int)((Position.Y - 2) * 16f + 8f));

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<Beacon>());
}

sealed class Beacon : ModTile {
    private static int _variantToShow;

    public override void SetStaticDefaults() {
        Main.tileTable[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;

        //TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<BeaconTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<BeaconTE>().Place(i, j);

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<BeaconTE>().Kill(i, j);

    private static BeaconTE GetTE(int i, int j) {
        BeaconTE foundTE = null;
        int j2 = 0;
        TileObjectData tileObjectData = TileObjectData.GetTileData(WorldGenHelper.GetTileSafely(i, j));
        if (tileObjectData == null) {
            return null;
        }
        while (j2 < tileObjectData.CoordinateHeights.Length) {
            BeaconTE desiredTE = TileHelper.GetTE<BeaconTE>(i, j + j2);
            if (desiredTE is null) {
                j2++;
                continue;
            }

            foundTE = desiredTE;
            break;
        }

        return foundTE;
    }

    private static bool IsTileValidToBeHovered(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != ModContent.TileType<Beacon>()) {
            return false;
        }

        return true;
    }

    public static int VariantToShow => _variantToShow;

    public static int GetGemItemID(int i, int j) {
        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return -1;
        }

        bool flag2 = te.HasGemInIt;
        bool flag = !flag2 && ModContent.GetInstance<BeaconInterface>().Active;
        if (flag) {
            double time = 50.0;
            if (Main.timeForVisualEffects % time == 0.0) {
                _variantToShow++;
                if (_variantToShow >= Enum.GetNames(typeof(BeaconTE.BeaconVariant)).Length - 2) {
                    _variantToShow = 0;
                }
            }
            return te.GetItemID(false, VariantToShow);
        }
        else {
            return flag2 ? te.GetItemID() : ModContent.ItemType<Items.Placeable.Crafting.Beacon>();
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return;
        }

        tileFrameY += (short)(54 * te.GetVariant(false));
    }

    public override void MouseOver(int i, int j) {
        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return;
        }

        Player player = Main.LocalPlayer;
        player.cursorItemIconID = !BeaconInterface.HasOpened(te) ? ModContent.ItemType<Items.Placeable.Crafting.Beacon>() : GetGemItemID(i, j);
        if (player.cursorItemIconID != -1) {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }
    }

    public override bool RightClick(int i, int j) {
        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return false;
        }

        if (!IsTileValidToBeHovered(i, j)) {
            return false;
        }

        while (WorldGenHelper.ActiveTile(i, j, Type)) {
            j--;
        }
        BeaconInterface.ToggleUI(i, j + 1, te);

        return true;
    }
}