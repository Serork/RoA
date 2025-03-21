using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class DruidBiomeWaterfallStyle : ModWaterfallStyle {
    public override string Texture => ResourceManager.BackwoodsTextures + GetType().Name;
}

sealed class DruidBiomeWaterStyle : ModWaterStyle {
    public override string Texture => ResourceManager.BackwoodsTextures + GetType().Name;

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>(RoA.ModName + "/DruidBiomeWaterfallStyle").Slot;

    public override int GetSplashDust() => DustID.Water_Jungle;

    public override int GetDropletGore() => ModContent.Find<ModGore>(RoA.ModName + "/DruidBiomeWaterDroplet").Type;

    public override Asset<Texture2D> GetRainTexture() => ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeRain");

    public override byte GetRainVariant() => (byte)Main.rand.Next(3);

    public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
        float value = MathHelper.Clamp((ModContent.GetInstance<TileCount>().BackwoodsTiles - 140f) / 100f, 0f, 0.5f);
        r = MathHelper.Lerp(0.88f, 0.1f, value);
        g = MathHelper.Lerp(0.96f, 0.7f, value);
        b = MathHelper.Lerp(1.015f, 0.3f, value);
    }

    public override Color BiomeHairColor() => new(45, 140, 88);
}