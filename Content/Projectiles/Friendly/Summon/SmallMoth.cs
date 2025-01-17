using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class SmallMoth : ModProjectile {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Small Moth");
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.alpha = 255;
        Projectile.timeLeft = 600;
        Projectile.extraUpdates = 3;
        Projectile.noEnchantmentVisuals = true;

        int width = 14; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.scale = 1f;
        Projectile.penetrate = 1;

        Projectile.minion = true;

        Projectile.minionSlots = 0f;
        Projectile.tileCollide = false;

        Projectile.timeLeft = 300;

        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;
    }

    public override void AI() {
        if (Projectile.timeLeft == 220) {
            Projectile.tileCollide = true;
        }

        if (Projectile.timeLeft % 2 == 0) {
            if (Projectile.direction < 0) {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - 2, Projectile.position.Y + 6), 1, 1, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f) * 0.75f);
                Main.dust[dust].velocity *= 0.25f;
                Main.dust[dust].noGravity = true;
            }
            else {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - 6 + Projectile.width, Projectile.position.Y + 4), 1, 1, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f) * 0.75f);
                Main.dust[dust].velocity *= 0.25f;
                Main.dust[dust].noGravity = true;
            }
        }

        Projectile.frameCounter++;
        if (Projectile.frameCounter > 2) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 4) Projectile.frame = 0;

        if (Projectile.wet && !Projectile.honeyWet && !Projectile.shimmerWet)
            Projectile.Kill();

        if (Projectile.alpha > 0)
            Projectile.alpha -= 50;
        else
            Projectile.extraUpdates = 0;

        if (Projectile.alpha < 0)
            Projectile.alpha = 0;

        if (Projectile.velocity.X > 0f) {
            Projectile.spriteDirection = 1;
        }
        else if (Projectile.velocity.X < 0f) {
            Projectile.spriteDirection = -1;
        }

        Projectile.rotation = Projectile.velocity.X * 0.1f;

        float num303 = Projectile.position.X;
        float num304 = Projectile.position.Y;
        float num305 = 100000f;
        bool flag15 = false;
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 30f) {
            Projectile.ai[0] = 30f;
            float num12 = 600f;
            bool flag = false;
            int num13 = -1;
            Vector2 vector = Projectile.position;
            NPC ownerMinionAttackTargetNPC2 = Projectile.OwnerMinionAttackTargetNPC;
            if (ownerMinionAttackTargetNPC2 != null && ownerMinionAttackTargetNPC2.CanBeChasedBy(this)) {
                float num17 = Vector2.Distance(ownerMinionAttackTargetNPC2.Center, Projectile.Center);
                float num18 = num12 * 3f;
                if (num17 < num18 && !flag) {
                    if (Collision.CanHit(Projectile.Center, 1, 1, ownerMinionAttackTargetNPC2.Center, 1, 1)) {
                        num12 = num17;
                        vector = ownerMinionAttackTargetNPC2.Center;
                        flag = true;
                        num13 = ownerMinionAttackTargetNPC2.whoAmI;
                    }
                }
            }
            if (!flag) {
                for (int num306 = 0; num306 < 200; num306++) {
                    if (Main.npc[num306].CanBeChasedBy(this)) {
                        float num307 = Main.npc[num306].position.X + (float)(Main.npc[num306].width / 2);
                        float num308 = Main.npc[num306].position.Y + (float)(Main.npc[num306].height / 2);
                        float num309 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num307) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num308);
                        if (num309 < 800f && num309 < num305 && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, Main.npc[num306].position, Main.npc[num306].width, Main.npc[num306].height)) {
                            num305 = num309;
                            num303 = num307;
                            num304 = num308;
                            flag15 = true;
                        }
                    }
                }
            }
        }

        if (!flag15) {
            num303 = Projectile.position.X + (float)(Projectile.width / 2) + Projectile.velocity.X * 100f;
            num304 = Projectile.position.Y + (float)(Projectile.height / 2) + Projectile.velocity.Y * 100f;
        }

        float num310 = 6f;
        float num311 = 0.1f;

        num310 = 9f;
        num311 = 0.2f;

        Vector2 vector26 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
        float num312 = num303 - vector26.X;
        float num313 = num304 - vector26.Y;
        float num314 = (float)Math.Sqrt(num312 * num312 + num313 * num313);
        float num315 = num314;
        num314 = num310 / num314;
        num312 *= num314;
        num313 *= num314;
        if (Projectile.velocity.X < num312) {
            Projectile.velocity.X += num311;
            if (Projectile.velocity.X < 0f && num312 > 0f)
                Projectile.velocity.X += num311 * 2f;
        }
        else if (Projectile.velocity.X > num312) {
            Projectile.velocity.X -= num311;
            if (Projectile.velocity.X > 0f && num312 < 0f)
                Projectile.velocity.X -= num311 * 2f;
        }

        if (Projectile.velocity.Y < num313) {
            Projectile.velocity.Y += num311;
            if (Projectile.velocity.Y < 0f && num313 > 0f)
                Projectile.velocity.Y += num311 * 2f;
        }
        else if (Projectile.velocity.Y > num313) {
            Projectile.velocity.Y -= num311;
            if (Projectile.velocity.Y > 0f && num313 < 0f)
                Projectile.velocity.Y -= num311 * 2f;
        }
    }
}