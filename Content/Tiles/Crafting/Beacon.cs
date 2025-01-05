using RoA.Content.Tiles.Plants;
using RoA.Core.Utility;

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
        Amber
    }

    private BeaconVariant _variant = BeaconVariant.Topaz;

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

    public static void PlaceBeacon(int i, int j) {

    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        BeaconTE foundTE = null;
        for (int j2 = 0; j2 < 2; j2++) {
            BeaconTE desiredTE = TileHelper.GetTE<BeaconTE>(i, j2);
            if (desiredTE != null) {
                foundTE = desiredTE;
                break;
            }
        }
        if (foundTE != null) {
            BeaconTE.BeaconVariant variant = foundTE.Variant;
            tileFrameY *= (byte)variant;
        }
    }
}