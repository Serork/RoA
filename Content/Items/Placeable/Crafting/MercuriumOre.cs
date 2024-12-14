using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

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
	public override void PostUpdateBuffs() {
		int type = ModContent.ItemType<MercuriumOre>();
        if (Player.HasItem(type) || (Player.whoAmI == Main.myPlayer && Main.mouseItem.type == type)) Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);

		foreach (Item item in Main.ActiveItems) {
            if (item is null || !item.active || item.Distance(Player.Center) > 100f)
                continue;

			if (Collision.CanHit(Player, item)) {
                Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 2);
            }
        }
	}
}