using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IgnisFatuus : NatureProjectile {
    private static ushort TIMELEFT => 40;

    private Color _lightColor;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float TypeValue => ref Projectile.ai[0];
    public ref float MaxSpeed => ref Projectile.ai[1];

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
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            _lightColor = GetLightColor((int)TypeValue).ModifyRGB(Main.rand.NextFloat(0.85f, 1.15f));
        }

        float progress = (float)Projectile.timeLeft / TIMELEFT;
        MaxSpeed = Ease.CubeInOut(progress) * 2f;

        Projectile.scale = 0.75f * MaxSpeed;
        Projectile.scale = MathF.Max(Projectile.scale, 1.125f);

        Projectile.Opacity = Utils.GetLerpValue(0f, 0.35f, progress, true) * Utils.GetLerpValue(1f, 0.9f, progress, true);

        if (++Projectile.frameCounter >= 4) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame > 7) {
                Projectile.frame = 4;
            }
        }

        Lighting.AddLight(Projectile.Center, _lightColor.ToVector3() * 0.5f);

        Projectile.rotation = Projectile.velocity.X * 0.35f;

        Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(MaxSpeed);

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity.RotatedBy(MathHelper.PiOver2), 0.05f);

        Projectile.velocity *= 0.5f;
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame frame = new(3, 8, (byte)TypeValue, (byte)Projectile.frame);
        Rectangle clip = frame.GetSourceRectangle(texture);
        Vector2 origin = clip.Centered();
        Color color = Color.White with { A = 220 } * Projectile.Opacity;
        Vector2 position = Projectile.Center;
        Vector2 scale = Vector2.One * Projectile.scale;
        float rotation = Projectile.rotation;

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
            Rotation = rotation
        });

        return false;
    }
}
