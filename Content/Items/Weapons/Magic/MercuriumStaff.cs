using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

sealed class MercuriumStaff : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 46; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 22;
        Item.autoReuse = true;
        Item.useTurn = true;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 26;
        Item.knockBack = 2.5f;

        Item.noMelee = true;
        Item.mana = 9;

        Item.value = Item.sellPrice(silver: 42);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item43;

        Item.shoot = ModContent.ProjectileType<QuicksilverBolt>();
        Item.shootSpeed = 6f;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 velocity2 = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += velocity2 * 34.5f;
        position += new Vector2(-velocity2.Y, velocity2.X) * (-16.875f * player.direction);
    }

    public override Vector2? HoldoutOffset() => new Vector2(1f, -7f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!Collision.CanHit(position, 0, 0, position + Vector2.Normalize(velocity) * Item.width, 0, 0))
            return false;
        return true;
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient(ModContent.ItemType<MercuriumNugget>(), 16)
    //		.AddTile(TileID.Anvils)
    //		.Register();
    //}
}
