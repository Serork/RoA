using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core.Utility;
using RoA.Utilities;

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

        int tilesAway = 7;
        int x = i;
        int y = j;
        //int tilesMinX = 0;
        //while (!WorldGen.SolidTile(i - tilesMinX, j) || WorldGenHelper.GetTileSafely(i - tilesMinX, j).TileType == Type) {
        //    tilesMinX--;
        //    if (tilesMinX <= -tilesAway) {
        //        break;
        //    }
        //}
        //tilesMinX = Math.Abs(tilesMinX);
        //int tileMaxX = 0;
        //while (!WorldGen.SolidTile(i + tileMaxX, j) || WorldGenHelper.GetTileSafely(i + tileMaxX, j).TileType == Type) {
        //    tileMaxX++;
        //    if (tileMaxX >= tilesAway) {
        //        break;
        //    }
        //}
        //int tilesMinY = 0;
        //while (!WorldGen.SolidTile(i, j - tilesMinY) || WorldGenHelper.GetTileSafely(i, j - tilesMinY).TileType == Type) {
        //    tilesMinY--;
        //    if (tilesMinY <= -tilesAway) {
        //        break;
        //    }
        //}
        //tilesMinY = Math.Abs(tilesMinY);
        //int tileMaxY = 0;
        //while (!WorldGen.SolidTile(i, j + tileMaxY) || WorldGenHelper.GetTileSafely(i, j + tileMaxY).TileType == Type) {
        //    tileMaxY++;
        //    if (tileMaxY >= tilesAway) {
        //        break;
        //    }
        //}
        //for (int i2 = x - tilesAway; i2 < x + tilesAway + 1; i2++) {
        //    for (int j2 = y - tilesAway; j2 < y + tilesAway + 1; j2++) {
        for (int i2 = x - tilesAway; i2 < x + tilesAway + 1; i2++) {
            for (int j2 = y - tilesAway; j2 < y + tilesAway + 1; j2++) {
                if (WorldGen.InWorld(i2, j)) {
                    if (WorldGen.SolidTile(i2, j2)) {
                        continue;
                    }
                    if (Main.rand.Next(100) == 0) {
                        Vector2 position = new((i2 - 1) * 16, (j2 - 1) * 16);
                        Vector2 start = new Vector2(x - 1, y - 1) * 16f;
                        Vector2 start2 = Vector2.Lerp(start, position, Main.rand.NextFloat());
                        Vector2 velocity = Helper.VelocityToPoint(start2, position, (position - start2).Length() * 0.4f);
                        start2 += velocity.SafeNormalize(Vector2.Zero) * 32f;
                        velocity = Helper.VelocityToPoint(start2, position, (position - start2).Length() * 0.6f);
                        int dust = Dust.NewDust(start2, 16, 16, ModContent.DustType<Dusts.ToxicFumes>(), 0f, 0f, 0, default, 1.5f);
                        if (WorldGen.SolidTile((int)(start2.X / 16), (int)(start2.Y / 16))) {
                            Main.dust[dust].active = false;
                        }
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = velocity * 0.5f;
                    }
                }
            }
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