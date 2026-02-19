using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

using static Terraria.GameContent.Animations.Actions.Sprites;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeTrap : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    private Vector2 _scale;

    public override void SetStaticDefaults() {

    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
        _scale = new Vector2(1.5f, 0f);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox = hitbox.AdjustY(-80).AdjustHeight(80 - Projectile.height);
        hitbox.Inflate(20, 0);
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            Projectile.SetDirection(Projectile.ai[1] == 0f ? Projectile.GetOwnerAsPlayer().direction : (int)Projectile.ai[1]);
            if (Main.rand.NextBool()) {
                Projectile.SetDirection(-Projectile.spriteDirection);
            }

            int attempts = 16;
            while (attempts-- > 0) {
                if (!WorldGenHelper.SolidTile(Projectile.Bottom.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y -= 1f;
            }
            attempts = 16;
            while (attempts-- > 0) {
                if (WorldGenHelper.SolidTile(Projectile.Bottom.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y += 1f;
            }
        }

        Vector2 desiredScale = Vector2.One;

        float lerpModifier = 1.25f;
        _scale.X = Helper.Approach(_scale.X, 1f, 0.1f * lerpModifier);
        _scale.Y = Helper.Approach(_scale.Y, 1f, 0.2f * lerpModifier);
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f * lerpModifier);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Vector2 origin = texture.Bounds.BottomCenter();
        float opacity2 = Ease.QuadOut(Projectile.Opacity);
        Vector2 position = Projectile.position;
        Projectile.position.Y += 10f;
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
