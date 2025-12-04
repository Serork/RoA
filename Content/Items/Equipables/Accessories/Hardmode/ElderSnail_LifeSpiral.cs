using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

// also see Hooks_Player
sealed class LifeSpiral : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(22, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));

        Item.lifeRegen = 2;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.pStone = true;
        player.GetCommon().IsElderShellEffectActive = true;
    }
}
