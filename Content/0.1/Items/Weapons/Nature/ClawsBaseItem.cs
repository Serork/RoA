using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Nature;

[WeaponOverlay(WeaponType.Claws)]
abstract class ClawsBaseItem : NatureItem {
    public virtual bool IsHardmodeClaws { get; } = false;

    public virtual float FirstAttackSpeedModifier { get; } = 1f;
    public virtual float SecondAttackSpeedModifier { get; } = 1f;
    public virtual float ThirdAttackSpeedModifier { get; } = 1f;

    public virtual float FirstAttackScaleModifier { get; } = 1f;
    public virtual float SecondAttackScaleModifier { get; } = 1f;
    public virtual float ThirdAttackScaleModifier { get; } = 1f;

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (!IsHardmodeClaws || player.ItemAnimationJustStarted) {
            return;
        }

        CompositeArmStretchAmount compositeArmStretchAmount = CompositeArmStretchAmount.Full;
        float attackProgress = player.itemAnimation / (float)player.itemAnimationMax;
        float rotation;
        if (attackProgress >= 0.75f) {
            rotation = MathHelper.Pi;
        }
        else if (attackProgress >= 0.5f) {
            rotation = MathHelper.Pi + MathHelper.PiOver2 / 2f;
        }
        else if (attackProgress >= 0.25f) {
            rotation = MathHelper.Pi + MathHelper.PiOver2;
        }
        else {
            rotation = -MathHelper.PiOver4;
        }
        rotation *= player.direction;
        bool front = false,
             back = false;
        switch (player.GetClawsHandler().AttackType) {
            case ClawsHandler.ClawsAttackType.Front:
                front = true;
                break;
            case ClawsHandler.ClawsAttackType.Back:
                back = true;
                break;
            case ClawsHandler.ClawsAttackType.Both:
                front = back = true;
                break;
        }
        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount, !front ? 0f : rotation);
        player.SetCompositeArmBack(enabled: true, compositeArmStretchAmount, !back ? 0f : rotation);
    }

    protected sealed override void SafeSetDefaults2() {
        Item.SetShootableValues((ushort)ModContent.ProjectileType<ClawsSlash>(), 1.2f);

        if (IsHardmodeClaws) {
            Item.useStyle = -1;
        }
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

            if (IsHardmodeClaws) {
                player.GetClawsHandler().AttackCount++;
            }
            else {
                player.GetClawsHandler().AttackCount = 0;
            }

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
        ushort attackTime = NatureWeaponHandler.GetUseSpeedForClaws(Item, player);
        ClawsHandler.ClawsAttackType clawsAttackType = player.GetClawsHandler().AttackType;
        switch (clawsAttackType) {
            case ClawsHandler.ClawsAttackType.Back:
                attackTime = (ushort)(attackTime * FirstAttackSpeedModifier);
                break;
            case ClawsHandler.ClawsAttackType.Front:
                attackTime = (ushort)(attackTime * SecondAttackSpeedModifier);
                break;
            case ClawsHandler.ClawsAttackType.Both:
                attackTime = (ushort)(attackTime * ThirdAttackSpeedModifier);
                break;
        }
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction/* * player.gravDir*/,
            attackTime);

        return false;
    }
}
