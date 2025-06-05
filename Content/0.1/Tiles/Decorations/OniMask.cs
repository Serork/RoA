using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class OniMask : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoAttach[Type] = true;

        Main.tileSpelunker[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = AnchorData.Empty;
        TileObjectData.newTile.AnchorWall = true;

        AddMapEntry(new Color(161, 59, 50), CreateMapEntryName());

        TileObjectData.addTile(Type);
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool(3)) {
            type = ModContent.DustType<OniMaskDust2>();
        }
        else {
            type = ModContent.DustType<OniMaskDust1>();
        }

        return base.CreateDust(i, j, ref type);
    }

    //public override void KillMultiTile(int i, int j, int frameX, int frameY) {
    //	if (frameX == 0) Item.NewItem(i * 16, j * 16, 32, 48, ModContent.ItemType<Items.Equipables.Armor.Miscellaneous.OniMask>(), 1, false, 0, false, false);
    //}

    public override void NearbyEffects(int i, int j, bool closer) {
        Player player = Main.player[Main.myPlayer];
        if (closer && !player.dead) player.AddBuff(ModContent.BuffType<Buffs.OniMask>(), 60, true);
    }
}