﻿using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class GalipotStream : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public bool IsActive => Projectile.ai[1] == 0f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 15;
    }

    protected override void SafeSetDefaults() {
        Projectile.width = Projectile.height = 10;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 500;
        Projectile.penetrate = 2;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.immune[Projectile.owner] = 20;
        //if (Projectile.ai[1] != 1f) {
        //    Projectile.Kill();
        //}
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //if (Projectile.ai[1] != 1f) {
        //    Projectile.Kill();
        //}
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.position -= oldVelocity;
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
        Projectile.ai[2] *= 0.99f;
        void drop() {
            if (Main.rand.NextBool(2)) {
                GalipotDrop drop = VisualEffectSystem.New<GalipotDrop>(VisualEffectLayer.BEHINDTILESBEHINDNPCS).Setup(Projectile.Center - Vector2.UnitY * Projectile.ai[2],
                    Projectile.velocity);
                drop.Projectile = Projectile;
                drop.Scale = Main.rand.NextFloat(8f, 10f);
                drop.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                drop.AI0 = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            }
        }
        if (IsActive) {
            drop();
            if (Main.rand.NextBool(2)) {
                Vector2 pos = Projectile.Center - new Vector2(4f) + Projectile.velocity * Main.rand.NextFloat(1f);
                Dust dust = Dust.NewDustDirect(pos - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.25f, 3f), 6, 6, ModContent.DustType<Galipot>(), 0, 0, 0, default, 0.6f + Main.rand.NextFloatRange(0.2f));
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            Projectile.ai[2] = 0f;
            Projectile.velocity.Y += 0.15f;
            if (Projectile.ai[1] != 1f && Collision.SolidCollision(Projectile.Center, 2, 2)) {
                Projectile.velocity = Vector2.Zero;

                Projectile.ai[1] = 1f;
                Projectile.maxPenetrate = Projectile.penetrate = -1;
            }
        }
        else {
            if (Main.rand.NextBool(3)) {
                drop();
            }
            float value = 0.05f * Main.rand.NextFloat();
            Projectile.ai[2] += value;
            Projectile.position.Y += value;
            if (Projectile.ai[1] != 0f && !Collision.SolidCollision(Projectile.Center, 0, 0)) {
                Projectile.ai[1] = 0f;
                Projectile.maxPenetrate = Projectile.penetrate = 2;
            }
        }
    }
}
