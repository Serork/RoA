using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;

using ReLogic.Content;

using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Content.Gores;

namespace RoA.Content.Tiles.Trees;

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