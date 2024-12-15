using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class MillionDollarPainting : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Million Dollar Painting");
        //Tooltip.SetDefault("'VFD'");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1000000;
    }

    public override void SetDefaults() {
        int width = 18; int height = 28;
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
        Item.createTile = ModContent.TileType<Tiles.Decorations.MillionDollarPainting>();
    }
}