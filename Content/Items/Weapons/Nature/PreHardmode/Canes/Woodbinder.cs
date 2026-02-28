using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class Woodbinder : CaneBaseItem<Woodbinder.WoodbinderBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<ProtectiveRoots>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 42);
        Item.SetUsableValues(-1, 24, useSound: SoundID.Item1);
        Item.SetWeaponValues(10, 2f);

        NatureWeaponHandler.SetPotentialDamage(Item, 22);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.25f);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 50, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class WoodbinderBase : CaneBaseProjectile {
        private Vector2 _mousePosition;
        private float _strength = 0.15f;

        private float Strength => Ease.SineIn(Ease.CubeOut(_strength));

        private static float GetCubicBezierEaseInForCavernCaneVisuals(float t, float control) => 3f * control * t * t * (1f - t) + t * t * t;

        protected override float OffsetPositionMult => -2.5f;

        protected override Vector2 CorePositionOffsetFactor() => new(0.275f, 0f);

        protected override bool ShouldWaitUntilProjDespawn() => false;

        public override bool IsInUse => Owner.IsAliveAndFree() && Owner.controlUseItem;

        protected override ushort TimeAfterShootToExist(Player player) {
            byte result = (byte)(UseTime * 1f);
            float bezierControlValue = 0.7f;
            float attackProgress = GetCubicBezierEaseInForCavernCaneVisuals(AttackProgress01, bezierControlValue);
            float minPenaltyFactor = 0.65f,
                  maxPenaltyFactor = 1f;
            result = (byte)(result * (Utils.Clamp(attackProgress, minPenaltyFactor, maxPenaltyFactor)));
            return result;
        }

        protected override bool ShouldPlayShootSound() => false;

        protected override void SafeSendExtraAI(BinaryWriter writer) {
            base.SafeSendExtraAI(writer);

            writer.WriteVector2(_mousePosition);
        }

        protected override void SafeReceiveExtraAI(BinaryReader reader) {
            base.SafeReceiveExtraAI(reader);

            _mousePosition = reader.ReadVector2();
        }

        public override void SafePostAI() {
            if (_strength < 1f) {
                if (_strength == 0.15f) {
                    if (Owner.whoAmI == Main.myPlayer) {
                        _mousePosition = Owner.GetViableMousePosition();
                        Projectile.netUpdate = true;
                    }
                }
                float value = 0.05f - NatureWeaponHandler.GetUseSpeed(Owner.GetSelectedItem(), Owner) / 1000f;
                _strength += value * 0.425f;
                if (Owner.whoAmI == Main.myPlayer) {
                    _mousePosition = Vector2.SmoothStep(_mousePosition, Owner.GetViableMousePosition(), 0.1f + Owner.velocity.LengthSquared() * 0.005f);
                    Projectile.netUpdate = true;
                }
            }
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            ai0 = _mousePosition.X;
            ai1 = _mousePosition.Y;
            ai2 = 100f * Strength;
        }

        public override void PostDraw(Color lightColor) {
            if (Projectile.owner != Main.myPlayer) {
                return;
            }
            if (CurrentUseTime <= 0f || !PreparingAttack) {
                return;
            }
            float opacity = Ease.CubeOut(Utils.GetLerpValue(1f, 0.85f, AttackTimeLeftProgress, true));
            opacity *= Utils.GetLerpValue(0.2f, 0.285f, _strength, true);
            Player player = Owner;
            float distY = 100f * Strength;
            Vector2 pos = player.GetPlayerCorePoint();
            Vector2 velocity = Helper.VelocityToPoint(pos, _mousePosition, 1f).SafeNormalize(Vector2.Zero);
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * distY;
            pos += muzzleOffset;
            float rejection = (float)Math.PI / 10f;
            int projectileCount = 4;
            float betweenProjs = distY * 1.75f;
            Vector2 vector2 = new(velocity.X, velocity.Y);
            vector2.Normalize();
            vector2 *= betweenProjs;
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<ProtectiveRoots>()].Value;
            for (int i = 0; i < projectileCount; i++) {
                float factor = (float)i - ((float)projectileCount - 1f) / 2f;
                Vector2 newPos = vector2.RotatedBy(rejection * factor);
                Vector2 position = pos + newPos;
                //position.Y += 192 / 2f - 24f;
                Color color = Lighting.GetColor(position.ToTileCoordinates()).MultiplyRGB(Color.White);
                Main.EntitySpriteDraw(texture, position - Main.screenPosition, new Rectangle(0, 0, 48, 192 / 4), color * 0.5f * opacity, _strength * 2f * MathHelper.TwoPi * player.direction, texture.Frame(1, 4, 0, 0).Size() / 2f, 1f, default);
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            int count = (int)(16 * Ease.QuadOut(1f - AttackTimeLeftProgress));
            for (int i = 0; i < count; i++) {
                int type = Main.rand.NextBool(4) ? ModContent.DustType<Dusts.Woodbinder>() : ModContent.DustType<WoodTrash>();
                Vector2 position = corePosition + new Vector2(20f, 0).RotatedBy(i * Math.PI * 2 / 16f);
                int dust = Dust.NewDust(position, 6, 6, type, 0, 0, 0, default(Color), Scale: Main.rand.NextFloat(1.25f, 1.5f));
                Main.dust[dust].position = position + Main.rand.RandomPointInArea(6);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = 1.25f;
                Main.dust[dust].velocity += Helper.VelocityToPoint(position, _mousePosition, 2f);
            }

            PlayAttackSound();
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (!PreparingAttack) {
                return;
            }
            float offset = 10f;
            if (step > 0f) {
                Vector2 spawnPosition = corePosition - Vector2.One * 1f + (Vector2.UnitY * offset).RotatedBy(step * MathHelper.TwoPi * Utils.Remap(step, 0f, 1f, 2f, 5f) * -player.direction);

                for (int i = 0; i < 3; i++) {
                    Dust dust = Dust.NewDustPerfect(spawnPosition,
                                                    Main.rand.NextBool(4) ? ModContent.DustType<Dusts.Woodbinder>() : ModContent.DustType<WoodTrash>(),
                                                    Vector2.Zero,
                                                    Scale: Main.rand.NextFloat(1.25f, 1.5f) * Utils.Remap(step, 0f, 1f, 0.5f, 1f));
                    dust.noGravity = true;
                    dust.customData = player;
                }
            }
        }
    }
}