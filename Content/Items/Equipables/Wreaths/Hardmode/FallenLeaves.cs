using Microsoft.Xna.Framework;

using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class FallenLeaves : WreathItem, WreathItem.IWreathGlowMask {
    Color IWreathGlowMask.GlowColor => Color.White;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 40);

        Item.SetShopValues(ItemRarityColor.Lime7, Item.buyPrice());
    }
}
