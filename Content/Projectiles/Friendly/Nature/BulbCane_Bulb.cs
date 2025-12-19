using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Bulb : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(30);

    private static byte LEAFFRAMECOUNT => 2;
    private static byte STAMENFRAMECOUNT => 4;

    public enum Bulb_RequstedTextureType : byte { 
        Bulb,
        Stem1,
        Stem2,
        Stem3,
        LeafStem1,
        Leaf,
        Stamen
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)Bulb_RequstedTextureType.Bulb, ResourceManager.NatureProjectileTextures + "Bulb"),
         ((byte)Bulb_RequstedTextureType.Stem1, ResourceManager.NatureProjectileTextures + "Bulb_Stem1"),
         ((byte)Bulb_RequstedTextureType.Stem2, ResourceManager.NatureProjectileTextures + "Bulb_Stem2"),
         ((byte)Bulb_RequstedTextureType.Stem3, ResourceManager.NatureProjectileTextures + "Bulb_Stem3"),
         ((byte)Bulb_RequstedTextureType.LeafStem1, ResourceManager.NatureProjectileTextures + "Bulb_LeafStem1"),
         ((byte)Bulb_RequstedTextureType.Leaf, ResourceManager.NatureProjectileTextures + "Bulb_Leaf"),
         ((byte)Bulb_RequstedTextureType.Stamen, ResourceManager.NatureProjectileTextures + "Bulb_Stamen")];

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float RootPositionX => ref Projectile.ai[0];
    public ref float RootPositionY => ref Projectile.ai[1];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }
    public Vector2 RootPosition {
        get => new(RootPositionX, RootPositionY);
        set {
            RootPositionX = value.X;
            RootPositionY = value.Y;
        }
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = false;
    }

    public override void AI() {
        void init() {
            if (!Init) {
                float yOffset = 100f;
                RootPosition = Projectile.Center + Vector2.UnitY * yOffset;

                Init = true;
            }
        }

        init();
    }

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Bulb>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;

        Texture2D bulbTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb].Value,
                  stem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem1].Value,
                  stem2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem2].Value,
                  stem3Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem3].Value,
                  leafStem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.LeafStem1].Value,
                  leafTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Leaf].Value;

        int texturePadding = 4;

        // BULB
        Rectangle bulbClip = bulbTexture.Bounds;
        Vector2 bulbOrigin = bulbClip.BottomCenter();
        DrawInfo bulbDrawInfo = new() {
            Clip = bulbClip,
            Origin = bulbOrigin
        };

        // STEM 1
        Rectangle stem1Clip = stem1Texture.Bounds;
        Vector2 stem1Origin = stem1Clip.BottomCenter();
        DrawInfo stem1DrawInfo = new() {
            Clip = stem1Clip,
            Origin = stem1Origin
        };

        // STEM 2
        Rectangle stem2Clip = stem2Texture.Bounds;
        Vector2 stem2Origin = stem2Clip.BottomCenter();
        DrawInfo stem2DrawInfo = new() {
            Clip = stem2Clip,
            Origin = stem2Origin
        };

        // STEM 3
        Rectangle stem3Clip = stem3Texture.Bounds;
        Vector2 stem3Origin = stem3Clip.BottomCenter();
        DrawInfo stem3DrawInfo = new() {
            Clip = stem3Clip,
            Origin = stem3Origin
        };

        // LEAF STEM 1
        Rectangle leafStem1Clip = leafStem1Texture.Bounds;
        Vector2 leafStem1Origin = leafStem1Clip.BottomCenter();
        DrawInfo leafStem1DrawInfo = new() {
            Clip = leafStem1Clip,
            Origin = leafStem1Origin
        };

        Vector2 center = Projectile.Center;

        void drawMainStem() {
            int height = stem1Clip.Height - texturePadding;
            Vector2 startPosition = RootPosition;
            Vector2 endPosition = center + center.DirectionFrom(startPosition) * height;
            float scaleFactor = 0f;
            float getDistanceToBulb() => Vector2.Distance(startPosition, endPosition);
            while (getDistanceToBulb() > height) {
                float lerpValue = 0.25f;
                scaleFactor = Helper.Approach(scaleFactor, height, lerpValue);

                Vector2 scale = Vector2.One * (0.25f + Utils.GetLerpValue(0f, height, scaleFactor, true));
                float sizeIncreaseFluff = 3f;
                if (getDistanceToBulb() < height * sizeIncreaseFluff) {
                    scale = Helper.Approach(scale, new Vector2(10f), lerpValue);
                }

                Vector2 position = startPosition;
                batch.Draw(stem1Texture, position, stem1DrawInfo with {
                    Scale = scale
                });

                Vector2 velocityToBulbPosition = startPosition.DirectionTo(endPosition);
                startPosition += velocityToBulbPosition * scaleFactor;
            }
        }
        void drawBulb() {
            Vector2 bulbPosition = center;

            float stem2OffsetFromBulbValue = 8f;
            Vector2 angleFromBulbToRoot = bulbPosition.DirectionTo(RootPosition);
            Vector2 stem2Position = bulbPosition + angleFromBulbToRoot * stem2OffsetFromBulbValue;

            float stem3OffsetFromBulbValue = 4f;
            Vector2 stem3Position = bulbPosition + angleFromBulbToRoot * stem3OffsetFromBulbValue;

            batch.Draw(stem2Texture, stem2Position, stem2DrawInfo);
            batch.Draw(bulbTexture, bulbPosition, bulbDrawInfo);
            batch.Draw(stem3Texture, stem3Position, stem3DrawInfo);
        }
        void drawLeafStem() {
            Vector2 startPosition = center,
                    startVelocity = new(5f, 0f);
            float startPositionOffsetFactor = 2.5f;
            startPosition += startVelocity * startPositionOffsetFactor;

            float currentLength = 7f,
                  length = currentLength;
            int height = leafStem1Clip.Height - texturePadding;
            float xLerpValue = 0f,
                  yLerpValue = 0f;
            float scaleFactor = 0f;
            while (currentLength > 0f) {
                float leafStemScaleLerpValue = 0.25f;
                scaleFactor = Helper.Approach(scaleFactor, height, leafStemScaleLerpValue);

                float lengthProgress = 1f - currentLength / length;

                float rotation = startVelocity.ToRotation() - MathHelper.PiOver2;

                float yLerpValueTo = 1f;
                yLerpValue = Helper.Approach(yLerpValue, yLerpValueTo, 0.25f);

                ref float velocityX = ref startVelocity.X,
                          velocityY = ref startVelocity.Y;
                float xTo = 0.25f,
                      yTo = -5f;
                if (lengthProgress > 0.375f) {
                    yTo = 0f;
                }
                velocityX = Helper.Approach(velocityX, xTo, xLerpValue);
                velocityY = Helper.Approach(velocityY, yTo, Ease.ExpoOut(yLerpValue));

                bool shouldDrawLeaf = currentLength <= 1f;
                Texture2D texture = leafStem1Texture;
                Vector2 position = startPosition;
                batch.Draw(texture, position, leafStem1DrawInfo with {
                    Rotation = rotation
                });

                currentLength = Helper.Approach(currentLength, 0f, 1f);

                Vector2 velocityToLeafPosition = startVelocity.SafeNormalize();
                startPosition += velocityToLeafPosition * height;
            }
        }

        drawLeafStem();
        drawMainStem();
        drawBulb();
    }
}
