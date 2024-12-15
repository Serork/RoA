using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

public class TheLegend : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("The Legend");
        //Tooltip.SetDefault("'CKnight'");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1000000;
    }

    public override void SetDefaults() {
        int width = 32; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;

        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.consumable = true;
        Item.rare = ItemRarityID.White;
        //Item.value = Item.buyPrice(platinum: 999);
        //Item.value = Item.sellPrice(platinum: 666);
        Item.createTile = ModContent.TileType<Tiles.Decorations.TheLegend>();
    }
}