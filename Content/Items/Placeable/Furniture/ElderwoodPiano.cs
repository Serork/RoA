﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodPiano : ModItem {
    public override void SetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = Item.sellPrice(copper: 60);
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodPiano>();
    }
}
