using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class BackwoodsStoneChest : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(32, 30);

        Item.SetDefaultsToUsable(ItemUseStyleID.Swing, 10, 15, useTurn: true, autoReuse: true);

        Item.SetDefaultsToStackable(Terraria.Item.CommonMaxStack);

        Item.value = 500;

        Item.createTile = ModContent.TileType<Tiles.Furniture.BackwoodsStoneChest>();
    }
}