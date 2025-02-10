using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

sealed class FireLighter : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Fire Lighter");
        // Tooltip.SetDefault("Lights the fire deep in your heart...");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = 3;
        Item.useTime = Item.useAnimation = 14;
        Item.autoReuse = false;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 8;
        Item.knockBack = 4;

        Item.noMelee = true;
        Item.reuseDelay = 28;
        Item.mana = 10;

        Item.value = Item.sellPrice(silver: 40);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item17;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Magic.HarmonizingBeam>();
        Item.shootSpeed = 3f;
    }

    public override Vector2? HoldoutOffset()
        => new Vector2(2f, 0f);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        position += newVelocity * (player.direction == -1 ? 20f : 14f);
        position += new Vector2(0f, -10f * player.direction).RotatedBy(newVelocity.ToRotation());
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        for (int i = 0; i < 5; i++) {
            int dust = Dust.NewDust(position, 0, 0, DustID.PurificationPowder, 0, 0, 0, default, 1.2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
        }
        // пиздец....
        float speedX, speedY;
        speedX = velocity.X; speedY = velocity.Y;
        float slomatKoleni = 0.25f;
        float datLescha = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
        double mox = Math.Atan2(speedX, speedY);
        double pososi = mox + 0.4f * slomatKoleni;
        double unichtozhitHrebet = mox + 0f * slomatKoleni;
        double tsoy = mox - 0.4f * slomatKoleni;
        float davalka = Main.rand.NextFloat() * 0.2f + 0.95f;
        speedX = datLescha * davalka * (float)Math.Sin(pososi);
        speedY = datLescha * davalka * (float)Math.Cos(pososi);
        Projectile.NewProjectile(source, position.X, position.Y, datLescha * davalka * (float)Math.Sin(pososi), datLescha * davalka * (float)Math.Cos(pososi), type, damage, knockback, player.whoAmI, ai2: player.direction);
        Projectile.NewProjectile(source, position.X, position.Y, datLescha * davalka * (float)Math.Sin(tsoy), datLescha * davalka * (float)Math.Cos(tsoy), type, damage, knockback, player.whoAmI, ai2: player.direction);
        Projectile.NewProjectile(source, position.X, position.Y, datLescha * davalka * (float)Math.Sin(unichtozhitHrebet), datLescha * davalka * (float)Math.Cos(unichtozhitHrebet), type, damage, knockback, player.whoAmI, ai2: player.direction);
        return false;
    }
}