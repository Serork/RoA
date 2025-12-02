using Microsoft.Xna.Framework;

using Mono.Cecil;

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

sealed class ToothFairy : ModItem, IRecipeDuplicatorItem {
    ushort[] IRecipeDuplicatorItem.SourceItemTypes => [(ushort)ItemID.ClingerStaff];

    public override void SetDefaults() {
        Item.SetSizeValues(56, 30);
        Item.SetWeaponValues(18, 5f, damageClass: DamageClass.Magic);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 8, autoReuse: true, useSound: new SoundStyle(ResourceManager.ItemSounds + "SkullCrusher") with { Pitch = 0f });
        Item.SetShootableValues((ushort)ModContent.ProjectileType<CorruptorBone>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        Item.mana = 2;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-4f, 0f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float modifier = 1f;
        float collisionCheckSize = Item.width * modifier * 0.9f;
        Vector2 collisionCheckPosition = position + Vector2.Normalize(velocity) * collisionCheckSize;
        if (!Collision.CanHit(player.Center, 0, 0, collisionCheckPosition, 0, 0)) {
            return false;
        }

        Vector2 shootVelocityNormalized = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        Vector2 itemSizeOffset = shootVelocityNormalized * Item.width * (modifier - 0.4f);
        position += itemSizeOffset;

        position += new Vector2(player.direction < 0 ? 7f : -2f, -8f * player.direction * player.gravDir + (player.gravDir == -1f ? -8f * player.direction : 0f)).RotatedBy(shootVelocityNormalized.ToRotation());

        velocity = position.DirectionTo(player.GetWorldMousePosition());

        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);

        position += new Vector2(-17.5f + (player.direction < 0 ? 0f : 9f), player.gravDir == -1f ? 4f * player.direction : 0f).RotatedBy(shootVelocityNormalized.ToRotation());

        Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(10f), ModContent.DustType<Dusts.Corruptor>(), Vector2.Zero);
        dust.velocity.X *= Main.rand.NextFloat(1.5f);
        dust.velocity.Y *= Main.rand.NextFloat(0.5f, 1f);
        dust.scale *= Main.rand.NextFloat(0.5f, 0.75f);
        dust.scale *= 1.25f;
        dust.customData = true;
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

        return false;
    }
}
