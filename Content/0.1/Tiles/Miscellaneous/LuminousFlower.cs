using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class LuminousFlower : ModTile {
    public const float MINLIGHTMULT = 0.5f;

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new((ushort)ModContent.ItemType<Items.Miscellaneous.LuminousFlower>())];

    public override void Load() {
        On_WorldGen.plantDye += On_WorldGen_plantDye;
    }

    private void On_WorldGen_plantDye(On_WorldGen.orig_plantDye orig, int i, int j, bool exoticPlant) {
        orig(i, j, exoticPlant);

        if (!Main.hardMode && !exoticPlant) {
            UnifiedRandom unifiedRandom = (WorldGen.gen ? WorldGen.genRand : Main.rand);
            if (!Main.tile[i, j].HasTile || i < 95 || i > Main.maxTilesX - 95 || j < 95 || j > Main.maxTilesY - 95)
                return;

            int num = 90;
            num = 240;

            if (((double)j < Main.worldSurface && !WorldGen.remixWorldGen) && (!Main.tile[i, j - 1].HasTile ||
                Main.tileCut[Main.tile[i, j - 1].TileType]) && unifiedRandom.NextChance(0.5)) {
                int num2 = Utils.Clamp(i - num, 1, Main.maxTilesX - 1 - 1);
                int num3 = Utils.Clamp(i + num, 1, Main.maxTilesX - 1 - 1);
                int num4 = Utils.Clamp(j - num, 1, Main.maxTilesY - 1 - 1);
                int num5 = Utils.Clamp(j + num, 1, Main.maxTilesY - 1 - 1);
                ushort tileType = (ushort)ModContent.TileType<LuminousFlower>();
                for (int k = num2; k < num3; k++) {
                    for (int l = num4; l < num5; l++) {
                        if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileType)
                            return;
                    }
                }
                TileObjectData objectData = TileObjectData.GetTileData(tileType, 0);
                if (objectData.AnchorValidTiles.Contains(Main.tile[i, j].TileType)) {
                    if (!Main.tile[i, j - 1].AnyLiquid() && !Main.tile[i, j - 1].AnyWall()) {
                        //Main.LocalPlayer.position = new Vector2(i, j).ToWorldCoordinates();
                        WorldGenHelper.Place2x3(i, j - 1, tileType, countCut: false);
                    }
                }
            }
        }
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileLavaDeath[Type] = true;

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new(211, 141, 162), name);

        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 750;

        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        List<int> anchorValidTiles = [];
        for (int i = 0; i < TileLoader.TileCount; i++) {
            if (TileID.Sets.Conversion.Grass[i]) {
                anchorValidTiles.Add(i);
            }
        }
        anchorValidTiles.Add(TileID.CorruptGrass);
        anchorValidTiles.Add(TileID.CrimsonGrass);
        anchorValidTiles.Add(ModContent.TileType<BackwoodsGrass>());
        TileObjectData.newTile.AnchorValidTiles = [.. anchorValidTiles];
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.addTile(Type);

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        AnimationFrameHeight = 54;

        Main.tileCut[Type] = false;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        fail = false;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        //if (!TileDrawing.IsVisible(Main.tile[i, j])) {
        //    return;
        //}

        LuminiousFlowerLightUp(i, j, out float _, modTile: TileLoader.GetTile(Type));
    }

    public static void LuminiousFlowerLightUp(int i, int j, out float progress, ModTile modTile = null, bool shouldLightUp = true) {
        if (!Helper.OnScreenWorld(i, j)) {
            progress = 0f;
            return;
        }
        float[] lengths = new float[Main.CurrentFrameFlags.ActivePlayersCount];
        for (int k = 0; k < Main.CurrentFrameFlags.ActivePlayersCount; k++) {
            Player player = Main.player[k];
            if (!player.active) {
                continue;
            }
            lengths[k] = player.Distance(new Point(i, j).ToWorldCoordinates());
        }
        if (lengths.Length <= 0) {
            progress = 0f;
            return;
        }
        float dist = lengths.Min();
        float maxDist = 300f;
        void lightUp(float progress) {
            float r = 0.9f * progress;
            float g = 0.7f * progress;
            float b = 0.3f * progress;
            if (shouldLightUp) {
                Lighting.AddLight(i, j, r, g, b);
            }
            if (modTile != null) {
                if (TileDrawing.IsVisible(Main.tile[i, j])) {
                    Tile tile = Main.tile[i, j];
                    Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
                    if (Main.drawToScreen) {
                        zero = Vector2.Zero;
                    }
                    int height = tile.TileFrameY == 36 ? 18 : 16;
                    Main.spriteBatch.Draw(modTile.GetTileGlowTexture(),
                                          new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                          new Rectangle(tile.TileFrameX, tile.TileFrameY + Main.tileFrame[modTile.Type] * 18 * 3 - 2, 16, height),
                                          Color.White * progress * 0.9f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
        if (dist < maxDist) {
            progress = MathHelper.Clamp(1f - dist / maxDist, MINLIGHTMULT, 1f);
            lightUp(progress);
        }
        else {
            progress = MINLIGHTMULT;
            lightUp(MINLIGHTMULT);
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        frameCounter++;
        if (frameCounter > 5) {
            frameCounter = 0;

            frame++;
            if (frame > 14) {
                frame = 0;
            }
        }
    }
}
