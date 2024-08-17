using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class CactiCaster : BaseRodItem<CactiCaster.CactiCasterBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<Cacti>();

    protected override void SafeSetDefaults() {
        Item.SetSize(38);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(8, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 26);
        NatureWeaponHandler.SetFillingRate(Item, 1f);
    }

    public sealed class CactiCasterBase : BaseRodProjectile {
        protected override float MinUseTimeToShootFactor() => 0.61f;

        protected override Vector2 CorePositionOffsetFactor() => new(0.1f, 0.1f);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override byte TimeAfterShootToExist(Player player) => (byte)(player.itemAnimationMax * 4);

        protected override bool DespawnWithProj() => true;

        protected override bool ShouldPlayShootSound() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) => ai1 = Projectile.whoAmI;

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            float offset = 10f;
            float reversed = 1f - step;
            if (step > 0f) {
                Vector2 spawnPosition = corePosition + (Vector2.UnitY * offset * reversed).RotatedBy(step * MathHelper.Pi * 5f * -player.direction);

                for (int i = 0; i < 4; i++) {
                    Dust dust = Dust.NewDustPerfect(spawnPosition,
                                                    ModContent.DustType<CactiCasterDust>(),
                                                    Vector2.Zero,
                                                    Scale: Main.rand.NextFloat(1.25f, 1.5f));
                    dust.noGravity = true;
                }
            }
        }
    }
}