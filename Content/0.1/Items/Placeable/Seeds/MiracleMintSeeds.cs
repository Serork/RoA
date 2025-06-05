using RoA.Content.Tiles.Plants;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Seeds;

sealed class MiracleMintSeeds : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults() {
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useStyle = 1;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<MiracleMint>();
        Item.placeStyle = 0;
        Item.width = 22;
        Item.height = 18;

        Item.value = Item.sellPrice(0, 0, 0, 16);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.AlchemySeeds;
    }
}