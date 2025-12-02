using Microsoft.Xna.Framework;

using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class LargeBee : NatureProjectile {
    public override string Texture => ResourceManager.NatureProjectileTextures + nameof(LargeBee);

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    protected override void SafeSetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.noEnchantmentVisuals = true;
        Projectile.extraUpdates = 3;

        int width = 16; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.scale = 1f;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 660;

        Projectile.appliesImmunityTimeOnSingleHits = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;

        Projectile.tileCollide = true;

        Projectile.friendly = true;
        Projectile.hostile = false;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        WreathFillingFine *= 3f;
    }

    public override void AI() {
        //if (Projectile.timeLeft == 220) {
        //    Projectile.tileCollide = true;
        //}

        if (Projectile.alpha > 0) Projectile.alpha -= 10;

        Projectile.velocity *= 0.98f;

        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 4) Projectile.frame = 0;

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

            //NPC ownerMinionAttackTargetNPC2 = Projectile.OwnerMinionAttackTargetNPC;
            //if (ownerMinionAttackTargetNPC2 != null && ownerMinionAttackTargetNPC2.CanBeChasedBy(this)) {
            //    float distanceBetween = Vector2.Distance(ownerMinionAttackTargetNPC2.Center, Projectile.Center);
            //    float neededDistance = targetDistance;
            //    if (distanceBetween < neededDistance && !targetFoundByOwner) {
            //        if (Collision.CanHit(Projectile.Center, 1, 1, ownerMinionAttackTargetNPC2.Center, 1, 1)) {
            //            targetDistance = distanceBetween;
            //            targetPosition = ownerMinionAttackTargetNPC2.Center;
            //            targetFoundByOwner = true;
            //        }
            //    }
            //}

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

        //speed = 9f;
        //acceleration = 0.2f;

        speed = 6.8f;
        acceleration = 0.14f;

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

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.penetrate > 0) {
            Projectile.penetrate--;
            if (Projectile.velocity.X != Projectile.oldVelocity.X)
                Projectile.velocity.X = 0f - Projectile.oldVelocity.X;

            if (Projectile.velocity.Y != Projectile.oldVelocity.Y)
                Projectile.velocity.Y = 0f - Projectile.oldVelocity.Y;

            return false;
        }

        return base.OnTileCollide(oldVelocity);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.expertMode) {
            if (target.type >= 13 && target.type <= 15)
                modifiers.FinalDamage /= 5;
        }
    }

    public override void SafePostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 3) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }

        if (Projectile.frame >= 3)
            Projectile.frame = 0;
    }

    public override void OnKill(int timeLeft) {
        for (int num510 = 0; num510 < 6; num510++) {
            int num511 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 150, Projectile.velocity.X, Projectile.velocity.Y, 50);
            Main.dust[num511].noGravity = true;
            Main.dust[num511].scale = 1f;
        }
    }
}
