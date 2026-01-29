using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Dusts;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class DistilleryOfDeathGustDust2_2 : ModDust {
    public override string Texture => DustLoader.GetDust(ModContent.DustType<DistilleryOfDeathGustDust2>()).Texture;

    public override void OnSpawn(Dust dust) {
        DistilleryOfDeathGustDust2.SpawnSelf(dust);
    }

    public override bool Update(Dust dust) {
        DistilleryOfDeathGustDust2.UpdateSelf(dust);

        return false;
    }

    public override bool PreDraw(Dust dust) {
        DistilleryOfDeathGustDust2.DrawSelf(dust);

        return false;
    }
}


sealed class DistilleryOfDeathGustDust2 : ModDust, IDrawDustPreProjectiles {
    public override void OnSpawn(Dust dust) {
        SpawnSelf(dust);
    }

    public override bool Update(Dust dust) {
        UpdateSelf(dust);

        return false;
    }

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        DrawSelf(dust);
    }

    public static void SpawnSelf(Dust dust) {
        int width = 24; int height = 24;
        int frame = Main.rand.Next(0, 3);
        dust.frame = new Rectangle(0, frame * height, width, height);

        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1f;
    }

    public static void UpdateSelf(Dust dust) {
        bool hasCustomData = dust.customData is float;
        float opacity = Utils.GetLerpValue(0f, 0.2f, 1f - (float)dust.alpha / 255, true);
        dust.position += dust.velocity * 0.12f;
        dust.scale *= 0.99f;
        dust.velocity *= 0.97f;
        dust.alpha += 10;
        if (dust.alpha >= 250) dust.active = false;

        int direction = (dust.velocity.X + dust.velocity.Y).GetDirection();
        dust.rotation += dust.velocity.Length() * 0.0375f * direction;

        Projectile projectile = ((DistilleryOfDeathGust)dust.customData).Projectile;
        dust.position += dust.position.DirectionTo(projectile.position);
    }

    public static void DrawSelf(Dust dust) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = DustLoader.GetDust(dust.type).Texture2D.Value;
        Vector2 position = dust.position - Main.screenPosition;
        Rectangle clip = dust.frame;
        Vector2 origin = clip.Centered();
        SpriteEffects flip = SpriteEffects.None;
        Color lightColor = Lighting.GetColor(dust.position.ToTileCoordinates());
        Color baseColor = DistilleryOfDeathGust.GetColorPerType((DistilleryOfDeathGust.GustType)((DistilleryOfDeathGust)dust.customData).CurrentGustType).MultiplyRGB(lightColor);
        float rotation = dust.rotation;
        float scale = 1.5f * MathF.Max(0.75f, dust.scale);
        float opacity = 0.5f * (1f - dust.alpha / 255f) * 1f * dust.fadeIn * 0.625f * 0.8f;
        for (float num11 = 0f; num11 < 1f; num11 += 1f / 3f) {
            float num12 = (TimeSystem.TimeForVisualEffects) % 2f / 1f;
            Color color = Main.hslToRgb((num12 + num11) % 1f, 1f, 0.5f).MultiplyRGB(baseColor);
            color.A = 0;
            color *= 0.5f;
            for (int j = 0; j < 2; j++) {
                for (int k = 0; k < 2; k++) {
                    Vector2 drawPosition = position + ((num12 + num11) * ((float)Math.PI * 2f)).ToRotationVector2() * 2f;
                    batch.Draw(texture, drawPosition, clip, Color.Lerp(baseColor, color, 0.5f) * opacity, rotation, origin, scale, flip, 0f);
                }
            }
        }
        baseColor.A = 100;
        baseColor *= 0.75f;
        batch.Draw(texture, position, clip, baseColor * opacity, rotation, origin, scale, flip, 0f);
    }

    public override bool PreDraw(Dust dust) => false;
}
