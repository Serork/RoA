using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;
sealed class CactiCaster : BaseRodItem<CactiCaster.CactiCasterBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<Cacti>();

    protected override ushort GetUseTime(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2 - NatureWeaponHandler.GetUseSpeed(Item, player) / 3);

    protected override void SafeSetDefaults() {
        Item.SetSize(38);
        Item.SetDefaultToUsable(-1, 35, useSound: SoundID.Item7);
        Item.SetWeaponValues(14, 4f);

        //NatureWeaponHandler.SetPotentialDamage(Item, 26);
        NatureWeaponHandler.SetFillingRate(Item, 1f);
        NatureWeaponHandler.SetPotentialUseSpeed(Item, 15);
    }

    public sealed class CactiCasterBase : BaseRodProjectile {
        private Vector2 _position = Vector2.Zero, _pointPosition;
        private float _rotation2 = float.MaxValue, _rotation3;
        private bool _makeDust = true;

        public override void PostDraw(Color lightColor) {
            if (Projectile.owner != Main.myPlayer || !_makeDust) {
                return;
            }
            Player player = Owner;
            int type = ModContent.ProjectileType<Cacti>();
            float opacity = 1f/*Utils.GetLerpValue(1f, 0.75f, UseTime, true) * Utils.GetLerpValue(0f, 0.1f, UseTime, true)*/;
            Vector2 mousePoint = player.GetViableMousePosition();
            float useTimeFactor = 0.0275f * (float)(1f - 0.75f);
            float y = player.MountedCenter.Y - player.height * (0.9f + useTimeFactor * player.height * 0.75f);
            if (CurrentUseTime > _maxUseTime * MinUseTimeToShootFactor()) {
                _position = _pointPosition = new(mousePoint.X, y);
                //_rotation2 = _rotation3 = (_pointPosition - player.position).X * 0.1f;
            }
            else if (_leftTimeToReuse <= TimeAfterShootToExist(player) * 0.75f && _makeDust) {
                _makeDust = false;
                for (int num559 = 0; num559 < 10; num559++) {
                    bool flag = Main.rand.NextBool(4);
                    int dustType = flag ? ModContent.DustType<CactiCasterDust>() : DustID.JunglePlants;
                    int num560 = Dust.NewDust(_position - Vector2.One * 12, 24, 24, dustType, Alpha: flag ? 0 : 120);
                    Dust dust2 = Main.dust[num560];
                    dust2.velocity = dust2.velocity.RotatedByRandom(Main.rand.NextFloat(MathHelper.TwoPi));
                    dust2.noLight = true;
                    dust2.scale *= 1.2f;
                    dust2.velocity *= Main.rand.NextFloat(1f, 1.25f);
                    dust2.velocity *= 1.25f;
                    dust2.noGravity = true;
                    if (Main.rand.NextBool(2)) {
                        dust2.scale *= 1.2f;
                    }
                }
            }
            //if (_position == Vector2.Zero) {
            //    _position = _pointPosition;
            //}
            //_position = Vector2.SmoothStep(_position, _pointPosition, 0.1f);
            //if (_rotation2 == float.MaxValue) {
            //    _rotation2 = _rotation3;
            //}
            //_rotation2 = MathHelper.SmoothStep(_rotation2, _rotation3, 0.1f);
            //Texture2D texture = TextureAssets.Projectile[type].Value;
            if (Main.rand.NextBool(5)) {
                bool flag = Main.rand.NextBool(4);
                int dustType = flag ? ModContent.DustType<CactiCasterDust>() : DustID.JunglePlants;
                int num560 = Dust.NewDust(_position - Vector2.One * 12, 24, 24, dustType, Alpha: flag ? 0 : 120);
                Dust dust2 = Main.dust[num560];
                dust2.velocity = dust2.velocity.RotatedByRandom(Main.rand.NextFloat(MathHelper.TwoPi));
                dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
                dust2.noGravity = true;
                dust2.scale *= 1f;
                dust2.noLight = true;
                if (Main.rand.NextBool(2)) {
                    dust2.scale *= 1.2f;
                }
            }
            //Main.EntitySpriteDraw(texture, _position - Main.screenPosition, null, Color.White * 0.5f * opacity, _rotation2, texture.Size() / 2f, 1f, default);
        }

        protected override bool ShouldntUpdateRotationAndDirection() => _shot && _leftTimeToReuse < TimeAfterShootToExist(Owner) / 5f;

        protected override float MinUseTimeToShootFactor() => 0.61f;

        protected override Vector2 CorePositionOffsetFactor() => new(0.135f, 0.15f);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override byte TimeAfterShootToExist(Player player) => (byte)(player.itemTimeMax * 4);

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