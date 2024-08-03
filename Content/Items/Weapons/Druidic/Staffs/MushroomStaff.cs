using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using RoA.Core.Utility;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Druidic;

namespace RoA.Content.Items.Weapons.Druidic.Staffs;

sealed class MushroomStaff : NatureItem {
    protected override void SafeSetDefaults() {
        int width = 38; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 30;
        Item.autoReuse = false;

        Item.noMelee = true;
        Item.knockBack = 2f;

        Item.damage = 4;

        NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.5f);

        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item156;

        Item.shootSpeed = 4f;
        Item.shoot = ModContent.ProjectileType<MushroomSpore>();

        Item.staff[Type] = true;
    }

	public override bool Shoot (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		int count = 3;
		for (int i = 0; i < count; i++) {
			Vector2 perturbedSpeed = new Vector2(velocity.X, velocity.Y).RotatedByRandom(MathHelper.ToRadians(30)) * 1.6f;
			Projectile.NewProjectile(source, position.X + Main.rand.NextFloatRange(0.05f), position.Y, perturbedSpeed.X, perturbedSpeed.Y, type, damage, knockback, player.whoAmI);
		}
		return false;
	}

	public override Vector2? HoldoutOffset() => new Vector2(-18f, -4f);
}