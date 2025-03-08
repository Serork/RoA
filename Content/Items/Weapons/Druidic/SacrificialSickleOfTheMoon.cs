using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic;

[AutoloadGlowMask]
sealed class SacrificialSickleOfTheMoon : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Sacrificial Sickle Of The Moon");
		//Tooltip.SetDefault("Changes attacks depending on current moon phase");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override Color? GetAlpha(Color lightColor) {
		return base.GetAlpha(lightColor);
	}

    protected override void SafeSetDefaults() {
		int width = 40; int height = width;
		Item.Size = new Vector2(width, height);

		Item.useStyle = ItemUseStyleID.Shoot;
		Item.useTime = Item.useAnimation = 20;
		Item.autoReuse = false;

		Item.useTurn = false;
		Item.noMelee = true;

		Item.damage = 16;
		Item.knockBack = 4f;

		Item.rare = ItemRarityID.Orange;
		Item.UseSound = SoundID.Item71;

		Item.shootSpeed = 5f;
		Item.shoot = ProjectileID.WoodenArrowFriendly;

        Item.value = Item.sellPrice(0, 1, 50, 0);

        Item.staff[Item.type] = true;

		NatureWeaponHandler.SetPotentialDamage(Item, 38);
		NatureWeaponHandler.SetFillingRate(Item, 0.5f);
	}

    public override void MeleeEffects(Player player, Rectangle hitbox) {
		if (Main.moonPhase == 2 || Main.moonPhase == 3 || Main.moonPhase == 6 || Main.moonPhase == 7)
		if (Main.rand.NextBool(3)) {
			int num7 = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.AncientLight, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
			Main.dust[num7].noGravity = true;
		}
    }

    public override bool CanUseItem(Player player) {
		if (Main.moonPhase == 0 || Main.moonPhase == 1) {
			Item.useStyle = ItemUseStyleID.Shoot;
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonSigil>()] == 1) Item.UseSound = SoundID.Item1;
		}
		if (Main.moonPhase == 4 || Main.moonPhase == 5) {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item71;
			if (player.ownedProjectileCounts[ModContent.ProjectileType<SacrificialSickle>()] == 1) return false;
		}
		if (Main.moonPhase == 2 || Main.moonPhase == 3 || Main.moonPhase == 6 || Main.moonPhase == 7) {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item71;
		}
		if (Main.moonPhase == 4 || Main.moonPhase == 5) {
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}
		else {
			Item.noMelee = false;
			Item.noUseGraphic = false;
		}
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		if (Main.moonPhase == 0 || Main.moonPhase == 1) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonSigil>()] < 1)
				Projectile.NewProjectile(source, player.GetViableMousePosition(), Vector2.Zero, ModContent.ProjectileType<MoonSigil>(), damage, knockback, player.whoAmI);
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonSigil>()] == 1) return false;
		}
		if (Main.moonPhase == 2 || Main.moonPhase == 3 || Main.moonPhase == 6 || Main.moonPhase == 7) {
			Projectile.NewProjectile(source, new Vector2(player.position.X, player.position.Y + player.height / 2), new Vector2(velocity.X, velocity.Y), ModContent.ProjectileType<MoonSickle>(), damage, knockback, player.whoAmI);
		}
		if (Main.moonPhase == 4 || Main.moonPhase == 5)
			Projectile.NewProjectile(source, new Vector2(player.position.X, player.position.Y + player.height / 2), new Vector2(velocity.X, velocity.Y), ModContent.ProjectileType<SacrificialSickle>(), damage, knockback, player.whoAmI);
		return false;
	}

	public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
		position += newVelocity * 50;
		position += new Vector2(-newVelocity.Y, newVelocity.X) * (-5f * player.direction);
	}
}

