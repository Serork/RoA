using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class PerfectedBouquet : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 36);
        Item.SetWeaponValues(36, 2f, 6);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 25, autoReuse: true, useSound: SoundID.Item65);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<TulipPetalSoul>(), 8f);
        Item.SetShopValues(ItemRarityColor.Lime7, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);

        Item.staff[Type] = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float collisionCheckSize = Item.width * 1.4f;
        Vector2 collisionCheckPosition = position + Vector2.Normalize(velocity) * collisionCheckSize;
        if (!Collision.CanHit(player.Center, 0, 0, collisionCheckPosition, 0, 0)) {
            return false;
        }

        TulipPetalSoul.PetalType petalType = (TulipPetalSoul.PetalType)Main.rand.Next(3);
        Vector2 shootVelocityNormalized = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        float itemRotation = shootVelocityNormalized.ToRotation();
        Vector2 itemSizeOffset = shootVelocityNormalized * Item.width;
        position += itemSizeOffset;
        Vector2 petalOffset;
        if (petalType == TulipPetalSoul.PetalType.SkeletronPrime) {
            petalOffset = new Vector2(10f, -5f * player.direction).RotatedBy(itemRotation);
        }
        else if (petalType == TulipPetalSoul.PetalType.Destroyer) {
            petalOffset = new Vector2(4f, -12f * player.direction).RotatedBy(itemRotation);
        }
        else {
            petalOffset = new Vector2(9f, 3f * player.direction).RotatedBy(itemRotation);
        }
        petalOffset -= new Vector2(2f, 2f * player.direction).RotatedBy(itemRotation);
        position += petalOffset;
        velocity = position.DirectionTo(player.GetMousePosition());
        int whoAmI = ProjectileUtils.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, beforeNetSend: (projectile) => {
            _ = new TulipPetalSoul.TulipPetalSoulValues(projectile) {
                CurrentType = petalType
            };
        }, centered: true);

        return false;
    }

    public override Vector2? HoldoutOffset() => new(-14, 10);
}
