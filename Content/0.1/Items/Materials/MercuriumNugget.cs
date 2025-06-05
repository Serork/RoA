using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class MercuriumNugget : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("'Shiny, but dangerous'");

        ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;

        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.MercuriumNugget>());

        int width = 20; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.sellPrice(0, 0, 40, 0);
    }
}