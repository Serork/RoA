using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using RoA.Core;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class DryadStatue : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dryad Statue");
        // Tooltip.SetDefault("'Lost and forsaken'");
        Item.ResearchUnlockCount = 1000000;
    }

    public override void SetDefaults() {
        Item.SetSize(28, 42);

        Item.SetDefaultToStackable(99);

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 10, 15, true, true, true);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(gold: 3);
        Item.createTile = ModContent.TileType<Tiles.Ambient.DryadStatue>();
    }
}