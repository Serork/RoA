using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Common.Utilities.Extensions;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Gores;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Tiles.Trees;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Tiles.Ambient.LargeTrees;

sealed class BackwoodsBigTree : ModTile, TileHooks.ITileHaveExtraDraws, TileHooks.IRequireMinAxePower, TileHooks.IResistToAxe {
    int TileHooks.IRequireMinAxePower.MinAxe => PrimordialTree.MINAXEREQUIRED;

    bool TileHooks.IResistToAxe.CanBeApplied(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && (IsTrunk(i, j) || IsStart(i, j));
    float TileHooks.IResistToAxe.ResistToPick => 0.25f;

    public override void SetStaticDefaults() {
        LocalizedText name = CreateMapEntryName();

        DustType = ModContent.DustType<WoodTrash>();
        HitSound = SoundID.Dig;

        TileID.Sets.IsATreeTrunk[Type] = true;

        Main.tileMergeDirt[Type] = false;
        Main.tileSolid[Type] = false;
        Main.tileLighted[Type] = false; 
        Main.tileBlockLight[Type] = false; 
        Main.tileFrameImportant[Type] = true;
        Main.tileAxe[Type] = true;

        AddMapEntry(new Color(114, 81, 57), name);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
    }

    private static bool IsStart(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && !IsBranch(i, j) && !WorldGenHelper.ActiveTile(i, j + 1, GetSelfType());

    private static bool IsTrunk(int i, int j) => WorldGenHelper.ActiveTile(i, j, GetSelfType()) && !IsStart(i, j) && !IsNormalBranch(i, j) && !IsBigBranch(i, j);

    private static bool IsBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && tile.TileFrameY >= 108 && tile.TileFrameY < 180;
    }

    private static bool IsBranch2(int i, int j) => IsNormalBranch(i, j) || IsBigBranch(i, j);

    private static bool IsBigBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && IsBranch(i, j) && tile.TileFrameX == 144;
    }

    private static bool IsNormalBranch(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && IsBranch(i, j) && tile.TileFrameX == 108;
    }

    private static bool IsTop(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        return tile.ActiveTile(GetSelfType()) && tile.TileFrameX == 54 && tile.TileFrameY == 0;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (fail) {
            return;
        }

        if (IsStart(i, j + 1)) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j + 1);
            tile.TileFrameX += 54;
        }

        UnifiedRandom placeRandom = WorldGen.genRand;
        bool left = !WorldGenHelper.ActiveTile(i - 1, j, Type);
        int direction = -left.ToDirectionInt();
        if (IsTop(i, j)) {
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int count = placeRandom.Next(3, 6) * 10;
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 100f) * direction, placeRandom.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i, j - 15) * 16) + offset;
                Gore.NewGore(null, 
                    position, 
                    new Vector2(),
                    leafGoreType,
                    placeRandom.Next(90, 130) * 0.01f);
            }

            count = placeRandom.Next(3, 6) * 40;
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 100f) * direction, placeRandom.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i, j - 12) * 16) + offset;
                Dust.NewDustPerfect(position, DustType);
            }
        }

        if (IsBigBranch(i, j)) {
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int count = placeRandom.Next(3, 6);
            for (int k = 0; k < count; k++) {
                Vector2 offset = new Vector2(placeRandom.NextFloat(0f, 20f) * direction, placeRandom.NextFloat(0f, 40f)).RotatedBy(MathHelper.TwoPi);
                Vector2 position = (new Vector2(i + 1 * direction, j - 2) * 16) + offset;
                Gore.NewGore(null,
                    position,
                    new Vector2(),
                    leafGoreType,
                    placeRandom.Next(90, 130) * 0.01f);
                Dust.NewDustPerfect(position, DustType);
            }
        }

        if (IsStart(i, j) && !noItem) {
            for (int checkX = i; checkX < i + 3; checkX++) {
                for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(checkX, checkJ).ActiveTile(Type); checkJ--) {
                    WorldGen.KillTile(checkX, checkJ, false, false, false);
                }
            }
            for (int checkX = i; checkX > i - 3; checkX--) {
                for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(checkX, checkJ).ActiveTile(Type); checkJ--) {
                    WorldGen.KillTile(checkX, checkJ, false, false, false);
                }
            }
            for (int checkX = i; checkX < i + 4; checkX++) {
                if (checkX != i && !WorldGenHelper.GetTileSafely(checkX, j).HasTile) {
                    break;
                }
                if (IsStart(checkX, j)) {
                    WorldGenHelper.GetTileSafely(checkX, j).HasTile = false;
                    for (int k = 0; k < 5; k++) {
                        Dust.NewDustDirect(new Vector2(checkX, j).ToWorldCoordinates(), 16, 16, DustType);
                    }
                    int itemWhoAmI = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(checkX, j), checkX * 16, j * 16, 16, 16, ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
                    if (Main.netMode == NetmodeID.MultiplayerClient && itemWhoAmI >= 0) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
            for (int checkX = i; checkX > i - 4; checkX--) {
                if (checkX != i && !WorldGenHelper.GetTileSafely(checkX, j).HasTile) {
                    break;
                }
                if (IsStart(checkX, j)) {
                    WorldGenHelper.GetTileSafely(checkX, j).HasTile = false;
                    for (int k = 0; k < 5; k++) {
                        Dust.NewDustDirect(new Vector2(checkX, j).ToWorldCoordinates(), 16, 16, DustType);
                    }
                    int itemWhoAmI = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(checkX, j), checkX * 16, j * 16, 16, 16, ModContent.ItemType<Elderwood>(), Main.rand.Next(2, 6));
                    if (Main.netMode == NetmodeID.MultiplayerClient && itemWhoAmI >= 0) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f, 0f, 0f, 0, 0, 0);
                    }
                }
            }

            return;
        }
        if (IsBranch2(i, j) && noItem) {
            return;
        }

        if (IsNormalBranch(i, j)) {
            if (IsBranch(i + 1, j)) {
                WorldGenHelper.GetTileSafely(i + 1, j).TileFrameY -= 72;
            }
            if (IsBranch(i - 1, j)) {
                WorldGenHelper.GetTileSafely(i - 1, j).TileFrameY -= 72;
            }
        }
        if (IsBigBranch(i, j)) {
            if (IsBranch(i + 1, j)) {
                Tile tile = WorldGenHelper.GetTileSafely(i + 1, j);
                tile.TileFrameX -= 54;
                tile.TileFrameY -= 72;
            }
            if (IsBranch(i - 1, j)) {
                Tile tile = WorldGenHelper.GetTileSafely(i - 1, j);
                tile.TileFrameX -= 54;
                tile.TileFrameY -= 72;
            }
        }

        if (IsTrunk(i, j)) {
            if (!noItem) {
                if (IsTrunk(i + 1, j)) {
                    WorldGen.KillTile(i + 1, j, false, false, true);
                }
                if (IsTrunk(i - 1, j)) {
                    WorldGen.KillTile(i - 1, j, false, false, true);
                }
                if (IsBranch(i - 1, j)) {
                    WorldGen.KillTile(i - 1, j, false, false, false);
                }
                if (IsBranch(i + 1, j)) {
                    WorldGen.KillTile(i + 1, j, false, false, false);
                }
                if (IsBranch(i - 2, j)) {
                    WorldGen.KillTile(i - 2, j, false, false, false);
                }
                if (IsBranch(i + 2, j)) {
                    WorldGen.KillTile(i + 2, j, false, false, false);
                }
                for (int destroyExtraX = 1; destroyExtraX < 3; destroyExtraX++) {
                    if (IsBranch2(i + destroyExtraX, j + 1)) {
                        WorldGen.KillTile(i + destroyExtraX, j + 1, false, false, true);
                    }
                    if (IsBranch2(i - destroyExtraX, j + 1)) {
                        WorldGen.KillTile(i - destroyExtraX, j + 1, false, false, true);
                    }
                }
            }
            for (int checkJ = j - 1; WorldGenHelper.GetTileSafely(i, checkJ).ActiveTile(Type); checkJ--) {
                WorldGen.KillTile(i, checkJ, false, false, false);
                if (IsBranch(i - 1, checkJ)) {
                    WorldGen.KillTile(i - 1, checkJ, false, false, false);
                }
                if (IsBranch(i + 1, checkJ)) {
                    WorldGen.KillTile(i + 1, checkJ, false, false, false);
                }
                if (IsTrunk(i + 1, checkJ)) {
                    WorldGen.KillTile(i + 1, checkJ, false, false, false);
                    if (IsBranch(i + 2, checkJ)) {
                        WorldGen.KillTile(i + 2, checkJ, false, false, false);
                    }
                }
                if (IsTrunk(i - 1, checkJ))  {
                    WorldGen.KillTile(i - 1, checkJ, false, false, false);
                    if (IsBranch(i - 2, checkJ)) {
                        WorldGen.KillTile(i - 2, checkJ, false, false, false);
                    }
                }
            }
            SetFramingForCutTrees(i, j + 1, placeRandom);
            SetFramingForCutTrees(i + 1, j + 1, placeRandom);
            SetFramingForCutTrees(i - 1, j + 1, placeRandom);
        }
    }

    private static void SetFramingForCutTrees(int i, int j, UnifiedRandom placeRandom) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (IsTrunk(i, j)) {
            tile.TileFrameX = (short)(18 + (placeRandom.NextBool() ? 18 : 0));
            tile.TileFrameY = (short)(placeRandom.NextBool() ? 18 : 0);
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY += 2;

    public static void Place(int i, int j, int height = 20, UnifiedRandom placeRand = null) {
        placeRand ??= Main.rand;

        PlaceBegin(i, j, placeRand, out Point pointToStartPlacingTrunk);
        PlaceTrunk(pointToStartPlacingTrunk, height, placeRand);
    }

    private static ushort GetSelfType() => (ushort)ModContent.TileType<BackwoodsBigTree>();

    private static void PlaceTrunk(Point pointToPlaceTrunk, int height, UnifiedRandom placeRand) {
        int i = pointToPlaceTrunk.X, j = pointToPlaceTrunk.Y;
        bool hasProperStart() {
            for (int checkX = i - 1; checkX < i + 3; checkX++) {
                if (!WorldGenHelper.ActiveTile(checkX, j + 1, GetSelfType())) {
                    return false;
                }
            }
            return true;
        }
        if (!hasProperStart()) {
            return;
        }
        short tileFrameX, tileFrameY;
        for (int placeY = j; placeY > j - height; placeY--) {
            bool canPlaceBranch = placeY != j;
            bool canPlaceBigBranch = WorldGenHelper.GetTileSafely(i, placeY - 1).TileFrameX < 72 && WorldGenHelper.GetTileSafely(i, placeY + 1).TileFrameX < 72;
            GetFramingForTrunk(canPlaceBranch, canPlaceBigBranch, placeRand, out tileFrameX, out tileFrameY, out bool shouldPlaceBranch, out bool shouldPlaceBigBranch);
            short frameXForBranch = (short)(shouldPlaceBigBranch ? 144 : 108);
            if (shouldPlaceBranch || shouldPlaceBigBranch) {
                PlaceTileInternal(i, placeY, tileFrameX, tileFrameY);
                PlaceTileInternal(i - 1, placeY, frameXForBranch, tileFrameY);
            }
            else {
                PlaceTileInternal(i, placeY, tileFrameX, tileFrameY);
            }
            GetFramingForTrunk(canPlaceBranch, canPlaceBigBranch, placeRand, out tileFrameX, out tileFrameY, out shouldPlaceBranch, out shouldPlaceBigBranch, true);
            frameXForBranch = (short)(shouldPlaceBigBranch ? 144 : 108);
            if (shouldPlaceBranch || shouldPlaceBigBranch) {
                PlaceTileInternal(i + 1, placeY, tileFrameX, tileFrameY);
                PlaceTileInternal(i + 2, placeY, frameXForBranch, tileFrameY);
            }
            else {
                PlaceTileInternal(i + 1, placeY, tileFrameX, tileFrameY);
            }
        }
        int topPlaceY = j - height;
        GetFramingForTop(placeRand, out tileFrameX, out tileFrameY);
        PlaceTileInternal(i, topPlaceY, tileFrameX, tileFrameY);
        PlaceTileInternal(i + 1, topPlaceY, tileFrameX, tileFrameY);
    }

    private static void GetFramingForTop(UnifiedRandom placeRand, out short tileFrameX, out short tileFrameY) {
        tileFrameX = 54;
        tileFrameY = 0;
    }

    private static void GetFramingForTrunk(bool canPlaceBranch, bool canPlaceBigBranch, UnifiedRandom placeRand, out short tileFrameX, out short tileFrameY, out bool shouldPlaceBranch, out bool shouldPlaceBigBranch, bool second = false) {
        shouldPlaceBranch = canPlaceBranch && placeRand.NextBool(5);
        shouldPlaceBigBranch = canPlaceBigBranch && canPlaceBranch && !shouldPlaceBranch && placeRand.NextBool(5);
        if (shouldPlaceBigBranch) {
            shouldPlaceBranch = true;
        }
        short frameY = (short)(shouldPlaceBranch ? 108 : 36);
        tileFrameY = (short)(frameY + placeRand.Next(4) * 18);
        tileFrameX = (short)((shouldPlaceBigBranch ? 72 : 18) + (second ? 18 : 0));
    }

    private static void PlaceBegin(int i, int j, UnifiedRandom placeRand, out Point pointToStartPlacingTrunk) {
        short getFrameYForStart() => (short)(180 + (placeRand.NextBool() ? 18 : 0));
        PlaceTileInternal(i - 1, j, 0, getFrameYForStart());
        PlaceTileInternal(i, j, 18, getFrameYForStart());
        PlaceTileInternal(i + 1, j, 36, getFrameYForStart());
        PlaceTileInternal(i + 2, j, 54, getFrameYForStart());
        pointToStartPlacingTrunk = new Point(i, j - 1);
    }

    private static void PlaceTileInternal(int i, int j, short tileFrameX, short tileFrameY) {
        WorldGen.PlaceTile(i, j, GetSelfType(), true, false, -1, 0);
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        tile.TileFrameX = tileFrameX;
        tile.TileFrameY = tileFrameY;
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        DrawTop(i, j, spriteBatch);
    }

    private void DrawTop(int i, int j, SpriteBatch spriteBatch) {
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (IsNormalBranch(i, j) || IsBigBranch(i, j) || IsTop(i, j)) {
            TileHelper.AddPostDrawPoint(this, i, j);

            return false;
        }

        return true;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_treeWindCounter")]
    public extern static ref double TileDrawing_treeWindCounter(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetWindCycle")]
    public extern static float TileDrawing_GetWindCycle(TileDrawing self, int x, int y, double windCounter);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rand")]
    public extern static ref UnifiedRandom TileDrawing_rand(TileDrawing self);

    void TileHooks.ITileHaveExtraDraws.PostDrawExtra(SpriteBatch spriteBatch, Point pos) {
        int i = pos.X, j = pos.Y;
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        bool shouldDrawBranch = IsNormalBranch(i, j);
        Vector2 drawPosition = new(i * 16 - (int)Main.screenPosition.X - 18,
                                   j * 16 - (int)Main.screenPosition.Y);
        bool shouldDrawBigBranch = IsBigBranch(i, j);
        bool left = !WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(GetSelfType());
        SpriteFrame spriteFrame;
        byte variant;
        int offsetX, offsetY;
        Color color = Lighting.GetColor(i, j);
        SpriteEffects effects = SpriteEffects.None;
        UnifiedRandom random = TileDrawing_rand(Main.instance.TilesRenderer);
        if (!left) {
            effects = SpriteEffects.FlipHorizontally;
        }
        bool flag = tile.WallType > 0;
        if (shouldDrawBigBranch) {
            Texture2D bigBranchTexture = ModContent.Request<Texture2D>(Texture + "_BigBranches").Value;
            Vector2 textureSize = bigBranchTexture.Size();
            spriteFrame = new(1, 3);
            ulong seed = (ulong)(i * j % 192372);
            variant = (byte)Utils.RandomInt(ref seed, 3);
            spriteFrame = spriteFrame.With(0, variant);
            Rectangle sourceRectangle = spriteFrame.GetSourceRectangle(bigBranchTexture);
            int direction = -left.ToDirectionInt();
            offsetX = -left.ToDirectionInt() * (bigBranchTexture.Width - 12);
            offsetX -= bigBranchTexture.Width / 2 * direction;
            offsetX -= 4 * direction;
            if (left) {
                offsetX -= 8;
            }
            offsetY = -sourceRectangle.Height / 2;
            drawPosition.X += offsetX;
            drawPosition.Y += offsetY;
            float num8 = 0f;
            float num4 = 0.06f;
            if (!flag)
                num8 = TileDrawing_GetWindCycle(Main.instance.TilesRenderer, i, j, TileDrawing_treeWindCounter(Main.instance.TilesRenderer));
            if (num8 < 0f)
                drawPosition.X += num8 / 3f;

            drawPosition.X -= Math.Abs(num8 / 3f) * 2f;
            Vector2 origin = new(!left ? 0f : textureSize.X, textureSize.Y / 2f);
            float num = Main.WindForVisuals;
            if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
                num = Math.Max(Math.Abs(Main.WindForVisuals), 401 * 0.001f);
                drawPosition.X -= 3f;
            }
            spriteBatch.Draw(bigBranchTexture, drawPosition - Vector2.UnitX * (10f - Math.Abs(num * 2.5f) * 2.5f) + origin, sourceRectangle, color, num8 * num4, origin, 1f, effects, 0f);
        }
        if (shouldDrawBranch) {
            Texture2D branchTexture = ModContent.Request<Texture2D>(Texture + "_Branches").Value;
            offsetY = 0;
            if (left) {
                offsetX = 10;
            }
            else {
                offsetX = 26;
            }
            drawPosition.X += offsetX;
            drawPosition.Y += offsetY;
            variant = (byte)((tile.TileFrameY - 108) / 18);
            spriteFrame = new(1, 4);
            spriteFrame = spriteFrame.With(0, variant);
            spriteBatch.Draw(branchTexture, drawPosition - Vector2.UnitX * 10f, spriteFrame.GetSourceRectangle(branchTexture), color, 0f, Vector2.Zero, 1f, effects, 0f);
        }
        bool shouldDrawTop = IsTop(i, j);
        if (shouldDrawTop) {
            if (WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(Type)) {
                return;
            }
            Texture2D topTexture = ModContent.Request<Texture2D>(Texture + "_Top").Value;
            Vector2 textureSize = topTexture.Size();
            Vector2 offset = -textureSize;
            offset.X += textureSize.X / 2f;
            offset += new Vector2(36f, 18f);
            effects = SpriteEffects.None;
            offset.X += effects == SpriteEffects.None ? 4f : -10f;

            float num3 = 0.02f;
            float num15 = 0f;
            if (!flag)
                num15 = TileDrawing_GetWindCycle(Main.instance.TilesRenderer, i, j, TileDrawing_treeWindCounter(Main.instance.TilesRenderer));
            drawPosition.X += num15 * 2f;
            drawPosition.Y += Math.Abs(num15) * 2f;
            Vector2 origin = new(textureSize.X / 2f, textureSize.Y);
            spriteBatch.Draw(topTexture, drawPosition + offset + origin, null, color, num15 * num3, origin, 1f, effects, 0f);

            Vector2 position = (new Vector2(i, j - 15) * 16) + new Vector2(random.NextFloat(-100f, 100f), random.NextFloat(0f, 200f)).RotatedBy(MathHelper.TwoPi);
            spawnLeafs(position, true);
        }

        void spawnLeafs(Vector2? position = null, bool increasedSpawnRate = false) {
            IEntitySource entitySource = new EntitySource_TileUpdate(i, j);
            ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
            int x = i, y = j;
            if (Main.rand.Next(typeof(TileDrawing).GetFieldValue<int>("_leafFrequency", Main.instance.TilesRenderer) / (increasedSpawnRate ? 10 : 1)) == 0) {
                tile = Main.tile[x, y + 1];
                if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                    float windForVisuals = Main.WindForVisuals;
                    if ((!(windForVisuals < -0.2f) || (!WorldGen.SolidTile(Main.tile[x - 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x - 2, y + 1]))) && (!(windForVisuals > 0.2f) || (!WorldGen.SolidTile(Main.tile[x + 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x + 2, y + 1]))))
                        Gore.NewGorePerfect(entitySource, position != null ? position.Value : new Vector2(x * 16, y * 16 + 16), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                }
                if (Main.rand.NextBool()) {
                    int num = 0;
                    if (Main.WindForVisuals > 0.2f)
                        num = 1;
                    else if (Main.WindForVisuals < -0.2f)
                        num = -1;

                    tile = Main.tile[x + num, y];
                    if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                        int num2 = 0;
                        if (num == -1)
                            num2 = -10;

                        Gore.NewGorePerfect(entitySource, position != null ? new Vector2(position.Value.X + 8 + 4 * num + num2, position.Value.Y * 16 + 8) : new Vector2(x * 16 + 8 + 4 * num + num2, y * 16 + 8), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                    }
                }
            }
        }
        spawnLeafs();
    }
}
