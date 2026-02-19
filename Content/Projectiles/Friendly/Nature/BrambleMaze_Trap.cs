using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeTrap : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    public override void SetStaticDefaults() {

    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            Projectile.SetDirection(Projectile.ai[1] == 0f ? Projectile.GetOwnerAsPlayer().direction : (int)Projectile.ai[1]);
            if (Main.rand.NextBool()) {
                Projectile.SetDirection(-Projectile.spriteDirection);
            }

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
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Vector2 origin = texture.Bounds.BottomCenter();
        Projectile.QuickDrawAnimated(lightColor, origin: origin);

        return false;
    }
}
