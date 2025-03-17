﻿using RoA.Content.Tiles.Miscellaneous;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Banners;

[Autoload(false)]
sealed class SentinelWarriorBanner : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<MonsterBanners>(), (int)MonsterBanners.StyleID.SentinelWarrior);
        Item.width = 10;
        Item.height = 24;
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
    }
}
