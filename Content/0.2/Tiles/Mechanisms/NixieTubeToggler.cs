using RoA.Content.Items.Placeable.Mechanisms;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Mechanisms;

sealed class NixieTubeToggler : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        int tileFrame = Main.tile[i, j].TileFrameX / 18;
        int cursorItemIconID = ModContent.ItemType<Toggler1>();
        switch (tileFrame) {
            case 1:
                cursorItemIconID = ModContent.ItemType<Toggler2>();
                break;
            case 2:
                cursorItemIconID = ModContent.ItemType<Toggler3>();
                break;
            case 3:
                cursorItemIconID = ModContent.ItemType<Toggler4>();
                break;
            case 4:
                cursorItemIconID = ModContent.ItemType<Toggler5>();
                break;
            case 5:
                cursorItemIconID = ModContent.ItemType<Toggler6>();
                break;
            case 6:
                cursorItemIconID = ModContent.ItemType<Toggler7>();
                break;
            case 7:
                cursorItemIconID = ModContent.ItemType<Toggler8>();
                break;
            case 8:
                cursorItemIconID = ModContent.ItemType<Toggler9>();
                break;
            case 9:
                cursorItemIconID = ModContent.ItemType<Toggler10>();
                break;
        }
        player.cursorItemIconID = cursorItemIconID;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 1;
    }
}
