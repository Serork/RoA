using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class HornetSpear : ModProjectile {
    private static ushort STARTDISAPPEARINGTIME => 15;

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.hide = true;

        Projectile.ownerHitCheck = true;
        Projectile.ownerHitCheckDistance = 300f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.ai[1] != 0f) {
            Projectile.ai[1] = 1f;
            Projectile.netUpdate = true;
        }
    }

    public override bool? CanDamage() => Projectile.timeLeft > STARTDISAPPEARINGTIME;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float f = Projectile.rotation - MathHelper.PiOver2;
        float collisionPoint2 = 0f;
        float num10 = 95f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + f.ToRotationVector2() * num10, 20f * Projectile.scale, ref collisionPoint2)) {
            return true;
        }

        return false;
    }

    public override void AI() {
        if (Projectile.ai[0] != 0f) {
            Projectile.timeLeft = (int)Projectile.ai[0];
            Projectile.ai[0] = 0f;
        }
        if (Projectile.ai[1] != 1f) {
            Projectile.timeLeft = STARTDISAPPEARINGTIME;
            Projectile.ai[1] = 0f;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        if (!owner.IsAlive()) {
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * owner.gfxOffY;

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnKill(int timeLeft) {
        base.OnKill(timeLeft);
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor);

        return false;
    }
}
