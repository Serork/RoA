using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class AetherWreath : WreathItem, WreathItem.IWreathGlowMask {
    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f);

    Color IWreathGlowMask.GlowColor => Color.White;

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
            player.GetWreathHandler().CapMax1();
            player.noItems = true;

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
