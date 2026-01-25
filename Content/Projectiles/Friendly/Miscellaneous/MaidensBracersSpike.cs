using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class MaidensBracersSpike : ModProjectile {
    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.Opacity = 0f;

        Projectile.ignoreWater = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        Projectile.timeLeft = 2;

        Projectile.Center = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint();

        float mult = 1.2f;

        Projectile.localAI[0] += 0.25f * mult;
        if (Projectile.localAI[0] >= 5f) {
            float to = 0.65f;
            Projectile.localAI[1] = Helper.Approach(Projectile.localAI[1], to, 0.05f * mult);
            if (Projectile.localAI[1] >= to) {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.2f);
                if (Projectile.Opacity <= 0f) {
                    Projectile.Kill();
                }
            }
        }
        else {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        int count = 21;
        Player owner = Projectile.GetOwnerAsPlayer();
        Color color;
        float AreaSize = 40f;
        float AREASIZE = 100f;
        for (int i = 0; i < count; i++) {
            Texture2D texture = Projectile.GetTexture();
            Rectangle clip = texture.Bounds;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float circleFactor = MathHelper.TwoPi / count * i - MathHelper.Pi + Projectile.localAI[0] * 0.05f;
            float visualTimer = (float)Main.timeForVisualEffects * 0.1f;
            visualTimer = Projectile.localAI[0];
            float maxOffset = 5f;
            Vector2 position = Projectile.Center;
            Vector2 drawPos = position + Utils.ToRotationVector2(circleFactor) * AreaSize * 0.95f;
            float rotation = circleFactor - MathHelper.PiOver2;
            float areaFactor0 = AreaSize / AREASIZE * 1.5f;
            Vector2 areaFactor = new(1f, areaFactor0);
            //rotation += MathF.Sin(circleFactor + visualTimer) * 0.3f;
            position += Utils.ToRotationVector2(circleFactor) * (AreaSize + MathF.Sin(circleFactor * 7.5f + visualTimer) * maxOffset) * (1f - Projectile.localAI[1]);
            float rotation1 = rotation;
            Color baseColor = Color.White;
            color = baseColor * Projectile.Opacity * 0.575f;
            Vector2 origin = clip.Centered();
            Main.spriteBatch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color
            });
        }

        return false;
    }
}
