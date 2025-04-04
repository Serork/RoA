﻿using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Crafting;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class AnimalLeather : ModItem {
    private class LeatherValueChanger : GlobalItem {
        public override void SetDefaults(Item entity) {
            if (entity.type == ItemID.Leather) {
                entity.value = Item.sellPrice(0, 0, 1, 0);
            }
        }
    }

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Animal Leather");
        Item.ResearchUnlockCount = 50;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[ItemID.Leather] = 50;
    }

    public override void SetDefaults() {
        int width = 26; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.useAnimation = Item.useTime = 18;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.sellPrice(0, 0, 0, 25);
    }

    public override bool CanUseItem(Player player)
        => Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == (ushort)ModContent.TileType<TanningRack>()
        && player.position.X / 16f - Player.tileRangeX - Item.tileBoost - player.blockRange <= Player.tileTargetX
            && (player.position.X + player.width) / 16f + Player.tileRangeX + Item.tileBoost - 1f + player.blockRange >= Player.tileTargetX && player.position.Y / 16f - Player.tileRangeY - Item.tileBoost - player.blockRange <= Player.tileTargetY && (player.position.Y + player.height) / 16f + Player.tileRangeY + Item.tileBoost - 2f + player.blockRange >= Player.tileTargetY;
}