using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core.Utility;

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

        TileHelper.MergeWith(Type, [7, 166, 6, 167, 9, 168, 8, 169, 22, 204]);

        HitSound = SoundID.Tink;

        MineResist = 2f;
		MinPick = 55;
	}

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (Main.rand.Next(2) == 0) {
            Vector2 position = new Vector2(i, j).ToWorldCoordinates();
            int dust = Dust.NewDust(position - Vector2.One * 16f, 16, 16, ModContent.DustType<Dusts.ToxicFumes>(), 0f, -4f, 100, new Color(), 1.5f);
            Dust dust2 = Main.dust[dust];
            dust2.scale *= 0.5f;
            dust2 = Main.dust[dust];
            dust2.velocity *= 1.5f;
            dust2 = Main.dust[dust];
            dust2.velocity.Y *= -0.5f;
            dust2.noLight = false;
        }
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