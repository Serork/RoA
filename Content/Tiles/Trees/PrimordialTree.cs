using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Gores;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Trees;

sealed class MakePrimordialTreeImmuneToLava : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_WorldGen.WaterCheck += On_WorldGen_WaterCheck;
        On_Liquid.AddWater += On_Liquid_AddWater;
    }

    // also look for On_Liquid_DelWater in BackwoodsLilypad
    private bool CheckTile(int checkX, int checkY) {
        bool result = true;
        if (Main.tile[checkX, checkY].HasTile) {
            if (Main.tile[checkX, checkY].TileType == TileID.Trees && PrimordialTree.IsPrimordialTree(checkX, checkY) && !Main.hardMode) {
                result = false;
            }
        }
        return result;
    }

    private void On_Liquid_AddWater(On_Liquid.orig_AddWater orig, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (Main.tile[x, y] == null || tile.CheckingLiquid || x >= Main.maxTilesX - 5 || y >= Main.maxTilesY - 5 || x < 5 || y < 5 || tile.LiquidAmount == 0 || (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && tile.TileType != 546 && !Main.tileSolidTop[tile.TileType]))
            return;

        if (Liquid.numLiquid >= Liquid.curMaxLiquid - 1) {
            LiquidBuffer.AddBuffer(x, y);
            return;
        }

        tile.CheckingLiquid = true;
        tile.SkipLiquid = false;
        Main.liquid[Liquid.numLiquid].kill = 0;
        Main.liquid[Liquid.numLiquid].x = x;
        Main.liquid[Liquid.numLiquid].y = y;
        Main.liquid[Liquid.numLiquid].delay = 0;
        Liquid.numLiquid++;
        if (Main.netMode == 2)
            Liquid.NetSendLiquid(x, y);

        if (!tile.HasTile || WorldGen.gen)
            return;

        bool flag = false;
        if (tile.LiquidAmount == LiquidID.Lava) {
            if (TileObjectData.CheckLavaDeath(tile) && CheckTile(x, y))
                flag = true;
        }
        else if (TileObjectData.CheckWaterDeath(tile)) {
            flag = true;
        }

        if (CheckTile(x, y)) {
            flag = false;
        }

        if (flag) {
            WorldGen.KillTile(x, y);
            if (Main.netMode == 2)
                NetMessage.SendData(17, -1, -1, null, 0, x, y);
        }
    }

    private void On_WorldGen_WaterCheck(On_WorldGen.orig_WaterCheck orig) {
        Liquid.tilesIgnoreWater(ignoreSolids: true);
        Liquid.numLiquid = 0;
        LiquidBuffer.numLiquidBuffer = 0;
        for (int i = 1; i < Main.maxTilesX - 1; i++) {
            for (int num = Main.maxTilesY - 2; num > 0; num--) {
                Tile tile = Main.tile[i, num];
                tile.CheckingLiquid = false;
                if (tile.LiquidAmount > 0 && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
                    tile.LiquidAmount = 0;
                }
                else if (tile.LiquidAmount > 0) {
                    if (tile.HasTile) {
                        if (tile.LiquidType == LiquidID.Lava) {
                            if (TileObjectData.CheckLavaDeath(tile) && CheckTile(i, num)) {
                                WorldGen.KillTile(i, num);
                            }
                        }
                        else if (TileObjectData.CheckWaterDeath(tile)) {
                            WorldGen.KillTile(i, num);
                        }
                    }

                    Tile tile2 = Main.tile[i, num + 1];
                    if ((!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType]) && tile2.LiquidAmount < byte.MaxValue) {
                        if (tile2.LiquidAmount > 250)
                            tile2.LiquidAmount = byte.MaxValue;
                        else
                            Liquid.AddWater(i, num);
                    }

                    Tile tile3 = Main.tile[i - 1, num];
                    Tile tile4 = Main.tile[i + 1, num];
                    if ((!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) && tile3.LiquidAmount != tile.LiquidAmount)
                        Liquid.AddWater(i, num);
                    else if ((!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) && tile4.LiquidAmount != tile.LiquidAmount)
                        Liquid.AddWater(i, num);

                    if (tile.LiquidType == LiquidID.Lava) {
                        if (tile3.LiquidAmount > 0 && tile3.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (tile4.LiquidAmount > 0 && tile4.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (Main.tile[i, num - 1].LiquidAmount > 0 && Main.tile[i, num - 1].LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (tile2.LiquidAmount > 0 && tile2.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                    }
                }
            }
        }

        Liquid.tilesIgnoreWater(ignoreSolids: false);
    }

    void ILoadable.Unload() { }
}

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
            if (!TileDrawing.IsVisible(Main.tile[position.X, position.Y])) {
                continue;
            }

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
            if (bluePart || BackwoodsFogHandler.Opacity > 0f) {
                int height = tile.TileFrameY == 36 ? 18 : 16;
                ulong speed = (((ulong)j << 32) | (ulong)i);
                float posX = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                float posY = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
                int directionX = Utils.RandomInt(ref speed, 2) == 0 ? 1 : -1;
                int directionY = Utils.RandomInt(ref speed, 2) != 0 ? 1 : -1;
                float opacity = BackwoodsFogHandler.Opacity > 0f ? BackwoodsFogHandler.Opacity : 1f;
                var texture = PaintsRenderer.TryGetPaintedTexture(i, j, PrimordialTree.TexturePath + "_Glow");
                Main.spriteBatch.Draw(texture,
                                      new Vector2(i * 16 - (int)Main.screenPosition.X - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionX * posX,
                                      j * 16 - (int)Main.screenPosition.Y + 2 - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionY * posY),
                                      new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                                      Color.Lerp(Color.White, Lighting.GetColor(i, j), bluePart ? 0.6f : 0.8f) * opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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

    public override TreePaintingSettings TreeShaderSettings => WoodJungle;

    private static TreePaintingSettings WoodJungle = new TreePaintingSettings {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 1f / 6f,
        SpecialGroupMaximumHueValue = 5f / 6f,
        SpecialGroupMinimumSaturationValue = 0f,
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