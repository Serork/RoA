using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Mechanisms;

sealed class NixieIndexator : ModItem {
    public override void SetDefaults() {
        Item.mech = true;
        Item.noWet = true;
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator>();
        Item.placeStyle = 0;
        Item.width = 10;
        Item.height = 12;
        Item.SetShopValues(ItemRarityColor.White0, Item.buyPrice(0, 2));
    }
}
