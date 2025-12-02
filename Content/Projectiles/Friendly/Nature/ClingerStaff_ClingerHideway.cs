using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
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
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class ClingerHideway : NatureProjectile_NoTextureLoad, IRequestAssets {
    public static ushort MAXAVAILABLE => 2;
    private static ushort MAXTIMELEFT => 9000;
    private static ushort MINTIMELEFT => 220;
    private static float TIMELEFTODISAPPEAR => MAXTIMELEFT * 0.15f;
    private static float TIMELEFTOSTOPATTACKING => TIMELEFTODISAPPEAR;
    private static byte FRAMECOUNT => 3;
    private static float SHAKESTRENGTH => 5f;
    private static float SHAKEMINTOTRANSFORM => 1.5f;
    private static float SHAKESLOWDOWN => 0.7f;
    private static ushort CLINGERTIMETOSPAWNINTICKS => 30;
    private static byte SEGMENTCOUNT => 5;
    private static float CLINGERLENGTH => 70f;

    public enum ClingerHidewayRequstedTextureType : byte {
        Hideway,
        Clinger,
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ClingerHidewayRequstedTextureType.Hideway, ResourceManager.NatureProjectileTextures + "ClingerHideway"),
         ((byte)ClingerHidewayRequstedTextureType.Clinger, ResourceManager.NatureProjectileTextures + "Clinger")];

    private static readonly Rectangle _clingerHead1Clip = new(8, 2, 24, 32);
    private static readonly Rectangle _clingerHead2Clip = new(10, 46, 20, 28);
    private static readonly Rectangle _clingerHead3Clip = new(10, 48, 20, 28);

    private static readonly Rectangle _clingerSegment1Clip = new(42, 6, 24, 18);
    private static readonly Rectangle _clingerSegment2Clip = new(46, 36, 16, 14);
    private static readonly Rectangle _clingerSegment3Clip = new(46, 66, 16, 16);
    private static readonly Rectangle _clingerSegment4Clip = new(48, 96, 12, 12);

    public ref struct HidewayValues(Projectile projectile) {
        public enum State : byte {
            Spawned,
            Grown,
            Empty,
            Count,
        }

        public enum ClingerState : byte {
            Spawning,
            ReadyToAttack,
            Count
        }

        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float ClingerAITimer = ref projectile.localAI[1];
        public ref float ClingerFollowCursorFactor = ref projectile.localAI[2];
        public ref float OffsetX = ref projectile.ai[0];
        public ref float OffsetY = ref projectile.ai[1];
        public ref float StateValue = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public State CurrentState {
            readonly get => (State)StateValue;
            set => StateValue = Utils.Clamp((byte)value, (byte)State.Spawned, (byte)State.Count);
        }

        public Vector2 Shake {
            readonly get => new(OffsetX, OffsetY);
            set {
                OffsetX = value.X;
                OffsetY = value.Y;
            }
        }

        public float ClingerAIValue {
            readonly get => MathF.Max(ClingerAITimer - CLINGERTIMETOSPAWNINTICKS, 0f);
            set => ClingerAITimer = value + CLINGERTIMETOSPAWNINTICKS;
        }

        public ClingerState ClingerCurrentState {
            readonly get => (ClingerState)ClingerAIValue;
            set => ClingerAIValue = Utils.Clamp((byte)value, (byte)ClingerState.Spawning, (byte)ClingerState.Count);
        }

        public readonly bool IsEmpty => CurrentState == State.Empty;
        public readonly bool JustSpawned => CurrentState == State.Spawned;

        public readonly bool IsClingerSpawning => ClingerAITimer < CLINGERTIMETOSPAWNINTICKS;
        public readonly bool IsClingerSpawned => IsEmpty && !IsClingerSpawning;
        public readonly bool DidClingerAttackedEnough => projectile.timeLeft < TIMELEFTODISAPPEAR;
        public readonly bool DoesClingerNeedToHide => projectile.timeLeft < TIMELEFTOSTOPATTACKING;
        public readonly bool CanClingerDamage => IsClingerSpawned && !DidClingerAttackedEnough && ClingerFollowCursorFactor < 0.95f;

        public void RandomShake(float modifier = 1f) {
            if (projectile.IsOwnerLocal()) {
                Shake = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * SHAKESTRENGTH * modifier;
                projectile.As<ClingerHideway>()._oldShake = -Shake;
                projectile.netUpdate = true;
            }

            MakeStoneDusts();

            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Stonebreak") { Volume = 0.7f, PitchVariance = 0.1f, Pitch = 0.3f, MaxInstances = 3 }, projectile.Center);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "ClawsRoot") { Volume = 0.5f, PitchVariance = 0.1f, Pitch = -0.5f, MaxInstances = 3 }, projectile.Center);
        }

        public void MakeStoneDusts(int? dustCount = null) {
            bool flag = dustCount == null;
            dustCount ??= 6;
            float speed = 1f;
            if (!JustSpawned && !IsEmpty) {
                speed = 2f;
            }
            if (flag) {
                if (JustSpawned) {
                    dustCount = 4;
                }
                else if (IsEmpty) {
                    dustCount = 4;
                }
            }
            for (int i = 0; i < dustCount; i++) {
                int mid = dustCount.Value / 2;
                bool firstPair = i < mid;
                float extraAngle = MathHelper.PiOver4 * (firstPair ? (i < mid / 2) : (i - mid < mid / 2)).ToDirectionInt();
                extraAngle -= MathHelper.PiOver4;
                float angle = firstPair ? extraAngle : (MathHelper.Pi + extraAngle);
                Vector2 velocity = Vector2.One.RotatedBy(angle);
                velocity = velocity.RotatedByRandom(Main.rand.NextFloatRange(MathHelper.PiOver2)) * Main.rand.NextFloat(0.75f, 1f);
                velocity *= speed;
                Vector2 position = projectile.Center;
                if (!JustSpawned) {
                    position += velocity * projectile.width / 2f;
                }
                else {
                    position += velocity * projectile.width / 6f;
                }
                ushort type = (ushort)ModContent.DustType<Dusts.ClingerHideway>();
                Dust dust = Dust.NewDustPerfect(position, type, velocity);
                dust.customData = (byte)(!JustSpawned).ToInt();
            }
        }
    }

    private record struct SegmentInfo(Vector2 Position);

    private Vector2 _oldShake;
    private Vector2 _clingerPosition = Vector2.Zero;
    private float _clingerWaveMovementFactor;
    private SegmentInfo[]? _segmentData;
    private Vector2 _mousePosition;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.manualDirectionChange = true;
    }

    public override void AI() {
        void init() {
            HidewayValues hidewayValues = new(Projectile);
            if (!hidewayValues.Init) {
                hidewayValues.Init = true;
                hidewayValues.RandomShake();

                hidewayValues.ClingerFollowCursorFactor = 1f;

                Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

                Projectile.velocity *= 0f;
                _segmentData = new SegmentInfo[SEGMENTCOUNT];
                for (int i = 0; i < SEGMENTCOUNT; i++) {
                    ref SegmentInfo segmentInfo = ref _segmentData[i];
                    segmentInfo.Position = Projectile.Center;
                }
            }
        }
        void handleShakes() {
            HidewayValues hidewayValues = new(Projectile);
            Vector2 shake = hidewayValues.Shake;
            float lerpValue = shake.Length() * SHAKESLOWDOWN;
            hidewayValues.Shake = Helper.Approach(shake, _oldShake, lerpValue);
            if (_oldShake.Distance(shake) < 0.1f) {
                HidewayValues.State state = hidewayValues.CurrentState;
                float oldShakeLength = _oldShake.Length();
                if (state == HidewayValues.State.Empty && oldShakeLength <= SHAKEMINTOTRANSFORM) {
                    _oldShake *= 0f;
                    return;
                }
                _oldShake = -hidewayValues.Shake.SafeNormalize().RotatedBy(Main.rand.NextFloat(MathHelper.PiOver2)) * oldShakeLength;
                _oldShake *= 0.5f;
                float shakeModifier = 0.5f;
                if (oldShakeLength <= SHAKEMINTOTRANSFORM * shakeModifier) {
                    hidewayValues.CurrentState++;
                    hidewayValues.RandomShake(shakeModifier);
                }
            }
        }
        void handleClinger() {
            HidewayValues hidewayValues = new(Projectile);
            if (!hidewayValues.IsEmpty) {
                return;
            }

            ref float clingerAITimer = ref hidewayValues.ClingerAITimer;
            Vector2 center = Projectile.Center;
            Player owner = Projectile.GetOwnerAsPlayer();
            if (hidewayValues.IsClingerSpawning) {
                if (clingerAITimer == CLINGERTIMETOSPAWNINTICKS - 5) {
                    SoundEngine.PlaySound(SoundID.Item100, Projectile.Center);
                    SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "SteamBurst") { Volume = 0.7f, Pitch = -0.3f }, Projectile.Center);
                }
                clingerAITimer++;
                _clingerPosition = center;
            }
            else {
                ref float clingerFollowCursorFactor = ref hidewayValues.ClingerFollowCursorFactor;
                _mousePosition = Vector2.Lerp(_mousePosition, owner.GetWorldMousePosition(), clingerFollowCursorFactor);
                float maxOffset = CLINGERLENGTH;
                float distance = _mousePosition.Distance(Projectile.Center) * 0.5f;
                distance = MathF.Max(10f, distance);
                float offset = distance < maxOffset ? distance : maxOffset;
                Vector2 mousePositionOffset = _mousePosition.DirectionFrom(center) * offset;
                Vector2 destination = center + mousePositionOffset;
                if (hidewayValues.DidClingerAttackedEnough) {
                    destination = center;
                    float minDistance = 50f;
                    if (_clingerPosition.Distance(destination) < minDistance) {
                        //Projectile.Opacity = 0f;
                        Projectile.Kill();
                    }
                }
                clingerFollowCursorFactor = Helper.Approach(clingerFollowCursorFactor, 0f, 0.01f);
                _clingerPosition = Vector2.Lerp(_clingerPosition, destination, 0.15f/* * clingerFollowCursorFactor*/);
                float baseRotation = Projectile.DirectionTo(_clingerPosition).ToRotation() - MathHelper.PiOver2;
                _clingerPosition += (Vector2.UnitY * Utils.Remap(1f - hidewayValues.ClingerAITimer / CLINGERTIMETOSPAWNINTICKS, 0f, 1f, 1.5f, 5f)).RotatedBy(baseRotation + Math.Sin(Projectile.whoAmI + Main.timeForVisualEffects / 10))/* * clingerFollowCursorFactor*/;
            }
        }
        void handleClingerBody() {
            HidewayValues hidewayValues = new(Projectile);
            if (!hidewayValues.IsEmpty) {
                return;
            }

            Vector2 centerPoint(Vector2 a, Vector2 b) => new((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
            for (int i = 0; i < SEGMENTCOUNT; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, currentSegmentIndex - 1),
                    nextSegmentIndex = Math.Min(SEGMENTCOUNT - 1, currentSegmentIndex + 1);
                ref SegmentInfo currentSegmentInfo = ref _segmentData![currentSegmentIndex],
                                previousSegmentInfo = ref _segmentData[previousSegmentIndex],
                                nextSegmentInfo = ref _segmentData[nextSegmentIndex];
                Vector2 startPosition = previousSegmentInfo.Position;
                Vector2 center = Projectile.Center + Vector2.One * 1f;
                if (previousSegmentIndex == currentSegmentIndex) {
                    Vector2 offset = center.DirectionTo(_clingerPosition) * 4f;
                    startPosition = center;
                    if (!Utils.HasNaNs(offset)) {
                        startPosition += offset;
                    }
                }
                if (currentSegmentIndex == 1) {
                    Vector2 offset = startPosition.DirectionTo(_clingerPosition) * -2f;
                    if (!Utils.HasNaNs(offset)) {
                        startPosition += offset;
                    }
                }
                Vector2 destination = nextSegmentInfo.Position;
                if (currentSegmentIndex == nextSegmentIndex) {
                    Vector2 offset = _clingerPosition.DirectionTo(center) * 16f;
                    destination = _clingerPosition;
                    if (!Utils.HasNaNs(offset)) {
                        destination += offset;
                    }
                }
                if (currentSegmentIndex == 0) {
                    currentSegmentInfo.Position = startPosition;
                    continue;
                }
                if (startPosition.Distance(destination) < 1f) {
                    continue;
                }
                currentSegmentInfo.Position = centerPoint(startPosition, destination);
            }
        }
        void spawnCursedDusts() {
            HidewayValues hidewayValues = new(Projectile);
            if (!hidewayValues.CanClingerDamage) {
                return;
            }

            Vector2 startPosition = _clingerPosition;
            int length = 30;
            for (int i = 0; i < length; i++) {
                float areaSize = 8f;
                Vector2 offset = Projectile.Center.DirectionTo(_clingerPosition);
                int direction = offset.X.GetDirection();
                startPosition += offset * areaSize;
                for (float angle = 0; (float)angle < MathHelper.TwoPi; angle += 0.1f) {
                    if (Main.rand.NextChance(0.015f)) {
                        Vector2 position = startPosition + Vector2.One.RotatedBy(angle) * Main.rand.NextFloat(areaSize);
                        Dust dust = Dust.NewDustPerfect(position, 75, Alpha: 100);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                        dust.velocity.Y -= 0.5f;
                        dust.scale = 1.4f;
                        dust.position.X += 6f * direction;
                    }
                }
            }
        }
        void emitDusts() {
            if (Main.rand.NextBool(50)) {
                Dust.NewDust(Projectile.Center - Vector2.One * 10f, 20, 20, DustID.Demonite);
            }
        }

        init();
        handleShakes();
        handleClinger();
        handleClingerBody();
        spawnCursedDusts();
        emitDusts();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        HidewayValues hidewayValues = new(Projectile);
        if (!hidewayValues.CanClingerDamage) {
            return false;
        }

        Vector2 startPosition = _clingerPosition;
        int length = 30;
        for (int i = 0; i < length; i++) {
            float areaSize = 8f;
            Vector2 offset = Projectile.Center.DirectionTo(_clingerPosition);
            startPosition += offset * areaSize;
            for (float angle = 0; (float)angle < MathHelper.TwoPi; angle += 0.1f) {
                Vector2 position = startPosition + Vector2.One.RotatedBy(angle) * Main.rand.NextFloat(areaSize);
                if (targetHitbox.Contains(position.ToPoint())) {
                    return true;
                }
            }
        }

        return false;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_oldShake);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _oldShake = reader.ReadVector2();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<ClingerHideway>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D hidewayTexture = indexedTextureAssets[(byte)ClingerHidewayRequstedTextureType.Hideway].Value,
                  clingerTexture = indexedTextureAssets[(byte)ClingerHidewayRequstedTextureType.Clinger].Value;
        SpriteBatch batch = Main.spriteBatch;
        Color color = lightColor;
        void drawHideway() {
            HidewayValues hidewayValues = new(Projectile);
            Texture2D texture = hidewayTexture;
            Vector2 position = Projectile.Center + hidewayValues.Shake;
            SpriteFrame frame = new(1, FRAMECOUNT, 0, (byte)hidewayValues.CurrentState);
            Rectangle clip = frame.GetSourceRectangle(texture);
            Vector2 scale = Vector2.One;
            Vector2 origin = clip.Size() / 2f;
            SpriteEffects flip = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            batch.Draw(texture, position, DrawInfo.Default with {
                Origin = origin,
                Clip = clip,
                Color = color,
                Scale = scale,
                ImageFlip = flip
            });
        }
        Vector2 waveAnimationOffset(float baseRotation, int index) => Vector2.UnitY.RotatedBy(baseRotation + Math.Sin(index + Main.timeForVisualEffects / 10));
        float clingerRotation = _clingerPosition.DirectionFrom(Projectile.Center).ToRotation() + MathHelper.PiOver2;
        void drawClinger() {
            HidewayValues hidewayValues = new(Projectile);
            if (hidewayValues.IsClingerSpawning) {
                return;
            }

            Texture2D texture = clingerTexture;
            Vector2 position = _clingerPosition;
            position += waveAnimationOffset(clingerRotation, SEGMENTCOUNT - 1);
            position += Vector2.UnitY.RotatedBy(clingerRotation) * 6f;
            Rectangle clip = _clingerHead1Clip;
            Vector2 scale = Vector2.One * ((float)hidewayValues.ClingerAITimer / CLINGERTIMETOSPAWNINTICKS);
            Vector2 origin = clip.Size() / 2f;
            Color headColor = Lighting.GetColor(position.ToTileCoordinates()) * Projectile.Opacity;
            batch.Draw(texture, position, DrawInfo.Default with {
                Origin = origin,
                Clip = clip,
                Color = headColor,
                Scale = scale,
                Rotation = clingerRotation
            });
        }
        void drawClingerBody() {
            HidewayValues hidewayValues = new(Projectile);
            if (hidewayValues.IsClingerSpawning) {
                return;
            }

            Texture2D texture = clingerTexture;
            List<SegmentInfo> segmentsToDraw = [];
            SegmentInfo[] segmentData = _segmentData!;
            for (int i = 2; i < SEGMENTCOUNT; i++) {
                segmentsToDraw.Add(segmentData[i]);
            }
            segmentsToDraw.Add(segmentData[1]);
            segmentsToDraw.Add(segmentData[0]);
            int last = segmentsToDraw.Count - 1;
            for (int i = last; i >= 0; i--) {
                int currentSegmentIndex = i;
                int previousSegmentIndex = Math.Max(0, currentSegmentIndex - 1),
                    nextSegmentIndex = Math.Min(last - 1, currentSegmentIndex + 1);
                SegmentInfo segmentInfo = segmentsToDraw[currentSegmentIndex],
                            previousSegmentInfo = segmentsToDraw[previousSegmentIndex],
                            nextSegmentInfo = segmentsToDraw[nextSegmentIndex];
                Vector2 position = segmentInfo.Position;
                float rotation = position.DirectionTo(nextSegmentInfo.Position).ToRotation() + MathHelper.PiOver2;
                if (nextSegmentIndex == currentSegmentIndex) {
                    rotation = clingerRotation;
                }
                position += waveAnimationOffset(rotation, currentSegmentIndex);
                Rectangle clip = _clingerSegment3Clip;
                if (currentSegmentIndex == last) {
                    clip = _clingerSegment4Clip;
                }
                else if (currentSegmentIndex >= 1) {
                    clip = _clingerSegment2Clip;
                }
                Vector2 scale = Vector2.One;
                Vector2 origin = clip.Size() / 2f;
                Color bodyColor = Lighting.GetColor(position.ToTileCoordinates()) * Projectile.Opacity;
                batch.Draw(texture, position, DrawInfo.Default with {
                    Origin = origin,
                    Clip = clip,
                    Color = bodyColor,
                    Scale = scale,
                    Rotation = rotation
                });
            }
        }

        drawHideway();
        drawClingerBody();
        drawClinger();
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 20; i++) {
            int width = 30, height = 30;
            Vector2 position = Projectile.Center - new Vector2(width, height) / 2;
            ushort type = (ushort)ModContent.DustType<Dusts.ClingerHideway>();
            Dust dust = Dust.NewDustDirect(position, width, height, type);
            dust.customData = 1;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(39, 420);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(39, 420);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (target.aiStyle == 6 || target.aiStyle == 37) {
            modifiers.FinalDamage /= 2;
        }
    }
}
