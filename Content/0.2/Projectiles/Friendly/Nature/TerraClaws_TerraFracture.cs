using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Metaballs;
using RoA.Common.Projectiles;
using RoA.Content.Items.Dyes;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class TerraFracture : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 60;

    private static bool _drawMetaballs;

    public enum TerraFractureRequstedTextureType : byte {
        Part
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)TerraFractureRequstedTextureType.Part, ResourceManager.NatureProjectileTextures + "TerraFracturePart")];

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
                    Vector2 getMoveVector() => Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.PiOver2 * Main.rand.NextFloatDirection() + i) * Main.rand.NextFloat(1f, size) * 5f;
                    Vector2 startPosition = Vector2.Zero,
                            endPosition = startPosition + getMoveVector();
                    for (int k = 0; k < count; k++) {
                        float baseProgress = k / (float)count;
                        float progress = Ease.SineOut(baseProgress);
                        float progress2 = 1f - Ease.QuintIn(baseProgress);
                        _fractureParts.AddLast(new LinkedListNode<FracturePartInfo>(new FracturePartInfo() {
                            StartPosition = startPosition,
                            EndPosition = endPosition,
                            Color = Color.Lerp(Color.Lerp(new Color(45, 124, 205), new Color(34, 177, 76), progress), Color.White, Utils.GetLerpValue(0.95f, 1f, progress, true)),
                            Scale = progress2
                        }));
                        startPosition = endPosition;
                        endPosition += getMoveVector();
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
        //DrawWithMetaballs();
        _drawMetaballs = true;

        //Texture2D texture = ResourceManager.Bloom;
        //SpriteBatch batch = Main.spriteBatch;
        //foreach (FracturePartInfo fracturePart in _fractureParts) {
        //    Rectangle clip = texture.Bounds;
        //    Vector2 origin = clip.Centered();
        //    float length = fracturePart.StartPosition.Distance(fracturePart.EndPosition) * 0.1f;
        //    batch.Draw(texture,
        //               Projectile.Center + Vector2.Lerp(fracturePart.StartPosition, fracturePart.EndPosition, 0.5f) * Projectile.Opacity - Main.screenPosition,
        //               clip,
        //               fracturePart.Color with { A = 50 } * 0.5f * Projectile.Opacity,
        //               fracturePart.StartPosition.DirectionTo(fracturePart.EndPosition).ToRotation() - MathHelper.PiOver2,
        //               origin,
        //               new Vector2(1f, length) * 0.25f,
        //               SpriteEffects.None,
        //               0f);
        //}

        if (!AssetInitializer.TryGetRequestedTextureAssets<TerraFracture>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)TerraFractureRequstedTextureType.Part].Value;
        SpriteBatch batch = Main.spriteBatch;

        for (LinkedListNode<FracturePartInfo> node = _fractureParts.First; node != null; node = node.Next) {
            var fracturePart = node.Value;
            var nextFracturePart = (node.Next ?? node).Value;
            SpriteFrame frame = new(4, 1, 0, 0);
            float length = fracturePart.StartPosition.Distance(fracturePart.EndPosition) * 0.1f;
            Rectangle clip = frame.GetSourceRectangle(texture);
            Vector2 origin = new(10, 2);
            batch.Draw(texture,
                        Projectile.Center + fracturePart.StartPosition * Projectile.Opacity - Main.screenPosition,
                        clip,
                        Color.Lerp(Color.Lerp(fracturePart.Color, nextFracturePart.Color, 0.5f), Color.Black, 0.1f) with { A = 255 } * Projectile.Opacity * fracturePart.Scale,
                        fracturePart.StartPosition.DirectionTo(fracturePart.EndPosition).ToRotation() - MathHelper.PiOver2,
                        origin,
                        new Vector2(1f, length) * MathF.Max(0.65f, fracturePart.Scale),
                        Projectile.ai[0] > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                        0f);
        }
    }

    public void DrawWithMetaballs() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<TerraFracture>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }
    }
}