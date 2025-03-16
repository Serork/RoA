using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class CosmicMana : ModProjectile {
    private readonly VertexStrip vertexStrip = new VertexStrip();

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Cosmic Mana");

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        int width = 18; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.alpha = 0;
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = (int)(Projectile.ai[0] + 1) * 60;

            Projectile.timeLeft = (int)Projectile.localAI[0];
        }

        double deg = (double)(Projectile.ai[1] + Projectile.ai[0] * 240) / 2;
        double rad = deg * (Math.PI / 180);
        double dist = 100;
        Projectile.position.X = MathHelper.SmoothStep(Projectile.position.X, 
            player.MountedCenter.X - (int)(Math.Cos(rad) * dist) - player.width / 2 + Projectile.velocity.X, 0.15f);
        Projectile.position.Y = MathHelper.SmoothStep(Projectile.position.Y,
            player.MountedCenter.Y - (int)(Math.Sin(rad) * dist) - player.height / 2 + 4 + player.gfxOffY + Projectile.velocity.Y, 0.15f);
        Projectile.ai[1] += 3f;
        Projectile.velocity = -Helper.VelocityToPoint(player.MountedCenter, Projectile.Center, 1f) * Projectile.ai[1] / 200f;

        int maxTimeLeft = (int)Projectile.localAI[0];
        Projectile.Opacity = Utils.GetLerpValue(0, 15, Projectile.timeLeft, true) *
            Utils.GetLerpValue(maxTimeLeft, maxTimeLeft - 15, Projectile.timeLeft, true);
        Projectile.rotation = Projectile.ai[1] * 0.025f + Projectile.ai[0] * 0.1f;

        if (Projectile.soundDelay == 0) {
            Projectile.soundDelay = 20 + Main.rand.Next(40);
            SoundStyle sound = SoundID.Item9;
            SoundEngine.PlaySound(sound.WithVolumeScale(0.5f), Projectile.position);
        }
        if (Main.rand.NextBool())
        for (int i = 0; i < 1; i++)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.2f);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        float lifetime = Projectile.timeLeft < 25 ? Projectile.timeLeft / 25f : 1f;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
        miscShaderData.UseSaturation(-2.8f);
        miscShaderData.UseOpacity(2f);
        miscShaderData.Apply();
        vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot,
            StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
        vertexStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spriteBatch.EndBlendState();
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        spriteBatch.Draw(projectileTexture, Projectile.Center - Main.screenPosition, new Rectangle?(),
            new Color(0.9f, 1f, 0.7f, 0.5f) * Projectile.Opacity, Projectile.rotation, projectileTexture.Size() / 2f,
            Projectile.scale, SpriteEffects.None, 0.0f);
        for (int i = 0; i < 4; i++) {
            spriteBatch.Draw(projectileTexture, Projectile.Center - Main.screenPosition, new Rectangle?(), 
                new Color(0.9f, 1f, 0.7f, 0.3f) * Projectile.Opacity * 0.75f, 
                Main.GlobalTimeWrappedHourly * i * 3f * Projectile.direction + Projectile.rotation, projectileTexture.Size() / 2f, Projectile.scale - (float)(0.15f * Math.Sin(Main.time / 10.0 * i)), SpriteEffects.None, 0.0f);
        }
        return false;
    }
    private Color StripColors(float progressOnStrip) {
        float num = 1f - progressOnStrip;
        Color result = new Color(48, 63, 150) * (num * num * num * num) ;
        return result;
    }

    private float StripWidth(float progressOnStrip) => 16f;

    public override Color? GetAlpha(Color lightColor)
        => new Color?(Color.White);

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 62, Main.rand.Next(-4, 4), Main.rand.Next(-4, 4), 150, default, 1.2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 3f * Main.rand.NextFloat();
        }
    }
}
