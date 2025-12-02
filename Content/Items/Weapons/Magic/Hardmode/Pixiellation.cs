using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Common.Recipes;
using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic.Hardmode;

sealed class Pixiellation : ModItem, IRecipeDuplicatorItem {
    ushort[] IRecipeDuplicatorItem.SourceItemTypes => [(ushort)ItemID.CrystalVileShard];

    public override Color? GetAlpha(Color lightColor) {
        int num5 = lightColor.A;
        int num2 = (int)((double)(int)lightColor.R * 1.5);
        int num3 = (int)((double)(int)lightColor.G * 1.5);
        int num4 = (int)((double)(int)lightColor.B * 1.5);
        if (num2 > 255)
            num2 = 255;

        if (num3 > 255)
            num3 = 255;

        if (num4 > 255)
            num4 = 255;

        if (num5 < 0)
            num5 = 0;

        if (num5 > 255)
            num5 = 255;

        return new Color(num2, num3, num4, num5) * (1f - (Item.alpha / 255f));
    }

    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(44);
        Item.SetWeaponValues(40, 5f, damageClass: DamageClass.Magic);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 16, autoReuse: true, useSound: new SoundStyle(ResourceManager.ItemSounds + "Pixiellation") with { Pitch = 0f });
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Pixie>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        Item.mana = 10;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float collisionCheckSize = Item.width * 1.4f;
        Vector2 collisionCheckPosition = position + Vector2.Normalize(velocity) * collisionCheckSize;
        //if (!Collision.CanHit(player.Center, 0, 0, collisionCheckPosition, 0, 0)) {
        //    return false;
        //}

        Vector2 shootVelocityNormalized = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        float itemRotation = shootVelocityNormalized.ToRotation();
        Vector2 itemSizeOffset = shootVelocityNormalized * Item.width;
        position += itemSizeOffset;
        velocity = position.DirectionTo(player.GetWorldMousePosition());

        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);

        return false;
    }
}
