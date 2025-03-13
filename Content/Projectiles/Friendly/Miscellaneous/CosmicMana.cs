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

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
    }

    public override void SetDefaults() {
        int width = 16; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.tileCollide = false;
        Projectile.aiStyle = 1;
        Projectile.timeLeft = 145;
        Projectile.alpha = 255;
        Projectile.friendly = true;
        Projectile.hostile = false;
        AIType = 14;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Projectile.rotation += Projectile.velocity.X * 0.2f;
        if (player.active && (double)Vector2.Distance(player.Center, Projectile.Center) < 35.0) {
            int newMana = Main.rand.Next(9, 16);
            player.statMana += newMana;
            player.ManaEffect(newMana);
            Projectile.Kill();
        }
        if (Projectile.position.Y > player.Bottom.Y) {
            Projectile.tileCollide = true;
        }
        if (Projectile.soundDelay == 0) {
            Projectile.soundDelay = 20 + Main.rand.Next(40);
            SoundStyle sound = SoundID.Item9;
            SoundEngine.PlaySound(sound.WithVolumeScale(0.5f), Projectile.position);
        }
        for (int num28 = Projectile.oldPos.Length - 1; num28 > 0; num28--) {
            Projectile.oldPos[num28] = Projectile.oldPos[num28 - 1];
            Projectile.oldRot[num28] = Projectile.oldRot[num28 - 1];
        }

        Projectile.oldPos[0] = Projectile.position;
        Projectile.oldRot[0] = Projectile.rotation;
        for (int i = 0; i < 1; i++)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.2f);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        float lifetime = Projectile.timeLeft < 25 ? Projectile.timeLeft / 25f : 1f;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
        miscShaderData.UseSaturation(-2.8f);
        miscShaderData.UseOpacity(1f);
        miscShaderData.Apply();
        vertexStrip.PrepareStripWithProceduralPadding(
            Projectile.oldPos,
            Projectile.oldRot,
            p => Color.Lerp(Color.DarkBlue.MultiplyAlpha(lifetime * (p <= 0.25 ? p / 0.25f : 1f)), Color.OrangeRed.MultiplyAlpha(0.5f), p), 
            p => (float)(60.0 * Projectile.scale * (1.0 - p)),
            -Main.screenPosition + Projectile.Size / 2, true);
        vertexStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spriteBatch.EndBlendState();
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        spriteBatch.Draw(projectileTexture, Projectile.Center - Main.screenPosition, new Rectangle?(), new Color(0.9f, 1f, 0.7f, 0.4f), Main.GlobalTimeWrappedHourly * 8f * Projectile.direction + Projectile.whoAmI, projectileTexture.Size() / 2f, Projectile.scale - (float)(0.15f * Math.Sin(Main.time / 10.0)), SpriteEffects.None, 0.0f);
        return false;
    }

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
