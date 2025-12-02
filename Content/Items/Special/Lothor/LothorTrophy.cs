using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

sealed class LothorTrophy : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Decorations.LothorTrophy>();
        Item.width = 32;
        Item.height = 32;
        Item.value = Item.sellPrice(0, 1);
        Item.rare = ItemRarityID.Blue;
    }
}