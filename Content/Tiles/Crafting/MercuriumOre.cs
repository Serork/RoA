using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Crafting;

sealed class MercuriumOre : ModTile {
	public override void SetStaticDefaults() {
		TileID.Sets.Ore[Type] = true;

		Main.tileSpelunker[Type] = true;
		Main.tileOreFinderPriority[Type] = 410; 

		Main.tileShine2[Type] = true; 
		Main.tileShine[Type] = 1000;

		Main.tileMergeDirt[Type] = true;
		Main.tileSolid[Type] = true;

		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(141, 163, 171), CreateMapEntryName());

        DustType = ModContent.DustType<ToxicFumes>();
        //DustType = ModContent.DustType<ToxicFumes>();
        //ItemDrop = ModContent.ItemType<Items.Placeable.Crafting.MercuriumOre>();

        HitSound = SoundID.Tink;

        MineResist = 2f;
		MinPick = 55;
	}

    public override bool CreateDust(int i, int j, ref int type) {
		if (!Main.rand.NextBool(4)) {
            type = ModContent.DustType<ToxicFumes>();
        }
		else {
			type = ModContent.DustType<Dusts.MercuriumOre>();
        }

        return base.CreateDust(i, j, ref type);
    }

	public override void NumDust(int i, int j, bool fail, ref int num) => num *= !fail ? 2 : 1;

    public override bool HasWalkDust() => true;	
}