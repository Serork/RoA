﻿using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodChest : ModItem {
    public override void SetDefaults() {
        Item.SetSize(32, 28);

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 10, 15, useTurn: true, autoReuse: true);

        Item.SetDefaultToStackable(Terraria.Item.CommonMaxStack);

        Item.value = Item.buyPrice(0, 0, 50);

        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodChest>();
    }
}