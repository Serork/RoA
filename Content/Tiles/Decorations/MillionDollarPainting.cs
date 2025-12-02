using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class MillionDollarPainting : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSpelunker[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.Width = 6;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
        TileObjectData.newTile.Origin = new Point16(2, 2);
        TileObjectData.addTile(Type);

        AddMapEntry(new Microsoft.Xna.Framework.Color(99, 50, 30), Language.GetText("MapObject.Painting"));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void KillMultiTile(int i, int j, int TileFrameX, int TileFrameY) {
        //int item = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 34, 38, ModContent.ItemType<Items.Placeable.Decorations.MillionDollarPainting>(), 1, false, 0, false, false);
        //if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
        //	NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
    }
}