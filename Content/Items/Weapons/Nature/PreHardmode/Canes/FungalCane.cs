using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class FungalCane : CaneBaseItem<FungalCane.FungalCaneBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<FungalCaneMushroom>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 40);
        Item.SetUsableValues(-1, 40, useSound: SoundID.Item7);
        Item.SetWeaponValues(12, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 30);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.15f);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 1, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class FungalCaneBase : CaneBaseProjectile {
        private bool[] _smallShroomSpawned = null!;

        protected override void Initialize() {
            _smallShroomSpawned = new bool[5];
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.3f, -0.2f);

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            float distance = 790f + 20f * 1f;

            float num1595 = Main.rand.NextFloat() * ((float)Math.PI * 2f);
            float num1596 = 10;
            if (AttackProgress01 >= 0.8f) {
                num1596 *= 2f;
                distance *= 0.9625f;
            }
            for (int num1597 = 0; (float)num1597 < num1596; num1597++) {
                if (Main.rand.NextBool(2)) {
                    continue;
                }
                float num1598 = (float)num1597 / num1596 * ((float)Math.PI * 2f);
                float num1599 = Main.rand.NextFloat();
                Vector2 vector313 = corePosition + num1598.ToRotationVector2() * (815f - distance);
                Vector2 vector314 = (num1598 - (float)Math.PI).ToRotationVector2() * (14f + 5f * (distance / 800f) + 8f * num1599);
                Dust dust27 = Dust.NewDustPerfect(vector313, ModContent.DustType<Dusts.GlowingMushroom>(), vector314);
                dust27.scale = 0.9f;
                dust27.fadeIn = 1.15f + num1599 * 0.3f;
                dust27.noGravity = true;
                dust27.noLight = true;
                dust27.noLightEmittence = true;
                dust27.scale *= 0.75f;
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (Projectile.localAI[0] < 1f) {
                return;
            }

            Projectile.localAI[0] = 0f;

            float distance = 790f + 15f * step;

            float num1595 = Main.rand.NextFloat() * ((float)Math.PI * 2f);
            float num1596 = 10;
            for (int num1597 = 0; (float)num1597 < num1596; num1597++) {
                if (Main.rand.NextBool(3)) {
                    continue;
                }
                float num1598 = (float)num1597 / num1596 * ((float)Math.PI * 2f);
                float num1599 = Main.rand.NextFloat();
                Vector2 vector313 = corePosition + num1598.ToRotationVector2() * (815f - distance);
                Vector2 vector314 = (num1598 - (float)Math.PI).ToRotationVector2() * (14f + 5f * (distance / 800f) + 8f * num1599);
                Dust dust27 = Dust.NewDustPerfect(vector313, ModContent.DustType<Dusts.GlowingMushroom>(), vector314);
                dust27.scale = 0.9f;
                dust27.fadeIn = 1.15f + num1599 * 0.3f;
                dust27.noGravity = true;
                dust27.noLight = true;
                dust27.noLightEmittence = true;
                dust27.scale *= 0.75f;
            }
        }

        protected override void AfterProcessingCane() {
            float num11 = (float)Main.rand.Next(28, 42) * 0.005f;
            num11 += (float)(270 - Main.mouseTextColor) / 1000f;
            float R = 0f;
            float G = 0.2f + num11 / 2f;
            float B = 1f;
            Lighting.AddLight(CorePosition, new Vector3(R, G, B) * Projectile.Opacity * 0.875f);

            int index = 0;
            for (float i = 0f; i < 1f;) {
                i += 0.2f;
                if (AttackProgress01 > i && !_smallShroomSpawned[index]) {
                    _smallShroomSpawned[index] = true;
                    Projectile.localAI[0] = 1f;

                    SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 1.75f, Volume = 0.5f }, CorePosition);

                    if (Owner.IsLocal()) {
                        ProjectileUtils.SpawnPlayerOwnedProjectile<FungalCaneSmallShroom>(new ProjectileUtils.SpawnProjectileArgs(Owner, Projectile.GetSource_FromThis()) {
                            Position = CorePosition,
                            Damage = Projectile.damage,
                            KnockBack = Projectile.knockBack
                        });
                    }
                }
                index++;
            }
        }
    }
}
