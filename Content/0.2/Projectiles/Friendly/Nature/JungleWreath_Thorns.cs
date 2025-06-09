using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Thorns : NatureProjectile_NoTextureLoad {
    private static short TIMELEFT => 180;
    private static byte FRAMECOUNT => 5;
    private static byte BASELENGTH => 10;
    private static byte SEGMENTHEIGHT => 18;

    private static Asset<Texture2D>? _thornsTexture;

    private struct SegmentInfo(byte frameToUse, SegmentInfo.SegmentTypeInfo segmentType) {
        public enum SegmentTypeInfo {
            Start,
            Mid,
            End
        }

        private float _progress = 0f;

        public float Progress {
            readonly get => _progress;
            set => _progress = Helper.Clamp01(value);
        }
        public readonly byte FrameToUse = frameToUse;
        public readonly SegmentTypeInfo SegmentType = segmentType;

        public readonly bool IsStartSegment => SegmentType == SegmentTypeInfo.Start;
        public readonly bool IsMidSegment => SegmentType == SegmentTypeInfo.Mid;
        public readonly bool IsEndSegment => SegmentType == SegmentTypeInfo.End;
    }

    private SegmentInfo[] _segmentData = [];

    public ref struct ThornsValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float LengthValue = ref projectile.ai[0];
        public ref float WrapDirectionValue = ref projectile.ai[1];
        public ref float LostHPProcentValue = ref projectile.ai[2];

        public readonly int Length => (int)LengthValue;
        public readonly int WrapDirection => (int)WrapDirectionValue;
    }

    private int GetThornsLength() => new ThornsValues(Projectile).Length;

    public override void Load() {
        LoadThornsTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSize(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 5;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        void initSegmentInfo() {
            ThornsValues thornValues = new(Projectile);
            if (thornValues.InitOnSpawnValue == 0f) {
                thornValues.InitOnSpawnValue = 1f;

                if (Projectile.IsOwnerLocal()) {
                    thornValues.LengthValue = BASELENGTH + thornValues.LostHPProcentValue * BASELENGTH;
                    Vector2 checkPosition = Projectile.Center + Projectile.velocity;
                    thornValues.WrapDirectionValue = Math.Sign(checkPosition.X - Projectile.Center.X) * (checkPosition.Y < Projectile.Center.Y).ToDirectionInt();
                    Projectile.netUpdate = true;
                }

                int thornsLength = thornValues.Length;
                _segmentData = new SegmentInfo[thornsLength];
                for (int i = 0; i < thornsLength; i++) {
                    bool isStart = i == 0,
                         isEnd = i == thornsLength - 1;
                    byte frameToUse = (byte)(isStart ? 0 : isEnd ? FRAMECOUNT - 1 : (1 + (i % 3)));
                    SegmentInfo.SegmentTypeInfo segmentType = isStart ? SegmentInfo.SegmentTypeInfo.Start : isEnd ? SegmentInfo.SegmentTypeInfo.End : SegmentInfo.SegmentTypeInfo.Mid;
                    _segmentData[i] = new(frameToUse, segmentType);
                }
            }
        }
        void updateSegments() {
            ThornsValues thornValues = new(Projectile);
            int thornsLength = thornValues.Length;
            for (int i = 0; i < thornsLength; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref SegmentInfo currentSegmentData = ref _segmentData[currentSegmentIndex],
                                previousSegmentData = ref _segmentData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                    continue;
                }
                //currentSegmentData.Progress += thornsLength / 50f;
                currentSegmentData.Progress += 0.2f;
                currentSegmentData.Progress = MathF.Min(1f, currentSegmentData.Progress);
            }
        }
        void makeDustsOnGrowth() {
            Vector2 velocityToMove = Projectile.velocity;
            Vector2 positionForDusts = Projectile.Center;
            int segmentHeight = SEGMENTHEIGHT;
            int thornsLength = GetThornsLength();
            for (int i = 0; i < thornsLength; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                SegmentInfo currentSegmentData = _segmentData[currentSegmentIndex],
                            previousSegmentData = _segmentData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                    continue;
                }

                int currentSegmentHeight = (int)(segmentHeight * currentSegmentData.Progress);
                Vector2 segmentVelocityToMove = velocityToMove.SafeNormalize();
                Vector2 offset = segmentVelocityToMove * currentSegmentHeight;
                positionForDusts += offset;

                if (Main.rand.NextChance(1.25f - i / (float)thornsLength)) {
                    if (currentSegmentData.Progress < 1f) {
                        SpawnThornsDust(positionForDusts - offset * 2f);
                    }
                }

                UpdateSegmentVelocity(ref velocityToMove, i);
            }
        }

        makeDustsOnGrowth();
        initSegmentInfo();
        updateSegments();
    }

    protected override void Draw(ref Color lightColor) {
        if (_thornsTexture?.IsLoaded != true) {
            return;
        }

        Texture2D segmentTexture = _thornsTexture.Value;
        int segmentWidth = segmentTexture.Width,
            segmentHeight = segmentTexture.Height / FRAMECOUNT;
        Vector2 velocityToMove = Projectile.velocity;
        Vector2 positionToDraw = Projectile.Center;
        for (int i = 0; i < GetThornsLength(); i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            SegmentInfo currentSegmentData = _segmentData[currentSegmentIndex],
                        previousSegmentData = _segmentData[previousSegmentIndex];
            if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                continue;
            }
            int currentSegmentHeight = (int)(segmentHeight * currentSegmentData.Progress);
            float segmentRotation = velocityToMove.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
            Vector2 segmentVelocityToMove = velocityToMove.SafeNormalize();
            Main.spriteBatch.DrawWith(segmentTexture, positionToDraw, DrawInfo.Default with {
                Color = Lighting.GetColor(positionToDraw.ToTileCoordinates()),
                Rotation = segmentRotation,
                Origin = new Vector2(segmentWidth, segmentHeight) / 2f,
                Clip = new Rectangle(0, currentSegmentData.FrameToUse * segmentHeight, segmentWidth, currentSegmentHeight)
            });
            if (currentSegmentData.IsStartSegment) {
                positionToDraw -= segmentVelocityToMove * 4f;
            }
            else if (!currentSegmentData.IsEndSegment) {
                positionToDraw -= segmentVelocityToMove * 2f;
            }
            positionToDraw += segmentVelocityToMove * segmentHeight;

            UpdateSegmentVelocity(ref velocityToMove, i);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 velocityToMove = Projectile.velocity;
        int collisionCheckHeight = SEGMENTHEIGHT;
        Vector2 positionForColliding = Projectile.Center + velocityToMove.SafeNormalize() * collisionCheckHeight * 2f;
        for (int i = 0; i < GetThornsLength(); i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            SegmentInfo currentSegmentData = _segmentData[currentSegmentIndex],
                        previousSegmentData = _segmentData[previousSegmentIndex];
            if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                continue;
            }

            int currentSegmentHeight = (int)(collisionCheckHeight * currentSegmentData.Progress);
            Vector2 segmentVelocityToMove = velocityToMove.SafeNormalize();
            positionForColliding += segmentVelocityToMove * currentSegmentHeight;
            if (Helper.CenteredSquare(positionForColliding, collisionCheckHeight).Intersects(targetHitbox)) {
                return true;
            }

            UpdateSegmentVelocity(ref velocityToMove, i);
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        void makeDustsOnKill() {
            Vector2 velocityToMove = Projectile.velocity;
            int segmentHeight = SEGMENTHEIGHT;
            Vector2 positionForDusts = Projectile.Center + velocityToMove.SafeNormalize() * segmentHeight * 2f;
            for (int i = 0; i < GetThornsLength(); i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                SegmentInfo currentSegmentData = _segmentData[currentSegmentIndex],
                            previousSegmentData = _segmentData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                    continue;
                }

                int currentSegmentHeight = (int)(segmentHeight * currentSegmentData.Progress);
                Vector2 segmentVelocityToMove = velocityToMove.SafeNormalize();
                Vector2 offset = segmentVelocityToMove * currentSegmentHeight;
                positionForDusts += offset;

                int dustCount = 4;
                for (int j = 0; j < dustCount; j++) {
                    SpawnThornsDust(positionForDusts - offset * 3f);
                }

                UpdateSegmentVelocity(ref velocityToMove, i);
            }
        }

        makeDustsOnKill();
    }

    private void SpawnThornsDust(Vector2 dustSpawnPosition) {
        float dustScale = 0.915f + 0.15f * Main.rand.NextFloat();
        Dust dust = Main.dust[Dust.NewDust(dustSpawnPosition, Projectile.width, Projectile.height, DustID.JunglePlants, 0f, 0f, 0, default, dustScale)];
        dust.noGravity = true;
        dust.fadeIn = 0.5f;
        dust.noLight = true;
    }

    private void LoadThornsTextures() {
        if (Main.dedServ) {
            return;
        }

        _thornsTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "JungleThorn");
    }

    private void UpdateSegmentVelocity(ref Vector2 velocityToMove, int segmentIndex) {
        ThornsValues thornValues = new(Projectile);
        static float smoothStep(float t) => t * t * (3f - 2f * t);
        float strength = MathF.Pow(smoothStep(segmentIndex / (float)thornValues.Length), 0.5f);
        float angle = MathHelper.WrapAngle(MathF.Sin(segmentIndex - MathHelper.PiOver4) * strength);
        velocityToMove += velocityToMove.RotatedBy(angle);
        velocityToMove += velocityToMove.RotatedBy(segmentIndex / MathF.Pow(MathHelper.TwoPi, 2f) * thornValues.WrapDirection);
    }
}
