using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Gores;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Trees;

sealed class PrimordialTreeGlow : GlobalTile {
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.ActiveTile(TileID.Trees)) {
            int checkJ = j;
            while (!WorldGenHelper.GetTileSafely(i, checkJ).HasTile || WorldGenHelper.ActiveTile(i, checkJ, TileID.Trees)) {
                checkJ++;
            }
            Tile grassTile = WorldGenHelper.GetTileSafely(i, checkJ);
            if (grassTile.ActiveTile(ModContent.TileType<BackwoodsGrass>())) {
                Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen) {
                    zero = Vector2.Zero;
                }
                bool bluePart = tile.TileFrameX == 88 && tile.TileFrameY == 22;
                Vector2 position = new Vector2(i, j).ToWorldCoordinates();
                if (bluePart) {
                    if (!Main.dedServ) {
                        Lighting.AddLight(position, new Color(95, 110, 255).ToVector3() * 0.5f);
                    }
                }
                else if (Main.rand.NextBool(1000)) {
                    Dust dust = Dust.NewDustPerfect(position + Main.rand.Random2(0, tile.TileFrameX, 0, tile.TileFrameY), ModContent.DustType<TreeDust>());
                    dust.velocity *= 0.75f + Main.rand.NextFloat() * 0.25f;
                    dust.scale *= 1.1f;
                }
                int height = tile.TileFrameY == 36 ? 18 : 16;
                Main.spriteBatch.Draw(ModContent.Request<Texture2D>(PrimordialTree.TexturePath + "_Glow").Value,
                                      new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                                      new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                                      Color.Lerp(Color.White, Lighting.GetColor(i, j), bluePart ? 0.5f : 0.7f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}

sealed class PrimordialTree : ModTree {
    public static string TexturePath => $"{RoA.ModName}/Resources/Textures/Tiles/Trees/{nameof(PrimordialTree)}";

    public override TreePaintingSettings TreeShaderSettings => new() {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<Solid.Backwoods.BackwoodsGrass>()];

    public override int TreeLeaf() => ModContent.GoreType<BackwoodsLeaf>();

    public override int CreateDust() => ModContent.DustType<Dusts.Backwoods.WoodTrash>();

    public override int DropWood() => ModContent.ItemType<Items.Materials.Elderwood>();

    public override int SaplingGrowthType(ref int style) {
        style = 0;
        return ModContent.TileType<PrimordialSapling>();
    }

    public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(TexturePath);

    public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{TexturePath}_Branches");

    public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{TexturePath}_Tops");

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
        topTextureFrameWidth = 114;
        topTextureFrameHeight = 96;
        xoffset = 48;
    }

    public override bool Shake(int x, int y, ref bool createLeaves) {
        //NPC.NewNPCDirect(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ModContent.NPCType<Content.NPCs.Backwoods.BabyFleder>());
        return false;
    }
}