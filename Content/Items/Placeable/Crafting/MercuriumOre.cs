using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Achievements;
using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class MercuriumOre : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 16;
        Item.height = 20;

        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;

        Item.createTile = ModContent.TileType<Tiles.Crafting.MercuriumOre>();
    }

    public override void PostUpdate() {
        if (Main.rand.Next(4) == 0) {
            int dust = Dust.NewDust(Item.position - Item.Size / 2f, Item.width, Item.height, ModContent.DustType<Dusts.ToxicFumes>(), 0f, -4f, 100, new Color(), 1.5f);
            Dust dust2 = Main.dust[dust];
            dust2.scale *= 0.5f;
            dust2 = Main.dust[dust];
            dust2.velocity *= 1.5f;
            dust2 = Main.dust[dust];
            dust2.velocity.Y *= -0.5f;
            dust2.noLight = false;
        }
    }
}

sealed class MercuriumOrePlayerHandler : ModPlayer {
    private bool _updateInventory;

    private class MercuriumOreAchievement : GlobalItem {
        public override bool InstancePerEntity => true;

        public override bool OnPickup(Item item, Player player) {
            if (player.buffImmune[BuffID.Poisoned] && player.whoAmI == Main.myPlayer && item.type == ModContent.ItemType<MercuriumOre>()) {
                ModContent.GetInstance<MineMercuriumNugget>().MineMercuriumAndIgnoreItsEffectCondition.Complete();

                //RoA.CompleteAchievement("MineMercuriumNugget");
                //player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>().MineMercuriumNugget = true;
            }

            return base.OnPickup(item, player);
        }
    }

    private class MercuriumInInventoryHack : GlobalItem {
        public override void UpdateInventory(Item item, Player player) {
            if (player.GetModPlayer<MercuriumOrePlayerHandler>()._updateInventory && item.type == ModContent.ItemType<MercuriumOre>()) {
                player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
            }
        }
    }

    public override void PostUpdate() {
        if (Player.dead) {
            return;
        }

        _updateInventory = false;

        if (Player.buffImmune[BuffID.Poisoned]) {
            return;
        }

        if (Player.HasBuff(ModContent.BuffType<ToxicFumesNoTimeDisplay>())) {
            return;
        }

        _updateInventory = true;

        int type = ModContent.ItemType<MercuriumOre>();
        if (Player.HasItemInAnyInventory(type) || (Player.whoAmI == Main.myPlayer && Main.mouseItem.type == type) || Player.trashItem.type == type)
            Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);

        if (Player.whoAmI == Main.myPlayer) {
            if (Player.chest == -2) {
                Chest chest = Player.bank;
                for (int j = 0; j < 40; j++) {
                    Item item = chest.item[j];
                    if (!item.IsEmpty() && item.type == ModContent.ItemType<MercuriumOre>()) {
                        Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
                        break;
                    }
                }
            }

            if (Player.chest == -4) {
                Chest chest2 = Player.bank3;
                for (int k = 0; k < 40; k++) {
                    Item item = chest2.item[k];
                    if (!item.IsEmpty() && item.type == ModContent.ItemType<MercuriumOre>()) {
                        Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
                        break;
                    }
                }
            }

            if (Player.chest == -5) {
                Chest chest3 = Player.bank4;
                for (int l = 0; l < 40; l++) {
                    Item item = chest3.item[l];
                    if (!item.IsEmpty() && item.type == ModContent.ItemType<MercuriumOre>()) {
                        Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
                        break;
                    }
                }
            }

            if (Player.chest == -3) {
                Chest chest4 = Player.bank2;
                for (int m = 0; m < 40; m++) {
                    Item item = chest4.item[m];
                    if (!item.IsEmpty() && item.type == ModContent.ItemType<MercuriumOre>()) {
                        Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
                        break;
                    }
                }
            }
        }
        foreach (Item item in Main.ActiveItems) {
            if (item is null || !item.active || item.type != ModContent.ItemType<MercuriumOre>() || item.Distance(Player.Center) > 100f)
                continue;

            if (Collision.CanHit(Player, item)) {
                Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
            }
        }

        Check(7, 50);
    }

    private void Check(int fluff, int buffTime) {
        if (Player.HasBuff(ModContent.BuffType<ToxicFumesNoTimeDisplay>()) ||
            Player.HasBuff(ModContent.BuffType<ToxicFumes>())) {
            return;
        }
        int tilesAway = fluff;
        int x = (int)Player.Center.X / 16;
        int y = (int)Player.Center.Y / 16;
        for (int i = x - tilesAway; i < x + tilesAway + 1; i++) {
            for (int j = y - tilesAway; j < y + tilesAway + 1; j++) {
                if (WorldGen.InWorld(i, j)) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    int tileType = ModContent.TileType<Tiles.Crafting.MercuriumOre>();
                    if (!tile.HasTile) {
                        continue;
                    }
                    if (!Main.tileSolid[tile.TileType]) {
                        continue;
                    }
                    if (tile.TileType != tileType) {
                        continue;
                    }
                    if (WorldGenHelper.ModifiedCanHit(Player.position, 0, 0, new Point(i, j).ToWorldCoordinates(), 0, 0, tileType)) {
                        Player.AddBuff(ModContent.BuffType<ToxicFumesNoTimeDisplay>(), buffTime);
                    }
                }
            }
        }
    }
}