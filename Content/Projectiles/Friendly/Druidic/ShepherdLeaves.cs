using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class ShepherdLeaves : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    protected override void SafeSetDefaults() {
        Projectile.Size = 8 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 100;
        Projectile.netImportant = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 0.2f;
        }
        Player player = Main.player[Projectile.owner];
        if (Projectile.IsOwnerMyPlayer(player)) {
            Vector2 pointPosition = Main.player[Projectile.owner].GetViableMousePosition();
            Projectile.ai[1] = pointPosition.X;
            Projectile.ai[2] = pointPosition.Y;
            Projectile.netUpdate = true;
        }
        if (Projectile.ai[0] > 0.1f) {
            Projectile.ai[0] -= TimeSystem.LogicDeltaTime;
        }
        Vector2 mousePosition = new(Projectile.ai[1], Projectile.ai[2]);
        Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Helper.VelocityToPoint(Projectile.Center, mousePosition, 6f), Projectile.ai[0]);
        Projectile.position += Projectile.velocity;
    }

    public override void PostAI() {
        if (Main.netMode != NetmodeID.Server) {
            Dust dust = Main.dust[Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(3f, 3f), 0, 0, DustID.AmberBolt, Scale: Main.rand.NextFloat(1f, 1.3f))];
            dust.velocity *= 0.4f;
            dust.noGravity = true;
        }

        ProjectileHelper.Animate(Projectile, 4);
    }
}