using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CoralBubble : NatureProjectile {
    private static ushort TIMELEFT => 120;

    public ref float BubbleSquishX => ref Projectile.ai[1];
    public ref float BubbleSquishY => ref Projectile.ai[2];
    public ref float SquishVelocityX => ref Projectile.localAI[1];
    public ref float SquishVelocityY => ref Projectile.localAI[2];

    public Vector2 SquishVelocity {
        get => new(SquishVelocityX, SquishVelocityY);
        set {
            SquishVelocityX = value.X;
            SquishVelocityY = value.Y;
        } 
    }

    public Vector2 BubbleSquish {
        get => new(BubbleSquishX, BubbleSquishY);
        set {
            BubbleSquishX = value.X;
            BubbleSquishY = value.Y;
        }
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();
        Vector2 destination = owner.GetWorldMousePosition();
        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
        float minDistance = 60f;
        float inertiaValue = 5f, extraInertiaValue = inertiaValue * 5f;
        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
        float speed = 7.5f;
        Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, speed: speed, inertia: inertia);
        owner.SyncMousePosition();
        float offsetSpeed = 2.5f;
        Projectile.OffsetTheSameProjectile(offsetSpeed);
        if (Projectile.NearestTheSame(out Projectile projectile)) {
            if (Projectile.position.X < projectile.position.X) {
                Projectile.position.X -= offsetSpeed;
            }
            else {
                Projectile.position.X += offsetSpeed;
            }
            if (Projectile.position.Y < projectile.position.Y) {
                Projectile.position.Y -= offsetSpeed;
            }
            else {
                Projectile.position.Y += offsetSpeed;
            }
        }
        Projectile.rotation = Projectile.velocity.X * 0.025f;
        float squishVelocityBlend = 0.1f;
        float interpolationBlend = 0.1f;
        float movementInfluence = 0.5f;
        float maxSquishDeformation = 0.1f;
        float velocityChangeThreshold = 10f;
        Vector2 targetSquish = Vector2.One;
        Vector2 absoluteVelocity = new(Math.Abs(Projectile.velocity.X), Math.Abs(Projectile.velocity.Y));
        float movementRatio = MathHelper.Clamp((1f + absoluteVelocity.X * movementInfluence) / (1f + absoluteVelocity.Y * movementInfluence), 1f - maxSquishDeformation, 1f + maxSquishDeformation);
        targetSquish.X *= movementRatio;
        targetSquish.Y /= movementRatio;
        Vector2 squishDirection = (targetSquish - BubbleSquish).SafeNormalize(Vector2.One);
        if (Vector2.Distance(targetSquish, BubbleSquish) > 0.05f) {
            SquishVelocity = Vector2.Lerp(SquishVelocity, squishDirection, interpolationBlend);
        }
        if (Math.Abs(Projectile.velocity.Length() - Projectile.oldVelocity.Length()) > velocityChangeThreshold) {
            SquishVelocity += squishDirection * 0.5f;
        }
        BubbleSquish += SquishVelocity * squishVelocityBlend;
        Vector2 clampedSquish = new(MathHelper.Clamp(BubbleSquish.X, 1f - maxSquishDeformation, 1f + maxSquishDeformation), MathHelper.Clamp(BubbleSquish.Y, 1f - maxSquishDeformation, 1f + maxSquishDeformation));
        BubbleSquish = Vector2.Lerp(BubbleSquish, clampedSquish, interpolationBlend);
    }

    public override bool PreDraw(ref Color lightColor) {
        Vector2 scale = Vector2.One * BubbleSquish;
        Projectile.QuickDraw(lightColor, scale: scale);

        return false;
    }
}
