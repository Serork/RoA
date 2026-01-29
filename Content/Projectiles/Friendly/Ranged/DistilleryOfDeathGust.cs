using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class DistilleryOfDeathGust : ModProjectile {
    public enum GustType : byte {
        Up,
        Down,
        Magnet,
        Push,
        Count
    }

    private Vector2 _initialSpeed;

    public ref float GustTypeValue => ref Projectile.ai[0];
    public ref float VisualOffsetValue => ref Projectile.localAI[0];

    public GustType CurrentGustType {
        get => (GustType)GustTypeValue;
        set => GustTypeValue = (float)value;
    }

    public Color GetColorPerType() {
        Color result = CurrentGustType switch {
            GustType.Up => new Color(80, 255, 120),
            GustType.Down => new Color(255, 150, 100),
            GustType.Magnet => new Color(100, 150, 255),
            _ => new Color(240, 125, 200),
        };

        result = result.ModifyRGB(0.75f);

        return result;
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(30);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (VisualOffsetValue == 0f) {
            VisualOffsetValue = Main.rand.NextFloat(0.1f, 10f);

            _initialSpeed = Projectile.velocity;
        }

        float ySpeed = 0.05f;
        Projectile projectile = Projectile;
        int width = 100;
        switch (CurrentGustType) {
            case GustType.Up:
                Projectile.velocity.Y -= ySpeed;
                if (Projectile.velocity.Y < -16f)
                    Projectile.velocity.Y = -16f;
                break;
            case GustType.Down:
                Projectile.velocity.Y += ySpeed;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
                break;
            case GustType.Magnet:
                for (int i = 0; i < Main.projectile.Length; i++) {
                    Projectile projectile2 = Main.projectile[i];
                    if (i != projectile.whoAmI && projectile2.active && projectile2.type == projectile.type && Math.Abs(projectile.position.X - projectile2.position.X) + Math.Abs(projectile.position.Y - projectile2.position.Y) < width) {
                        Vector2 destination = projectile2.Center;
                        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
                        float minDistance = 100f;
                        float inertiaValue = 200f, extraInertiaValue = inertiaValue * 5;
                        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
                        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
                        Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);
                    }
                }
                break;
            case GustType.Push:
                for (int i = 0; i < Main.projectile.Length; i++) {
                    Projectile projectile2 = Main.projectile[i];
                    if (i != projectile.whoAmI && projectile2.active && projectile2.type == projectile.type && Math.Abs(projectile.position.X - projectile2.position.X) + Math.Abs(projectile.position.Y - projectile2.position.Y) < width * 2) {
                        Vector2 destination = projectile2.Center;
                        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
                        float minDistance = 100f;
                        float inertiaValue = 200f, extraInertiaValue = inertiaValue * 5;
                        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
                        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
                        Helper.InertiaMoveAway(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);
                    }
                }
                break;
        }

        Projectile.OffsetTheSameProjectile(0.05f);

        Projectile.position += _initialSpeed.SafeNormalize();

        Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.005f * (float)Projectile.direction;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle clip = Utils.Frame(texture, 1, Projectile.GetFrameCount(), frameY: Projectile.frame);
        Vector2 origin = clip.Centered();
        SpriteEffects flip = SpriteEffects.None;
        Color baseColor = GetColorPerType().MultiplyRGB(lightColor);
        float rotation = Projectile.rotation;
        float scale = 1f;
        for (float num11 = 0f; num11 < 1f; num11 += 1f / 3f) {
            float num12 = (TimeSystem.TimeForVisualEffects + VisualOffsetValue) % 2f / 1f * Projectile.direction;
            Color color = Main.hslToRgb((num12 + num11) % 1f, 1f, 0.5f).MultiplyRGB(baseColor);
            color.A = 0;
            color *= 0.5f;
            for (int j = 0; j < 2; j++) {
                for (int k = 0; k < 2; k++) {
                    Vector2 drawPosition = position + ((num12 + num11) * ((float)Math.PI * 2f)).ToRotationVector2() * 4f;
                    batch.Draw(texture, drawPosition, clip, Color.Lerp(baseColor, color, 0.5f), rotation, origin, scale, flip, 0f);
                }
            }
        }
        baseColor.A = 100;
        baseColor *= 0.75f;
        batch.Draw(texture, position, clip, baseColor, rotation, origin, scale, flip, 0f);

        return false;
    }
}
