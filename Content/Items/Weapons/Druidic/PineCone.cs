using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

using RoA.Core;
using RoA.Common.Druid;
using Terraria.ModLoader;
using RoA.Utilities;
using RoA.Core.Utility;
using System;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class PineCone : NatureItem {
    protected override void SafeSetDefaults() {
		Item.SetSize(18, 28);
		Item.SetWeaponValues(2, 0.25f);
		Item.SetDefaultToUsable(ItemUseStyleID.Swing, 35, false, useSound: SoundID.Item1);
		Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<Projectiles.Friendly.Druidic.PineCone>(), 0f);
		Item.SetDefaultOthers(Item.sellPrice(silver: 10), ItemRarityID.White);

		NatureWeaponHandler.SetPotentialDamage(Item, 10);
        NatureWeaponHandler.SetFillingRate(Item, 0.8f);
    }

	public override bool CanUseItem(Player player) {
		if (player.IsLocal()) {
			if (Collision.CanHitLine(player.Center, 2, 2, player.GetViableMousePosition(), 2, 2)) {
				return player.ownedProjectileCounts[Item.shoot] <= 2;
			}
		}

		return false;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		Projectile.NewProjectile(source, Vector2.Zero, Vector2.Zero, type, damage, knockback, player.whoAmI);

		return false;
	}
}