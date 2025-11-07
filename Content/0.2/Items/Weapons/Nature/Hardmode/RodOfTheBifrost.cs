using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class RodOfTheBifrost : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 36);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.HiddenAnimation, 20, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice());
        Item.SetShootableValues();

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        player.itemLocation = player.Center + new Vector2(player.direction * 4f, 6f * player.gravDir);
        float maxRotation = -0.15f;
        player.itemRotation = player.DirectionTo(player.Center + new Vector2(player.direction * 100f, -10f)).ToRotation() + Helper.Wave(-maxRotation, maxRotation, 10f, player.whoAmI) * player.direction
            + MathHelper.Pi * (!player.FacedRight()).ToInt();

        CompositeArmStretchAmount compositeArmStretchAmount2 = CompositeArmStretchAmount.Full;
        float rotation = player.itemRotation * 0.375f * -player.direction;
        rotation += MathHelper.PiOver4 * 1f;
        rotation *= -player.direction;
        if (player.FacedRight()) {
            rotation += -0.1f;
        }
        rotation += -0.5f * player.direction;
        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount2, rotation);
        player.SetCompositeArmBack(enabled: true, CompositeArmStretchAmount.Full, rotation);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        //player.GetCommon().UseRodOfTheBifrostPattern();
        Projectile magicalBlock = ProjectileUtils.SpawnPlayerOwnedProjectile<MagicalBifrostBlock>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)) {
            Damage = damage,
            KnockBack = knockback,
            AI1 = player.miscCounterNormalized * 12f % 1f
        });

        return false;
    }
}
