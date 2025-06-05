using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class FlamingFabric : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.FlamingFabric>());

        int width = 24; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}