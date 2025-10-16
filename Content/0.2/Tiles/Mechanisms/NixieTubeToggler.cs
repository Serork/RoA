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
        int num27 = Main.tile[i, j].TileFrameX / 18;
        if (num27 < 3)
            cursorItemIconID = 583 + num27;
        else
            cursorItemIconID = 4484 + (num27 - 3);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 1;
    }
}
