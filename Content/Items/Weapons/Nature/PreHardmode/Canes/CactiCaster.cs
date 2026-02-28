using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class CactiCaster : CaneBaseItem<CactiCaster.CactiCasterBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Cacti>();

    protected override ushort TimeToCastAttack(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2 - NatureWeaponHandler.GetUseSpeed(Item, player) / 3);

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(40);
        Item.SetUsableValues(-1, 25, useSound: SoundID.Item7);
        Item.SetWeaponValues(14, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 26);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.4f);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 30, 0);

        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 15);
    }

    public sealed class CactiCasterBase : CaneBaseProjectile {
        internal Vector2 _position = Vector2.Zero;
        private float _rotation2 = float.MaxValue, _rotation3;
        private bool _makeDust = true;

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();

            ProjectileID.Sets.NeedsUUID[Type] = true;
        }

        protected override void SafeSendExtraAI(BinaryWriter writer) {
            base.SafeSendExtraAI(writer);

            writer.WriteVector2(_position);
        }

        protected override void SafeReceiveExtraAI(BinaryReader reader) {
            base.SafeReceiveExtraAI(reader);

            _position = reader.ReadVector2();
        }

        public override void SafePostAI() {
            Player player = Owner;
            Vector2 mousePoint = Helper.GetLimitedPosition(player.GetPlayerCorePoint(), player.GetViableMousePosition(), 400f);
            float useTimeFactor = 0.0275f * (float)(1f - 0.75f);
            float y = mousePoint.Y + player.height - player.height * (0.9f + useTimeFactor * player.height * 0.75f);
            if (CurrentUseTime > UseTime * MinUseTimeToShootFactor()) {
                if (Projectile.owner == Main.myPlayer && _position == Vector2.Zero) {
                    _position = new(mousePoint.X, y);
                    Projectile.netUpdate = true;
                }
            }

            if (Projectile.IsOwnerLocal() && Shot && (!LastShotProjectile.active || LastShotProjectile?.As<Cacti>()._state == Cacti.State.Enchanted)) {
                Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }

        public override void PostDraw(Color lightColor) {
            if (!_makeDust) {
                return;
            }
            Player player = Owner;
            int type = ModContent.ProjectileType<Cacti>();
            //float opacity = 1f;
            Vector2 mousePoint = Helper.GetLimitedPosition(player.GetPlayerCorePoint(), player.GetViableMousePosition(), 400f);
            float useTimeFactor = 0.0275f * (float)(1f - 0.75f);
            float y = mousePoint.Y + player.height - player.height * (0.9f + useTimeFactor * player.height * 0.75f);
            if (CurrentUseTime > UseTime * MinUseTimeToShootFactor()) {

            }
            else if (CurrentReleaseTime <= TimeAfterShootToExist(player) * 0.6f && _makeDust) {
                _makeDust = false;
                if (!Main.gamePaused && Main.instance.IsActive) {
                    for (int num559 = 0; num559 < 10; num559++) {
                        int dustType = DustID.OasisCactus;
                        int num560 = Dust.NewDust(_position - Vector2.One * 12, 24, 24, dustType, Alpha: Main.rand.Next(80));
                        Dust dust2 = Main.dust[num560];
                        dust2.velocity = dust2.velocity.RotatedByRandom(Main.rand.NextFloat(MathHelper.TwoPi));
                        dust2.scale *= 1.2f;
                        dust2.velocity *= Main.rand.NextFloat(1f, 1.25f);
                        dust2.velocity *= 1.25f;
                        dust2.noGravity = true;
                        if (Main.rand.NextBool(2)) {
                            dust2.scale *= 1.2f;
                        }
                    }
                }
            }
            if (!Main.gamePaused && Main.instance.IsActive) {
                if (Main.rand.NextBool(5)) {
                    int dustType = DustID.OasisCactus;
                    int num560 = Dust.NewDust(_position - Vector2.One * 12, 24, 24, dustType, Alpha: Main.rand.Next(80));
                    Dust dust2 = Main.dust[num560];
                    dust2.velocity = dust2.velocity.RotatedByRandom(Main.rand.NextFloat(MathHelper.TwoPi));
                    dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust2.noGravity = true;
                    dust2.scale *= 1f;
                    if (Main.rand.NextBool(2)) {
                        dust2.scale *= 1.2f;
                    }
                }
            }
        }

        //protected override bool ShouldStopUpdatingRotationAndDirection() => Shot && _penaltyTime < TimeAfterShootToExist(Parent) / 5f;

        protected override ushort TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(AttachedNatureWeapon, player) * 4);

        protected override float MinUseTimeToShootFactor() => 0.61f;

        protected override Vector2 CorePositionOffsetFactor() => new(0.1f, 0.15f);

        protected override bool ShouldWaitUntilProjDespawn() => true;

        protected override bool DespawnWithProj() => true;

        protected override bool ShouldPlayShootSound() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2)
            => ai1 = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            float offset = 10f;
            float reversed = 1f - step;
            if (step > 0.025f) {
                Vector2 spawnPosition = corePosition - Vector2.One * 1f + (Vector2.UnitY * offset * reversed).RotatedBy(step * MathHelper.Pi * 5f * player.direction);

                for (int i = 0; i < 4; i++) {
                    Dust dust = Dust.NewDustPerfect(spawnPosition,
                                                    ModContent.DustType<CactiCasterDust>(),
                                                    Vector2.Zero,
                                                    Scale: Main.rand.NextFloat(1.25f, 1.5f));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.customData = player;
                }
            }
        }
    }
}