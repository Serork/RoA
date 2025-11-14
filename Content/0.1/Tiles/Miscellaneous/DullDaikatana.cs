using Microsoft.Xna.Framework;

using RoA.Content.Items.Placeable.Solid;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class DullDaikatana : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoAttach[Type] = true;

        Main.tileSpelunker[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 2000;

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        Main.tileOreFinderPriority[Type] = 550;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = AnchorData.Empty;
        TileObjectData.newTile.AnchorWall = true;

        AddMapEntry(new Color(137, 151, 164), CreateMapEntryName());

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Items.Materials.DullDaikatana>(), Type, 0);

        TileObjectData.addTile(Type);

        MinPick = 65;
    }

    public override bool CanExplode(int i, int j) => Main.hardMode;

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        int itemType = ModContent.ItemType<Items.Materials.DullDaikatana>();
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType);
        itemType = ModContent.ItemType<Items.Equipables.Vanity.StrangerCoat>();
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType);
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool(8)) {
            type = ModContent.DustType<Dusts.DullDaikatana3>();
        }
        else if (Main.rand.NextBool(8)) {
            type = ModContent.DustType<Dusts.DullDaikatana2>();
        }
        else {
            type = ModContent.DustType<Dusts.DullDaikatana1>();
        }

        return base.CreateDust(i, j, ref type);
    }

    //public override void KillMultiTile(int i, int j, int frameX, int frameY) {
    //	if (frameX == 0) Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<Items.Equipables.Armor.Miscellaneous.OniMask>(), 1, false, 0, false, false);
    //}
}