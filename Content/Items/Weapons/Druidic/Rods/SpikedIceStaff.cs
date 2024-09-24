using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Content.Projectiles.Friendly.Druidic;
using Terraria.ModLoader;
using RoA.Core.Utility;
using RoA.Utilities;
using RoA.Content.Dusts;
using System;
using Terraria.Audio;
using System.Drawing;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class SpikedIceStaff : BaseRodItem<SpikedIceStaff.SpikedIceStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<SharpIcicle>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36);
        Item.SetDefaultToUsable(ItemUseStyleID.Shoot, 28, useSound: SoundID.Item17);
        Item.SetWeaponValues(6, 4f);
        Item.SetDefaultOthers(Item.sellPrice(silver: 15), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 18);
        NatureWeaponHandler.SetFillingRate(Item, 0.65f);

        Item.staff[Type] = true;
    }

    public sealed class SpikedIceStaffBase : BaseRodProjectile {
        private const byte MAXSHOOTCOUNT = 3;

        private bool _shouldShoot;
        private bool _stopCounting;
        private byte _shootCount = MAXSHOOTCOUNT;

        private int Min => (int)(Owner.itemTimeMax * 0.9f);
        private int PerShoot => (int)(Owner.itemTimeMax * 0.4f);
        private bool MinPassed => Projectile.localAI[0] >= Min;

        protected override bool IsInUse => !Owner.CCed && (Owner.controlUseItem || !MinPassed);

        protected override Vector2 CorePositionOffsetFactor() => new(0.05f, 0.08f);

        protected override bool DespawnWithProj() => false;

        protected override bool ShouldShoot() => base.ShouldShoot() || Projectile.localAI[0] >= Min + PerShoot * MAXSHOOTCOUNT;

        protected override bool ShouldPlayShootSound() => false;

        public override void PostAI() {
            if (_shouldShoot) {
                if (Projectile.localAI[0] >= Min - PerShoot && _shootCount > 0) {
                    if (Projectile.localAI[0] % PerShoot == 0f) {
                        ShootProjectile();
                        _shootCount--;

                        SoundEngine.PlaySound(SoundID.Item20, CorePosition);

                        if (Main.netMode != NetmodeID.Server) {
                            void spawnAttackDust(float num) {
                                if (Main.rand.NextChance(0.5f)) {
                                    Vector2 velocity = Helper.VelocityToPoint(CorePosition, Owner.GetViableMousePosition(), 3f + Main.rand.NextFloatRange(0.5f));
                                    Vector2 vector2 = velocity.RotatedBy(num * (MathHelper.Pi + MathHelper.PiOver4) / 25f);
                                    Dust dust = Dust.NewDustDirect(CorePosition, 5, 5, DustID.BubbleBurst_Blue, Scale: Main.rand.NextFloat(1.05f, 1.35f));
                                    dust.velocity = vector2;
                                    dust.noGravity = true;
                                }
                            }
                            float min = 8f, max = 8f;
                            if (Owner.direction == 1) {
                                for (float num = -min; num < max; num += 1f) {
                                    spawnAttackDust(num);
                                }
                            }
                            else {
                                for (float num = -max; num < min; num += 1f) {
                                    spawnAttackDust(num);
                                }
                            }
                        }
                    }
                    Projectile.localAI[0]--;
                }
            }
            else {
                if (!_stopCounting) {
                    Projectile.localAI[0]++;
                    if (!IsInUse) {
                        _stopCounting = true;
                    }
                }
            }
        }

        protected override void ShootProjectile() {
            if (!Collision.CanHit(Owner.Center, 0, 0, CorePosition, 0, 0)) {
                return;
            }

            if (MinPassed && Projectile.localAI[0] < Min + PerShoot) {
                base.ShootProjectile();

                SoundEngine.PlaySound(SoundID.Item20, CorePosition);

                return;
            }
            if (!_shouldShoot) {
                _shouldShoot = true;

                return;
            }


            base.ShootProjectile();
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            if (player.whoAmI == Main.myPlayer) {
                Vector2 pointPosition = player.GetViableMousePosition();
                Vector2 center = player.Center;
                float speed = MathHelper.Clamp((pointPosition - center).Length() * 0.025f, 8.5f, 11f);
                velocity = Helper.VelocityToPoint(center, pointPosition, speed);
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (Main.rand.NextChance(step)) {
                for (int i = 0; i < (int)(2 * step) + 1; i++) {
                    Dust dust = Dust.NewDustPerfect(corePosition, 176, Scale: 1f);
                    dust.noGravity = true;
                }
            }
        }
    }
}