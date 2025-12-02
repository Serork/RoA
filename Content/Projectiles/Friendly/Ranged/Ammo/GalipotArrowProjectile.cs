using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.AdvancedDusts;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged.Ammo;

sealed class GalipotArrowProjectile : ModProjectile {
    private class GalipotStream2 : ModProjectile {
        public bool IsActive => Projectile.ai[1] == 0f;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;

            Main.projFrames[Type] = 3;
        }

        public override void SetDefaults() {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 500;
            Projectile.penetrate = 1;

            Projectile.DamageType = DamageClass.Ranged;

            //Projectile.extraUpdates = 1;

            Projectile.hide = true;

            Projectile.frame = Main.rand.Next(3);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = height = 10;

            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.immune[Projectile.owner] = 20;
            //target.AddBuff(ModContent.BuffType<SolidifyingSap>(), (int)(60f * num2 * 2f) / 2);
            //if (Projectile.ai[1] != 1f) {
            //    Projectile.Kill();
            //}
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) {
            float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
            //target.AddBuff(ModContent.BuffType<SolidifyingSap>(), (int)(60f * num2 * 2f) / 2);
            //if (Projectile.ai[1] != 1f) {
            //    Projectile.Kill();
            //}
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            //Projectile.position -= oldVelocity;
            Projectile.velocity = Vector2.Zero;

            Projectile.ai[1] = 1f;

            return false;
        }

        public override bool? CanDamage() => Projectile.ai[1] == 1f;

        public override void OnSpawn(IEntitySource source) {
            Projectile.velocity *= Main.rand.NextFloat(1.25f, 1.75f) * Main.rand.NextFloat(0.75f, 1f);
            Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloatDirection() * MathHelper.PiOver2) * Projectile.velocity.Length() * 0.1f;
            Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
        }

        public override void AI() {
            bool flag2 = IsActive && Projectile.velocity.Length() > 0.5f;

            //Lighting.AddLight(Projectile.Top, DrawColor.Lerp(new DrawColor(255, 241, 44), new DrawColor(204, 128, 14), 0.75f).ToVector3() * 0.5f);
            bool flag = false;

            if (Collision.WetCollision(Projectile.Center, 0, 0)) {
                if (Projectile.velocity.Length() > 1f) {
                    Projectile.velocity.Y *= 0.7f;
                    if (Math.Abs(Projectile.velocity.X) > 0.2f) {
                        Projectile.velocity.X *= 0.7f;
                    }
                }
                //Projectile.position.Y += 0.05f * Main.rand.NextFloat();
                flag = true;
            }

            if (flag) {
                Projectile.scale -= 0.02f;
            }
            if (Projectile.scale <= 0.1f) {
                Projectile.Kill();
            }

            Projectile.ai[2] *= 0.99f;
            void drop() {
                if (Projectile.timeLeft < 500 - 1) {
                    if (Main.rand.NextBool(2)) {
                        GalipotDrop drop = AdvancedDustSystem.New<GalipotDrop>(AdvancedDustLayer.BEHINDTILESBEHINDNPCS).Setup(Projectile.Center - Vector2.UnitY * Projectile.ai[2],
                            Projectile.velocity);
                        drop.Projectile = Projectile;
                        drop.Scale = Main.rand.NextFloat(8f, 10f) * Projectile.scale;
                        drop.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                        drop.AI0 = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                        drop.ShouldDrop = Projectile.velocity.Length() < 1f || Projectile.ai[1] != 1f;
                    }
                }
            }
            if (IsActive) {
                for (int num164 = 0; num164 < 2; num164++) {
                    if (Main.rand.NextBool(4)) {
                        Dust obj14 = Main.dust[Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, ModContent.DustType<Galipot>(), Projectile.velocity.X, Projectile.velocity.Y, 150)];
                        obj14.noGravity = true;
                        obj14.color = default;
                        obj14.velocity = obj14.velocity / 4f + Projectile.velocity / 2f;
                        obj14.scale = 1.2f;
                        obj14.position = Projectile.Center + Main.rand.NextFloat() * Projectile.velocity * 2f;
                        obj14.velocity *= 0.5f;
                    }
                }

                drop();
                if (Main.rand.NextBool(2)) {
                    Vector2 pos = Projectile.Center - new Vector2(4f) + Projectile.velocity * Main.rand.NextFloat(1f);
                    Dust dust = Dust.NewDustDirect(pos - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.25f, 3f), 6, 6, ModContent.DustType<Galipot>(), 0, 0, 50, default, 0.6f + Main.rand.NextFloatRange(0.2f));
                    dust.velocity *= 0.3f;
                    dust.noGravity = true;
                }

                drop();

                Projectile.ai[2] = 0f;
                Projectile.velocity.Y += 0.15f;
                if (Projectile.ai[1] != 1f && Collision.SolidCollision(Projectile.Center, 2, 2)) {
                    Projectile.velocity = Vector2.Zero;

                    Projectile.ai[1] = 1f;
                }
            }
            else {
                //if (Main.rand.NextBool(3)) {
                //    drop();
                //}
                float value = 0.05f * Main.rand.NextFloat();
                Projectile.ai[2] += value;
                Projectile.position.Y += value;
                if (Projectile.ai[1] != 0f && !Collision.SolidCollision(Projectile.Center, 2, 2)) {
                    Projectile.ai[1] = 0f;
                }
            }
        }
    }

    public override void SetDefaults() {
        Projectile.arrow = true;
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1200;
    }

    public override void AI() {
        if (Projectile.shimmerWet) {
            Projectile.velocity.Y -= 0.4f;
        }

        if (Main.rand.NextBool(3)) {
            int num67 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Galipot>(), 0f, 0f, Alpha: 50);
            Main.dust[num67].noGravity = true;
            Main.dust[num67].fadeIn = 1.5f;
            Main.dust[num67].velocity *= 0.25f;
            Main.dust[num67].velocity += Projectile.velocity * 0.25f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        //target.AddBuff(ModContent.BuffType<Buffs.SolidifyingSap>(), 300);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //target.AddBuff(ModContent.BuffType<Buffs.SolidifyingSap>(), 300);
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        if (Projectile.owner == Main.myPlayer) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GalipotStream2>(), Projectile.damage, 0, Projectile.owner);
        }
        for (int num671 = 0; num671 < 6; num671++) {
            int num672 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Galipot>(), 0f, 0f, Alpha: 50);
            Main.dust[num672].noGravity = true;
            Main.dust[num672].fadeIn = 1.5f;
            Dust dust2 = Main.dust[num672];
        }
    }
}