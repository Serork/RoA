using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths.Tier3;

sealed class BeachWreathTier3 : WreathItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(width: 30, height: 28);

        DefaultsToTier3Wreath();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        DruidStats.ApplyUpTo10ReducedDamageTaken(player);

        DruidStats.Apply15MovementSpeedWhenCharged(player);

        RandomlyGrowCoralsWhenCharged(player);
    }

    private void RandomlyGrowCoralsWhenCharged(Player player) {
        if (!WreathHandler.IsWreathCharged(player)) {
            return;
        }

        player.GetModPlayer<RandomlyGrowCoralsHandler>().IsEffectActive = true;
    }

    private class RandomlyGrowCoralsHandler : ModPlayer {
        private static byte TIMETOMAKEACORAL => 100;

        private float _makeCoralTimer = TIMETOMAKEACORAL;

        public bool IsEffectActive;

        public override void OnRespawn() {
            if (!Player.IsLocal()) {
                return;
            }

            _makeCoralTimer = TIMETOMAKEACORAL;
        }

        public override void ResetEffects() => IsEffectActive = false;

        public override void PreUpdateMovement() {
            if (!IsEffectActive) {
                return;
            }

            GrowCorals();
        }

        private void GrowCorals() {
            if (!Player.IsLocal()) {
                return;
            }

            if (++_makeCoralTimer < TIMETOMAKEACORAL) {
                return;
            }

            ushort coralType = (ushort)ModContent.ProjectileType<BeachWreath_Coral>();
            int activeCoralCountAllowed = 5;
            int activeCoralCount = Player.ownedProjectileCounts[coralType];
            bool enoughCoralsActive = activeCoralCount > activeCoralCountAllowed - 1;
            if (enoughCoralsActive) {
                return;
            }

            float maxDistance = 110f;
            IReadOnlyCollection<Vector2> allCoralsPositions = BeachWreath_Coral.AllCoralsPositions;
            Vector2 getRandomazedOffsetValue() => (Vector2.UnitY * maxDistance * Main.rand.NextFloat(0.5f, 1f)).RotatedByRandom(MathHelper.TwoPi);
            Vector2 spawnPositionOffset = getRandomazedOffsetValue();
            Vector2 getFinalDestinationPosition() => Player.Center + spawnPositionOffset;
            int maxAttempts = 100;
            while (maxAttempts-- > 0 && allCoralsPositions.Any(checkCoralPosition => Vector2.Distance(checkCoralPosition, spawnPositionOffset) < 60f) ||
                   WorldGen.SolidTile(getFinalDestinationPosition().ToTileCoordinates())) {
                spawnPositionOffset = getRandomazedOffsetValue();
            }
            Vector2 coralSpawnPosition = getFinalDestinationPosition();

            int damage = 75;
            if (Main.masterMode) {
                damage *= 3;
            }
            else if (Main.expertMode) {
                damage *= 2;
            }
            float knockBack = 3f;

            int denom = 3;
            damage /= denom;

            ProjectileUtils.SpawnPlayerOwnedProjectile<BeachWreath_Coral>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc(nameof(BeachWreathTier3))) with {
                Position = coralSpawnPosition,
                AI1 = spawnPositionOffset.X,
                AI2 = spawnPositionOffset.Y,
                Damage = damage,
                KnockBack = knockBack
            });

            _makeCoralTimer = 0f;
        }
    }
}
