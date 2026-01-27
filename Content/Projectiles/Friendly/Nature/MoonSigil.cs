using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;
sealed class MoonSigil : NatureProjectile {
    private int explosionCounter;
    private float cloneDrawRotation, cloneDrawAlpha, cloneDrawReturn;
    private float cloneDrawOffset = 200;

    private Vector2 _mousePosition;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Moon Sigil");
        Main.projFrames[Projectile.type] = 4;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    protected override void SafeSetDefaults() {
        int width = 32; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = -1;

        Projectile.friendly = true;
        Projectile.tileCollide = true;

        Projectile.scale = 1f;
        Projectile.alpha = 255;

        Projectile.penetrate = 6;
        Projectile.timeLeft = 10 * 60;
    }

    public override bool? CanDamage() => false;

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteVector2(_mousePosition);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _mousePosition = reader.ReadVector2();
    }

    public override void AI() {
        if (cloneDrawRotation < 360) cloneDrawRotation += 1f;
        else cloneDrawRotation = 0;

        if (cloneDrawOffset > 5f) {
            cloneDrawOffset = cloneDrawOffset * 0.9f - 0.1f;
            cloneDrawAlpha += 0.02f;
        }
        else {
            cloneDrawOffset = MathHelper.Lerp(cloneDrawOffset, Utils.Remap((float)Math.Sin(TimeSystem.TimeForVisualEffects * 5f), -1f, 1f, 0.5f, 5f), 1f - cloneDrawOffset / 5f);

            if (Main.rand.NextBool(5)) {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - 5f, Projectile.position.Y) - Vector2.One * 3f, 48, 48, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity.Y -= 2f;
                Main.dust[dust].velocity.X *= 0.1f;
            }
        }
        Projectile.frame = (int)Utils.Remap((float)Math.Sin(TimeSystem.TimeForVisualEffects * 5f), -1f, 1f, 0f, 4f);

        if (Projectile.timeLeft == 595) SoundEngine.PlaySound(SoundID.Item82, Projectile.position);
        if (Projectile.timeLeft == 580) {
            for (int i = 0; i < 30; i++) {
                int dust3 = Dust.NewDust(new Vector2(Projectile.position.X - 5f, Projectile.position.Y - 25f), 48, 48, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust3].noGravity = true;
                Main.dust[dust3].noLight = false;
                Main.dust[dust3].velocity.Y += 2f;
                Main.dust[dust3].velocity.X *= 0.1f;
            }
        }
        Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 0.3f);

        if (Projectile.owner == Main.myPlayer) {
            _mousePosition = Main.player[Projectile.owner].GetViableMousePosition();
            Projectile.netUpdate = true;
        }
        Vector2 mousePos = _mousePosition;
        Vector2 projectilePos = new Vector2(Projectile.Center.X, Projectile.Center.Y);
        Vector2 direction = new Vector2(mousePos.X - projectilePos.X, mousePos.Y - projectilePos.Y);
        direction.Normalize();
        direction *= 8;
        Player player = Main.player[Projectile.owner];
        if (player.ItemAnimationJustStarted && player.inventory[player.selectedItem].type == ModContent.ItemType<SacrificialSickleOfTheMoon>() && Projectile.timeLeft < 590) {
            for (int num615 = 0; num615 < 10; num615++) {
                int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.AncientLight, direction.X, direction.Y, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[num616].noGravity = true;
                Dust dust2 = Main.dust[num616];
                dust2.scale *= 1.25f;
                dust2 = Main.dust[num616];
                dust2.velocity *= 0.5f;
            }
            SoundEngine.PlaySound(SoundID.Item92, projectilePos);
            explosionCounter++;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X + 3, Projectile.Center.Y + 3), direction, ModContent.ProjectileType<MoonlightBeam>(), 30, 0, player.whoAmI);
        }
        if (explosionCounter == 5) Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < 3; k++) {
            Vector2 drawPos = Projectile.oldPos[0] - Main.screenPosition + drawOrigin + new Vector2(0, cloneDrawOffset).RotatedBy(MathHelper.ToRadians(cloneDrawRotation + k * 120));
            Color color = Projectile.GetAlpha(lightColor) * cloneDrawAlpha;
            spriteBatch.Draw(texture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        return false;
    }

    public override Color? GetAlpha(Color lightColor)
     => new Color(255, 255, 200, 200);

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 50; i++) {
            int dust3 = Dust.NewDust(new Vector2(Projectile.position.X - 5f, Projectile.position.Y), 48, 48, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust3].noGravity = true;
            Main.dust[dust3].noLight = false;
            Main.dust[dust3].velocity.Y -= 2f;
            Main.dust[dust3].velocity.X *= 0.1f;
        }
    }
}
