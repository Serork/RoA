using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class TanningRack : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Tanning Rack");
        // Tooltip.SetDefault("Allows leather to be obtained from enemies\nIt must be processed at the rack before spoiling");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crafting.TanningRack>());

        Item.consumable = true;
        Item.rare = ItemRarityID.White;

        Item.value = Item.sellPrice(0, 1, 0, 0);
    }

    public override bool CanUseItem(Player player) => !player.HasBuff<Skinning>();
}