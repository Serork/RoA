using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class Beacon : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;

        ItemID.Sets.SortingPriorityWiring[Type] = 82;
    }

    public override void SetDefaults() {
        int width = 14; int height = 38;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 10, 0);

        Item.createTile = ModContent.TileType<Tiles.Crafting.Beacon>();
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Wiring;
}