using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Mono.Cecil;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class Woodbinder : BaseRodItem<Woodbinder.WoodbinderBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<ProtectiveRoots>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36, 42);
        Item.SetDefaultToUsable(-1, 28, useSound: SoundID.Item80);
        Item.SetWeaponValues(10, 2f);

        //NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.45f);
        NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class WoodbinderBase : BaseRodProjectile {
        private Vector2 _mousePosition;
        private float _strength = 0.2f;

        private float Strength => Ease.SineIn(Ease.CubeOut(_strength));

        protected override Vector2 CorePositionOffsetFactor() => new(0.275f, 0f);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override bool IsInUse => !Owner.CCed && Owner.controlUseItem;

        protected override byte TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(player.GetSelectedItem(), player) * 2);

        public override void SendExtraAI(BinaryWriter writer) {
            base.SendExtraAI(writer);

            writer.WriteVector2(_mousePosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader) {
            base.ReceiveExtraAI(reader);

            _mousePosition = reader.ReadVector2();
        }

        public override void PostAI() {
            base.PostAI();

            if (_strength < 1f) {
                float value = 0.05f - NatureWeaponHandler.GetUseSpeed(Owner.GetSelectedItem(), Owner) / 1000f;
                _strength += value * 0.425f;
            }
            if (Owner.whoAmI == Main.myPlayer) {
                _mousePosition = Owner.GetViableMousePosition();
            }
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            ai2 = 100f * Strength;
        }

        public override void PostDraw(Color lightColor) {
            if (Projectile.owner != Main.myPlayer) {
                return;
            }
            if (CurrentUseTime <= 0f || _shot) {
                return;
            }
            float opacity = Ease.CubeOut(Utils.GetLerpValue(1f, 0.85f, UseTime, true));
            opacity *= Utils.GetLerpValue(0.2f, 0.285f, _strength, true);
            Player player = Owner;
            float distY = 100f * Strength;
            Vector2 pos = player.Center;
            Vector2 velocity = Helper.VelocityToPoint(pos, _mousePosition, 1f).SafeNormalize(Vector2.Zero);
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * distY;
            pos += muzzleOffset;
            float rejection = (float)Math.PI / 10f;
            int projectileCount = 4;
            float betweenProjs = distY * 1.5f;
            Vector2 vector2 = new(velocity.X, velocity.Y);
            vector2.Normalize();
            vector2 *= betweenProjs;
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<ProtectiveRoots>()].Value;
            for (int i = 0; i < projectileCount; i++) {
                float factor = (float)i - ((float)projectileCount - 1f) / 2f;
                Vector2 newPos = vector2.RotatedBy(rejection * factor);
                Vector2 position = pos + newPos;
                position.Y += 192 / 2f - 24f;
                Color color = Lighting.GetColor(position.ToTileCoordinates()).MultiplyRGB(Color.White);
                Main.EntitySpriteDraw(texture, position - Main.screenPosition, new Rectangle(0, 0, 48, 192 / 4), color * 0.4f * opacity, 0f, texture.Size() / 2f, 1f, default);
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            int count = (int)(16 * (1f - UseTime));
            for (int i = 0; i < count; i++) {
                int type = Main.rand.NextBool(4) ? ModContent.DustType<Dusts.Woodbinder>() : ModContent.DustType<WoodTrash>();
                Vector2 position = corePosition + new Vector2(0, -6) + new Vector2(20f, 0).RotatedBy(i * Math.PI * 2 / 16f) - new Vector2(8f, 4f);
                int dust = Dust.NewDust(position, 0, 0, type, 0, 0, 0, default(Color), Scale: Main.rand.NextFloat(1.25f, 1.5f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = 1.25f;
                Main.dust[dust].velocity += Helper.VelocityToPoint(position, _mousePosition, 2f);
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (_shot) {
                return;
            }
            float offset = 10f;
            if (step > 0f) {
                Vector2 spawnPosition = corePosition + (Vector2.UnitY * offset).RotatedBy(step * MathHelper.TwoPi * Utils.Remap(step, 0f, 1f, 2f, 5f) * -player.direction);

                for (int i = 0; i < 3; i++) {
                    Dust dust = Dust.NewDustPerfect(spawnPosition,
                                                    Main.rand.NextBool(4) ? ModContent.DustType<Dusts.Woodbinder>() : ModContent.DustType<WoodTrash>(),
                                                    Vector2.Zero,
                                                    Scale: Main.rand.NextFloat(1.25f, 1.5f) * Utils.Remap(step, 0f, 1f, 0.5f, 1f));
                    dust.noGravity = true;
                }
            }
        }
    }
}