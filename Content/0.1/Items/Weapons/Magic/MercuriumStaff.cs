using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Magic;

using System;

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
        Item.useTurn = false;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 26;
        Item.knockBack = 5f;

        Item.noMelee = true;
        Item.mana = 12;

        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item43;

        Item.shoot = ModContent.ProjectileType<QuicksilverBolt>();
        Item.shootSpeed = 6f;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 velocity2 = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += velocity2 * 34.5f;
        position += new Vector2(-velocity2.Y, velocity2.X) * (-16.875f * player.direction);
    }

    public override Vector2? HoldoutOffset() => new Vector2(1f, -7f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!Collision.CanHit(position, 0, 0, position + Vector2.Normalize(velocity) * 10f, 0, 0))
            return false;

        if (!Collision.CanHit(player.Center, 0, 0, position, 0, 0))
            return false;

        Vector2 dustPosition = position + new Vector2((player.direction == 1 ? 2f : 2f) * 2f, 4f * player.direction).RotatedBy(velocity.ToRotation());
        int k = Main.rand.Next(20, 26);
        for (int i = 0; i < k; i++) {
            int x = (int)((double)dustPosition.X - 3.0 + (double)2 / 2.0);
            int y = (int)((double)dustPosition.Y - 8.0 + (double)2 / 2.0);
            Vector2 vector3 = (new Vector2((float)2 / 2f, 2) * 0.8f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
            Vector2 vector2 = -(vector3 - new Vector2((float)x, (float)y));
            int dust2 = Dust.NewDust(vector3 + vector2 * 2f * Main.rand.NextFloat() - new Vector2(1f, 2f), 0, 0, ModContent.DustType<MercuriumDust>(), vector2.X * 2f, vector2.Y * 2f, 0, default(Color), Main.rand.NextFloat(2.5f, 3.3f));
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].scale *= 0.3f;
            Main.dust[dust2].velocity = -Vector2.Normalize(vector2) * Main.rand.NextFloat(1.5f, 3f) * Main.rand.NextFloat();
        }
        return true;
    }
}
