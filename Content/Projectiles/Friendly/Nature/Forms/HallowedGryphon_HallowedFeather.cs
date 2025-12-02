using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class HallowedFeather : FormProjectile, IRequestAssets {
    private static ushort TIMELEFT => 180;

    public enum HallowedFeatherRequstedTextureType : byte {
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)HallowedFeatherRequstedTextureType.Glow, Texture + "_Glow")];

    public override void SetStaticDefaults() => Projectile.SetTrail(2, 6);

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(20);

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.alpha = 0;

        Projectile.timeLeft = TIMELEFT;
    }

    public override void AI() {
        Projectile.Opacity = Utils.GetLerpValue(0f, 30f, Projectile.ai[0], true);

        if (Projectile.timeLeft < TIMELEFT * 0.425f && Projectile.ai[2] != 1f) {
            KillOnTileCollide();
        }

        if (Projectile.ai[2] == 0f) {
            Projectile.ai[0] += 2f;
            if (!(Projectile.ai[0] < 30f)) {
                if (Projectile.ai[0] < 150f)
                    Projectile.velocity *= 1.06f;
                else
                    Projectile.ai[0] = 250f;
            }

            Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(25f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        else {
            if (Projectile.ai[0] > 0f) {
                Projectile.ai[0] -= 2f;
            }
            else {
                Projectile.Kill();
                for (int num729 = 0; num729 < 10; num729++) {
                    int num730 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
                    Main.dust[num730].noGravity = true;
                    Main.dust[num730].fadeIn = 1.35f - Main.rand.NextFloat(0.4f);
                    Main.dust[num730].scale = 0.35f;
                    Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
                }
            }
        }
    }

    public override bool ShouldUpdatePosition() {
        return !(Projectile.ai[0] < 30f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int num729 = 0; num729 < 10; num729++) {
            int num730 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
            Main.dust[num730].noGravity = true;
            Main.dust[num730].fadeIn = 1.35f - Main.rand.NextFloat(0.4f);
            Main.dust[num730].scale = 0.35f;
            Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        for (int num729 = 0; num729 < 10; num729++) {
            int num730 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
            Main.dust[num730].noGravity = true;
            Main.dust[num730].fadeIn = 1.35f - Main.rand.NextFloat(0.4f);
            Main.dust[num730].scale = 0.35f;
            Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.TintableDustLighted, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, Main.rand.Next(100), Color.Lerp(Color.LightYellow, Color.Yellow, Main.rand.NextFloat()), 1f + Main.rand.NextFloatRange(0.1f));
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.penetrate == Projectile.maxPenetrate) {
            Player owner = Projectile.GetOwnerAsPlayer();
            ProjectileUtils.SpawnPlayerOwnedProjectile<HallowedZone>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_Death()) with {
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack,
                Position = Projectile.Center
            });
        }
        SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 1.2f, PitchVariance = 0.1f, Volume = 0.6f, MaxInstances = 5 }, Projectile.Center);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 12;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        KillOnTileCollide();

        return false;
    }

    private void KillOnTileCollide() {
        if (Projectile.ai[2] != 1f) {
            Projectile.ai[2] = 1f;
            if (Projectile.ai[0] > 30f) {
                Projectile.ai[0] = 30f;
                SoundEngine.PlaySound(SoundID.Dig with { Pitch = 1.2f, Volume = 0.3f, MaxInstances = 5 }, Projectile.Center);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<HallowedFeather>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets) ||
            !AssetInitializer.TryGetRequestedTextureAssets<HallowedZone>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets2)) {
            return false;
        }

        Texture2D glowTexture = indexedTextureAssets[(byte)HallowedFeatherRequstedTextureType.Glow].Value;
        Color color = Color.Lerp(lightColor, Color.White, 0.5f) with { A = 100 } * 0.25f;
        Color color2 = lightColor with { A = 100 } * 0.25f;
        color *= Projectile.Opacity;
        color2 *= Projectile.Opacity;
        float rotation;
        for (int i = 0; i < 4; i++) {
            rotation = Helper.Wave(-MathHelper.PiOver4 / 2f, MathHelper.PiOver4 / 2f, MathHelper.PiOver2 / 2f, i);
            ProjectileUtils.QuickDrawShadowTrails(Projectile, color2, 0.5f, 1, rotation);
            ProjectileUtils.QuickDrawShadowTrails(Projectile, color, 0.5f, 1, rotation, texture: glowTexture);
        }

        ProjectileUtils.QuickDraw(Projectile, lightColor);

        for (int i = 0; i < 4; i++) {
            rotation = Helper.Wave(-MathHelper.PiOver4, MathHelper.PiOver4, MathHelper.PiOver2, i);
            ProjectileUtils.QuickDraw(Projectile, color2, rotation);
            ProjectileUtils.QuickDraw(Projectile, color, rotation, glowTexture);
        }

        Texture2D lightTexture = indexedTextureAssets2[(byte)HallowedZone.HallowedZoneRequstedTextureType.Light].Value;
        Rectangle clip = lightTexture.Bounds;
        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
        Vector2 origin = clip.Centered();
        batch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        float num2 = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
        for (float num3 = 0f; num3 < 4f; num3 += 1f) {
            Vector2 vector = ((float)Math.PI / 2f * num3 + num2).ToRotationVector2() * 4f;
            float opacity = 1f - Projectile.Opacity;
            Vector2 scale = new Vector2(1f, 0.5f) * 0.25f * opacity;
            color = Color.Lerp(Color.Yellow, Color.LightYellow, 0.5f) * opacity * Helper.Wave(0.75f, 1.25f, speed: 5f);
            rotation = 0.01f * Main.rand.NextFloatRange(1f) * opacity;
            batch.Draw(lightTexture, Projectile.Center - vector * 0.2f, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Scale = scale,
                Color = color * (Projectile.timeLeft >= (int)(TIMELEFT * 0.825f) ? 1f : 0.75f),
                Rotation = rotation + vector.ToRotation()
            });
        }
        batch.Begin(snapshot, true);

        return false;
    }
}
