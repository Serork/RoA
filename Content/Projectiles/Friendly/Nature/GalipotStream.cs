using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.AdvancedDusts;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GalipotStream : NatureProjectile {
    public bool IsActive => Projectile.ai[1] == 0f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 15;

        Main.projFrames[Type] = 3;
    }

    protected override void SafeSetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 500;
        Projectile.penetrate = 2;

        //Projectile.extraUpdates = 1;

        Projectile.hide = true;

        Projectile.frame = Main.rand.Next(3);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.immune[Projectile.owner] = 20;
        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        Projectile.damage = (int)(Projectile.damage * 0.75f);
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

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Projectile.velocity *= Main.rand.NextFloat(1.25f, 1.75f) * Main.rand.NextFloat(0.75f, 1f);
        Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloatDirection() * MathHelper.PiOver2) * Projectile.velocity.Length() * 0.1f;
        Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
    }

    public override void AI() {
        ShouldApplyAttachedNatureWeaponCurrentDamage = Projectile.penetrate >= 2;

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
                    Dust obj14 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Galipot>(), Projectile.velocity.X, Projectile.velocity.Y, 150)];
                    obj14.noGravity = true;
                    obj14.color = default;
                    obj14.velocity = obj14.velocity / 4f + Projectile.velocity / 2f;
                    obj14.scale = 1.2f;
                    obj14.position = Projectile.Center + Main.rand.NextFloatDirection() * Projectile.velocity * 1f;
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
                Projectile.maxPenetrate = Projectile.penetrate = 3;
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
                Projectile.maxPenetrate = Projectile.penetrate = 2;
            }
        }
    }
}
