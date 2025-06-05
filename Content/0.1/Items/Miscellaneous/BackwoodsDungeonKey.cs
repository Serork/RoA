using RoA.Content.Items.Placeable.Furniture;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class BackwoodsDungeonKey : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BackwoodsDungeonChest>();
        ItemID.Sets.UsesCursedByPlanteraTooltip[Type] = true;
    }

    public override void SetDefaults() {
        Item.width = 18;
        Item.height = 28;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 8;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Keys;
}