using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Pets;

sealed class SmallMoon : ModProjectile {
    private static Asset<Texture2D> _lightTexture = null!;

    private float glowAlpha;
    private bool glowAlphaIncrease;

    public override void SetStaticDefaults() {
        Main.projPet[Projectile.type] = true;
        ProjectileID.Sets.LightPet[Projectile.type] = true;
        Main.projFrames[Projectile.type] = 4;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;

        if (Main.dedServ) {
            return;
        }

        _lightTexture = ModContent.Request<Texture2D>(ResourceManager.VisualEffectTextures + "SmallLight");
    }

    public override void SetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.netImportant = true;

        Projectile.timeLeft *= 5;

        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

        Projectile.scale = 1.0f;
        Projectile.alpha = 100;
        Projectile.light = 0.3f;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        var smallMoonPlayer = player.GetModPlayer<SmallMoonPlayer>();

        if (Main.moonPhase == 0 || Main.moonPhase == 1) Projectile.frame = 0;
        if (Main.moonPhase == 2 || Main.moonPhase == 3) Projectile.frame = 1;
        if (Main.moonPhase == 4 || Main.moonPhase == 5) Projectile.frame = 2;
        if (Main.moonPhase == 6 || Main.moonPhase == 7) Projectile.frame = 3;

        if (!Main.dedServ)
            Lighting.AddLight(Projectile.Center, smallMoonPlayer.smallMoonColor.ToVector3());

        double deg = (double)Projectile.ai[1] / 2;
        double rad = deg * (Math.PI / 180);
        double dist = player.width + player.height;
        if (Projectile.ai[2] == 0f) {
            Projectile.ai[2] = (float)dist;
        }
        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], (float)dist, 0.01f);
        Projectile.Center = player.MountedCenter;
        Projectile.Center = Utils.Floor(Projectile.Center);
        Projectile.Center = new Vector2(Projectile.Center.X - (int)(Math.Cos(rad) * Projectile.ai[2]), Projectile.Center.Y - (int)(Math.Sin(rad) * Projectile.ai[2]) + player.gfxOffY);

        if (player.name == "has2r") Projectile.ai[1] -= 3f;
        else Projectile.ai[1] += 3f;

        ++Projectile.ai[0];
        if (Projectile.ai[0] > 120.0) Projectile.ai[0] = 0.0f;

        if (!player.HasBuff(ModContent.BuffType<Buffs.SmallMoon>())) {
            Projectile.Kill();
        }

        for (int l = 0; l < 2; l++) {
            if (Main.rand.NextBool(3)) {
                int num54 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 20, 20, DustID.RainbowMk2);
                Main.dust[num54].velocity *= 0.2f;
                Main.dust[num54].noGravity = true;
                Main.dust[num54].scale = 1.25f;
                Main.dust[num54].color = smallMoonPlayer.smallMoonColor;
                Main.dust[num54].shader = GameShaders.Armor.GetSecondaryShader(player.cLight, player);
            }
        }

        if (player.dead)
            player.GetModPlayer<SmallMoonPlayer>().smallMoon = false;
        if (!player.GetModPlayer<SmallMoonPlayer>().smallMoon)
            return;
        Projectile.timeLeft = 2;
    }

    public override Color? GetAlpha(Color lightColor) {
        Player player = Main.player[Projectile.owner];
        var smallMoonPlayer = player.GetModPlayer<SmallMoonPlayer>();
        Color SmallMoonColorAdditive = smallMoonPlayer.smallMoonColor;
        SmallMoonColorAdditive.A = 0;
        return SmallMoonColorAdditive;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player player = Main.player[Projectile.owner];
        var shader = GameShaders.Armor.GetSecondaryShader(player.cLight, player);
        ArmorShaderData armorShaderData = null;
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
        if (shader != armorShaderData) {
            spriteBatch.End();
            armorShaderData = shader;
            if (armorShaderData == null) {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
            }
            else {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                shader.Apply(null);
            }
        }

        Texture2D texture = Projectile.GetTexture();
        Texture2D glowTexture = _lightTexture.Value;
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Rectangle glowframeRect = new Rectangle(0, 0, glowTexture.Width, glowTexture.Height);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, Projectile.scale - k * 0.1f, SpriteEffects.None, 0f);
        }

        if (glowAlpha < 1f) {
            if (glowAlphaIncrease)
                glowAlpha += 0.02f;
        }
        else glowAlphaIncrease = false;
        if (glowAlpha > 0.6f) {
            if (!glowAlphaIncrease)
                glowAlpha -= 0.01f;
        }
        else glowAlphaIncrease = true;
        Vector2 glowDrawPos = Projectile.oldPos[0] - Main.screenPosition + drawOrigin - new Vector2(20f, 18f);
        Color color2 = Projectile.GetAlpha(lightColor) * glowAlpha;
        float globalTimeWrappedHourly2 = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly2 %= 5f;
        globalTimeWrappedHourly2 /= 2.5f;
        if (globalTimeWrappedHourly2 >= 1f)
            globalTimeWrappedHourly2 = 2f - globalTimeWrappedHourly2;
        color2.A = (byte)(60 + 100 * globalTimeWrappedHourly2);
        spriteBatch.Draw(glowTexture, glowDrawPos, glowframeRect, color2, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        spriteBatch.Draw(texture, Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY), frameRect, color2, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        spriteBatch.EndBlendState();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        return false;
    }
}
