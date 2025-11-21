using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class TerraFracture : NatureProjectile_NoTextureLoad, IRequestAssets {
    public enum TerraFractureRequstedTextureType : byte {
        Part,
        Part2,
        Flash
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)TerraFractureRequstedTextureType.Part, ResourceManager.NatureProjectileTextures + "TerraFracturePart"),
         ((byte)TerraFractureRequstedTextureType.Part2, ResourceManager.NatureProjectileTextures + "TerraFracturePart2"),
         ((byte)TerraFractureRequstedTextureType.Flash, ResourceManager.NatureProjectileTextures + "TerraFractureFlash")];

    public readonly record struct FracturePartInfo(Vector2 StartPosition, Vector2 EndPosition, Color Color, float Scale);

    private readonly LinkedList<FracturePartInfo> _fractureParts = [];

    public ref float InitValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.aiStyle = -1;

        ShouldChargeWreathOnDamage = false;

        Projectile.Opacity = 0f;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        int x = (int)Projectile.Center.X;
        int y = (int)Projectile.Center.Y;
        int width = (int)(500 * Projectile.scale * Projectile.ai[0]);
        int height = (int)(200 * Projectile.scale);
        y -= height / 2;
        if (new Rectangle(x, y, width, height).Intersects(targetHitbox)) {
            return true;
        }

        return false;
    }

    private float Opacity => Utils.GetLerpValue(0, Projectile.ai[2] / 3, Projectile.timeLeft, true);

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, TimeSystem.LogicDeltaTime * 3f);
        Projectile.Opacity = Ease.CircOut(Projectile.Opacity);

        Projectile.localAI[2] = Helper.Approach(Projectile.localAI[2], 1f, TimeSystem.LogicDeltaTime * 3f);

        Projectile.scale = Projectile.ai[1];

        Player owner = Projectile.GetOwnerAsPlayer();

        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = owner.direction;
            Projectile.timeLeft = (int)Projectile.ai[2];
        }

        if (!Init) {
            Init = true;

            Projectile.velocity = Projectile.Center.DirectionTo(Projectile.Center + Vector2.UnitX * 2f * owner.direction);

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            float rotation2 = Projectile.rotation;
            rotation2 += 0.1f * Projectile.ai[0];
            if (Projectile.ai[0] < 0f) {
                rotation2 += 0.35f;
            }

            Projectile.Center -= Projectile.velocity * 40f;

            _fractureParts.Clear();

            for (float i = -MathHelper.PiOver4; i < MathHelper.PiOver4; i += MathHelper.PiOver4 / 2f) {
                int count = 30;
                float size = 5f, size2 = size;
                Vector2 getMoveVector() {
                    Vector2 result = Vector2.UnitY.RotatedBy(rotation2 + MathHelper.PiOver2 * Main.rand.NextFloatDirection() + i + i) * Main.rand.NextFloat(1f, size) * 5f;
                    return result;
                }
                Vector2 startPosition = Vector2.Zero,
                        endPosition = startPosition + getMoveVector();
                for (int k = 0; k < count; k++) {
                    float baseProgress = k / (float)count;
                    float progress = Ease.CubeOut(baseProgress);
                    float progress2 = 1f - Ease.QuintIn(baseProgress);
                    _fractureParts.AddLast(new LinkedListNode<FracturePartInfo>(new FracturePartInfo() {
                        StartPosition = startPosition,
                        EndPosition = endPosition,
                        Color = Color.Lerp(Color.Lerp(Color.Lerp(new Color(34, 177, 76), new Color(45, 124, 205), 0.75f), new Color(34, 177, 76), progress), new Color(34, 177, 76), Utils.GetLerpValue(0.95f, 1f, progress, true)),
                        Scale = progress2
                    }));
                    startPosition = endPosition;
                    endPosition += getMoveVector();
                    Vector2 beforePosition = endPosition;
                    int attempt = _fractureParts.Count;
                    while (attempt-- > 0 && _fractureParts.Any(x => x.StartPosition.Distance(endPosition) < 10f)) {
                        endPosition = beforePosition + getMoveVector();
                    }
                    endPosition += endPosition.DirectionTo(Projectile.Center) * 5f * new Vector2(Projectile.ai[0], 1f);
                    size *= 0.9f;
                    if (size < size2 / 2f) {
                        size = size2;
                    }
                }
            }
        }

        float num5 = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
        num5 = 0.75f + num5 * 0.25f;
        num5 = Utils.Remap(num5, 0.2f, 1f, 0f, 1f);
        float num6 = MathUtils.Clamp01((1f - num5) * 7.5f);
        float num7 = MathUtils.Clamp01((1f - num5) * 6.5f);
        Color green = new Color(34, 177, 76) * num5;
        Color blue = Color.Lerp(new Color(45, 124, 205), green, num6) * num5;
        float opacity = Opacity * 0.5f * Projectile.scale;
        Vector3 lightColor = green.ToVector3() * opacity;
        foreach (FracturePartInfo part in _fractureParts) {
            Lighting.AddLight(Projectile.Center + Vector2.Lerp(part.StartPosition, part.EndPosition, 0.5f), lightColor);
        }
        lightColor *= 0.5f;
        DelegateMethods.v3_1 = lightColor;
        Utils.PlotTileLine(Projectile.Center, Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * Projectile.velocity.SafeNormalize() * 450 * opacity, 75 * opacity, DelegateMethods.CastLight);

        Projectile.velocity *= 0.8f;
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<TerraFracture>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)TerraFractureRequstedTextureType.Part].Value,
                  texture2 = indexedTextureAssets[(byte)TerraFractureRequstedTextureType.Part2].Value;
        SpriteBatch batch = Main.spriteBatch;
        bool right = Projectile.ai[0] > 0;
        SpriteEffects effects = SpriteEffects.None;

        float colorWaveSpeed = 15f;
        float getColorWave(float offset = 0f) => Helper.Wave(0f, 1f, colorWaveSpeed, Projectile.whoAmI + offset);

        float num5 = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
        num5 = 0.75f + num5 * 0.25f;
        num5 = Utils.Remap(num5, 0.2f, 1f, 0f, 1f);
        float num6 = MathUtils.Clamp01((1f - num5) * 7.5f);
        float num7 = MathUtils.Clamp01((1f - num5) * 6.5f);

        Color green = new Color(34, 177, 76) * num5;
        Color blue = Color.Lerp(new Color(45, 124, 205), green, num6) * num5;

        float shakeFactor = 1f - Utils.GetLerpValue(0.25f, 0.875f, Projectile.localAI[2], true);

        Vector2 getShakeValue() => Main.rand.NextVector2Circular(6f * shakeFactor, 6f * shakeFactor);


        void drawSelf(Vector2? offset = null, float alpha = 1f, float opacity = 1f, bool onlyBase = false) {
            offset ??= Vector2.Zero;

            Vector2 center = Projectile.Center;
            Projectile.Center = center + offset.Value + getShakeValue();

            int index = 0;
            for (LinkedListNode<FracturePartInfo> node = _fractureParts.First!; node != null; node = node.Next!) {
                var fracturePart = node.Value;
                var nextFracturePart = (node.Next ?? node).Value;
                SpriteFrame frame = new(2, 3, (byte)(!right).ToInt(), (byte)index);
                index++;
                if (index > 2) {
                    index = 0;
                }
                float length = fracturePart.StartPosition.Distance(fracturePart.EndPosition) * 0.1f;
                Rectangle clip = frame.GetSourceRectangle(texture);
                Vector2 origin = new(10, 2);
                Vector2 position = Projectile.Center + fracturePart.StartPosition * Projectile.Opacity - Main.screenPosition;
                float rotation = fracturePart.StartPosition.DirectionTo(fracturePart.EndPosition).ToRotation() - MathHelper.PiOver2;
                float opacity2 = Projectile.Opacity * fracturePart.Scale * opacity * Opacity;
                opacity2 *= num5;
                Vector2 scale = new Vector2(Utils.Remap(Ease.SineInOut(opacity2), 0f, 1f, 0.75f, 1f), length) * MathF.Max(0.65f, fracturePart.Scale);
                scale *= Projectile.scale;
                Color baseColor = Color.Lerp(Color.Lerp(fracturePart.Color, nextFracturePart.Color, 0.5f), green, num6) * num5;
                Color baseColor2 = Color.Lerp(baseColor, onlyBase ? Color.Lerp(green, blue, getColorWave()) with { A = 100 } : Color.Black, 0.1f).MultiplyAlpha(alpha) * opacity2 * 1.5f;
                batch.Draw(texture,
                           position,
                           clip,
                           baseColor2,
                           rotation,
                           origin,
                           scale,
                           effects,
                           0f);
                if (!onlyBase) {
                    batch.DrawWithSnapshot(() => {
                        batch.Draw(ResourceManager.Bloom,
                                   position,
                                   ResourceManager.Bloom.Bounds,
                                   Color.Lerp(baseColor, Color.Black, 0.1f).MultiplyAlpha(alpha) * opacity2 * 0.75f,
                                   rotation,
                                   ResourceManager.Bloom.Bounds.Centered(),
                                   scale * 0.25f,
                                   effects,
                                   0f);
                    }, blendState: BlendState.Additive);
                    batch.DrawWithSnapshot(() => {
                        for (float i = 1f; i > 0f; i -= 0.25f) {
                            float colorWave = getColorWave(i);
                            Color color = Color.Lerp(Color.Lerp(baseColor, Color.Lerp(blue, green, 1f - colorWave), colorWave), Color.Black, 0.1f);
                            batch.Draw(texture,
                                       position,
                                       clip,
                                       color.MultiplyAlpha(alpha) * opacity2 * 0.25f,
                                       rotation,
                                       origin,
                                       scale * i,
                                       effects,
                                       0f);
                        }
                        for (float i = 1f; i > 0f; i -= 0.25f) {
                            float colorWave = getColorWave(i);
                            Color color = Color.Lerp(Color.Lerp(baseColor, Color.Lerp(blue, green, Math.Max(0.25f, Ease.SineInOut(1f - colorWave))), colorWave), Color.Black, 0.5f);
                            batch.Draw(texture2,
                                       position,
                                       clip,
                                       color.MultiplyAlpha(alpha) * opacity2 * 0.9f,
                                       rotation,
                                       origin,
                                       scale * i * 1.25f,
                                       effects,
                                       0f);
                        }
                        batch.Draw(texture,
                                   position,
                                   clip,
                                   Color.Lerp(baseColor, Color.Lerp(Color.White, green, num7), 0.25f).MultiplyAlpha(alpha) * opacity2 * 1.5f,
                                   rotation,
                                   origin,
                                   scale,
                                   effects,
                                   0f);
                    }, blendState: BlendState.Additive);
                }
            }

            Projectile.Center = center;
        }

        for (float i2 = 1f; i2 > 0f; i2 -= 0.05f) {
            drawSelf(-Vector2.One * (1f - i2) * 30f * new Vector2(-Projectile.direction, 0.75f), alpha: 0.5f, opacity: i2 * 0.375f * 0.25f, onlyBase: true);
        }
        drawSelf();

        Texture2D flashTexture = indexedTextureAssets[(byte)TerraFractureRequstedTextureType.Flash].Value;
        float opacity = Opacity;
        Rectangle clip = flashTexture.Bounds;
        float offset = 30f;
        batch.Draw(flashTexture, Projectile.Center + getShakeValue() / 2f + Projectile.velocity.SafeNormalize() * offset, DrawInfo.Default with {
            Clip = clip,
            Origin = clip.BottomCenter() * new Vector2(1f, 0.85f),
            Rotation = Projectile.rotation - MathHelper.Pi,
            Scale = Projectile.scale * new Vector2(1.5f, 8f) * Ease.CubeOut(opacity) * (0.75f + 0.25f * Ease.SineInOut(1f - Projectile.localAI[2])),
            Color = Color.Lerp(Color.Lerp(green, blue, getColorWave()), Color.White, 0.175f) with { A = 0 } * opacity * num6
        });
        batch.DrawWithSnapshot(() => {
            for (float i2 = 0.5f; i2 < 1.5f; i2 += 0.5f) {
                batch.Draw(flashTexture, Projectile.Center + getShakeValue() / 2f + Projectile.velocity.SafeNormalize() * offset, DrawInfo.Default with {
                    Clip = clip,
                    Origin = clip.BottomCenter() * new Vector2(1f, 0.85f),
                    Rotation = Projectile.rotation - MathHelper.Pi,
                    Scale = Projectile.scale * new Vector2(1.5f, 8f * i2) * Ease.CubeOut(opacity) * (0.75f + 0.25f * Ease.SineInOut(1f - Projectile.localAI[2])),
                    Color = Color.Lerp(Color.Lerp(green, blue, getColorWave(i2 * 5f)), Color.White, 0.175f) * opacity
                });
            }
        }, blendState: BlendState.Additive);
    }

    public override bool ShouldUpdatePosition() => true;

    public void DrawWithMetaballs() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<TerraFracture>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }
    }
}