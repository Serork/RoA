using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Face)]
sealed class DoubleGoggles : ModItem {
    public override void Load() {
        On_PlayerDrawHelper.SetShaderForData += On_PlayerDrawHelper_SetShaderForData;
    }

    public float AperiodicSin(float x, float dx = 0f, float a = MathHelper.Pi, float b = MathHelper.E) {
        return (MathF.Sin(x * a + dx) + MathF.Sin(x * b + dx)) * 0.5f;
    }

    private void On_PlayerDrawHelper_SetShaderForData(On_PlayerDrawHelper.orig_SetShaderForData orig, Player player, int cHead, ref DrawData cdd) {
        if (player.GetCommon().IsDoubleGogglesEffectActive) {
            orig(player, cHead, ref cdd);

            Effect chromaticAberrationShader = ShaderLoader.ChromaticAberrationShader.Value;
            float width = MathF.Max(100, player.width * 2.25f);
            float height = MathF.Max(100, player.height * 2.25f);
            Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
            Vector2 size = new(width, height);
            float aberrationPower = 0.08f;
            float waveFrequency = 2f;
            Vector2 point = (Vector2.One * 1f).RotatedBy(Helper.Wave(0f, MathHelper.TwoPi, waveFrequency, player.whoAmI));
            chromaticAberrationShader.Parameters["splitIntensity"].SetValue(aberrationPower);
            chromaticAberrationShader.Parameters["impactPoint"].SetValue(point);
            chromaticAberrationShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
            chromaticAberrationShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
            chromaticAberrationShader.Parameters["uImageSize0"].SetValue(size);
            chromaticAberrationShader.Parameters["uTime"].SetValue(TimeSystem.TimeForVisualEffects);
            chromaticAberrationShader.Parameters["uSaturation"].SetValue(Helper.Wave(0.25f, 0.75f, waveFrequency, 1f + player.whoAmI) * player.GetCommon().DoubleGogglesEffectOpacity * 1.5f);
            chromaticAberrationShader.CurrentTechnique.Passes[0].Apply();

            return;
        }

        orig(player, cHead, ref cdd);
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(26, 12);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.statLife >= player.statLifeMax2 * 0.5) {
            player.GetCommon().IsDoubleGogglesEffectActive = true;
        }
    }
}
