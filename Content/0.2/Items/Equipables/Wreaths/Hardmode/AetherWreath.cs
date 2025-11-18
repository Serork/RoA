using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class AetherWreath : WreathItem, WreathItem.IWreathGlowMask {
    private static float _shimmerAlpha;

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f);

    Color IWreathGlowMask.GlowColor => Color.White;

    public override void Load() {
        On_Main.DoLightTiles += On_Main_DoLightTiles;
        On_Main.DrawCapture += On_Main_DrawCapture;
        On_OverlayManager.Draw += On_OverlayManager_Draw;
    }

    private void On_OverlayManager_Draw(On_OverlayManager.orig_Draw orig, OverlayManager self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch) {
        orig(self, spriteBatch, layer, beginSpriteBatch);

        if (!Main.gameMenu && !Main.LocalPlayer.CanSeeShimmerEffects()) {
            if (Main.LocalPlayer.GetCommon().IsAetherInvincibilityActive ||
                Main.LocalPlayer.GetCommon().AetherShimmerAlpha > 0f) {
                if (layer == RenderLayers.InWorldUI && _shimmerAlpha != 0f) {
                    Main.shimmerAlpha = _shimmerAlpha;
                    _shimmerAlpha = 0f;
                }
            }
        }
    }

    private void On_Main_DoLightTiles(On_Main.orig_DoLightTiles orig, Main self) {
        orig(self);

        if (!Main.gameMenu && !Main.LocalPlayer.CanSeeShimmerEffects()) {
            if (Main.LocalPlayer.GetCommon().IsAetherInvincibilityActive ||
                Main.LocalPlayer.GetCommon().AetherShimmerAlpha > 0f) {
                _shimmerAlpha = Main.LocalPlayer.GetCommon().AetherShimmerAlpha;
                Main.shimmerAlpha *= 0.25f;
            }
        }
    }

    private void On_Main_DrawCapture(On_Main.orig_DrawCapture orig, Main self, Rectangle area, Terraria.Graphics.Capture.CaptureSettings settings) {
        if (!Main.gameMenu && !Main.LocalPlayer.CanSeeShimmerEffects()) {
            if (Main.LocalPlayer.GetCommon().IsAetherInvincibilityActive ||
                Main.LocalPlayer.GetCommon().AetherShimmerAlpha > 0f) {
                float alpha = Main.LocalPlayer.GetCommon().AetherShimmerAlpha;
                Main.shimmerAlpha *= 0.25f;
                orig(self, area, settings);
                Main.shimmerAlpha = alpha;
                return;
            }
        }

        orig(self, area, settings);
    }

    protected override void SafeSetDefaults() {
        int width = 30; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.GetWreathHandler().IsFull1) {
            player.GetCommon().IsAetherInvincibilityActive = true;
            //player.GetWreathHandler().CapMax1();
            player.noItems = true;

            if (player.CanSeeShimmerEffects()) {
                return;
            }

            if (!player.IsLocal()) {
                return;
            }

            float num5 = 1f;
            float num6 = 1f;
            float num7 = Main.shimmerAlpha;

            float max = 0.875f;

            num5 *= 1f * max;
            num6 *= 0.7f * max;
            if (num7 < 1f * max) {
                num7 += 0.025f * 3f;
                if (num7 > 1f * max)
                    num7 = 1f * max;
            }

            if (num7 >= 0.5f * max) {
                Main.shimmerDarken = MathHelper.Clamp(Main.shimmerDarken + 0.025f * 3f, 0f, 1f * max * 0.5f);
                Main.shimmerBrightenDelay = 4f;
            }

            Main.shimmerAlpha = num7;
            player.GetCommon().AetherShimmerAlpha = Main.shimmerAlpha;

            if (num5 != Player.airLightDecay) {
                if (Player.airLightDecay >= num5) {
                    Player.airLightDecay -= 0.005f;
                    if (Player.airLightDecay < num5)
                        Player.airLightDecay = num5;
                }
                else {
                    Player.airLightDecay += 0.005f;
                    if (Player.airLightDecay > num5)
                        Player.airLightDecay = num5;
                }
            }

            if (num6 != Player.solidLightDecay) {
                if (Player.solidLightDecay >= num6) {
                    Player.solidLightDecay -= 0.005f;
                    if (Player.solidLightDecay < num6)
                        Player.solidLightDecay = num6;
                }
                else {
                    Player.solidLightDecay += 0.005f;
                    if (Player.solidLightDecay > num6)
                        Player.solidLightDecay = num6;
                }
            }
        }
    }
}
