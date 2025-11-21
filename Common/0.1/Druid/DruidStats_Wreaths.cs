using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Content;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class DruidStats : ModPlayer {
    public static DruidStats GetDruidStats(Player player) => player.GetModPlayer<DruidStats>();

    private bool _shouldInflictVenomOnAttackers;
    private bool _shouldInflictPoisonOnNatureDamage;

    private void ResetEquippableWreathStats() {
        _shouldInflictVenomOnAttackers = false;
        _shouldInflictPoisonOnNatureDamage = false;
    }

    public static void ApplyUpTo10ReducedDamageTaken(Player target) {
        ReduceDamageTakenByValueBasedOnCharge(target, 0.1f);
    }

    public static void ApplyUpTo5ReducedDamageTaken(Player target) {
        ReduceDamageTakenByValueBasedOnCharge(target, 0.05f);
    }

    private static void ReduceDamageTakenByValueBasedOnCharge(Player target, float value) {
        float reducedDamageTaken = value * WreathHandler.GetWreathChargeProgress(target);
        target.endurance += reducedDamageTaken;
    }

    public static void Apply15MovementSpeedWhenCharged(Player target) {
        if (!WreathHandler.IsWreathCharged(target)) {
            return;
        }

        target.moveSpeed += 0.15f;
    }

    public static void Apply40MaximumLifeWhenCharged(Player target) {
        if (!WreathHandler.IsWreathCharged(target)) {
            return;
        }

        target.statLifeMax2 += 40;
    }

    public static void InflictVenomOnAttackersAndDamageThemWhenCharged(Player target) {
        if (!WreathHandler.IsWreathCharged(target)) {
            return;
        }

        GetDruidStats(target)._shouldInflictVenomOnAttackers = true;
    }

    public static void InflictPoisonOnNatureDamageWhenCharged(Player target) {
        if (!WreathHandler.IsWreathCharged(target)) {
            return;
        }

        GetDruidStats(target)._shouldInflictPoisonOnNatureDamage = true;
    }

    public static void ApplyVenomToAttackerAndDamageIt(Player player, NPC target, Player.HurtInfo hurtInfo) {
        if (!GetDruidStats(player)._shouldInflictVenomOnAttackers) {
            return;
        }

        int damage = 15;
        if (Main.masterMode)
            damage = 45;
        else if (Main.expertMode)
            damage = 30;
        damage *= 3;
        DamageAttacker(player, target, damage, hurtInfo, onDamage: (damageNPC) => {
            damageNPC.AddBuff(BuffID.Venom, 150, false);
        });
    }

    public static void ApplyPoisonOnNatureDamage(Player player, NPC target, Entity damageSource) {
        if (!GetDruidStats(player)._shouldInflictPoisonOnNatureDamage) {
            return;
        }

        bool isDamageSourceDruidic = (damageSource is Projectile projectileAsDamageSource && projectileAsDamageSource.IsNature()) || (damageSource is Item itemAsDamageSource && itemAsDamageSource.IsANatureWeapon());
        if (!isDamageSourceDruidic) {
            return;
        }

        int chance = 4;
        bool rolled = Main.rand.NextBool(chance);
        if (!rolled) {
            return;
        }

        int timeInTicks = 150;
        target.AddBuff(BuffID.Poisoned, timeInTicks);
    }

    public static void DamageAttacker(Player player, NPC target, int damage, Player.HurtInfo hurtInfo, float knockBackModifier = 1f, Action<NPC> onDamage = null) {
        int specialHitSetter = -1;
        switch (target.type) {
            case 396:
            case 397:
            case 398:
            case 400:
            case 401:
                specialHitSetter = 1;
                break;
            case 636:
                specialHitSetter = 1;
                if (target.ai[0] == 0f || target.ai[0] == 10f)
                    return;
                break;
        }

        if (!CombinedHooks.CanNPCHitPlayer(target, player, ref specialHitSetter))
            return;

        //float num2 = Math.Max(player.thorns, 0.5f);
        Rectangle rectangle = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
        Rectangle npcRect = new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height);
        bool num = player.CanParryAgainst(rectangle, npcRect, target.velocity);
        float knockback = 10f;
        //if (player.turtleThorns)
        //    num2 = 2f;

        if (num) {
            //num2 = 2f;
            knockback = 5f;
        }

        knockback *= knockBackModifier;

        float damageMultiplier = 1f;

        NPC.GetMeleeCollisionData(rectangle, target.whoAmI, ref specialHitSetter, ref damageMultiplier, ref npcRect);

        bool flag3 = !player.immune;
        if (specialHitSetter >= 0)
            flag3 = player.hurtCooldowns[specialHitSetter] == 0;

        if (player.whoAmI == Main.myPlayer && flag3 && !target.dontTakeDamage) {
            onDamage(target);

            player.ApplyDamageToNPC(target, damage, knockback, -hurtInfo.HitDirection, crit: false);
        }
    }

    public static void Apply8CritChanceWhenCharged(Player target) {
        if (!WreathHandler.IsWreathCharged(target)) {
            return;
        }

        target.GetCritChance(DruidClass.Nature) += 8;
    }
}
