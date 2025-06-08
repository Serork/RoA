using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Rods;

sealed class SpikedIceStaff : BaseRodItem<SpikedIceStaff.SpikedIceStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<SharpIcicle>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36);
        Item.SetDefaultsToUsable(ItemUseStyleID.Shoot, 24, useSound: SoundID.Item7);
        Item.SetWeaponValues(6, 4f);
        Item.SetDefaultsOthers(Item.sellPrice(silver: 15), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 20);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.4f);

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 1, 0, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 22);

        Item.staff[Type] = true;
    }

    public sealed class SpikedIceStaffBase : BaseRodProjectile {
        private const byte MAXSHOOTCOUNT = 3;

        private bool _shouldShoot;
        private bool _stopCounting;
        private byte _shootCount = MAXSHOOTCOUNT;
        private float _attackTimer, _attackTime;
        private Vector2 _mousePos;

        private int Min => (int)(_attackTime * 0.4f);
        private int PerShoot => (int)(_attackTime * 0.4f);
        private bool MinPassed => _attackTimer >= Min;

        protected override bool IsInUse => !Owner.CCed && (Owner.controlUseItem || !MinPassed);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override Vector2 CorePositionOffsetFactor() => new(0.05f, 0.08f);

        protected override bool DespawnWithProj() => false;

        protected override bool ShouldShoot() => base.ShouldShoot() || _attackTimer >= Min + PerShoot * MAXSHOOTCOUNT;

        protected override bool ShouldPlayShootSound() => false;

        protected override bool ShouldntUpdateRotationAndDirection() => false;

        protected override byte TimeAfterShootToExist(Player player) {
            return (byte)(NatureWeaponHandler.GetUseSpeed(AttachedNatureWeapon, player) * 3 * Math.Min(1f, _attackTimer / _attackTime));
        }

        protected override void SafestOnSpawn(IEntitySource source) {
            _attackTime = NatureWeaponHandler.GetUseSpeed(Owner.GetSelectedItem(), Owner);
            Projectile.netUpdate = true;
        }

        protected override void SafeSendExtraAI(BinaryWriter writer) {
            base.SafeSendExtraAI(writer);

            writer.Write(_shootCount);
            writer.Write(_stopCounting);
            writer.Write(_attackTimer);
            writer.Write(_attackTime);
            writer.WriteVector2(_mousePos);
        }

        protected override void SafeReceiveExtraAI(BinaryReader reader) {
            base.SafeReceiveExtraAI(reader);

            _shootCount = reader.ReadByte();
            _stopCounting = reader.ReadBoolean();
            _attackTimer = reader.ReadSingle();
            _attackTime = reader.ReadSingle();
            _mousePos = reader.ReadVector2();
        }

        public override void SafePostAI() {
            //Main.NewText(CurrentUseTime);
            if (_shouldShoot) {
                if (_attackTimer >= Min - PerShoot && _shootCount > 0) {
                    if (_attackTimer % PerShoot == 0f) {
                        if (_shootCount == MAXSHOOTCOUNT) {
                            SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
                        }

                        ShootProjectile();
                        _shootCount--;
                        if (Owner.whoAmI == Main.myPlayer && _mousePos != Owner.GetViableMousePosition()) {
                            _mousePos = Owner.GetViableMousePosition();
                            Projectile.netUpdate = true;
                        }

                        SoundEngine.PlaySound(SoundID.Item20, CorePosition);

                        if (Main.netMode != NetmodeID.Server) {
                            void spawnAttackDust(float num) {
                                if (Main.rand.NextChance(0.4f)) {
                                    Vector2 velocity = Helper.VelocityToPoint(CorePosition, _mousePos, 2.5f + Main.rand.NextFloatRange(1f));
                                    Vector2 vector2 = velocity.RotatedBy(num * (MathHelper.Pi + MathHelper.PiOver4) / 25f);
                                    Dust dust = Dust.NewDustDirect(CorePosition, 5, 5, DustID.BubbleBurst_Blue, Scale: Main.rand.NextFloat(1.05f, 1.35f));
                                    dust.velocity = vector2 * Main.rand.NextFloat(0.8f, 1.1f);
                                    dust.noGravity = true;
                                }
                            }
                            float min = 7f, max = 5f;
                            if (Owner.direction == 1) {
                                for (float num = -min; num < max; num += 1.5f) {
                                    spawnAttackDust(num);
                                }
                            }
                            else {
                                for (float num = -max; num < min; num += 1.5f) {
                                    spawnAttackDust(num);
                                }
                            }
                        }

                        Projectile.netUpdate = true;
                    }
                    _attackTimer--;
                }
            }
            else {
                if (!_stopCounting) {
                    _attackTimer++;
                    if (!IsInUse) {
                        SpawnDustsOnShoot(Owner, CorePosition);
                        _stopCounting = true;
                        Projectile.netUpdate = true;
                    }
                }
            }
        }

        protected override void ShootProjectile() {
            bool flag = Collision.CanHit(Owner.Center, 0, 0, CorePosition, 0, 0);
            if (MinPassed && _attackTimer < Min + PerShoot) {
                if (flag) {
                    base.ShootProjectile();
                }

                SoundEngine.PlaySound(SoundID.Item20, CorePosition);

                return;
            }
            if (!_shouldShoot) {
                _shouldShoot = true;

                return;
            }

            if (flag) {
                base.ShootProjectile();
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            if (_stopCounting) {
                return;
            }
            if (ShouldShootInternal()) {
                SoundEngine.PlaySound(SoundID.MaxMana, corePosition);
            }
            int count = (int)MathHelper.Min(15, _attackTimer / 4);
            for (int i = 0; i < count - count / 3; i++) {
                Vector2 size = new(24f, 24f);
                Rectangle r = Utils.CenteredRectangle(corePosition, size);
                Dust dust = Dust.NewDustDirect(r.TopLeft(), r.Width, r.Height, 176, 0f, 0f, 0, default, Scale: Main.rand.NextFloat(1.05f, 1.35f) * 0.9f);
                dust.noGravity = true;
                dust.fadeIn = 0.9f;
                dust.velocity = new Vector2(Main.rand.Next(-50, 51) * 0.05f, Main.rand.Next(-50, 51) * 0.05f);
            }
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            if (player.whoAmI == Main.myPlayer) {
                Vector2 pointPosition = player.GetViableMousePosition();
                Vector2 center = player.Center;
                float speed = MathHelper.Clamp((pointPosition - center).Length() * 0.025f, 8.5f, 11f);
                velocity = Helper.VelocityToPoint(center, pointPosition, speed) * NatureWeaponHandler.GetUseSpeedMultiplier(player.GetSelectedItem(), player);
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (_shootCount < MAXSHOOTCOUNT || _shot) {
                return;
            }
            if (Main.rand.NextChance(MathHelper.Clamp(step * 2f, 0f, 0.75f))) {
                for (int i = 0; i < (int)(2 * step + step); i++) {
                    Dust dust = Dust.NewDustPerfect(corePosition, 176, Scale: 1f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.9f;
                }
            }
        }
    }
}