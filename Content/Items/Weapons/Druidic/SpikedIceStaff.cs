using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core.Utility;
using RoA.Utilities;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class SpikedIceStaff : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(36);
        Item.SetDefaultToUsable(ItemUseStyleID.Shoot, 24, useSound: SoundID.Item17);
        Item.SetWeaponValues(6, 4f);
        Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<SharpIcicle>(), 0f);
        Item.SetDefaultOthers(Item.sellPrice(silver: 15), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 18);
        NatureWeaponHandler.SetFillingRate(Item, 0.65f);

        Item.staff[Type] = true;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        if (player.whoAmI == Main.myPlayer) {
            Vector2 pointPosition = player.GetViableMousePosition();
            Vector2 center = player.Center;
            float speed = MathHelper.Clamp((pointPosition - center).Length() * 0.02f, 7.5f, 11f);
            velocity = Helper.VelocityToPoint(center, pointPosition, speed);
        }
        Vector2 muzzleOffset = Vector2.Normalize(velocity) * Item.height;
        if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0)) {
            position += muzzleOffset;
        }
    }
}