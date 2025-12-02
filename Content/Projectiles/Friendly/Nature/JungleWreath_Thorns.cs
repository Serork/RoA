using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class JungleWreath_Thorns : NatureProjectile_NoTextureLoad {
    private static ushort MAXTIMELEFT => 180;
    private static byte FRAMECOUNT => 5;
    private static byte BASELENGTH => 10;
    private static byte SEGMENTHEIGHT => 18;
    private static float GROWTHSPEED => 0.3f;

    private static Asset<Texture2D>? _thornsTexture;

    private struct SegmentIterationArgs {
        public SegmentInfo Info;
        public byte Index;
        public byte Length;
        public Vector2 Position, PositionOffset;
        public Vector2 Velocity;
    }

    private struct SegmentInfo(byte frameToUse, SegmentInfo.SegmentTypeInfo segmentType) {
        public enum SegmentTypeInfo {
            Start,
            Mid,
            End
        }

        private float _progress = 0f;

        public float Progress {
            readonly get => _progress;
            set => _progress = MathUtils.Clamp01(value);
        }

        public readonly byte FrameToUse = frameToUse;
        public readonly SegmentTypeInfo SegmentType = segmentType;

        public readonly bool IsStartSegment => SegmentType == SegmentTypeInfo.Start;
        public readonly bool IsMidSegment => SegmentType == SegmentTypeInfo.Mid;
        public readonly bool IsEndSegment => SegmentType == SegmentTypeInfo.End;
    }

    public ref struct ThornsValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float LengthValue = ref projectile.ai[0];
        public ref float WrapDirectionValue = ref projectile.ai[1];
        public ref float LostHPPercentageValue = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public readonly int Length => (int)LengthValue;
        public readonly int WrapDirection => (int)WrapDirectionValue;
    }

    private SegmentInfo[]? _segmentData;

    private int GetThornsLength() => new ThornsValues(Projectile).Length;

    public override void SetStaticDefaults() {
        LoadThornsTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 5;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        void initSegmentInfo() {
            ThornsValues thornValues = new(Projectile);
            if (!thornValues.Init) {
                thornValues.Init = true;

                if (Projectile.IsOwnerLocal()) {
                    thornValues.LengthValue = BASELENGTH + thornValues.LostHPPercentageValue * BASELENGTH;
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
                ref SegmentInfo currentSegmentData = ref _segmentData![currentSegmentIndex],
                                previousSegmentData = ref _segmentData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                    continue;
                }
                //currentSegmentData.Opacity += thornsLength / 50f;
                currentSegmentData.Progress += GROWTHSPEED;
            }
        }
        void makeDustsOnGrowth() {
            DoOnSegmentIteration((segmentIterationArgs) => {
                if (Main.rand.NextChance(1.5f - segmentIterationArgs.Index / (float)segmentIterationArgs.Length)) {
                    if (segmentIterationArgs.Info.Progress < 1f) {
                        SpawnThornsDust(segmentIterationArgs.Position);
                    }
                }
            });
        }

        makeDustsOnGrowth();
        initSegmentInfo();
        updateSegments();
    }

    protected override void Draw(ref Color lightColor) {
        ThornsValues thornValues = new(Projectile);
        if (!thornValues.Init) {
            return;
        }

        if (_thornsTexture?.IsLoaded != true) {
            return;
        }

        Texture2D segmentTexture = _thornsTexture.Value;
        int segmentWidth = segmentTexture.Width,
            segmentHeight = segmentTexture.Height / FRAMECOUNT;
        DoOnSegmentIteration((segmentIterationArgs) => {
            int currentSegmentHeight = (int)(segmentHeight * segmentIterationArgs.Info.Progress);
            byte frameToUse = segmentIterationArgs.Info.FrameToUse;
            float segmentRotation = segmentIterationArgs.Velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
            Rectangle clip = new(0, frameToUse * segmentHeight, segmentWidth, currentSegmentHeight);
            Color color = Lighting.GetColor(segmentIterationArgs.Position.ToTileCoordinates());
            Vector2 origin = new Vector2(segmentWidth, segmentHeight) / 2;
            Main.spriteBatch.Draw(segmentTexture, segmentIterationArgs.Position, DrawInfo.Default with {
                Color = color,
                Rotation = segmentRotation,
                Origin = origin,
                Clip = clip
            });
        });
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        bool result = false;
        DoOnSegmentIteration((segmentIterationArgs) => {
            if (GeometryUtils.CenteredSquare(segmentIterationArgs.Position, SEGMENTHEIGHT).Intersects(targetHitbox)) {
                result = true;
            }
        });
        return result;
    }

    public override void OnKill(int timeLeft) {
        void makeKillDusts() {
            DoOnSegmentIteration((segmentIterationArgs) => {
                int dustCount = 4;
                for (int j = 0; j < dustCount; j++) {
                    SpawnThornsDust(segmentIterationArgs.Position);
                }
            });
        }

        makeKillDusts();
    }

    private void DoOnSegmentIteration(Action<SegmentIterationArgs> onIteration) {
        Vector2 velocityToMove = Projectile.velocity;
        int segmentHeight = SEGMENTHEIGHT;
        Vector2 segmentPosition = Projectile.Center;
        int thornsLength = GetThornsLength();
        for (int i = 0; i < thornsLength; i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            SegmentInfo currentSegmentData = _segmentData![currentSegmentIndex],
                        previousSegmentData = _segmentData[previousSegmentIndex];
            if (currentSegmentIndex > 0 && previousSegmentData.Progress < 1f) {
                continue;
            }

            int currentSegmentHeight = (int)(segmentHeight * currentSegmentData.Progress);
            Vector2 segmentVelocityToMove = velocityToMove.SafeNormalize();
            Vector2 offset = segmentVelocityToMove * currentSegmentHeight;
            onIteration(new SegmentIterationArgs() {
                Info = currentSegmentData,
                Index = (byte)currentSegmentIndex,
                Length = (byte)thornsLength,
                Position = segmentPosition,
                Velocity = velocityToMove,
                PositionOffset = offset,
            });
            if (currentSegmentData.IsStartSegment) {
                segmentPosition -= segmentVelocityToMove * 4f;
            }
            else if (!currentSegmentData.IsEndSegment) {
                segmentPosition -= segmentVelocityToMove * 2f;
            }
            segmentPosition += offset;

            UpdateSegmentVelocity(ref velocityToMove, i);
        }
    }

    private void SpawnThornsDust(Vector2 dustSpawnPosition) {
        float dustScale = 0.915f + 0.15f * Main.rand.NextFloat();
        int segmentHeight = SEGMENTHEIGHT;
        Dust dust = Main.dust[Dust.NewDust(dustSpawnPosition - Vector2.One * segmentHeight / 2f, segmentHeight, segmentHeight, DustID.JunglePlants, 0f, 0f, 0, default, dustScale)];
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
