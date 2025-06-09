using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths.Tier3;

sealed class ForestWreathTier3 : WreathItem {
    private const int TIMETOSPAWNASUNFLOWER = 300;

    protected override void SafeSetDefaults() {
        Item.SetSize(width: 30, height: 28);

        DefaultsToTier3Wreath();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        DruidStats.ApplyUpTo10ReducedDamageTaken(player);

        DruidStats.Apply40MaximumLifeWhenCharged(player);

        OccasionallyGrowSunflowerWhenCharged(player);
    }

    private void OccasionallyGrowSunflowerWhenCharged(Player player) {
        if (!WreathHandler.IsWreathCharged(player)) {
            return;
        }

        SunflowerSpawnTimerHandler timerHandler = player.GetModPlayer<SunflowerSpawnTimerHandler>();
        timerHandler.ShouldCount = true;

        if (!player.IsLocal()) {
            return;
        }

        if (!timerHandler.Counted) {
            return;
        }
        ProjectileHelper.SpawnPlayerOwnedProjectile<Sunflower>(new ProjectileHelper.SpawnProjectileArgs(player, player.GetSource_Accessory(Item)));
        timerHandler.Counted = false;
    }

    private class SunflowerSpawnTimerHandler : ModPlayer {
        public bool ShouldCount, Counted;
        public int Time = TIMETOSPAWNASUNFLOWER;

        public override void OnRespawn() {
            if (Player.IsLocal()) {
                if (!ShouldCount) {
                    return;
                }

                Time = TIMETOSPAWNASUNFLOWER;
            }
        }

        public override void ResetEffects() => ShouldCount = false;

        public override void PostUpdate() {
            if (!ShouldCount) {
                return;
            }

            if (!Counted && ++Time > TIMETOSPAWNASUNFLOWER) {
                Time = 0;
                Counted = true;
            }
        }
    }
}
