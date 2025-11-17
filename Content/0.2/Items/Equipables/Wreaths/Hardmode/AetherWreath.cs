using Microsoft.Xna.Framework;

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
        }
    }
}
