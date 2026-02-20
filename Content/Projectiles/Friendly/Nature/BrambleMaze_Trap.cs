using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeTrap : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(2);

    private Vector2 _scale;
    private bool _shouldDisappear;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(2);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
        _scale = new Vector2(1.5f, 0f);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox = hitbox.AdjustY(-80).AdjustHeight(80 - Projectile.height);
        hitbox.Inflate(20, 0);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override void AI() {
        if (Projectile.GetOwnerAsPlayer().GetCommon().IsBrambleMazePlaced && !_shouldDisappear) {
            _shouldDisappear = true;
        }
        if (!_shouldDisappear) {
            Projectile.timeLeft++;
        }

        if (Projectile.localAI[1] == 0f) {
            Projectile.GetOwnerAsPlayer().GetCommon().IsBrambleMazePlaced = true;

            Projectile.localAI[1] = 1f;

            int dustType = TileHelper.GetKillTileDust((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16 + 1, Main.tile[(int)Projectile.position.X / 16, (int)Projectile.position.Y / 16 + 1]);
            for (int k = 0; k < 9; k++) { 
                int dust = Dust.NewDust(Projectile.Center, 60, 6, dustType, 0, Main.rand.NextFloat(-5f, -1f));
                Main.dust[dust].position.X = Projectile.Center.X + 40f * Main.rand.NextFloatDirection() - 10f;
                Main.dust[dust].position.Y = ((int)Projectile.position.Y / 16 + 1) * 16;
                Main.dust[dust].scale *= 1.2f;
            }

            Projectile.SetDirection(Projectile.ai[1] == 0f ? Projectile.GetOwnerAsPlayer().direction : (int)Projectile.ai[1]);
            if (Main.rand.NextBool()) {
                Projectile.SetDirection(-Projectile.spriteDirection);
            }

            int attempts = 16;
            while (attempts-- > 0) {
                if (!WorldGenHelper.SolidTile(Projectile.BottomLeft.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y -= 1f;
            }
            attempts = 16;
            while (attempts-- > 0) {
                if (WorldGenHelper.SolidTile(Projectile.BottomLeft.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y += 1f;
            }
            Projectile.position += Helper.OffsetPerSolidTileSlope_Bottom(WorldGenHelper.GetTileSafely(Projectile.BottomLeft.ToTileCoordinates()));
        }

        Projectile.velocity *= 0f;

        if (Projectile.frameCounter++ > 4) {
            Projectile.frame = 1;
        }

        Vector2 desiredScale = Vector2.One;

        float lerpModifier = 1f;
        _scale.X = Helper.Approach(_scale.X, 1f, 0.1f * lerpModifier);
        _scale.Y = Helper.Approach(_scale.Y, 1f, 0.2f * lerpModifier);
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f * lerpModifier);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Vector2 origin = Utils.Frame(texture, 1, Projectile.GetFrameCount()).BottomCenter();
        float opacity2 = Ease.QuadOut(Projectile.Opacity);
        opacity2 *= Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);
        Vector2 position = Projectile.position;
        Projectile.position.Y += 12f;
        if (Projectile.ai[1] > 0) {
            Projectile.position.X -= 5f * Projectile.ai[1];
        }
        else {
            Projectile.position.X += 9f * Projectile.ai[1];
        }
        Projectile.QuickDrawAnimated(lightColor * opacity2, origin: origin, scale: new Vector2(_scale.X, Ease.CubeOut(_scale.Y)));
        Projectile.position = position;

        return false;
    }
}
