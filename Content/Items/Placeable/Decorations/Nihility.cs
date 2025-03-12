using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class Nihility : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("MOX");
        // Tooltip.SetDefault("'CKnight'");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;

        Item.createTile = ModContent.TileType<Tiles.Decorations.Nihility>();
        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}