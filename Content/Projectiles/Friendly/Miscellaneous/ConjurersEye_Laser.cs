using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class ConjurersEyeLaser : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    private static ushort TIMELEFT => 90;

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 6);
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.SetSizeValues(14);

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.hide = true;

        Projectile.Opacity = 0f;

        Projectile.timeLeft = TIMELEFT;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override void AI() {
        Projectile.SetTrail(-1, 6);

        if (Projectile.localAI[0] == 0f) {
            Projectile.Center -= Projectile.velocity.SafeNormalize() * -200f;
            Vector2 spawnPosition = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint() + new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.velocity.SafeNormalize() * 5f;
            var unit = Vector2.Normalize(Projectile.velocity);
            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(spawnPosition + Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f),
                    ModContent.DustType<TintableDustGlow>(), unit.RotatedBy(MathHelper.PiOver4 * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(0.25f, 1.25f), Main.rand.Next(100), Color.Lerp(new Color(27, 177, 223), new Color(124, 255, 255), Main.rand.NextFloat()), 
                    MathF.Min(1f, Main.rand.NextFloat(2f))).noGravity = true;

            Projectile.ai[2] = 0.4f * Main.rand.NextFloatDirection();
        }

        Projectile.localAI[0] = Helper.Approach(Projectile.localAI[0], 1f, 1f);

        Projectile.localAI[2] += TimeSystem.LogicDeltaTime * 0.75f;

        if (Projectile.timeLeft < 20) {
            Projectile.Opacity = Utils.GetLerpValue(0, 20, Projectile.timeLeft, true);
        }
        else {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);
        }

        DelegateMethods.v3_1 = Color.Lerp(new Color(27, 177, 223), new Color(124, 255, 255), Helper.Wave(Projectile.localAI[2], 0f, 1f, 20f, Projectile.whoAmI)).ToVector3() * 0.65f;
        Utils.PlotTileLine(Projectile.Center + Projectile.velocity.SafeNormalize() * 200f, Projectile.Center - Projectile.velocity.SafeNormalize() * 100f, 8f * Projectile.Opacity, DelegateMethods.CastLight);

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        Projectile.ai[2] = Helper.Approach(Projectile.ai[2], 0f, 0.1f);
    }

    public override void OnKill(int timeLeft) {
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }

    public override bool PreDraw(ref Color lightColor) {
        Main.instance.LoadProjectile(ProjectileID.HallowBossRainbowStreak); 
        Main.instance.LoadProjectile(ProjectileID.DD2BetsyFireball);

        Vector2 center = Projectile.Center - Projectile.velocity.SafeNormalize() * 30f;

        Projectile projectile = Projectile;
        int trailLength = Projectile.GetTrailCount();
        for (int i = 0; i < trailLength; i++) {
            var texture = TextureAssets.Projectile[ProjectileID.HallowBossRainbowStreak].Value;

            float lerp = 1f - i / (float)(trailLength - 1);
            var brightest = Color.LightYellow;
            var color = (Color.Lerp(brightest.MultiplyRGBA(Color.Black * .5f), brightest, lerp) with { A = 0 }) * lerp;
            var position = center - projectile.velocity * i;
            var scale = new Vector2(.5f * lerp, 1f) * projectile.scale;

            texture = TextureAssets.Projectile[ProjectileID.HallowBossRainbowStreak].Value;
            lerp = 1f - i / (float)(trailLength - 1);
            brightest = Color.Lerp(new Color(27, 177, 223), new Color(124, 255, 255), Helper.Wave(Projectile.localAI[2], 0f, 1f, 20f, i * 10));
            color = (Color.Lerp(brightest.MultiplyRGBA(Color.Black * .5f), brightest, lerp) with { A = 50 }) * lerp * Projectile.Opacity;
            color *= 1.5f;
            color *= 0.9f;
            position = center - projectile.velocity * i * 3f;
            float y = 3f;
            //if (IsOutsideAngle(new Vector2(Projectile.ai[0], Projectile.ai[1]), Projectile.velocity, position + Projectile.velocity.SafeNormalize() * -60f, 100f)) {
            //    color *= 0f;
            //}
            scale = new Vector2(Helper.Wave(Projectile.localAI[2], 0.25f, 0.675f, 20f, i * 10) * lerp * 0.5f, 3f) * projectile.scale;

            Main.EntitySpriteDraw(texture, position - Main.screenPosition, null, color, projectile.rotation, texture.Size() / 2, scale, SpriteEffects.None);
        }

        Texture2D flare = ResourceManager.Flare;
        Vector2 eyePosition = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint() + new Vector2(MathF.Abs(Projectile.ai[0]) * Projectile.GetOwnerAsPlayer().direction, Projectile.ai[1]);
        Rectangle flareClip = flare.Bounds;
        Vector2 flareOrigin = flareClip.Centered();
        Color flareColor = Color.Lerp(new Color(27, 177, 223), new Color(124, 255, 255), Helper.Wave(Projectile.localAI[2], 0f, 1f, 20f, Projectile.whoAmI));
        flareColor *= 1.5f;
        float baseProgress = Utils.GetLerpValue(TIMELEFT - 20, TIMELEFT, Projectile.timeLeft, true);
        float progress = Ease.CubeOut(baseProgress);
        flareColor *= progress;
        flareColor *= 0.9f;
        Vector2 flareScale = new Vector2(MathHelper.Lerp(0.5f, 2f, Ease.QuadOut(baseProgress)), 0.75f);
        flareScale *= 0.425f;
        float rotation = Projectile.velocity.ToRotation() + Projectile.ai[2];
        DrawInfo flareDrawInfo = new() {
            Clip = flareClip,
            Origin = flareOrigin,
            Color = flareColor,
            Scale = flareScale,
            Rotation = rotation
        };
        Main.spriteBatch.DrawWithSnapshot(flare, eyePosition, flareDrawInfo, blendState: BlendState.Additive);
        flareColor = Color.Lerp(new Color(27, 177, 223), new Color(124, 255, 255), Helper.Wave(Projectile.localAI[2], 0f, 1f, 20f, Projectile.whoAmI));
        flareColor = flareColor with { A = 0 };
        flareColor *= 1.5f;
        flareColor *= progress;
        flareColor *= 0.425f;
        flareDrawInfo = new() {
            Clip = flareClip,
            Origin = flareOrigin,
            Color = flareColor,
            Scale = flareScale,
            Rotation = rotation
        };
        Main.spriteBatch.DrawWithSnapshot(flare, eyePosition, flareDrawInfo);

        Texture2D bloom = ResourceManager.Bloom2;
        flareDrawInfo = new() {
            Clip = bloom.Bounds,
            Origin = bloom.Bounds.Centered(),
            Color = flareColor * 0.875f,
            Scale = Vector2.One * flareScale.X * 0.375f * new Vector2(1.25f, 1f),
            Rotation = rotation
        };
        Main.spriteBatch.DrawWithSnapshot(bloom, eyePosition, flareDrawInfo);

        return false;
    }

    public static bool IsOutsideAngle(Vector2 origin, Vector2 direction, Vector2 target, float maxAngle) {
        Vector2 toTarget = target - origin;

        if (direction != Vector2.Zero)
            direction.Normalize();

        if (toTarget != Vector2.Zero)
            toTarget.Normalize();

        float dot = Vector2.Dot(direction, toTarget);

        float cosMaxAngle = MathHelper.Clamp((float)Math.Cos(MathHelper.ToRadians(maxAngle)), -1f, 1f);

        return dot < cosMaxAngle;
    }

    public static float GetAngleBetween(Vector2 v1, Vector2 v2) {
        v1.Normalize();
        v2.Normalize();

        float dot = Vector2.Dot(v1, v2);
        dot = MathHelper.Clamp(dot, -1f, 1f);

        return MathHelper.ToDegrees((float)Math.Acos(dot));
    }
}
