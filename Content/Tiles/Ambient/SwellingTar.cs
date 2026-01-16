using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Liquids;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace RoA.Content.Tiles.Ambient;

sealed class SwellingTar : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<SwellingTarTE>().Hook_AfterPlacement, -1, 0, true);
        TileObjectData.addTile(Type);

        AddMapEntry(Tar.LiquidColor/*, CreateMapEntryName()*/);

        DustType = (ushort)ModContent.DustType<Dusts.SolidifiedTar>();

        AnimationFrameHeight = 18 * 2;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        if (++frameCounter >= 8) {
            frameCounter = 0;
            frame = ++frame % 4;
        }
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        Point16 topLeft = TileHelper.GetTileTopLeft2(i, j, Type);
        SwellingTarTE? swellingTar = TileHelper.GetTE<SwellingTarTE>(topLeft.X, topLeft.Y);
        if (swellingTar is null) {
            return;
        }
        if (!swellingTar.IsReady) {
            frameYOffset = AnimationFrameHeight * 4;

            return;
        }

        frameYOffset = Main.tileFrame[Type];
        int num5 = i;
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0)
            num5--;

        int frameCount = 4;
        frameYOffset += num5 % frameCount;
        if (frameYOffset >= frameCount)
            frameYOffset -= frameCount;

        frameYOffset *= AnimationFrameHeight;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        Point16 topLeft = TileHelper.GetTileTopLeft2(i, j, Type);
        SwellingTarTE? swellingTar = TileHelper.GetTE<SwellingTarTE>(topLeft.X, topLeft.Y);
        if (swellingTar is null) {
            return;
        }
        if (!swellingTar.IsReady) {
            return;
        }

        float strengthFactor = 0f;
        int j2 = j;
        int j3 = 0;
        while (WorldGenHelper.GetTileSafely(i, j2).LiquidAmount > 0) {
            j2--;
            j3++;
        }
        strengthFactor = Helper.Approach(strengthFactor, 2f, j3 / 9f);
        if (Main.netMode != NetmodeID.Server && Main.tile[i - 1, j].TileType != Type && Main.tile[i, j + 1].TileType != Type) {
            var dims = new Vector2(35f, 35f);
            var startPosition = new Point16(i, j).ToWorldCoordinates();
            ProduceWaterRipples(startPosition + new Vector2(8f), dims, strengthFactor);
        }
    }

    private void ProduceWaterRipples(Vector2 startPosition, Vector2 dims, float strengthFactor) {
        WaterShaderData shaderData = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();

        FastRandom fastRandom = new FastRandom(Main.ActiveWorldFileData.Seed).WithModifier(65440uL);
        FastRandom fastRandom2 = fastRandom.WithModifier((int)startPosition.X / 16, (int)startPosition.Y / 16);
        // A universal time-based sinusoid which updates extremely rapidly. GlobalTime is 0 to 3600, measured in seconds.
        float waveSine = 0.1f * (float)Math.Sin((Main.GlobalTimeWrappedHourly * 0.2f + startPosition.Length() * fastRandom2.NextFloat()) * (5f + 15f * fastRandom2.NextFloat()));
        Vector2 ripplePos = startPosition;

        shaderData.QueueRipple(ripplePos, 350f * waveSine * strengthFactor, RippleShape.Circle);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        ModContent.GetInstance<SwellingTarTE>().Kill(i, j);
    }
}
