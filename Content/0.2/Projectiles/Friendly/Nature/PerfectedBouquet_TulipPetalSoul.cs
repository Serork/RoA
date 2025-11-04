using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Wreath;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TulipPetalSoul : NatureProjectile, IRequestExtraAIValue {
    private static byte BASESIZE => 16;
    private static ushort MAXTIMELEFT => 360;
    private static byte FRAMECOUNT => 3;
    private static byte TRAILCOUNT => 5;
    private static ushort TRAILFRAMETIME => 30;
    private static ushort SAWANIMATIONTIME => 10;
    private static float SAWANIMATIONSPEED => 0.5f;
    private static ushort LASERSPAWNTIME => 20;
    private static ushort BOUNCYTIMETODIVIDE => 30;
    private static float PSEUDODESTROYEDOPACITY => 0.5f;

    public static Color SoulColor => new Color(Color.White.R, Color.White.G, Color.White.B, 150) * 0.9f;

    private static Asset<Texture2D>? _sawEffectTexture;

    [Flags]
    public enum PetalState : byte {
        None = 0,
        Normal = 1 << 0,
        Saw = 1 << 1,
        Laser = 1 << 2,
        Bouncy = 1 << 3,
        BouncyRepeated = 1 << 4,
        Count = 1 << 5
    }

    public enum PetalType : byte {
        SkeletronPrime,
        Destroyer,
        Twins,
        Count
    }

    public ref struct TulipPetalSoulValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float RotationValue = ref projectile.localAI[1];
        public ref float RotationValue2 = ref projectile.localAI[2];
        public ref float StateValue = ref projectile.ai[0];
        public ref float SawHitEnemyValue = ref projectile.ai[1];
        public ref float SpawnLaserTimer = ref projectile.ai[2];
        public ref float BouncyTimer = ref projectile.GetExtraAI()[0];
        public ref float VariantValue = ref projectile.GetExtraAI()[1];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public PetalState CurrentState {
            readonly get => (PetalState)StateValue;
            set => StateValue = Utils.Clamp((byte)value, (byte)PetalState.Normal, (byte)PetalState.Count);
        }

        public PetalType CurrentType {
            readonly get => (PetalType)VariantValue;
            set => VariantValue = Utils.Clamp((byte)value, (byte)PetalType.SkeletronPrime, (byte)PetalType.Count);
        }

        public readonly byte GetStateIndex() => (byte)CurrentType;

        public bool SawHit {
            readonly get => SawHitEnemyValue != 0f;
            set => SawHitEnemyValue = value.ToInt();
        }

        public bool BouncyTimePassed {
            get => BouncyTimer++ >= BOUNCYTIMETODIVIDE;
            set => BouncyTimer = value ? BOUNCYTIMETODIVIDE : 0;
        }

        public readonly bool NoActiveStates() => CurrentState == PetalState.None;
        public readonly bool IsStateActive(PetalState checkState) => (CurrentState & checkState) != 0;
        public readonly bool CanApplySawEffect() => IsStateActive(PetalState.Saw) && SawHit;
        public readonly bool CanApplyLaserEffect() => IsStateActive(PetalState.Laser);
        public readonly bool CanApplyBouncyEffect() => IsStateActive(PetalState.Bouncy);
        public readonly bool ShouldMakeNormalTrails() => !CanApplySawEffect();

        public void ApplySawEffect() {
            SawHit = true;

            SoundEngine.PlaySound(SoundID.Item22 with { Pitch = 1f }, projectile.position);

            projectile.velocity *= 0.1f;
            projectile.netUpdate = true;
        }
    }

    private struct TrailInfo {
        private float _opacity;
        private byte _frame;

        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public Vector2 Scale;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);
        }

        public byte Frame {
            readonly get => _frame;
            set => _frame = Utils.Clamp<byte>(value, 0, Dusts.Tulip.ROWCOUNT);
        }
    }

    private TrailInfo[]? _trailData;
    private float _canSpawnTrailTimer;
    private byte _currentCopyIndex;

    private bool PseudoDestroyed => Projectile.Opacity <= PSEUDODESTROYEDOPACITY;
    private byte TulipDustFrame => (byte)(Dusts.Tulip.SOULORANGE + Projectile.frame);
    private bool DidEnoughDamage => Projectile.penetrate <= 1;
    private float PseudoKillOpacity => Utils.Remap(Projectile.Opacity, PSEUDODESTROYEDOPACITY, 1f, 0f, 1f);

    byte IRequestExtraAIValue.NeededAmountOfAI => 2;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(FRAMECOUNT);

        LoadExtraNeededTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(BASESIZE);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1 + 3;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        void pickTulipOption() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.Init) {
                tulipPetalSoulValues.Init = true;

                bool hasStateAlready = !tulipPetalSoulValues.NoActiveStates();
                _trailData = new TrailInfo[TRAILCOUNT];

                if (!hasStateAlready) {
                    Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                }

                tulipPetalSoulValues.RotationValue = tulipPetalSoulValues.RotationValue2 = MathHelper.Pi;

                float flySpeed = 9f;
                Projectile.velocity = Projectile.velocity.SafeNormalize() * flySpeed;

                tulipPetalSoulValues.SpawnLaserTimer = LASERSPAWNTIME / 2;

                if (!hasStateAlready && tulipPetalSoulValues.NoActiveStates() && Projectile.IsOwnerLocal()) {
                    tulipPetalSoulValues.CurrentState = PetalState.Normal;
                    //byte currentType = (byte)Main.rand.Next(3);
                    //tulipPetalSoulValues.CurrentType = (PetalType)currentType;
                    PetalState randomState = (PetalState)(1 << (1 + (byte)tulipPetalSoulValues.CurrentType));
                    tulipPetalSoulValues.CurrentState |= randomState;

                    if (WreathHandler.IsWreathCharged(Projectile.GetOwnerAsPlayer())) {
                        tulipPetalSoulValues.CurrentState |= PetalState.Saw | PetalState.Laser | PetalState.Bouncy;
                    }

                    Projectile.netUpdate = true;
                }
            }
        }
        void setFrame() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            Projectile.frame = tulipPetalSoulValues.GetStateIndex();
        }
        void handleMainMovement() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (tulipPetalSoulValues.IsStateActive(PetalState.Normal)) {
                if (tulipPetalSoulValues.ShouldMakeNormalTrails()) {
                    MakeTrails(-Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.25f) * 0.1f);
                }
                //Projectile.position += Projectile.velocity;
            }
        }
        void rotate() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.ShouldMakeNormalTrails()) {
                return;
            }
            ref float rotation = ref Projectile.rotation;
            ref float extraRotation = ref tulipPetalSoulValues.RotationValue;
            ref float extraRotation2 = ref tulipPetalSoulValues.RotationValue2;
            float velocityFactor = Utils.Remap(Projectile.velocity.Length(), 0f, 3f, 0.5f, 1f, true);
            rotation += extraRotation / extraRotation2 * Projectile.direction * velocityFactor;
            float timerValue = 0.01f;
            extraRotation2 += timerValue;
            float minFactor = 0.75f;
            float deceleration = 0.96f;
            if (extraRotation > minFactor) {
                extraRotation *= deceleration;
            }
        }
        void updateTrails() {
            for (int i = 0; i < TRAILCOUNT; i++) {
                ref TrailInfo trailInfo = ref _trailData![i];
                ref Vector2 position = ref trailInfo.Position;
                position = Vector2.Lerp(position, Projectile.position, 0.025f);
                position += trailInfo.Velocity;
                trailInfo.Rotation *= 0.997f;
                Vector2 circularMovement = (Vector2.One * trailInfo.Velocity.Length() * 2.5f * MathF.Sign(trailInfo.Velocity.X)).RotatedBy(trailInfo.Rotation);
                trailInfo.Position += circularMovement;
                trailInfo.Velocity *= 0.95f;
                if (trailInfo.Opacity > 0f) {
                    trailInfo.Opacity = MathF.Max(0f, trailInfo.Opacity - 0.025f);
                }
            }
        }
        void applySawEffect() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.CanApplySawEffect()) {
                return;
            }

            Projectile.Opacity = 1f;

            Projectile.tileCollide = false;
            Projectile.SetSizeValues(BASESIZE * 5);

            MakeTrails(Vector2.One.RotatedByRandom(MathHelper.TwoPi));

            Projectile.velocity *= 0.95f;
            ref float sawValue = ref tulipPetalSoulValues.SawHitEnemyValue;
            sawValue = MathF.Min(SAWANIMATIONTIME, sawValue + SAWANIMATIONSPEED);
            ref float rotation = ref Projectile.rotation;
            ref float extraRotation = ref tulipPetalSoulValues.RotationValue;
            ref float extraRotation2 = ref tulipPetalSoulValues.RotationValue2;
            extraRotation2 += 0.005f * sawValue;
            float velocityFactor = Utils.Remap(Projectile.velocity.Length(), 0f, 3f, 0.5f, 1f, true);
            extraRotation += velocityFactor * 0.005f;
            rotation += extraRotation / extraRotation2 * Projectile.direction * velocityFactor;
        }
        void applyLaserEffect() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.CanApplyLaserEffect()) {
                return;
            }

            NPC? target = NPCUtils.FindClosestNPC(Projectile.position, 12 * 16);
            if (target == null) {
                return;
            }

            if (tulipPetalSoulValues.SpawnLaserTimer++ > LASERSPAWNTIME) {
                tulipPetalSoulValues.SpawnLaserTimer = 0f;

                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 1f }, Projectile.position);
                MakeTulipDusts();

                if (Projectile.IsOwnerLocal()) {
                    Vector2 position = Projectile.position;
                    Vector2 velocity = position.DirectionTo(target.Center);
                    IEntitySource source = Projectile.GetSource_FromAI();
                    int damage = Projectile.damage;
                    float knockBack = Projectile.knockBack;
                    ProjectileUtils.SpawnPlayerOwnedProjectile<TulipBlast>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), source) with {
                        Position = position,
                        Velocity = velocity,
                        Damage = damage,
                        KnockBack = knockBack,
                        AI1 = Projectile.frame,
                        AI2 = target.whoAmI
                    });
                }
            }
        }
        void applyBouncyEffect() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.CanApplyBouncyEffect()) {
                return;
            }

            if (!tulipPetalSoulValues.BouncyTimePassed) {
                return;
            }

            if (Projectile.IsOwnerLocal()) {
                for (int i = -1; i < 3; i += 2) {
                    Vector2 baseVelocity = Projectile.velocity;
                    Vector2 velocity = baseVelocity.SafeNormalize().RotatedBy(MathHelper.PiOver4 * 0.5f * i) * baseVelocity.Length();
                    Projectile projectile = ProjectileUtils.SpawnPlayerOwnedProjectileCopy<TulipPetalSoul>(new ProjectileUtils.SpawnCopyArgs(Projectile, Projectile.GetSource_FromAI()) with {
                        Velocity = velocity,
                        AI0 = (byte)((tulipPetalSoulValues.CurrentState & ~PetalState.Bouncy) | PetalState.BouncyRepeated)
                    }, (projectile) => {
                        _ = new TulipPetalSoulValues(projectile) {
                            CurrentType = new TulipPetalSoulValues(Projectile).CurrentType
                        };
                    });
                    //projectile.netUpdate = true;
                }
            }

            SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 1f }, Projectile.position);

            PseudoKill(false);
        }
        void handlePseudoKill() {
            if (!PseudoDestroyed) {
                if (DidEnoughDamage) {
                    PseudoKill();
                }
                return;
            }

            Projectile.tileCollide = false;
            Projectile.damage = 0;

            Projectile.Opacity -= 0.025f;
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        void addLight() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            PetalType currentType = tulipPetalSoulValues.CurrentType;
            if (currentType == PetalType.Destroyer) {
                float num6 = (float)Main.rand.Next(90, 111) * 0.01f;
                num6 *= Main.essScale;
                Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), 0.1f * num6, 0.1f * num6, 0.6f * num6);
            }
            else if (currentType == PetalType.SkeletronPrime) {
                float num5 = (float)Main.rand.Next(90, 111) * 0.01f;
                num5 *= Main.essScale;
                Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), 0.5f * num5, 0.3f * num5, 0.05f * num5);
            }
            else if (currentType == PetalType.Twins) {
                float num8 = (float)Main.rand.Next(90, 111) * 0.01f;
                num8 *= Main.essScale;
                Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), 0.1f * num8, 0.5f * num8, 0.2f * num8);
            }
        }
        pickTulipOption();
        setFrame();
        handleMainMovement();
        rotate();
        updateTrails();
        if (!PseudoDestroyed) {
            applySawEffect();
            applyLaserEffect();
            applyBouncyEffect();
        }
        handlePseudoKill();
        addLight();
    }

    public override bool ShouldUpdatePosition() => true;

    public override void OnKill(int timeLeft) {
        if (PseudoDestroyed) {
            return;
        }

        MakeTulipDusts(true);
        SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) => hitbox = hitbox.AdjustPosition(-Projectile.Size / 2f);
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => HandleHitting();
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => HandleHitting();

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Color projectileColor = SoulColor;
        TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
        if (!tulipPetalSoulValues.Init) {
            return false;
        }
        float sawSizeFactor = Utils.Remap(tulipPetalSoulValues.SawHitEnemyValue, 0f, SAWANIMATIONTIME, 0f, 1f);
        SpriteEffects flip = Projectile.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        void drawTrails() {
            Texture2D texture = DustLoader.GetDust(ModContent.DustType<Dusts.Tulip>()).Texture2D.Value;
            for (int i = 0; i < TRAILCOUNT; i++) {
                TrailInfo trailInfo = _trailData![i];
                bool tooCloseToProjectile = MathUtils.Approximately(trailInfo.Position, Projectile.Center, 2f);
                if (trailInfo.Opacity <= 0f || tooCloseToProjectile) {
                    continue;
                }
                Vector2 position = trailInfo.Position;
                float rotation = Projectile.rotation;
                Vector2 scale = trailInfo.Scale * Projectile.scale * 0.85f;
                Color color = projectileColor * trailInfo.Opacity;
                Rectangle clip = new SpriteFrame(Dusts.Tulip.COLUMNCOUNT, Dusts.Tulip.ROWCOUNT, TulipDustFrame, trailInfo.Frame).GetSourceRectangle(texture);
                clip.Width += 2;
                Vector2 origin = clip.Size() / 2f;
                batch.Draw(texture, position, DrawInfo.Default with {
                    Color = color,
                    Rotation = rotation,
                    Scale = scale,
                    Origin = origin,
                    Clip = clip,
                    ImageFlip = flip
                });
            }
        }
        Vector2 getSawShakingOffset() => Main.rand.RandomPointInArea(5f * (1f - Math.Clamp(sawSizeFactor, 0f, 0.75f)));
        void drawSawEffect() {
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (!tulipPetalSoulValues.CanApplySawEffect()) {
                return;
            }

            if (_sawEffectTexture?.IsLoaded != true) {
                return;
            }
            Texture2D texture = _sawEffectTexture.Value;
            Vector2 position = Projectile.position + getSawShakingOffset();
            Color color = projectileColor * PseudoKillOpacity;
            float rotation = Projectile.rotation;
            Vector2 scale = Vector2.One * Ease.CircOut(sawSizeFactor) * Projectile.scale;
            Rectangle clip = new SpriteFrame(1, FRAMECOUNT, 0, (byte)Projectile.frame).GetSourceRectangle(texture);
            Vector2 origin = clip.Size() / 2f;
            batch.Draw(texture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Scale = scale,
                Clip = clip,
                Origin = origin,
                ImageFlip = flip
            });
        }
        void drawSelf() {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 position = Projectile.position;
            TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
            if (tulipPetalSoulValues.CanApplySawEffect()) {
                position += getSawShakingOffset();
            }
            Color color = projectileColor * (PseudoDestroyed && !DidEnoughDamage ? 0f : PseudoKillOpacity);
            float rotation = Projectile.rotation;
            Vector2 scale = Vector2.One * Projectile.scale;
            Rectangle clip = new SpriteFrame(1, FRAMECOUNT, 0, (byte)Projectile.frame).GetSourceRectangle(texture);
            Vector2 origin = clip.Size() / 2f;
            batch.Draw(texture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Scale = scale,
                Clip = clip,
                Origin = origin,
                ImageFlip = flip
            });
        }

        drawTrails();
        drawSawEffect();
        drawSelf();

        return false;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        PseudoKill();

        return false;
    }

    public override bool? CanCutTiles() => !PseudoDestroyed;
    public override bool? CanDamage() => !DidEnoughDamage && !PseudoDestroyed;

    private void MakeTulipDusts(bool onKill = false) {
        int tulipDustCount = new TulipPetalSoulValues(Projectile).SawHit ? (onKill ? 6 : 5) : (onKill ? 3 : 2);
        for (int i = 0; i < tulipDustCount; i++) {
            float offset2 = 10f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = Projectile.position - randomOffset / 2f + randomOffset;
            Dust dust = Dust.NewDustPerfect(Projectile.position,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - Projectile.position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f) + Projectile.velocity,
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f) * 1.5f,
                                            Alpha: TulipDustFrame);
            dust.customData = Main.rand.NextFloatRange(50f);
            if (onKill) {
                dust.velocity *= 0.75f;
            }
        }
    }

    private void MakeTrails(Vector2 velocity) {
        if (PseudoDestroyed) {
            return;
        }

        TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
        ref float extraRotation = ref tulipPetalSoulValues.RotationValue;
        float addValue = 0.5f + 2f * extraRotation / MathHelper.Pi;
        _canSpawnTrailTimer += addValue;
        if (_canSpawnTrailTimer < TRAILFRAMETIME) {
            return;
        }

        _canSpawnTrailTimer = 0;
        if (_currentCopyIndex >= TRAILCOUNT) {
            _currentCopyIndex = 0;
        }
        _trailData![_currentCopyIndex++] = new TrailInfo() {
            Position = Projectile.position,
            Opacity = 1f,
            Scale = Vector2.One,
            Rotation = Projectile.rotation,
            Velocity = velocity,
            Frame = (byte)Main.rand.Next(Dusts.Tulip.ROWCOUNT)
        };
    }

    private void HandleHitting() {
        TulipPetalSoulValues tulipPetalSoulValues = new(Projectile);
        if (tulipPetalSoulValues.IsStateActive(PetalState.Saw) && !tulipPetalSoulValues.SawHit) {
            tulipPetalSoulValues.ApplySawEffect();
        }
        if (DidEnoughDamage) {
            if (tulipPetalSoulValues.IsStateActive(PetalState.Bouncy)) {
                tulipPetalSoulValues.BouncyTimePassed = true;
            }
        }
    }

    private void LoadExtraNeededTextures() {
        if (Main.dedServ) {
            return;
        }

        _sawEffectTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "TulipBuzzsaw");
    }

    private void PseudoKill(bool onKill = true) {
        if (onKill) {
            SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
        }
        Projectile.Opacity = MathF.Min(Projectile.Opacity, PSEUDODESTROYEDOPACITY - 0.01f);
        MakeTulipDusts(onKill);
    }
}
