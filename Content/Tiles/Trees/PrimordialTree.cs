using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Gores;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;
using RoA.Utilities;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.Tiles.TileHooks;

namespace RoA.Content.Tiles.Trees;

sealed class PrimordialTreeGlow : GlobalTile {
    public static List<Point> PrimordialTreeDrawPoints { get; private set; } = [];

    public override void Load() {
        On_TileDrawing.DrawTrees += On_TileDrawing_DrawTrees;
        On_Main.ClearCachedTileDraws += On_Main_ClearCachedTileDraws;
    }

    private void On_TileDrawing_DrawTrees(On_TileDrawing.orig_DrawTrees orig, TileDrawing self) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = spriteBatch.CaptureSnapshot();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);
        foreach (Point position in PrimordialTreeDrawPoints.ToList()) {
            int i = position.X, j = position.Y;
            if (!PrimordialTree.IsPrimordialTree(i, j)) {
                PrimordialTreeDrawPoints.Remove(position);
            }
            Tile tile = WorldGenHelper.GetTileSafely(i, j);
            bool bluePart = tile.TileFrameX == 88 && tile.TileFrameY == 22;
            Vector2 drawPosition = new Vector2(i, j).ToWorldCoordinates();
            if (bluePart) {
                if (!Main.dedServ) {
                    Lighting.AddLight(drawPosition, new Color(95, 110, 255).ToVector3() * 0.5f);
                }
            }
            else if (Main.rand.NextBool(1050)) {
                Dust dust = Dust.NewDustPerfect(drawPosition + Main.rand.Random2(0, tile.TileFrameX, 0, tile.TileFrameY), ModContent.DustType<TreeDust>());
                dust.velocity *= 0.5f + Main.rand.NextFloat() * 0.25f;
                dust.scale *= 1.1f;
            }
            if (bluePart || BackwoodsFogHandler.IsFogActive) {
                int height = tile.TileFrameY == 36 ? 18 : 16;
                ulong speed = (((ulong)j << 32) | (ulong)i);
                float posX = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                float posY = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                int directionX = Utils.RandomInt(ref speed, 2) == 0 ? 1 : -1;
                int directionY = Utils.RandomInt(ref speed, 2) != 0 ? 1 : -1;
                Main.spriteBatch.Draw(ModContent.Request<Texture2D>(PrimordialTree.TexturePath + "_Glow").Value,
                                      new Vector2(i * 16 - (int)Main.screenPosition.X - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionX * posX,
                                      j * 16 - (int)Main.screenPosition.Y + 2 - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionY * posY),
                                      new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                                      Color.Lerp(Color.White, Lighting.GetColor(i, j), bluePart ? 0.6f : 0.8f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
        spriteBatch.End();
        spriteBatch.Begin(in snapshot);

        orig(self);
    }

    private static void On_Main_ClearCachedTileDraws(On_Main.orig_ClearCachedTileDraws orig, Main self) {
        orig(self);

        PrimordialTreeDrawPoints.Clear();
    }

    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
        Point position = new(i, j);
        if (WorldGenHelper.GetTileSafely(i, j).HasTile && PrimordialTree.IsPrimordialTree(i, j) && !PrimordialTreeDrawPoints.Contains(position)) {
            PrimordialTreeDrawPoints.Add(position);
        }
    }

    public override bool CanExplode(int i, int j, int type) {
        if (PrimordialTree.IsPrimordialTree(i, j) && !Main.hardMode) {
            return false;
        }

        return base.CanExplode(i, j, type);
    }
}

sealed class PrimordialTree : ModTree {
    public const int MINAXEREQUIRED = 75;

    public static bool IsPrimordialTree(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.ActiveTile(TileID.Trees)) {
            int checkJ = j;
            while (!WorldGenHelper.GetTileSafely(i, checkJ).HasTile || WorldGenHelper.ActiveTile(i, checkJ, TileID.Trees)) {
                checkJ++;
            }
            Tile grassTile = WorldGenHelper.GetTileSafely(i, checkJ);
            if (grassTile.ActiveTile(ModContent.TileType<BackwoodsGrass>())) {
                return true;
            }
        }

        return false;
    }

    public static string TexturePath => $"{RoA.ModName}/Resources/Textures/Tiles/Trees/{nameof(PrimordialTree)}";

    public override TreePaintingSettings TreeShaderSettings => new() {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<BackwoodsGrass>()];

    public override int TreeLeaf() => ModContent.GoreType<BackwoodsLeaf>();

    public override int CreateDust() => ModContent.DustType<WoodTrash>();

    public override int DropWood() => ModContent.ItemType<Items.Placeable.Crafting.Elderwood>();

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
        //NPC.NewNPCDirect(WorldGen.WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ModContent.NPCType<Content.NPCs.Backwoods.BabyFleder>());
        return true;
    }
}