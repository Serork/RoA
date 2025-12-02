using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Crafting;

sealed class MercuriumOre : ModTile {
    public override void Load() {
        On_TileDrawing.DrawTiles_EmitParticles += On_TileDrawing_DrawTiles_EmitParticles;
    }

    private void On_TileDrawing_DrawTiles_EmitParticles(On_TileDrawing.orig_DrawTiles_EmitParticles orig, TileDrawing self, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight) {
        if (typeCache == ModContent.TileType<MercuriumOre>()) {
            var _rand = Items.Weapons.Nature.PreHardmode.PineCone.TileDrawing_rand(self);
            if (Main.tileShine[typeCache] > 0) {
                if (tileLight.R <= 20 && tileLight.B <= 20 && tileLight.G <= 20)
                    return;

                int num36 = tileLight.R;
                if (tileLight.G > num36)
                    num36 = tileLight.G;

                if (tileLight.B > num36)
                    num36 = tileLight.B;

                num36 /= 30;
                if (_rand.Next(Main.tileShine[typeCache]) >= num36 || ((typeCache == 21 || typeCache == 441) && (tileFrameX < 36 || tileFrameX >= 180) && (tileFrameX < 396 || tileFrameX > 409)) || ((typeCache == 467 || typeCache == 468) && (tileFrameX < 144 || tileFrameX >= 180)))
                    return;

                Color newColor = Color.White;
                switch (typeCache) {
                    case 178: {
                        switch (tileFrameX / 18) {
                            case 0:
                                newColor = new Color(255, 0, 255, 255);
                                break;
                            case 1:
                                newColor = new Color(255, 255, 0, 255);
                                break;
                            case 2:
                                newColor = new Color(0, 0, 255, 255);
                                break;
                            case 3:
                                newColor = new Color(0, 255, 0, 255);
                                break;
                            case 4:
                                newColor = new Color(255, 0, 0, 255);
                                break;
                            case 5:
                                newColor = new Color(255, 255, 255, 255);
                                break;
                            case 6:
                                newColor = new Color(255, 255, 0, 255);
                                break;
                        }

                        int num37 = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, 43, 0f, 0f, 254, newColor, 0.5f);
                        Main.dust[num37].velocity *= 0f;
                        return;
                    }
                    case 63:
                        newColor = new Color(0, 0, 255, 255);
                        break;
                }

                newColor = Color.Green;

                int num38 = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, 43, 0f, 0f, 254, newColor, 0.5f);
                Main.dust[num38].velocity *= 0f;
            }
            else if (Main.tileSolid[tileCache.TileType] && Main.shimmerAlpha > 0f && (tileLight.R > 20 || tileLight.B > 20 || tileLight.G > 20)) {
                int num39 = tileLight.R;
                if (tileLight.G > num39)
                    num39 = tileLight.G;

                if (tileLight.B > num39)
                    num39 = tileLight.B;

                int maxValue = 500;
                if ((float)_rand.Next(maxValue) < 2f * Main.shimmerAlpha) {
                    Color white = Color.White;
                    float scale2 = ((float)num39 / 255f + 1f) / 2f;
                    int num40 = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, 43, 0f, 0f, 254, white, scale2);
                    Main.dust[num40].velocity *= 0f;
                }
            }
            return;
        }

        orig(self, j, i, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
    }

    public override void SetStaticDefaults() {
        TileID.Sets.Ore[Type] = true;
        TileID.Sets.OreMergesWithMud[Type] = true;

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
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

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
        //for (int i2 = x - tilesMinX; i2 < x + tileMaxX + 1; i2++) {
        //    for (int j2 = y - tilesMinY; j2 < y + tileMaxY + 1; j2++) {
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

    public override bool HasWalkDust() => true;
}