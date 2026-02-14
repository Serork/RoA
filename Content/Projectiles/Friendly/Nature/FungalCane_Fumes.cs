using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FungalCaneFumes : NatureProjectile {
    private Color _color;

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

    protected override void SafeSetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;

        Projectile.timeLeft = 250;
        Projectile.tileCollide = true;

        Projectile.friendly = true;

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = 50;

        Projectile.appliesImmunityTimeOnSingleHits = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;

        ShouldChargeWreathOnDamage = false;

        Projectile.aiStyle = -1;

        Projectile.manualDirectionChange = true;
    }

    //public override bool? CanDamage() => Projectile.Opacity >= 0.3f;

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Opacity -= 0.1f;
        return false;
    }

    public override void AI() {
        Projectile.localAI[2] = Helper.Approach(Projectile.localAI[2], 1f, 0.2f);

        if (Projectile.localAI[0] == 0f) {
            if (Projectile.owner == Main.myPlayer) {
                Projectile.ai[1] = Main.rand.NextFloat(0.75f, 1f);
                Projectile.netUpdate = true;
            }

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

            Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);

            Projectile.localAI[0] = 1f;

            _color = Color.Lerp(new Color(135, 137, 225), new Color(95, 98, 215), Main.rand.NextFloat());
            _color = _color with { A = 0 };
        }

        if (Projectile.ai[1] != 0f) {
            Projectile.Opacity = Projectile.ai[1];
            Projectile.localAI[0] = Projectile.Opacity;
            Projectile.ai[1] = 0f;
        }

        if (++Projectile.frameCounter >= 8) {
            Projectile.frameCounter = 0;
            //if (++Projectile.frame >= Main.projFrames[Projectile.type])
            //    Projectile.frame = 0;
        }

        if (Projectile.Opacity > 0f) {
            Projectile.Opacity -= Projectile.localAI[0] * 0.025f * 0.3f;
        }
        else {
            Projectile.Kill();
        }

        Projectile.velocity *= 0.995f;

        Projectile.rotation += Projectile.velocity.X * 0.01f;
    }

    public override Color? GetAlpha(Color lightColor) => _color.MultiplyRGB(lightColor) * Projectile.Opacity * 0.5f;

    //public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Vector2 drawOrigin = frameRect.Centered();
        Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin;
        Color color = Projectile.GetAlpha(lightColor) * 0.5f;
        for (int i = 0; i < 2; i++)
            spriteBatch.Draw(texture, drawPos + new Vector2(0, (i == 1 ? 2f : -2f) * (1f - Projectile.Opacity) * 2f).RotatedBy(TimeSystem.TimeForVisualEffects * 4f), frameRect, color * Projectile.localAI[2], Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}
