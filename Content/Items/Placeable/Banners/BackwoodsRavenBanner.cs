﻿using RoA.Content.Tiles.Miscellaneous;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Banners;

sealed class BackwoodsRavenBanner : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<MonsterBanners>(), (int)MonsterBanners.StyleID.BackwoodsRaven);
        Item.width = 10;
        Item.height = 24;
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
    }
}
