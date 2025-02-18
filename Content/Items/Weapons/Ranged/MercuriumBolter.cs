using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

sealed class MercuriumBolter : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 34; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.damage = 8;
        Item.knockBack = 0f;
        Item.DamageType = DamageClass.Ranged;

        Item.useTime = Item.useAnimation = 35;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.value = Item.sellPrice(silver: 42);
        Item.rare = ItemRarityID.Blue;

        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.shootSpeed = 5f;
        Item.useAmmo = AmmoID.Arrow;

        Item.UseSound = SoundID.Item61;
        Item.autoReuse = false;
        Item.noMelee = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        int projectilesCount = Main.rand.Next(3, 5);
        for (int i = 1; i < projectilesCount; i++) {
            Vector2 perturbedSpeed = new Vector2(velocity.X, velocity.Y).RotatedByRandom(MathHelper.ToRadians(15));
            int bolt = Projectile.NewProjectile(source, position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, type, damage, knockback * 0f, player.whoAmI);
            Main.projectile[bolt].scale = 0.75f;
            Main.projectile[bolt].extraUpdates = 1;
            Main.projectile[bolt].noDropItem = true;
            Main.projectile[bolt].netUpdate = true;
            Dust.NewDust(player.Center - Vector2.UnitX * 13f - Vector2.UnitY * 8f + velocity.SafeNormalize(Vector2.Zero) * 30f, 0, 0, ModContent.DustType<ToxicFumes>(), velocity.X * 0.5f, velocity.Y * 0.5f, 0, default(Color), 1.3f);
        }

        return false;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-1f, 0);

    //public override void AddRecipes() {
    //    CreateRecipe()
    //        .AddIngredient(ModContent.ItemType<MercuriumNugget>(), 14)
    //        .AddTile(TileID.Anvils)
    //        .Register();
    //}
}