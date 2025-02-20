using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class DryadStatue : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dryad Statue");
        // Tooltip.SetDefault("'Lost and forsaken'");
        Item.ResearchUnlockCount = 1000000;
    }

    public override void SetDefaults() {
        Item.SetSize(28, 42);

        Item.SetDefaultToStackable(Item.CommonMaxStack);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ambient.DryadStatue>());

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(gold: 3);
    }
}