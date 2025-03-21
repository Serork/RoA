using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class BackwoodsStoneChest : ModItem {
    public override void SetDefaults() {
        Item.SetSize(32, 30);

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 10, 15, useTurn: true, autoReuse: true);

        Item.SetDefaultToStackable(Terraria.Item.CommonMaxStack);

        Item.value = 500;

        Item.createTile = ModContent.TileType<Tiles.Furniture.BackwoodsStoneChest>();
    }
}