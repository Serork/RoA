using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeRoot : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            Projectile.frame = Main.rand.Next(4);

            Projectile.SetDirection(Projectile.ai[1] == 0f ? Projectile.GetOwnerAsPlayer().direction : (int)Projectile.ai[1]);

            int attempts = 1;
            while (attempts-- > 0) {
                if (!WorldGenHelper.SolidTile(Projectile.Center.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y -= 8f;
            }
            attempts = 10;
            while (attempts-- > 0) {
                if (WorldGenHelper.SolidTile(Projectile.Center.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y += 8f;
            }
            Projectile.position.Y -= 8f;
        }


        float max = 50f;
        if (Projectile.IsOwnerLocal() && Projectile.localAI[0] == 0f && Projectile.ai[0] < max) {
            Projectile.localAI[0] = 1f;

            Projectile.ai[0]++;

            Player player = Projectile.GetOwnerAsPlayer();
            Vector2 mousePosition = player.GetViableMousePosition();
            float xDif = mousePosition.X - Projectile.Center.X;
            if (MathF.Abs(xDif) <= 8f) {
                Projectile.ai[0] = max;
            }
            int direction = xDif.GetDirection();

            Vector2 position = Projectile.Center + Vector2.UnitX * Projectile.width * Projectile.direction;
            int damage = Projectile.damage;
            float knockBack = Projectile.knockBack;
            if (Projectile.ai[0] == max) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeTrap>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_FromThis()) {
                    Position = position,
                    Damage = damage,
                    KnockBack = knockBack,
                    AI1 = direction
                });
                return;
            }
            ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeRoot>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_FromThis()) {
                Position = position,
                Damage = damage,
                KnockBack = knockBack,
                AI0 = Projectile.ai[0],
                AI1 = direction
            });
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor);

        return false;
    }

    public override void OnKill(int timeLeft) {
        
    }
}
