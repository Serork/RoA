using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Hardmode;

sealed class NewMoney : ModItem {
    public static ushort DEBUFFTIMEPERHIT => 60 * 5;
    public static ushort DEBUFFTIMENEEDFORBUFRST => 60 * 20;

    public static ushort BURSTDAMAGE => 150;
    public static float BURSTKNOCKBACK => 0f;

    public override void SetDefaults() {
        Item.width = 40; Item.height = 36;

        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.useAnimation = 14; Item.useTime = Item.useAnimation;
        
        Item.useAmmo = AmmoID.Bullet;
        Item.UseSound = SoundID.Item11;

        Item.noMelee = true;

        //Item.value = Item.sellPrice(0, 7);
        Item.rare = ItemRarityID.Lime;

        Item.DamageType = DamageClass.Ranged;
        Item.damage = 30;
        Item.knockBack = 3.5f;

        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 10f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float num50 = velocity.X;
        float num51 = velocity.Y;
        num50 += (float)Main.rand.Next(-30, 31) * 0.015f;
        num51 += (float)Main.rand.Next(-30, 31) * 0.015f;
        if (type == ProjectileID.Bullet) {
            type = ModContent.ProjectileType<NewMoneyBullet>();
        }
        Projectile.NewProjectile(source, position.X, position.Y, num50, num51, type, damage, knockback, player.whoAmI);

        return false;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-8f, 4f);
}
