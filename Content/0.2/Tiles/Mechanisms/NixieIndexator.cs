using RoA.Content.Items.Placeable.Mechanisms;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Mechanisms;

sealed class NixieIndexator : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override bool RightClick(int i, int j) => true;

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        int tileFrame = Main.tile[i, j].TileFrameX / 18;
        int cursorItemIconID;
        switch (tileFrame) {
            default:
                cursorItemIconID = ModContent.ItemType<NixieIndexator1Plus>();
                break;
            case 1:
                cursorItemIconID = ModContent.ItemType<NixieIndexator1Minus>();
                break;
            case 2:
                cursorItemIconID = ModContent.ItemType<NixieIndexator3Plus>();
                break;
            case 3:
                cursorItemIconID = ModContent.ItemType<NixieIndexator3Minus>();
                break;
            case 4:
                cursorItemIconID = ModContent.ItemType<NixieIndexator5Plus>();
                break;
            case 5:
                cursorItemIconID = ModContent.ItemType<NixieIndexator5Minus>();
                break;
            case 6:
                cursorItemIconID = ModContent.ItemType<NixieIndexator10Plus>();
                break;
            case 7:
                cursorItemIconID = ModContent.ItemType<NixieIndexator10Minus>();
                break;
            case 8:
                cursorItemIconID = ModContent.ItemType<NixieResetter>();
                break;
        }
        player.cursorItemIconID = cursorItemIconID;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 1;
    }
}
