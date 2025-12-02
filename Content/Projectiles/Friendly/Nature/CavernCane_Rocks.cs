using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.Druid.Wreath;
using RoA.Common.Projectiles;
using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Items.Weapons.Nature.Hardmode.Canes;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CavernCane_Rocks : NatureProjectile_NoTextureLoad, IUseCustomImmunityFrames {
    public static Color GetGeodeColor(GemType gemType) {
        return gemType switch {
            GemType.Amethyst => Color.Purple,
            GemType.Topaz => Color.Yellow,
            GemType.Sapphire => Color.Blue,
            GemType.Emerald => Color.Green,
            GemType.Ruby => Color.Red,
            GemType.Diamond => Color.White,
            GemType.Amber => Color.Orange,
            _ => Color.Transparent
        };
    }
    public static short GetGeodeDustType(GemType gemType) {
        return gemType switch {
            GemType.Amethyst => DustID.GemAmethyst,
            GemType.Topaz => DustID.GemTopaz,
            GemType.Sapphire => DustID.GemSapphire,
            GemType.Emerald => DustID.GemEmerald,
            GemType.Ruby => DustID.GemRuby,
            GemType.Diamond => DustID.GemDiamond,
            GemType.Amber => DustID.AmberBolt,
            _ => 0
        };
    }

    private const byte FRAMECOUNT = 3;

    private static byte ROCKATTACKCOUNT => 3;
    private static ushort MAXTIMELEFT => 360;
    private static ushort MAXROCKDISTANCE => 300;
    private static float MAXPROGRESS => 1.35f;
    private static float MAXGRAVITY => 5f;

    private static Asset<Texture2D>? _rocksTexture;
    private static BlendState? _multiplyBlendState;

    private struct RocksInfo {
        public const byte ROCKSCOUNT = 2;

        public static byte HITBOXSIZE => 26;
        public static byte TRAILLENGTH => 3;

        private byte _usedFrame1, _usedFrame2;
        private float _progress;

        public float Angle;
        public Vector2 Scale;
        public float StartDistance;
        public bool Collided, Collided2;
        public float CollisionAngle;
        public Vector2 ExtraPosition1, ExtraPosition2;
        public float Gravity;
        public Vector2[] TrailPositions;

        public RocksInfo() => TrailPositions = new Vector2[TRAILLENGTH];

        public float Progress {
            readonly get => _progress;
            set => _progress = Utils.Clamp(value, 0f, MAXPROGRESS);
        }

        public byte Frame1 {
            readonly get => _usedFrame1;
            set => _usedFrame1 = Utils.Clamp<byte>(value, 0, FRAMECOUNT);
        }
        public byte Frame2 {
            readonly get => _usedFrame2;
            set => _usedFrame2 = Utils.Clamp<byte>(value, 0, FRAMECOUNT);
        }

        public readonly float Opacity => Utils.GetLerpValue(0f, 0.5f, Progress, true);

        public readonly byte GetUsedFrame(bool first) => first ? Frame1 : Frame2;

        public readonly float GetSizeByUsedFrame(bool first) {
            float result = 0.7f;
            switch (first ? Frame1 : Frame2) {
                case 0:
                    result = 0.9f;
                    break;
                case 1:
                    result = 0.7f;
                    break;
                case 2:
                    result = 0.7f;
                    break;
            }
            float resultModifier = 1f;
            return result * resultModifier;
        }

        public readonly Vector2 GetExtraPosition(bool first) => first ? ExtraPosition1 : ExtraPosition2;

        public void ApplyGravity() {
            Gravity += 0.1f;
            Gravity = MathF.Min(MAXGRAVITY, Gravity);

            ExtraPosition1 += Vector2.UnitY * Gravity;
            ExtraPosition2 += Vector2.UnitY * Gravity;
        }
    }
    public ref struct RocksValues(Projectile projectile) {
        public static float FORCEDOPACITYINCREASEVALUE => 0.015f;

        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float ForcedOpacityValue = ref projectile.localAI[1];
        public ref float ThrowRocksValue = ref projectile.localAI[2];
        public ref float GeodeTypeValue = ref projectile.ai[0];
        public ref float CaneAttackTimeValue = ref projectile.ai[1];
        public ref float OwnerAttackValue = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public float ForcedOpacity {
            readonly get => ForcedOpacityValue;
            set => ForcedOpacityValue = MathUtils.Clamp01(value);
        }

        public bool ThrewRocks {
            readonly get => ThrowRocksValue == 1f;
            set => ThrowRocksValue = value.ToInt();
        }

        public GemType GeodeType {
            readonly get => (GemType)GeodeTypeValue;
            set => GeodeTypeValue = Utils.Clamp((byte)value, (byte)GemType.Amethyst, (byte)(GemType.Amber + 1));
        }

        public bool OwnerStoppedAttacking {
            readonly get => OwnerAttackValue == 1f;
            set => OwnerAttackValue = value.ToInt();
        }

        public readonly int CaneUseTime => (int)CaneAttackTimeValue;
    }

    private RocksInfo[]? _rocks;

    public static SoundStyle GetStoneHitSound(float geodeProgress) => SoundID.DD2_MonkStaffGroundImpact with { Pitch = 1f - 0.5f * geodeProgress };
    public static SoundStyle StoneExplosion { get; private set; } = SoundID.Item89 with { Pitch = 1f };

    public override void SetStaticDefaults() {
        LoadRocksTexture();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(16);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        GemType geodeType = new RocksValues(Projectile).GeodeType;
        Color dustColor = GetGeodeColor(geodeType);
        int dustType = GetGeodeDustType(geodeType);
        void checkActive() {
            Player owner = Projectile.GetOwnerAsPlayer();
            if (owner.IsLocal()) {
                RocksValues rocksValues = new(Projectile);
                if (owner.GetWreathHandler().TryGetHeldCane(out CaneBaseProjectile? heldCane)) {
                    if (!rocksValues.OwnerStoppedAttacking) {
                        CavernCane.CavernCaneBase heldCavernCaneData = (CavernCane.CavernCaneBase)heldCane!;
                        if (!heldCavernCaneData.IsInUse) {
                            rocksValues.OwnerStoppedAttacking = true;
                            Projectile.netUpdate = true;
                        }
                    }
                }
                else {
                    rocksValues.OwnerStoppedAttacking = true;
                }
            }
        }
        void init() {
            RocksValues rocksValues = new(Projectile);
            if (!rocksValues.Init) {
                rocksValues.Init = true;

                CustomImmunityFramesHandler.Initialize(Projectile, (ushort)(ROCKATTACKCOUNT * 2));

                //if (Projectile.IsOwnerLocal()) {
                //    rocksValues.GeodeType = Main.rand.GetRandomEnumValue<GemType>();
                //    Projectile.netUpdate = true;
                //}

                _rocks = new RocksInfo[ROCKATTACKCOUNT];
                for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                    int nextIndex = i + 1;
                    float angle = nextIndex * MathHelper.PiOver2;
                    bool third = nextIndex % 3 == 0;
                    if (third) {
                        angle -= MathHelper.PiOver4;
                    }
                    byte getRandomizedFrame() => (byte)Main.rand.Next(FRAMECOUNT);
                    _rocks[i] = new RocksInfo() {
                        Progress = 0f,
                        Angle = MathHelper.PiOver4 + angle,
                        Scale = Vector2.One,
                        StartDistance = MAXROCKDISTANCE,
                        Frame1 = getRandomizedFrame(),
                        Frame2 = getRandomizedFrame()
                    };
                }
            }
        }
        void makeDustOnRockCollision() {
            float geodeSize = GetGeodeSize();
            float iterationValue = 1f - 0.17f * geodeSize / MathF.Pow(ROCKATTACKCOUNT, 0.1f);
            iterationValue /= ROCKATTACKCOUNT;
            for (float i = 0f; i < 1f; i += iterationValue) {
                Vector2 dustVelocity = Vector2.UnitY.RotatedBy(i * MathHelper.TwoPi + Main.rand.NextFloat() * MathHelper.TwoPi * 0.5f) * (4f + Main.rand.NextFloat() * 2f);
                Vector2 geodeProgressOffset = geodeSize * Vector2.One.RotatedBy(dustVelocity.ToRotation()) * iterationValue * 15f;
                Vector2 dustPosition = Projectile.Center + geodeProgressOffset;
                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Stone, dustVelocity, 0);
                dust.velocity *= 0.5f;
            }
        }
        void processRocks() {
            RocksValues rocksValues = new(Projectile);
            for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                int currentRocksIndex = i,
                    previousRocksIndex = Math.Max(0, i - 1);
                ref RocksInfo currentRocksData = ref _rocks![currentRocksIndex],
                              previousRocksData = ref _rocks[previousRocksIndex];
                if (!rocksValues.ThrewRocks) {
                    if (currentRocksIndex > 0 && previousRocksData.Progress < MAXPROGRESS) {
                        continue;
                    }
                }

                float rockSpeed = MAXPROGRESS * ROCKATTACKCOUNT / rocksValues.CaneUseTime;
                bool reachedPosition = currentRocksData.Progress >= 1f;
                if (reachedPosition) {
                    rockSpeed *= 2f;
                }
                if (rocksValues.OwnerStoppedAttacking || rocksValues.ThrewRocks) {
                    rockSpeed *= 0.625f;

                    currentRocksData.ApplyGravity();
                }
                if (!rocksValues.OwnerStoppedAttacking || rocksValues.ThrewRocks) {
                    currentRocksData.Progress += (!rocksValues.ThrewRocks).ToDirectionInt() * rockSpeed;
                    currentRocksData.Progress = Utils.Clamp(currentRocksData.Progress, 0f, MAXPROGRESS);
                    if (reachedPosition && !currentRocksData.Collided) {
                        currentRocksData.Collided = true;
                        makeDustOnRockCollision();

                        void makeHitSounds() {
                            SoundEngine.PlaySound(GetStoneHitSound(GetGeodeProgress()), Projectile.Center);
                        }
                        makeHitSounds();
                    }
                    float collisionAngleLerpValue = rockSpeed * 10f;
                    if (currentRocksData.Collided && !currentRocksData.Collided2) {
                        float sizeByUsedFrames = (currentRocksData.GetSizeByUsedFrame(true) + currentRocksData.GetSizeByUsedFrame(false)) / 2f;
                        float collisionAngle = currentRocksData.Angle / 2f * (1f + sizeByUsedFrames / 2f);
                        currentRocksData.CollisionAngle = Helper.Approach(currentRocksData.CollisionAngle, collisionAngle, collisionAngleLerpValue);
                        if (currentRocksData.CollisionAngle >= collisionAngle) {
                            currentRocksData.Collided2 = true;
                        }
                    }
                    if (currentRocksData.Collided2 && currentRocksData.CollisionAngle > 0f) {
                        currentRocksData.CollisionAngle = Helper.Approach(currentRocksData.CollisionAngle, 0f, collisionAngleLerpValue);
                    }
                }
                else {
                    currentRocksData.Progress += rockSpeed;
                    rocksValues.ForcedOpacity += RocksValues.FORCEDOPACITYINCREASEVALUE;
                }
            }
        }
        void makeGeodeDustsAndGoresAndSounds() {
            Vector2 getOffsetPosition(float value) {
                float geodeSize = GetGeodeSize();
                float iterationValue = 1f - 0.17f * geodeSize / MathF.Pow(ROCKATTACKCOUNT, 0.1f);
                iterationValue /= ROCKATTACKCOUNT;
                Vector2 dustVelocity = Vector2.UnitY.RotatedBy(value * MathHelper.TwoPi + Main.rand.NextFloat() * MathHelper.TwoPi * 0.5f) * (4f + Main.rand.NextFloat() * 2f);
                Vector2 geodeProgressOffset = geodeSize * Vector2.One.RotatedBy(dustVelocity.ToRotation()) * iterationValue * 15f;
                return geodeProgressOffset;
            }
            void makeDusts() {
                for (int i = 0; i < 8; i++) {
                    Vector2 dustSpawnPosition = Projectile.Center + getOffsetPosition(i / 8f);
                    float dustScale = 0.8f + Main.rand.NextFloat() * 0.6f;
                    int dustType2 = DustID.RainbowMk2;
                    int dust = Dust.NewDust(dustSpawnPosition, 4, 4, dustType2, Scale: dustScale, newColor: dustColor, Alpha: 0);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].color = dustColor;
                    Main.dust[dust].velocity *= 0.5f;
                }
            }
            void makeGores() {
                RocksValues rocksValues = new(Projectile);
                ushort option = (ushort)(rocksValues.GeodeType + 1);
                string name = string.Empty;
                switch (option) {
                    case 1:
                        name = "Amethyst";
                        break;
                    case 2:
                        name = "Topaz";
                        break;
                    case 3:
                        name = "Sapphire";
                        break;
                    case 4:
                        name = "Emerald";
                        break;
                    case 5:
                        name = "Ruby";
                        break;
                    case 6:
                        name = "Diamond";
                        break;
                    case 7:
                        name = "Amber";
                        break;
                }
                if (!Main.dedServ) {
                    if (name != string.Empty) {
                        int goreCount = 3;
                        for (int i = 0; i < goreCount; i++) {
                            int currentIndex = i + 1;
                            float progress = currentIndex / goreCount;
                            Vector2 gorePositionOffset = getOffsetPosition(progress);
                            Vector2 gorePosition = Projectile.Center + gorePositionOffset;
                            int gore = Gore.NewGore(Projectile.GetSource_Misc("caverncanehit"),
                                gorePosition - Vector2.UnitY * 4f,
                                Vector2.One.RotatedBy(currentIndex * MathHelper.TwoPi / goreCount) * 2f, ModContent.Find<ModGore>(RoA.ModName + $"/{name}2").Type, 1f);
                            Main.gore[gore].velocity *= 0.5f;
                        }
                    }
                }
            }
            void makeSounds() {
                SoundEngine.PlaySound(StoneExplosion, Projectile.Center);
            }

            makeGores();
            makeDusts();
            makeSounds();
        }
        void throwRocksWhenCharged() {
            RocksValues rocksValues = new(Projectile);
            if (rocksValues.ThrewRocks || rocksValues.OwnerStoppedAttacking || !IsGeodeCharged()) {
                return;
            }
            rocksValues.ThrewRocks = true;

            void resetSomeRockInfo() {
                RocksValues rocksValues = new(Projectile);
                for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                    ref RocksInfo currentRocksData = ref _rocks![i];
                    currentRocksData.Progress = 1f;
                    currentRocksData.StartDistance *= 0.7f;
                    ref float rocksAngle = ref currentRocksData.Angle;
                    int nextIndex = i + 1;
                    bool second = nextIndex % 2 == 0;
                    bool third = nextIndex % 3 == 0;
                    if (rocksValues.ThrewRocks) {
                        if (third) {
                            rocksAngle += MathHelper.PiOver2;
                        }
                        else if (second) {
                            rocksAngle += MathHelper.PiOver4 / 3f;
                        }
                        else {
                            rocksAngle -= MathHelper.PiOver4 / 3f;
                        }
                    }
                }
            }
            void resetDamageInfo() {
                for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                    for (int j = 0; j < RocksInfo.ROCKSCOUNT; j++) {
                        for (int npcId = 0; npcId < Main.npc.Length; npcId++) {
                            ref ushort immuneTime = ref GetImmuneTime((byte)i, (byte)j, npcId);
                            if (immuneTime > 0) {
                                immuneTime = 0;
                            }
                        }
                    }
                }
            }

            resetSomeRockInfo();
            makeDustOnRockCollision();
            makeGeodeDustsAndGoresAndSounds();
            resetDamageInfo();
        }
        float getGeneralProgressForVariousPurposes(RocksInfo rocksData) {
            RocksValues rocksValues = new(Projectile);
            return (1f - rocksValues.ForcedOpacity) * rocksData.Opacity * rocksData.Progress;
        }
        void damageNPCs() {
            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            RocksValues rocksValues = new(Projectile);
            for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                RocksInfo rocksData = _rocks![i];
                bool canDamage = rocksData.Progress != MAXPROGRESS && getGeneralProgressForVariousPurposes(rocksData) >= 0.01f;
                if (!canDamage) {
                    continue;
                }

                for (int j = 0; j < RocksInfo.ROCKSCOUNT; j++) {
                    bool firstRock = j == 0;
                    Vector2 rockPositionToHandleCollision = GetRockPosition(i, firstRock, out _);
                    foreach (NPC npcForCollisionCheck in Main.ActiveNPCs) {
                        if (!NPCUtils.DamageNPCWithPlayerOwnedProjectile(npcForCollisionCheck, Projectile,
                                                                         ref GetImmuneTime((byte)i, (byte)j, npcForCollisionCheck.whoAmI),
                                                                         damageSourceHitbox: GeometryUtils.CenteredSquare(rockPositionToHandleCollision, RocksInfo.HITBOXSIZE),
                                                                         direction: MathF.Sign(rockPositionToHandleCollision.X - npcForCollisionCheck.Center.X))) {
                            continue;
                        }
                    }
                }
            }
        }
        void cutTiles() {
            RocksValues rocksValues = new(Projectile);
            for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                RocksInfo rocksData = _rocks![i];
                bool canCutTiles = getGeneralProgressForVariousPurposes(rocksData) >= 0.01f;
                if (!canCutTiles) {
                    continue;
                }

                for (int j = 0; j < RocksInfo.ROCKSCOUNT; j++) {
                    ProjectileUtils.CutTilesAt(Projectile, GetRockPosition(i, j == 0, out _), RocksInfo.HITBOXSIZE, RocksInfo.HITBOXSIZE);
                }
            }
        }
        void releaseProjectile() {
            RocksValues rocksValues = new(Projectile);
            if (rocksValues.ForcedOpacity >= 1f || (rocksValues.ThrewRocks && GetGeodeProgress() <= 0f)) {
                Projectile.Kill();
            }
        }
        void makeGeodeDustsOverTime() {
            RocksValues rocksValues = new(Projectile);
            if (rocksValues.ThrewRocks) {
                return;
            }

            for (int i = 0; i < ROCKATTACKCOUNT; i++) {
                RocksInfo rocksData = _rocks![i];
                float geodeProgress = getGeneralProgressForVariousPurposes(rocksData);
                if (Main.rand.NextChance(geodeProgress / ROCKATTACKCOUNT / 20f)) {
                    float dustScale = 0.8f + Main.rand.NextFloat() * 0.6f;
                    int dustAreaSize = 10;
                    Vector2 dustVelocity = Vector2.One.RotateRandom(MathHelper.TwoPi);
                    Vector2 dustSpawnPosition = Projectile.Center + dustVelocity - Vector2.One * dustAreaSize / 2f;
                    int dust = Dust.NewDust(dustSpawnPosition, dustAreaSize, dustAreaSize, dustType, Scale: dustScale, newColor: dustColor, Alpha: 0);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].color = dustColor;
                    Main.dust[dust].velocity += dustVelocity * 5f * geodeProgress;
                    Main.dust[dust].velocity *= 0.5f;
                }
            }
        }

        checkActive();
        init();
        processRocks();
        throwRocksWhenCharged();
        damageNPCs();
        cutTiles();
        releaseProjectile();
        //makeGeodeDustsOverTime();
    }

    protected override void Draw(ref Color lightColor) {
        RocksValues rocksValues = new(Projectile);
        if (!rocksValues.Init) {
            return;
        }

        if (_rocksTexture?.IsLoaded != true) {
            return;
        }

        Texture2D rocksTexture = _rocksTexture!.Value;
        Color gemColor = Color.White * Ease.QuadIn(GetGeodeProgress()) * 0.5f;
        void drawGeodeGem(float opacity = 1f) {
            RocksValues rocksValues = new(Projectile);
            Texture2D gemTexture = TextureAssets.Gem[(byte)rocksValues.GeodeType].Value;
            Vector2 gemPosition = Projectile.Center;
            float forcedOpacity = 1f - rocksValues.ForcedOpacity;
            Main.spriteBatch.Draw(gemTexture, gemPosition, DrawInfo.Default with {
                Color = gemColor * opacity * forcedOpacity,
                Origin = gemTexture.Size() / 2f,
                Clip = gemTexture.Bounds,
                Scale = Vector2.One
            });
        }
        void drawRocks() {
            RocksValues rocksValues = new(Projectile);
            float forcedOpacity = 1f - rocksValues.ForcedOpacity;
            for (int i = ROCKATTACKCOUNT - 1; i >= 0; i--) {
                int currentRocksIndex = i,
                    previousRocksIndex = Math.Max(0, i - 1);
                ref RocksInfo currentRocksData = ref _rocks![currentRocksIndex],
                              previousRocksData = ref _rocks[previousRocksIndex];
                if (!rocksValues.ThrewRocks) {
                    if (currentRocksIndex > 0 && previousRocksData.Progress < MAXPROGRESS) {
                        continue;
                    }
                }

                float rocksOpacity = currentRocksData.Opacity;
                for (int j = 0; j < RocksInfo.ROCKSCOUNT; j++) {
                    bool firstRock = j == 0;
                    Vector2 rockdrawPosition = GetRockPosition(i, firstRock, out float rockProgress);
                    Color color = Lighting.GetColor(rockdrawPosition.ToTileCoordinates());
                    color *= rocksOpacity;
                    color *= forcedOpacity;
                    int width = rocksTexture.Width / FRAMECOUNT;
                    Rectangle sourceRectangle = Rectangle.Empty with { X = width * currentRocksData.GetUsedFrame(firstRock), Width = width, Height = rocksTexture.Height };
                    float xScaleFactor = Ease.CircOut(rockProgress) * 0.75f * Ease.CircOut(1f - MathUtils.Clamp01(currentRocksData.Progress));
                    xScaleFactor *= 1f - currentRocksData.Gravity / MAXGRAVITY;
                    Vector2 rockScale = currentRocksData.Scale * new Vector2(1f + xScaleFactor, 1f);
                    float rockRotation = rockdrawPosition.DirectionTo(Projectile.Center).ToRotation();
                    Vector2 rockOrigin = sourceRectangle.Size() / 2f;
                    // trails
                    for (int k = RocksInfo.TRAILLENGTH - 1; k > 0; k--) {
                        currentRocksData.TrailPositions[k] = currentRocksData.TrailPositions[k - 1];
                    }
                    currentRocksData.TrailPositions[0] = rockdrawPosition;
                    for (int k = 0; k < RocksInfo.TRAILLENGTH; k++) {
                        Vector2 traildrawPosition = currentRocksData.TrailPositions[k];
                        if (traildrawPosition == Vector2.Zero) {
                            continue;
                        }
                        float trailOpacity = 0.75f;
                        Color trailColor = Lighting.GetColor(traildrawPosition.ToTileCoordinates()) * ((float)k / RocksInfo.TRAILLENGTH * trailOpacity) * rocksOpacity * forcedOpacity;
                        Main.spriteBatch.Draw(rocksTexture, traildrawPosition, DrawInfo.Default with {
                            Color = trailColor,
                            Rotation = rockRotation,
                            Origin = rockOrigin,
                            Clip = sourceRectangle,
                            Scale = rockScale
                        });
                    }
                    // rock
                    Main.spriteBatch.Draw(rocksTexture, rockdrawPosition, DrawInfo.Default with {
                        Color = color,
                        Rotation = rockRotation,
                        Origin = rockOrigin,
                        Clip = sourceRectangle,
                        Scale = rockScale
                    });
                }
            }
        }
        void drawGeodeEffectOnTop() {
            _multiplyBlendState ??= new() {
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.SourceAlpha
            };
            SpriteBatch batch = Main.spriteBatch;
            SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
            batch.Begin(snapshot with { blendState = _multiplyBlendState }, true);
            drawGeodeGem(1f);
            batch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            drawGeodeGem(1.5f);
            batch.Begin(snapshot, true);
        }

        drawGeodeGem();
        drawRocks();
        drawGeodeEffectOnTop();
    }

    public override void OnKill(int timeLeft) { }

    public override bool? CanDamage() => false;

    private ref ushort GetImmuneTime(byte rockIndex, byte pairIndex, int npcId) => ref CustomImmunityFramesHandler.GetImmuneTime(Projectile, (byte)(rockIndex * 2 + pairIndex), npcId);

    private void LoadRocksTexture() {
        if (Main.dedServ) {
            return;
        }

        _rocksTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "CavernCaneRock");
    }

    private Vector2 GetRockPosition(int rockIndex, bool firstRock, out float rockProgress) {
        float collisionAngle = 0f;
        for (int i = 0; i < ROCKATTACKCOUNT; i++) {
            RocksInfo currentRocksData = _rocks![i];
            collisionAngle += currentRocksData.CollisionAngle;
        }
        RocksInfo rockData = _rocks![rockIndex];
        int nextIndex = rockIndex + 1;
        bool second = nextIndex % 2 == 0;
        bool third = nextIndex % 3 == 0;
        float rocksAngle = rockData.Angle;
        float rockCollisionProgress = MAXPROGRESS * rockData.GetSizeByUsedFrame(firstRock) - (MAXPROGRESS - rockData.Progress);
        float reachedPositionOffset = (third ? 0.055f : 0.035f) * rockCollisionProgress;
        float rockProgressBaseValue = Ease.SineIn(rockData.Progress);
        float reversedRockProgress = 1f - rockProgressBaseValue;
        rockProgress = Utils.Clamp(reversedRockProgress, reachedPositionOffset, 1f);
        int rockDirection = firstRock.ToDirectionInt();
        Vector2 rockPosition = Vector2.UnitY.RotatedBy(rocksAngle) * rockDirection * rockData.StartDistance * rockProgress;
        Vector2 rockExtraPosition = rockData.GetExtraPosition(firstRock);
        Vector2 resultRockPosition = Projectile.Center + rockPosition + rockExtraPosition;
        Vector2 movementOffset = rockCollisionProgress * Vector2.One.RotatedBy(rocksAngle);
        resultRockPosition += movementOffset;
        float shakeStrength = second ? 0.5f : 1f;
        Vector2 onCollisionShakeOffset = rockCollisionProgress * Vector2.One.RotatedBy(collisionAngle) * shakeStrength;
        resultRockPosition += onCollisionShakeOffset;
        return resultRockPosition;
    }

    private bool IsGeodeCharged() {
        float geodeProgress = GetGeodeProgress(true);
        float maxProgress = MAXPROGRESS * ROCKATTACKCOUNT;
        float progress = geodeProgress / maxProgress;
        return progress >= 0.95f;
    }

    private float GetGeodeSize() {
        float geodeSize = 0f;
        for (int i = 0; i < ROCKATTACKCOUNT; i++) {
            RocksInfo currentRocksData = _rocks![i];
            float value = (currentRocksData.GetSizeByUsedFrame(true) + currentRocksData.GetSizeByUsedFrame(false)) * 0.75f;
            value *= currentRocksData.Progress;
            geodeSize += value;
        }
        return geodeSize;
    }

    private float GetGeodeProgress(bool generalProgress = false) {
        float geodeProgress = 0f;
        for (int i = 0; i < ROCKATTACKCOUNT; i++) {
            RocksInfo currentRocksData = _rocks![i];
            geodeProgress += currentRocksData.Progress;
        }
        return geodeProgress / (generalProgress ? 1f : MathF.Pow(ROCKATTACKCOUNT, 1.5f));
    }
}
