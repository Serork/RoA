using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class GobletOfPain : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.GobletOfPain>());
        Item.DefaultToAccessory(26, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DamageClass.Generic) += 0.15f;

        player.GetCommon().IsGobletOfPainEffectActive = true;
    }
}
