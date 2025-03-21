using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

public class TheLegend : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("The Legend");
        //Tooltip.SetDefault("'CKnight'");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 32; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Decorations.TheLegend>();

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}