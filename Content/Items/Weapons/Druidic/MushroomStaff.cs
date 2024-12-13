using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using RoA.Core.Utility;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class MushroomStaff : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(38);
        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 35, useSound: SoundID.Item156);
        Item.SetWeaponValues(4, 2f);
        Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<MushroomSpore>(), 4f);
        Item.SetDefaultOthers(Item.sellPrice(silver: 10), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.5f);
    }

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		int count = 3;
		for (int i = 0; i < count; i++) {
            Vector2 newVelocity = new Vector2(velocity.X, velocity.Y).RotatedByRandom(MathHelper.ToRadians(30)) * 1.6f;
			Projectile.NewProjectile(source, position.X + Main.rand.NextFloatRange(0.05f), position.Y, newVelocity.X, newVelocity.Y, type, damage, knockback, player.whoAmI);
		}

        return false;
	}

	public override Vector2? HoldoutOffset() => new Vector2(-18f, -4f);
}