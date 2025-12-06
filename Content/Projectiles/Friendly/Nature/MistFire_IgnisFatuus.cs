using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IgnisFatuus : NatureProjectile {
    private static ushort TIMELEFT => 80;

    private Color _lightColor;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float TimeLeftFactor => ref Projectile.localAI[1];
    public ref float TypeValue => ref Projectile.ai[0];
    public ref float MaxSpeed => ref Projectile.ai[1];
    public ref float SavedRotation => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public static Color GetLightColor(int type) {
        Color color = Color.White;
        switch (type) {
            case 0:
                color = new(86, 252, 219);
                break;
            case 1:
                color = new(220, 240, 77);
                break;
            case 2:
                color = new(169, 255, 83);
                break;
        }
        return color;
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            TimeLeftFactor = 40;

            _lightColor = GetLightColor((int)TypeValue).ModifyRGB(Main.rand.NextFloat(0.85f, 1.15f));

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        float progress = (float)TimeLeftFactor / 40;
        MaxSpeed = Ease.CubeInOut(progress) * 2f;

        Projectile.scale = 0.75f * MaxSpeed;
        Projectile.scale = MathF.Max(Projectile.scale, 1.125f);

        Projectile.Opacity = Utils.GetLerpValue(0f, 0.35f, Projectile.timeLeft / (float)TIMELEFT, true) * Utils.GetLerpValue(1f, 0.9f, progress, true);

        if (progress <= 0.9f) {
            if (++Projectile.frameCounter >= 4) {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 7) {
                    Projectile.frame = 4;
                }
            }

            if (Projectile.Opacity >= 0.75f) {
                if (Main.rand.NextBool(6)) {
                    int size = 20;
                    int num876 = Dust.NewDust(Projectile.Center + Vector2.UnitY * 1f - Vector2.One * 4f + Vector2.UnitX * Projectile.FacedRight().ToInt() - Vector2.One * size / 2f, size, size, ModContent.DustType<IgnisFatuusDust>());
                    Dust dust = Main.dust[num876];
                    dust.velocity *= 0.1f;
                    dust.velocity.Y -= 1.5f * Main.rand.NextFloat(0.75f, 1f);
                    dust.velocity += Vector2.UnitY.RotatedBy(SavedRotation);
                    Main.dust[num876].scale = 1.3f;
                    Main.dust[num876].noGravity = true;
                    Main.dust[num876].customData = (int)TypeValue;
                }
            }
        }

        Lighting.AddLight(Projectile.Center, _lightColor.ToVector3() * 0.625f);

        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.35f, 0.25f);

        Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(MaxSpeed);

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity.RotatedBy(MathHelper.PiOver2 * Projectile.direction), TimeSystem.LogicDeltaTime);

        Projectile.velocity *= 0.75f;

        if (TimeLeftFactor > 0) {
            TimeLeftFactor--;
        }
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame frame = new(3, 8, (byte)TypeValue, (byte)Projectile.frame);
        Rectangle clip = frame.GetSourceRectangle(texture);
        Vector2 origin = clip.Centered();
        Color color = Color.White with { A = 200 } * Projectile.Opacity;
        Vector2 position = Projectile.Center;
        Vector2 scale = Vector2.One * Projectile.scale;
        float rotation = Projectile.rotation;
        SpriteEffects effects = Projectile.direction.ToSpriteEffects();

        Texture2D bloom = ResourceManager.Bloom;
        Rectangle bloomClip = bloom.Bounds;
        Vector2 bloomOrigin = bloomClip.Centered();
        Color bloomColor = GetLightColor((int)TypeValue) * Projectile.Opacity * 0.2f;
        batch.Draw(bloom, position, DrawInfo.Default with {
            Clip = bloomClip,
            Origin = bloomOrigin,
            Color = bloomColor,
            Scale = scale * 0.325f,
            Rotation = rotation
        });

        batch.Draw(texture, position - Vector2.One * 2f, DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color,
            Scale = scale,
            Rotation = rotation,
            ImageFlip = effects
        });

        return false;
    }
}
