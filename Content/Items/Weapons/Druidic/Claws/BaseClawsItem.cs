using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

[WeaponOverlay(WeaponType.Claws)]
abstract class BaseClawsItem : NatureItem {
    protected sealed override void SafeSetDefaults2() {
        Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<ClawsSlash>(), 1.2f);
    }

    public virtual void OnHit(Player player, float progress) { }

    protected abstract (Color, Color) SlashColors(Player player);

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

    public virtual void SafeOnUse(Player player, ClawsHandler clawsStats) { }

    protected virtual bool ShouldModifyShootStats => true;

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer && player.ItemAnimationJustStarted) {
            (Color, Color) slashColors = SlashColors(player);
            ClawsHandler clawsStats = player.GetModPlayer<ClawsHandler>();
            clawsStats.SetColors(slashColors.Item1, slashColors.Item2);

            SafeOnUse(player, clawsStats);
        }

        return base.UseItem(player);
    }

    public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        if (!ShouldModifyShootStats) {
            return;
        }

        Vector2 pointPosition = player.GetViableMousePosition();
        Vector2 point = Helper.VelocityToPoint(player.Center, pointPosition, 1f);
        position += point * 15f;
        velocity = point;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction/* * player.gravDir*/,
            NatureWeaponHandler.GetUseSpeedForClaws(Item, player));

        return false;
    }
}
