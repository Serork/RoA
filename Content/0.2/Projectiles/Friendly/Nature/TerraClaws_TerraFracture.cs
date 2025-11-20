using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class TerraFracture : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 60;

    public enum TerraFractureRequstedTextureType : byte {
        Part,
        Part2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)TerraFractureRequstedTextureType.Part, ResourceManager.NatureProjectileTextures + "TerraFracturePart"),
         ((byte)TerraFractureRequstedTextureType.Part2, ResourceManager.NatureProjectileTextures + "TerraFracturePart2")];

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

        Projectile.timeLeft = TIMELEFT;

        Projectile.aiStyle = -1;

        ShouldChargeWreathOnDamage = false;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        if (Projectile.timeLeft < 40) {
            Projectile.timeLeft = 40;
        }

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.025f);
        Projectile.Opacity = Ease.CircOut(Projectile.Opacity);
        Projectile.Opacity *= Utils.GetLerpValue(0, 5, Projectile.timeLeft, true);

        Player owner = Projectile.GetOwnerAsPlayer();

        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = owner.direction;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Z)) {
            Init = false;

            Projectile.Opacity = 0f;

            Projectile.ai[0] = 0f;
        }

        Projectile.Center = Utils.Floor(owner.MountedCenter) + Vector2.UnitY * owner.gfxOffY;

        if (!Init) {
            Init = true;

            Projectile.velocity = Projectile.Center.DirectionTo(Projectile.Center + Vector2.UnitX * 2f * owner.direction);

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Projectile.velocity *= 0f;

            _fractureParts.Clear();

            if (Projectile.IsOwnerLocal()) {
                for (float i = -MathHelper.PiOver4; i < MathHelper.PiOver4; i += MathHelper.PiOver4 / 3f) {
                    int count = 25;
                    float size = 10f;
                    Vector2 getMoveVector() {
                        Vector2 result = Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.PiOver2 * Main.rand.NextFloatDirection() + i) * Main.rand.NextFloat(1f, size) * 5f;
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
                            Color = Color.Lerp(Color.Lerp(new Color(45, 124, 205), new Color(34, 177, 76), progress), new Color(34, 177, 76), Utils.GetLerpValue(0.95f, 1f, progress, true)),
                            Scale = progress2
                        }));
                        startPosition = endPosition;
                        endPosition += getMoveVector();
                        Vector2 beforePosition = endPosition;
                        int attempt = _fractureParts.Count;
                        while (attempt-- > 0 && _fractureParts.Any(x => x.StartPosition.Distance(endPosition) < 20f)) {
                            endPosition = beforePosition + getMoveVector();
                        }
                        size *= 0.9f;
                        if (size < 5f) {
                            size = 10f;
                        }
                    }
                }
            }
        }
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

        float colorWaveSpeed = 7.5f;
        float getColorWave(float offset = 0f) => Helper.Wave(0f, 1f, colorWaveSpeed, Projectile.whoAmI + offset);
        Color blue = new(45, 124, 205);
        Color green = new(34, 177, 76);

        for (LinkedListNode<FracturePartInfo> node = _fractureParts.First!; node != null; node = node.Next!) {
            var fracturePart = node.Value;
            var nextFracturePart = (node.Next ?? node).Value;
            SpriteFrame frame = new(2, 1, (byte)(!right).ToInt(), 0);
            float length = fracturePart.StartPosition.Distance(fracturePart.EndPosition) * 0.1f;
            Rectangle clip = frame.GetSourceRectangle(texture);
            Vector2 origin = new(10, 2);

            Vector2 position = Projectile.Center + fracturePart.StartPosition * Projectile.Opacity - Main.screenPosition;
            float rotation = fracturePart.StartPosition.DirectionTo(fracturePart.EndPosition).ToRotation() - MathHelper.PiOver2;
            float opacity2 = Projectile.Opacity * fracturePart.Scale;
            Vector2 scale = new Vector2(1f, length) * MathF.Max(0.65f, fracturePart.Scale);

            Color baseColor = Color.Lerp(fracturePart.Color, nextFracturePart.Color, 0.5f);
            batch.Draw(texture,
                       position,
                       clip,
                       Color.Lerp(baseColor, Color.Black, 0.1f) with { A = 255 } * opacity2,
                       rotation,
                       origin,
                       scale,
                       effects,
                       0f);

            batch.DrawWithSnapshot(() => {
                for (float i = 1f; i > 0f; i -= 0.25f) {
                    float colorWave = getColorWave(i);
                    Color color = Color.Lerp(Color.Lerp(baseColor, Color.Lerp(blue, green, Math.Max(0.25f, Ease.SineInOut(1f - colorWave))), colorWave), Color.Black, 0.1f) with { A = 255 };
                    batch.Draw(texture,
                               position,
                               clip,
                               color * opacity2 * 0.5f,
                               rotation,
                               origin,
                               scale * i,
                               effects,
                               0f);
                }
            }, blendState: BlendState.Additive);
        }

        for (LinkedListNode<FracturePartInfo> node = _fractureParts.First!; node != null; node = node.Next!) {
            var fracturePart = node.Value;
            var nextFracturePart = (node.Next ?? node).Value;
            SpriteFrame frame = new(2, 1, (byte)(!right).ToInt(), 0);
            float length = fracturePart.StartPosition.Distance(fracturePart.EndPosition) * 0.1f;
            Rectangle clip = frame.GetSourceRectangle(texture);
            Vector2 origin = new(10, 2);

            Vector2 position = Projectile.Center + fracturePart.StartPosition * Projectile.Opacity - Main.screenPosition;
            float rotation = fracturePart.StartPosition.DirectionTo(fracturePart.EndPosition).ToRotation() - MathHelper.PiOver2;
            float opacity2 = Projectile.Opacity * fracturePart.Scale;
            Vector2 scale = new Vector2(1f, length) * MathF.Max(0.65f, fracturePart.Scale);

            Color baseColor = Color.Lerp(fracturePart.Color, nextFracturePart.Color, 0.5f);

            for (float i2 = 1f; i2 < 2f; i2 += 1f) {
                batch.DrawWithSnapshot(() => {
                    for (float i = 1f; i > 0f; i -= 0.25f) {
                        float colorWave = getColorWave(i + i2);
                        byte a = (byte)(100 - i2 * 25);
                        Color color = Color.Lerp(baseColor, Color.Lerp(blue, green, Math.Max(0.25f, Ease.SineInOut(1f - colorWave))), colorWave) with { A = a };
                        batch.Draw(texture2,
                                   position,
                                   clip,
                                   color * opacity2 * 0.5f,
                                   rotation,
                                   origin,
                                   scale * i * i2,
                                   effects,
                                   0f);
                    }
                }, blendState: BlendState.Additive);
            }
        }
    }

    public void DrawWithMetaballs() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<TerraFracture>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }
    }
}