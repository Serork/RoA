using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Druidic;

using RoA.Content.Items.Weapons.Melee;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class SoulOfTheWoods : NatureItem {
	public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;		
		ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BloodshedAxe>();
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
        int damage = (int)player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(40);
        int knockback = (int)player.GetTotalKnockback(DruidClass.NatureDamage).ApplyTo(0f);
        if (player.GetModPlayer<WreathHandler>().IsFull2 && player.ownedProjectileCounts[type] < 1) {
			Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, type,
               damage, knockback, player.whoAmI);
        }
    }
}