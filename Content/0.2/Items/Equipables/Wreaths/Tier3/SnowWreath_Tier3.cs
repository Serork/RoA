using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths.Tier3;

sealed class SnowWreathTier3 : WreathItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(width: 30, height: 28);

        DefaultsToTier3Wreath();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        DruidStats.ApplyUpTo10ReducedDamageTaken(player);

        DruidStats.Apply8CritChanceWhenCharged(player);

        SpawnCloudberriesOnNatureAttackWhenCharged(player);
    }

    private void SpawnCloudberriesOnNatureAttackWhenCharged(Player player) {
        if (!WreathHandler.IsWreathCharged(player)) {
            return;
        }

        player.GetModPlayer<SpawnCloudberriesOnNatureAttackHandler>().IsEffectActive = true;
    }

    private class SpawnCloudberriesOnNatureAttackHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostItemCheck() {
            if (!Player.IsLocal()) {
                return;
            }

            if (!IsEffectActive) {
                return;
            }

            ThrowCloudberries();
        }

        private void ThrowCloudberries() {
            if (!Player.ItemAnimationJustStarted) {
                return;
            }
            Item heldItem = Player.HeldItem;
            if (!heldItem.IsANatureWeapon()) {
                return;
            }

            int chance = 4;
            bool rolled = Main.rand.NextBool(chance);
            if (!rolled) {
                return;
            }

            int cloudberryCount = Main.rand.Next(3, 5);
            Vector2 cloudberrySpawnPosition = Player.Center;
            Vector2 mousePosition = Player.GetViableMousePosition();

            int damage = 75;
            if (Main.masterMode) {
                damage *= 3;
            }
            else if (Main.expertMode) {
                damage *= 2;
            }
            float knockBack = 5f;

            int denom = cloudberryCount - 1;
            damage /= denom;
            knockBack /= denom;

            for (int i = 0; i < cloudberryCount; i++) {
                Vector2 cloudberryVelocity = cloudberrySpawnPosition.DirectionTo(mousePosition).RotatedByRandom(MathHelper.PiOver4);
                cloudberryVelocity *= Main.rand.NextFloat(0.75f, 1f);
                cloudberryVelocity *= 1.5f;
                ProjectileHelper.SpawnPlayerOwnedProjectile<Cloudberry>(new ProjectileHelper.SpawnProjectileArgs(Player, Player.GetSource_ItemUse(heldItem)) with {
                    Position = cloudberrySpawnPosition,
                    Velocity = cloudberryVelocity,
                    Damage = damage,
                    KnockBack = knockBack
                });
            }
        }
    }
}
