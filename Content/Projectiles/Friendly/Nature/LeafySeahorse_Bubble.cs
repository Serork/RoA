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

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class LeafySeahorse_Bubble : NatureProjectile_NoTextureLoad {
    private static float STARTSPEED => 10f;
    private static ushort MAXTIMELEFT => 420;

    private static readonly Dictionary<BubbleValues.BubbleSizeType, Asset<Texture2D>?> _texturesPerBubbleType = [];

    public ref struct BubbleValues(Projectile projectile) {
        public enum BubbleSizeType : byte {
            Small,
            Medium,
            Large,
            ExtraLarge,
            Count,
            Mass
        }

        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float BaseDamageValue = ref projectile.localAI[2];
        public ref float SizeTypeValue = ref projectile.ai[0];
        public ref float AbsorbedBubblesCountValue = ref projectile.ai[1];
        public ref float DesiredScaleFactorValue = ref projectile.ai[2];

        public ref float ScaleX = ref projectile.scale;
        public ref float ScaleY = ref projectile.localAI[1];

        public int BaseDamage {
            readonly get => (int)BaseDamageValue;
            set => BaseDamageValue = value;
        }

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public bool KilledNotNaturally {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public BubbleSizeType SizeType {
            readonly get => (BubbleSizeType)SizeTypeValue;
            set => SizeTypeValue = Utils.Clamp((byte)value, (byte)BubbleSizeType.Small, (byte)BubbleSizeType.ExtraLarge);
        }

        public float DesiredScaleFactor {
            get {
                float absorbProgress = AbsorbedBubblesCountValue / NeededBubbleCountToIncreaseInSize() * 0.5f;
                return 1f + absorbProgress;
            }
        }

        public readonly Vector2 Scale => new(ScaleX, ScaleY);

        public readonly int NeededBubbleCountToIncreaseInSize() {
            switch (SizeType) {
                case BubbleSizeType.Small:
                    return 1;
                case BubbleSizeType.Medium:
                    return 2;
                case BubbleSizeType.Large:
                    return 4;
                default:
                    break;
            }
            return -1;
        }

        public void AbsorbBubbleAndIncreaseInSize(Projectile otherBubble, Action? onAbsorb = null) {
            float massFactor = 1f - SizeTypeValue / (float)BubbleSizeType.Count;
            projectile.velocity += otherBubble.velocity * 0.333f * massFactor;

            _ = new BubbleValues(otherBubble) {
                KilledNotNaturally = false
            };
            otherBubble.netUpdate = true;
            otherBubble.Kill();

            //SoundEngine.PlaySound(BubbleHit, otherBubble.position);

            Vector2 dustPosition = projectile.Center + projectile.DirectionTo(otherBubble.Center) * otherBubble.width * 0.95f;
            for (int i = 0; i < 10; i++) {
                if (!Main.rand.NextChance(0.7)) {
                    continue;
                }
                int dustType = Utils.IsPowerOfTwo(i) ? DustID.Water_Jungle : DustID.BubbleBurst_Green;
                Vector2 dustVelocity = -otherBubble.velocity.SafeNormalize().RotatedByRandom(MathHelper.PiOver4);
                dustVelocity += dustVelocity * Main.rand.NextFloat(0.5f, 1f);
                Dust dust41 = Dust.NewDustPerfect(dustPosition, dustType, Velocity: dustVelocity);
                dust41.color = Main.hslToRgb((float)(0.4000000059604645 + Main.rand.NextDouble() * 0.20000000298023224), 0.9f, 0.5f);
                dust41.color = Color.Lerp(dust41.color, Color.White, 0.3f);
                dust41.noGravity = true;
                dust41.scale = 0.75f;
                dust41.alpha = 100;
            }

            SoundEngine.PlaySound(BubbleCollide, otherBubble.position);

            if (projectile.IsOwnerLocal()) {
                AbsorbedBubblesCountValue++;
                if (AbsorbedBubblesCountValue >= NeededBubbleCountToIncreaseInSize()) {
                    AbsorbedBubblesCountValue = 0;
                    SizeType++;
                    onAbsorb?.Invoke();

                    SetSizeBasedOnType();

                    projectile.timeLeft = MAXTIMELEFT;
                }

                projectile.netUpdate = true;
            }
        }

        public void SetSizeBasedOnType(bool onSpawn = false) {
            if (!projectile.IsOwnerLocal()) {
                return;
            }

            switch (SizeType) {
                case BubbleSizeType.Small:
                    projectile.SetSizeValues(14);
                    break;
                case BubbleSizeType.Medium:
                    projectile.SetSizeValues(20);
                    break;
                case BubbleSizeType.Large:
                    projectile.SetSizeValues(30);
                    break;
                case BubbleSizeType.ExtraLarge:
                    projectile.SetSizeValues(44);
                    break;
            }
            if (!onSpawn) {
                Vector2 positionOffsetOnChangingSize = projectile.Size / 6f;
                projectile.position -= positionOffsetOnChangingSize;
            }
            projectile.netUpdate = true;
        }

        public void SetDamageBasedOnType() {
            float damageScale = 1f;
            switch (SizeType) {
                case BubbleSizeType.Small:
                    break;
                case BubbleSizeType.Medium:
                    damageScale = 2.5f;
                    break;
                case BubbleSizeType.Large:
                    damageScale = 5f;
                    break;
                case BubbleSizeType.ExtraLarge:
                    damageScale = 10f;
                    break;
            }
            projectile.damage = (int)(BaseDamage * damageScale);
        }
    }

    public static SoundStyle BubbleHit => SoundID.Item54 with { PitchVariance = 0.2f };
    public static SoundStyle BubbleCollide => SoundID.Item85 with { Pitch = 1f };

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write(Projectile.width);
        writer.Write(Projectile.timeLeft);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        Projectile.SetSizeValues(reader.ReadInt32());
        Projectile.timeLeft = reader.ReadInt32();
    }

    public override void SetStaticDefaults() {
        LoadBubbleTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldApplyAttachedItemDamage: false);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        void damageUp() {
            BubbleValues bubbleValues = new(Projectile) {
                BaseDamage = AttachedNatureWeaponDamage
            };
            bubbleValues.SetDamageBasedOnType();
        }
        void setStartVelocityAndSizeOnInit() {
            BubbleValues bubbleValues = new(Projectile);
            if (!bubbleValues.Init) {
                bubbleValues.Init = true;

                Projectile.velocity = Projectile.velocity.SafeNormalize() * STARTSPEED;

                bubbleValues.SetSizeBasedOnType(true);
            }
        }
        void slowDownOverTime() {
            int timeLeft = Projectile.timeLeft;
            float timeLeftProgress = 1f - (float)timeLeft / MAXTIMELEFT;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, Ease.SineOut(timeLeftProgress));
        }
        void absorbBubbles() {
            if (Projectile.Opacity < 0.5f) {
                return;
            }
            BubbleValues bubbleValues = new(Projectile);
            BubbleValues.BubbleSizeType size = bubbleValues.SizeType;
            foreach (Projectile otherBubble in TrackedEntitiesSystem.GetTrackedProjectile<LeafySeahorse_Bubble>((checkProjectile) => checkProjectile.owner != Projectile.owner || checkProjectile.identity == Projectile.identity)) {
                Rectangle otherBubbleHitBox = otherBubble.getRect();
                otherBubbleHitBox.Inflate(2, 2);
                Rectangle hitbox = Projectile.getRect();
                otherBubbleHitBox.Inflate(2, 2);
                bool collidingWithOtherBubble = otherBubbleHitBox.Intersects(hitbox);
                if (!collidingWithOtherBubble) {
                    continue;
                }

                if (size != BubbleValues.BubbleSizeType.ExtraLarge && new BubbleValues(otherBubble).SizeType == BubbleValues.BubbleSizeType.Small) {
                    bubbleValues.AbsorbBubbleAndIncreaseInSize(otherBubble);
                }
                else if (size != BubbleValues.BubbleSizeType.Small) {
                    Projectile.velocity += Projectile.DirectionFrom(otherBubble.Center);
                    float maxSpeedFromCollision = 1f;
                    if (Projectile.velocity.Length() > maxSpeedFromCollision) {
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * maxSpeedFromCollision;
                    }
                }
            }
        }
        void setScaleAndRotation() {
            BubbleValues bubbleValues = new(Projectile);
            Vector2 desiredScale = Vector2.One;
            float velocityLength = Projectile.velocity.Length();
            float checkVelocityLengthMin = 0.5f,
                  checkVelocityLengthMax = STARTSPEED;
            if (velocityLength > checkVelocityLengthMin && velocityLength < checkVelocityLengthMax) {
                Vector2 normalizedVelocity = Projectile.Center.AngleTo(Projectile.Center + Projectile.velocity).ToRotationVector2();
                float maxScale = 0.9f;
                desiredScale.X = MathF.Max(maxScale, MathF.Abs(normalizedVelocity.Y));
                desiredScale.Y = MathF.Max(maxScale, MathF.Abs(normalizedVelocity.X));
            }
            float extraScaleFactor = bubbleValues.DesiredScaleFactor;
            desiredScale *= extraScaleFactor;
            float approachSpeed = 0.075f;
            bubbleValues.ScaleX = Helper.Approach(bubbleValues.ScaleX, desiredScale.X, approachSpeed);
            bubbleValues.ScaleY = Helper.Approach(bubbleValues.ScaleY, desiredScale.Y, approachSpeed);

            float desiredRotation = Projectile.velocity.X * 0.1f;
            Projectile.rotation = Helper.Approach(Projectile.rotation, desiredRotation, approachSpeed * 0.5f);
        }
        void fadeIn() {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.05f);
        }
        void goUpInWater() {
            if (Projectile.wet) {
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y *= 0.98f;

                if (Projectile.velocity.Y > -8f)
                    Projectile.velocity.Y -= 0.2f;

                Projectile.velocity.X *= 0.94f;
            }
        }

        fadeIn();
        setStartVelocityAndSizeOnInit();
        slowDownOverTime();
        absorbBubbles();
        setScaleAndRotation();
        goUpInWater();
        damageUp();
    }

    protected override void Draw(ref Color lightColor) {
        BubbleValues bubbleValues = new(Projectile);
        Asset<Texture2D>? bubbleTextureAsset = _texturesPerBubbleType[bubbleValues.SizeType];
        if (bubbleTextureAsset?.IsLoaded != true) {
            return;
        }
        Texture2D texture = bubbleTextureAsset.Value;
        Vector2 position = Projectile.Center;
        Rectangle clip = texture.Bounds;
        Color color = lightColor * Projectile.Opacity * 0.8f;
        Vector2 origin = Projectile.Size / 2f;
        Vector2 scale = bubbleValues.Scale;
        float rotation = Projectile.rotation;
        Main.spriteBatch.Draw(texture, position, DrawInfo.Default with {
            Clip = clip,
            Color = color,
            Origin = origin,
            Scale = scale,
            Rotation = rotation
        });
    }

    public override void OnKill(int timeLeft) {
        BubbleValues bubbleValues = new(Projectile);
        if (!bubbleValues.KilledNotNaturally) {
            return;
        }

        SoundEngine.PlaySound(BubbleHit, Projectile.position);

        float sizeFactor = MathF.Max(1, bubbleValues.SizeTypeValue + 1);
        for (int num269 = 0; num269 < 5 + 8 * sizeFactor; num269++) {
            int type = Utils.IsPowerOfTwo(num269) ? DustID.Water_Jungle : DustID.BubbleBurst_Green;
            int num270 = (int)(3f * sizeFactor);
            int num271 = Dust.NewDust(Projectile.Center - Vector2.One * num270, num270 * 2, num270 * 2, type);
            Dust dust41 = Main.dust[num271];
            Vector2 vector35 = Vector2.Normalize(dust41.position - Projectile.Center);
            dust41.position = Projectile.Center + vector35 * num270 * bubbleValues.Scale;
            if (num269 < 30)
                dust41.velocity = vector35 * dust41.velocity.Length();
            else
                dust41.velocity = vector35 * Main.rand.Next(45, 91) / 10f;

            dust41.color = Main.hslToRgb((float)(0.4000000059604645 + Main.rand.NextDouble() * 0.20000000298023224), 0.9f, 0.5f);
            dust41.color = Color.Lerp(dust41.color, Color.White, 0.3f);
            dust41.alpha = 100;
            dust41.noGravity = true;
            dust41.scale = 0.7f + 0.1f * sizeFactor;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = Projectile.width / 2;
        height = Projectile.height / 2;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => new BubbleValues(Projectile).SizeType == BubbleValues.BubbleSizeType.Small;

    private void LoadBubbleTextures() {
        if (Main.dedServ) {
            return;
        }

        foreach (BubbleValues.BubbleSizeType bubbleType in Enum.GetValues(typeof(BubbleValues.BubbleSizeType))) {
            if (bubbleType > BubbleValues.BubbleSizeType.ExtraLarge) {
                continue;
            }
            string texturePath = ResourceManager.NatureProjectileTextures + nameof(LeafySeahorse_Bubble);
            _texturesPerBubbleType[bubbleType] = ModContent.Request<Texture2D>(texturePath + ((byte)bubbleType + 1));
        }
    }
}

