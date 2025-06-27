using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class RipePumpkin : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 28);
        Item.SetWeaponValues(4, 6f);
        Item.SetDefaultsToUsable(ItemUseStyleID.Swing, 45, 20, false, useSound: SoundID.Item7);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.RipePumpkin>());
        Item.SetOtherValues(Item.sellPrice(silver: 20), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 24);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }

    public override bool CanUseItem(Player player) {
        if (player.IsLocal()) {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.IsLocal()) {
            Vector2 pointPosition = player.GetViableMousePosition();
            float speed = (pointPosition - position).Length() * 0.05f, minSpeed = 4f, maxSpeed = 6f;
            speed = Math.Clamp(speed, minSpeed, maxSpeed);
            Projectile.NewProjectile(source, position, Helper.VelocityToPoint(position, pointPosition, speed), type, damage, knockback, player.whoAmI, ai2: speed);
        }

        return false;
    }
}