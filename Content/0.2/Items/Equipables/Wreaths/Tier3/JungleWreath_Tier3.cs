using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths.Tier3;

sealed class JungleWreathTier3 : WreathItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(width: 30, height: 28);

        DefaultsToTier3Wreath();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        DruidStats.ApplyUpTo10ReducedDamageTaken(player);

        //DruidStats.InflictVenomOnAttackersAndDamageThemWhenCharged(player);

        DruidStats.InflictPoisonOnNatureDamageWhenCharged(player);

        TriggerThornsAttackOnHurtWhenCharged(player);
    }

    private void TriggerThornsAttackOnHurtWhenCharged(Player player) {
        if (!WreathHandler.IsWreathCharged(player)) {
            return;
        }

        player.GetModPlayer<TriggerThornsAttackOnHurtHandler>().IsEffectActive = true;
    }

    private class TriggerThornsAttackOnHurtHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void OnHurt(Player.HurtInfo info) {
            if (!IsEffectActive) {
                return;
            }

            GrowThorns(info);
        }

        private void GrowThorns(Player.HurtInfo info) {
            Vector2 thornsSpawnPosition = Player.Center;
            float lostHPProcentValue = info.Damage / (float)Player.statLifeMax2;
            NPC? target = Helper.FindClosestNPC(thornsSpawnPosition, 300, false);
            bool foundTarget = target != null;
            Vector2 thornsVelocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
            if (foundTarget) {
                thornsVelocity = target!.Center.DirectionTo(thornsSpawnPosition);
            }
            Vector2 startOffset = thornsVelocity.SafeNormalize() * 10f;
            thornsSpawnPosition += startOffset;

            //int damage = info.Damage;
            //damage += (int)(info.Damage * lostHPProcentValue);
            //float knockBack = info.Knockback;
            //knockBack += knockBack * lostHPProcentValue;
            int damage = 75;
            if (Main.masterMode) {
                damage *= 3;
            }
            else if (Main.expertMode) {
                damage *= 2;
            }
            float knockBack = 5f;

            ProjectileHelper.SpawnPlayerOwnedProjectile<Thorns>(new ProjectileHelper.SpawnProjectileArgs(Player, Player.GetSource_OnHurt(info.DamageSource)) with {
                Position = thornsSpawnPosition,
                Velocity = thornsVelocity,
                Damage = damage,
                KnockBack = knockBack,
                AI2 = lostHPProcentValue
            });
        }
    }
}
