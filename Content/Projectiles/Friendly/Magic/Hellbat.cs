using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class Hellbat : ModProjectile {
    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f) * (1f - Projectile.alpha / 255f) * Projectile.Opacity;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 5;
    }

    public override void SetDefaults() {
        int width = 34, height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;

        Projectile.aiStyle = 1;
        AIType = ProjectileID.Bullet;

        Projectile.ignoreWater = false;

        Projectile.extraUpdates = 1;

        Projectile.alpha = 255;
        Projectile.timeLeft = 70;

        DrawOriginOffsetY = 1;
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox = new Rectangle((int)Projectile.Center.X - 6, (int)Projectile.Center.Y - 6, 12, 12);
    }

    public override void AI() {
        if (Projectile.wet && !Projectile.lavaWet) {
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height)) {
                Projectile.Kill();
            }
        }

        if (Projectile.alpha < 65 && Main.rand.NextBool(6)) {
            int num179 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1.2f + Main.rand.NextFloat() * 0.5f);
            //Main.dust[num179].noLightEmittence = true;
            Main.dust[num179].noGravity = true;
            Main.dust[num179].velocity *= 0.2f;
        }

        if (Projectile.timeLeft >= 20) {
            if (Projectile.ai[0] == 1f) {
                Projectile.scale += 0.008f;
                if (Projectile.scale >= 1.4f) {
                    Projectile.alpha += 25;
                }
            }
            else if (Projectile.alpha > 0) {
                Projectile.alpha -= 15;
            }
            else {
                Projectile.ai[0] = 1f;
            }
        }
        else {
            //Projectile.alpha = 0;
            Projectile.Opacity = 1f - Utils.GetLerpValue(20, 0, Projectile.timeLeft, true);

        }
        Projectile.rotation = 0f;
        Projectile.spriteDirection = -Projectile.direction;
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 6) {
            Projectile.frame++; Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= Main.projFrames[Type]) {
            Projectile.frame = 0;
        }

        int num606 = -1;
        Vector2 vector52 = Projectile.Center;
        float num607 = 300f;

        if (Projectile.localAI[0] == 0f && Projectile.ai[1] == 0f)
            Projectile.localAI[0] = 30f;

        if (Projectile.localAI[0] > 0f)
            Projectile.localAI[0]--;

        if (Projectile.ai[1] == 0f && Projectile.localAI[0] == 0f) {
            for (int num608 = 0; num608 < 200; num608++) {
                NPC nPC7 = Main.npc[num608];
                if (nPC7.CanBeChasedBy(this) && (Projectile.ai[1] == 0f || Projectile.ai[1] == (float)(num608 + 1))) {
                    Vector2 center7 = nPC7.Center;
                    float num609 = Vector2.Distance(center7, vector52);
                    if (num609 < num607 && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, nPC7.position, nPC7.width, nPC7.height)) {
                        num607 = num609;
                        vector52 = center7;
                        num606 = num608;
                    }
                }
            }

            if (num606 >= 0) {
                Projectile.ai[1] = num606 + 1;
                Projectile.netUpdate = true;
            }

            num606 = -1;
        }


        bool flag33 = false;
        if (Projectile.ai[1] != 0f) {
            int num610 = (int)(Projectile.ai[1] - 1f);
            if (Main.npc[num610].active && !Main.npc[num610].dontTakeDamage && Main.npc[num610].immune[Projectile.owner] == 0) {
                float num611 = Main.npc[num610].position.X + (float)(Main.npc[num610].width / 2);
                float num612 = Main.npc[num610].position.Y + (float)(Main.npc[num610].height / 2);
                float num613 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num611) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num612);
                if (num613 < num607) {
                    flag33 = true;
                    vector52 = Main.npc[num610].Center;
                }
            }
            else {
                Projectile.ai[1] = 0f;
                flag33 = false;
                Projectile.netUpdate = true;
            }
        }
        if (flag33) {
            Vector2 v6 = vector52 - Projectile.Center;
            float num614 = Projectile.velocity.ToRotation();
            float num615 = v6.ToRotation();
            double num616 = num615 - num614;
            if (num616 > Math.PI)
                num616 -= Math.PI * 2.0;

            if (num616 < -Math.PI)
                num616 += Math.PI * 2.0;

            Projectile.velocity = Projectile.velocity.RotatedBy(num616 * 0.05);
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        return true;
    }

    public override void OnKill(int timeLeft) {
        if (timeLeft == 0) return;

        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.position);

        for (int num303 = 0; num303 < 2; num303++) {
            int num304 = Dust.NewDust(Projectile.position + Vector2.One * 4f, Projectile.width - 8, Projectile.height - 8, 6, Scale: 1.5f);
            Main.dust[num304].noGravity = true;
            Dust dust2 = Main.dust[num304];
            dust2.velocity -= Projectile.oldVelocity * Main.rand.Next(20, 60) * 0.01f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.immune[Projectile.owner] = 8;
        int buff = ModContent.BuffType<Deceleration>();
        //int buff = BuffID.Wet;
        if (target.FindBuff(buff, out int buffIndex)) {
            target.DelBuff(buffIndex);
        }
        if (!Projectile.wet && !target.wet) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(30, 91), false);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //target.immune[Projectile.owner] = 8;
        int buff = ModContent.BuffType<Deceleration>();
        //int buff = BuffID.Wet;
        if (target.FindBuff(buff, out int buffIndex)) {
            target.DelBuff(buffIndex);
        }
        if (!Projectile.wet && !target.wet) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(30, 91), false);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        //if (Projectile.wet || target.wet) {
        //	modifiers.FinalDamage /= 2;
        //}
        //if (Main.player[Projectile.owner].position.Y > (Main.maxTilesY - 200) * 16) {
        //	modifiers.FinalDamage *= 1.5f;
        //}
    }
}