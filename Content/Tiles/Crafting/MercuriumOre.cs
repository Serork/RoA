using Microsoft.Xna.Framework;

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

		AddMapEntry(new Color(188, 143, 143), CreateMapEntryName());

        //DustType = ModContent.DustType<ToxicFumes>();
        //ItemDrop = ModContent.ItemType<Items.Placeable.Crafting.MercuriumOre>();

        HitSound = SoundID.Tink;

        MineResist = 2f;
		MinPick = 55;
	}

	public override bool HasWalkDust() => true;	
}