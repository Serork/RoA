using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class GalipotStream : NatureProjectile {
    public bool IsActive => Projectile.ai[1] == 0f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 15;

        Main.projFrames[Type] = 3;
    }

    protected override void SafeSetDefaults() {
        Projectile.width = 14;
        Projectile.height = 18;
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
        target.AddBuff(ModContent.BuffType<SolidifyingSap>(), (int)(60f * num2 * 2f) / 2);
        //if (Projectile.ai[1] != 1f) {
        //    Projectile.Kill();
        //}
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        target.AddBuff(ModContent.BuffType<SolidifyingSap>(), (int)(60f * num2 * 2f) / 2);
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
        bool flag2 = IsActive && Projectile.velocity.Length() > 0.5f;
        //Projectile.hide = flag2;
        //if (flag2) {
        //    for (int num132 = 0; num132 < 3; num132++) {
        //        float num133 = Projectile.velocity.X / 3f * (float)num132;
        //        float num134 = Projectile.velocity.Y / 3f * (float)num132;
        //        int num135 = 6;
        //        int num136 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num135, Projectile.position.Y + (float)num135), Projectile.width - num135 * 2, Projectile.height - num135 * 2,  ModContent.DustType<Galipot>(), 0f, 0f, 0, default(Color), 1.1f);
        //        Main.dust[num136].noGravity = true;
        //        Main.dust[num136].noLight = true;
        //        Dust dust2 = Main.dust[num136];
        //        dust2.velocity *= 0.3f;
        //        dust2 = Main.dust[num136];
        //        dust2.velocity += Projectile.velocity * 0.5f;
        //        Main.dust[num136].position.X -= num133;
        //        Main.dust[num136].position.Y -= num134;
        //    }

        //    if (Main.rand.Next(8) == 0) {
        //        int num137 = 6;
        //        int num138 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num137, Projectile.position.Y + (float)num137), Projectile.width - num137 * 2, Projectile.height - num137 * 2, ModContent.DustType<Galipot>(), 0f, 0f, 0, default(Color), 0.85f);
        //        Dust dust2 = Main.dust[num138];
        //        dust2.noLight = true;
        //        dust2.velocity *= 0.5f;
        //        dust2 = Main.dust[num138];
        //        dust2.velocity += Projectile.velocity * 0.5f;
        //    }
        //}
        //else {
        //    int dust = Dust.NewDust(Projectile.position - Vector2.UnitY * Projectile.height / 4, Projectile.width, Projectile.height, ModContent.DustType<Galipot>(), 0f, 0f, 0, default, 1f);
        //    Main.dust[dust].scale += Main.rand.Next(40) * 0.01f;
        //    Main.dust[dust].noGravity = true;
        //    Dust dust2 = Main.dust[dust];
        //    dust2.position.X -= 2f;
        //    Dust dust3 = Main.dust[dust];
        //    dust3.position.Y += 2f;
        //    Dust dust4 = Main.dust[dust];
        //    dust4.velocity.Y -= 2f;
        //}

        //Projectile.hide = flag2;
        //Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) + MathHelper.Pi;

        //if (Projectile.timeLeft < 500 - 1 && flag2) {
        //    float num3 = 0f;
        //    float y = 0f;
        //    Vector2 vector6 = Projectile.position;
        //    Vector2 vector7 = Projectile.oldPosition;
        //    vector7.Y -= num3 / 2f;
        //    vector6.Y -= num3 / 2f;
        //    int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
        //    if (Vector2.Distance(vector6, vector7) % 3f != 0f)
        //        num5++;

        //    for (float num6 = 1f; num6 <= (float)num5; num6 += 2f) {
        //        Dust obj = Main.dust[Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Galipot>(), Alpha: 0, Scale: 1f)];
        //        obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f;
        //        obj.velocity *= 0.1f;
        //        obj.noLight = true;
        //    }
        //}

        //Lighting.AddLight(Projectile.Top, Color.Lerp(new Color(255, 241, 44), new Color(204, 128, 14), 0.75f).ToVector3() * 0.5f);
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
            if (Main.rand.NextBool(2)) {
                GalipotDrop drop = VisualEffectSystem.New<GalipotDrop>(VisualEffectLayer.BEHINDTILESBEHINDNPCS).Setup(Projectile.Center - Vector2.UnitY * Projectile.ai[2],
                    Projectile.velocity);
                drop.Projectile = Projectile;
                drop.Scale = Main.rand.NextFloat(8f, 10f) * Projectile.scale;
                drop.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                drop.AI0 = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                drop.ShouldDrop = Projectile.velocity.Length() < 1f || Projectile.ai[1] != 1f;
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
