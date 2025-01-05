using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class BeaconTE : ModTileEntity {
    public enum BeaconVariant {
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

    public BeaconVariant Variant => _variant;

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
        while (j2 < TileObjectData.GetTileData(WorldGenHelper.GetTileSafely(i, j)).CoordinateHeights.Length) {
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

    private static int GetGemItemID(int i, int j) {
        bool flag = WorldGenHelper.GetTileSafely(i, j).TileFrameY < 54;
        if (flag) {
            double time = 50.0;
            if (Main.timeForVisualEffects % time == 0.0) {
                _variantToShow++;
                if (_variantToShow >= Enum.GetNames(typeof(BeaconTE.BeaconVariant)).Length - 2) {
                    _variantToShow = 0;
                }
            }
            return _variantToShow switch {
                0 => ItemID.Amethyst,
                1 => ItemID.Topaz,
                2 => ItemID.Sapphire,
                3 => ItemID.Emerald,
                4 => ItemID.Ruby,
                5 => ItemID.Diamond,
                6 => ItemID.Amber,
                _ => -1,
            };
        }
        else {
            return ModContent.ItemType<Items.Placeable.Crafting.Beacon>();
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return;
        }

        BeaconTE.BeaconVariant variant = te.Variant;
        tileFrameY += (short)(54 * (byte)variant);
    }

    public override void MouseOver(int i, int j) {
        if (IsTileValidToBeHovered(i, j)) {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = GetGemItemID(i, j);
        }
    }

    public override bool RightClick(int i, int j) {
        if (!IsTileValidToBeHovered(i, j)) {
            return false;
        }



        return true;
    }
}