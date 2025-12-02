using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Items.Placeable;
using RoA.Content.Items.Placeable.Wiring;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

public class TrappedChests : ModTile {
    public override void Load() {
        On_TileDrawing.ShouldTileShine += On_TileDrawing_ShouldTileShine;
    }

    private bool On_TileDrawing_ShouldTileShine(On_TileDrawing.orig_ShouldTileShine orig, ushort type, short frameX) {
        if (type == ModContent.TileType<TrappedChests>()) {
            bool result = frameX >= 36;
            if (result) {
                Main.tileShine[Type] = 1200;
            }
            else {
                Main.tileShine[Type] = 0;
            }
            return result;
        }

        return orig(type, frameX);
    }

    public override void SetStaticDefaults() {
        Main.tileSpelunker[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.BasicChestFake[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [
            16,
            18
        ];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        Color mapColor = new(174, 129, 92);
        AddMapEntry(mapColor, Language.GetText("Mods.RoA.Tiles.BackwoodsDungeonChest2.MapEntry"));
        mapColor = new(110, 91, 74);
        AddMapEntry(mapColor, Language.GetText("Mods.RoA.Tiles.ElderwoodChest.MapEntry"));
        mapColor = new(133, 111, 91);
        AddMapEntry(mapColor, Language.GetText("Mods.RoA.Tiles.ElderwoodChest2.MapEntry"));
        mapColor = new(95, 98, 113);
        AddMapEntry(mapColor, Language.GetText("Mods.RoA.Tiles.BackwoodsStoneChest.MapEntry"));

        DustType = -1;

        TileID.Sets.DisableSmartCursor[Type] = true;

        AdjTiles = new int[1] { TileID.FakeContainers };
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override ushort GetMapOption(int i, int j) => (ushort)((uint)Main.tile[i, j].TileFrameX / 36U);

    public override bool CanDrop(int i, int j) {
        Tile tile = Main.tile[i, j];
        int num1 = i;
        int num2 = j;
        if (tile.TileFrameX % 36 != 0)
            --num1;
        if (tile.TileFrameY != 0)
            --num2;
        int chestItem = GetChestItem(tile.TileFrameX / 36);
        if (chestItem > 0)
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), num1 * 16, num2 * 16, 32, 34, chestItem);
        return false;
    }

    public override void MouseOver(int i, int j) {
        Player localPlayer = Main.LocalPlayer;
        localPlayer.cursorItemIconID = GetChestItem(Main.tile[i, j].TileFrameX / 36);
        localPlayer.noThrow = 2;
        localPlayer.cursorItemIconEnabled = true;
    }

    private int GetChestItem(int style) {
        switch (style) {
            case 0:
                return ModContent.ItemType<BackwoodsDungeonChest_Trapped>();
            case 1:
                return ModContent.ItemType<ElderwoodChest_Trapped>();
            case 2:
                return ModContent.ItemType<ElderwoodChest2_Trapped>();
            case 3:
                return ModContent.ItemType<BackwoodsStoneChest_Trapped>();
            default:
                return -1;
        }
    }

    public override void AnimateIndividualTile(
      int type,
      int i,
      int j,
      ref int frameXOffset,
      ref int frameYOffset) {
        Tile tile = Main.tile[i, j];
        int x = i;
        int y = j;
        if (tile.TileFrameX % 36 != 0)
            --x;
        if (tile.TileFrameY != 0)
            --y;
        int frameData;
        if (!Animation.GetTemporaryFrame(x, y, out frameData))
            return;
        frameYOffset = 38 * frameData;
    }

    public override bool RightClick(int i, int j) {
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        int num1 = i;
        int num2 = j;
        if (tile.TileFrameX % 36 != 0)
            --num1;
        if (tile.TileFrameY != 0)
            --num2;
        Animation.NewTemporaryAnimation(2, tile.TileType, num1, num2);
        NetMessage.SendTemporaryAnimation(-1, 2, tile.TileType, num1, num2);
        Trigger(i, j);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new TrappedChestActionPacket(num1, num2));
        }
        return true;
    }

    public static void Trigger(int i, int j) {
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
            --left;
        if (tile.TileFrameY != 0)
            --top;
        SoundEngine.PlaySound(in SoundID.Mech, new Vector2?(new Vector2(i * 16, j * 16)));
        Wiring.TripWire(left, top, 2, 2);
    }
}
