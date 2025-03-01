using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Druidic;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class SoulOfTheWoods : NatureItem {
	public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 28; int height = 38;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.accessory = true;

        Item.value = Item.sellPrice(0, 2, 25, 0);
    }

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<DruidStats>().SoulOfTheWoods = true;

        int type = ModContent.ProjectileType<RootRing>();
        if (player.GetModPlayer<WreathHandler>().IsFull2 && player.ownedProjectileCounts[type] < 1) {
			Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, type, player.HeldItem.damage / 2, player.HeldItem.knockBack / 2, player.whoAmI);
        }
    }
}