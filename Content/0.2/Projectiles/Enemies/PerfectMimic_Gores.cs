using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

class PerfectMimicArm : PerfectMimicHead {
    public override string Texture => ResourceManager.TarEnemyNPCTextures + "PerfectMimic_GoreArm";
}

class PerfectMimicLeg : PerfectMimicHead {
    public override string Texture => ResourceManager.TarEnemyNPCTextures + "PerfectMimic_GoreLeg";
}

class PerfectMimicBody : PerfectMimicHead {
    public override string Texture => ResourceManager.TarEnemyNPCTextures + "PerfectMimic_GoreBody";
}

class PerfectMimicHead : ModProjectile {
    public static Color SkinColor;

    public override string Texture => ResourceManager.TarEnemyNPCTextures + "PerfectMimic_GoreHead";

    public override void SetDefaults() {
        Projectile.Size = new Vector2(14, 14);
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.timeLeft = Gore.goreTime;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;

            Projectile.velocity.Y -= (float)Main.rand.Next(10, 31) * 0.1f;
            Projectile.velocity.X += (float)Main.rand.Next(-20, 21) * 0.1f;
        }

        if (Projectile.timeLeft <= 2) {
            Projectile.timeLeft = 2;
            Projectile.alpha += 1;
            if (Projectile.alpha >= 255) {
                Projectile.alpha = 255;
                Projectile.Kill();
                return;
            }
        }

        Projectile.direction = Projectile.velocity.X.GetDirection();
        Projectile.rotation += Projectile.velocity.X * 0.1f;
        if (Projectile.velocity.Y == 0f) {
            Projectile.velocity.X *= 0.97f;
            if ((double)Projectile.velocity.X > -0.01 && (double)Projectile.velocity.X < 0.01)
                Projectile.velocity.X = 0f;
        }
        Projectile.velocity.Y += 0.1f;
        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);

        int chance = 40;
        if (Main.rand.NextBool(chance) && Projectile.Opacity >= 1f) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f, ModContent.DustType<Dusts.TarDebuff>());
            dust.alpha = 150;
            dust.velocity.X *= 0.1f;
        }

        if (Projectile.lavaWet) {
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Color color = PerfectMimicHead.SkinColor.MultiplyRGB(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) * Projectile.Opacity;
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame frame = new(2, 1, 0, 0);
        Rectangle clip = frame.GetSourceRectangle(texture);
        Projectile.QuickDraw(color, sourceRectangle: clip, origin: clip.Centered());
        frame = frame.With(1, 0);
        clip = frame.GetSourceRectangle(texture);
        Projectile.QuickDraw(lightColor * Projectile.Opacity, sourceRectangle: clip, origin: clip.Centered());

        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[1] != 1f) {
            Projectile.ai[1] = 1f;
        }
        else {
            //Projectile.velocity *= 0.97f;
        }

        return false;
    }
}
