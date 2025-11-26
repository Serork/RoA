using Microsoft.Xna.Framework;

using RoA.Core.Utility;

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
        Projectile.extraUpdates = 0;
        Projectile.noEnchantmentVisuals = true;

        int width = 12; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.scale = 1f;
        Projectile.penetrate = 1;

        Projectile.minion = true;

        Projectile.minionSlots = 0f;
        Projectile.tileCollide = false;

        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool? CanCutTiles() {
        Player owner = Projectile.GetOwnerAsPlayer();
        if (!Collision.CanHit(Projectile.Center, 0, 0, owner.position, owner.width, owner.height)) {
            return false;
        }

        return base.CanCutTiles();
    }

    public override void AI() {
        if (Projectile.timeLeft == 400) {
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

        float targetPositionX = Projectile.position.X;
        float targetPositionY = Projectile.position.Y;
        float targetDistance = 100000f;
        bool targetFoundManually = false;
        bool targetFoundByOwner = false;
        Vector2 targetPosition = Projectile.position;
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 30f) {
            Projectile.ai[0] = 30f;

            NPC ownerMinionAttackTargetNPC2 = Projectile.OwnerMinionAttackTargetNPC;
            if (ownerMinionAttackTargetNPC2 != null && ownerMinionAttackTargetNPC2.CanBeChasedBy(this)) {
                float distanceBetween = Vector2.Distance(ownerMinionAttackTargetNPC2.Center, Projectile.Center);
                float neededDistance = 800f;
                if (distanceBetween < neededDistance && !targetFoundByOwner) {
                    targetDistance = distanceBetween;
                    targetPosition = ownerMinionAttackTargetNPC2.Center;
                    targetFoundByOwner = true;
                }
            }

            if (!targetFoundByOwner) {
                for (int num306 = 0; num306 < 200; num306++) {
                    if (Main.npc[num306].CanBeChasedBy(this)) {
                        float num307 = Main.npc[num306].position.X + (float)(Main.npc[num306].width / 2);
                        float num308 = Main.npc[num306].position.Y + (float)(Main.npc[num306].height / 2);
                        float num309 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num307) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num308);
                        if (num309 < 800f && num309 < targetDistance) {
                            targetDistance = num309;
                            targetPositionX = num307;
                            targetPositionY = num308;
                            targetFoundManually = true;
                        }
                    }
                }
            }
        }

        if (targetFoundByOwner) {
            targetPositionX = targetPosition.X;
            targetPositionY = targetPosition.Y;
        }
        if (!targetFoundManually && !targetFoundByOwner) {
            targetPositionX = Projectile.position.X + (float)(Projectile.width / 2) + Projectile.velocity.X * 100f;
            targetPositionY = Projectile.position.Y + (float)(Projectile.height / 2) + Projectile.velocity.Y * 100f;
        }

        float speed = 6f;
        float acceleration = 0.1f;

        speed = 9f;
        acceleration = 0.2f;

        Vector2 vector26 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
        float num312 = targetPositionX - vector26.X;
        float num313 = targetPositionY - vector26.Y;
        float num314 = (float)Math.Sqrt(num312 * num312 + num313 * num313);
        float num315 = num314;
        num314 = speed / num314;
        num312 *= num314;
        num313 *= num314;
        if (Projectile.velocity.X < num312) {
            Projectile.velocity.X += acceleration;
            if (Projectile.velocity.X < 0f && num312 > 0f)
                Projectile.velocity.X += acceleration * 2f;
        }
        else if (Projectile.velocity.X > num312) {
            Projectile.velocity.X -= acceleration;
            if (Projectile.velocity.X > 0f && num312 < 0f)
                Projectile.velocity.X -= acceleration * 2f;
        }

        if (Projectile.velocity.Y < num313) {
            Projectile.velocity.Y += acceleration;
            if (Projectile.velocity.Y < 0f && num313 > 0f)
                Projectile.velocity.Y += acceleration * 2f;
        }
        else if (Projectile.velocity.Y > num313) {
            Projectile.velocity.Y -= acceleration;
            if (Projectile.velocity.Y > 0f && num313 < 0f)
                Projectile.velocity.Y -= acceleration * 2f;
        }
    }
}