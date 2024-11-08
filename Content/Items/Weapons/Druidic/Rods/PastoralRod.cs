using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class PastoralRod : BaseRodItem<PastoralRod.PastoralRodBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<ShepherdLeaves>();

    protected override ushort GetUseTime(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2 - NatureWeaponHandler.GetUseSpeed(Item, player) / 3);

    protected override void SafeSetDefaults() {
        Item.SetSize(38);
        Item.SetDefaultToUsable(-1, 35, useSound: SoundID.Item7);
        Item.SetWeaponValues(8, 1f);

        //NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.45f);
        NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class PastoralRodBase : BaseRodProjectile {
        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            count = 3;
            Vector2 pointPosition = player.GetViableMousePosition();
            Vector2 direction = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One);
            velocity = direction.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver2) + MathHelper.Pi) * 4f * NatureWeaponHandler.GetUseSpeedMultiplier(player.GetSelectedItem(), player);
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            for (int i = 0; i < 2; i++) {
                Vector2 randomPosition = Main.rand.NextVector2Unit();
                float areaSize = step * 2f;
                float speed = step;
                float scale = Math.Clamp(step, 0.1f, 0.8f);
                Dust dust = Dust.NewDustPerfect(corePosition + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                                DustID.AmberBolt,
                                                randomPosition.RotatedBy(player.direction * -MathHelper.PiOver2) * speed,
                                                Scale: scale);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
        }
    }
}