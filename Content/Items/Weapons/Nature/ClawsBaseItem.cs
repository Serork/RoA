using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Items.Weapons.Nature.Hardmode.Claws;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Nature;

abstract class ClawsBaseItem<T> : ClawsBaseItem where T : ClawsSlash {
    protected sealed override void SafeSetDefaults3() {
        Item.SetShootableValues(SetClawsSlash<T>(), 1.2f);

        if (IsHardmodeClaws) {
            Item.useStyle = -1;
        }
    }
}

[WeaponOverlay(WeaponType.Claws)]
abstract class ClawsBaseItem : NatureItem {
    public virtual bool IsHardmodeClaws { get; } = false;

    public virtual float BrightnessModifier { get; } = 0f;
    public virtual bool HasLighting { get; } = false;
    public virtual float HitEffectOpacity { get; } = 1f;

    public virtual float FirstAttackSpeedModifier { get; } = 1f;
    public virtual float SecondAttackSpeedModifier { get; } = 1f;
    public virtual float ThirdAttackSpeedModifier { get; } = 1f;

    public virtual float FirstAttackScaleModifier { get; } = 1f;
    public virtual float SecondAttackScaleModifier { get; } = 1f;
    public virtual float ThirdAttackScaleModifier { get; } = 1f;

    protected virtual ushort SetClawsSlash<T>() where T : ClawsSlash => (ushort)ModContent.ProjectileType<T>();

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (!IsHardmodeClaws || player.ItemAnimationJustStarted || Item.useStyle == ItemUseStyleID.Shoot) {
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

        float frontRotation = rotation,
              backRotation = rotation;
        if (player.ownedProjectileCounts[Item.shoot] > 1) {
            front = true;
            back = true;
            backRotation = MathHelper.Pi - backRotation + -MathHelper.PiOver4 * player.direction;
        }

        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount, !front ? 0f : frontRotation);
        player.SetCompositeArmBack(enabled: true, compositeArmStretchAmount, !back ? 0f : backRotation);
    }

    protected virtual void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) { }

    protected sealed override void SafeSetDefaults2() {
        Item.SetShootableValues(SetClawsSlash<ClawsSlash>(), 1.2f);

        if (IsHardmodeClaws) {
            Item.useStyle = -1;
        }
    }

    public virtual void OnHit(Player player, float progress) { }

    protected abstract (Color, Color) SetSlashColors(Player player);

    public sealed override bool CanUseItem(Player player) => player.ownedProjectileCounts[SpawnClawsProjectileType(player) ?? Item.shoot] < 1;

    public virtual void SafeOnUse(Player player, ClawsHandler clawsStats) { }

    protected virtual bool ShouldModifyShootStats => true;

    public virtual bool ResetOnHit => false;

    public override bool? UseItem(Player player) {
        if (!ResetOnHit && player.GetWreathHandler().ShouldClawsReset()) {
            Item.shootSpeed = 1f;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        else {
            Item.shootSpeed = 0f;
            Item.useStyle = IsHardmodeClaws ? -1 : ItemUseStyleID.Swing;
        }

        if (player.whoAmI == Main.myPlayer && player.ItemAnimationJustStarted) {
            (Color, Color) slashColors = SetSlashColors(player);
            ClawsHandler clawsStats = player.GetModPlayer<ClawsHandler>();
            clawsStats.SetColors(slashColors.Item1, slashColors.Item2);

            var args = new ClawsHandler.AttackSpawnInfoArgs() {
                Owner = Item
            };
            SetSpecialAttackData(player, ref args);
            clawsStats.SetSpecialAttackData(args);

            if (IsHardmodeClaws) {
                player.GetClawsHandler().AttackCount++;
            }
            else {
                player.GetClawsHandler().AttackCount = 0;
            }

            SafeOnUse(player, clawsStats);

            if (!ResetOnHit) {
                player.GetWreathHandler().TryToClawsReset(Item, false);
            }
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

    protected virtual ushort? SpawnClawsProjectileType(Player player) => null;

    public override float UseSpeedMultiplier(Player player) {
        if (!ResetOnHit && player.GetWreathHandler().ShouldClawsReset()) {
            ushort attackTime = NatureWeaponHandler.GetUseSpeedForClaws(Item, player);
            return NatureWeaponHandler.MAXCLAWSATTACKSPEED - (float)attackTime / Item.useTime;
        }

        return base.UseSpeedMultiplier(player);
    }

    public sealed override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!ResetOnHit && player.GetWreathHandler().ShouldClawsReset()) {
            return false;
        }

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
        SpawnClawsSlash(player, position, type, damage, knockback, attackTime);

        return false;
    }

    protected virtual void SpawnClawsSlash(Player player, Vector2 position, int type, int damage, float knockback, int attackTime) {
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), SpawnClawsProjectileType(player) ?? type, damage, knockback, player.whoAmI, player.direction/* * player.gravDir*/,
            attackTime);
    }
}
