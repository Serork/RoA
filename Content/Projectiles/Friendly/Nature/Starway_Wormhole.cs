using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TargetLoader : ILoadable {
    public static RenderTarget2D LowResTarget = null!;

    void ILoadable.Load(Mod mod) {
        Main.QueueMainThreadAction(() => {
            LowResTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, 840, 525, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        });

    }

    void ILoadable.Unload() {
        if (LowResTarget is not null) {
            Main.QueueMainThreadAction(() => {
                LowResTarget.Dispose();
            });

            LowResTarget = null!;
        }
    }
}

[Tracked]
sealed class StarwayWormhole : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    private static float STEP_BASEDONTEXTUREWIDTH => 90f * 0.85f;

    private static Asset<Texture2D> _behindTexture = null!,
                                    _tentacleTexture = null!;

    private record struct TentacleInfo(float Angle, float Length, float Progress = 0f);
    private record struct WormSegmentInfo(Vector2 Position, byte Frame, float Rotation, TentacleInfo[] TentacleData, bool Body = true, 
        bool Broken = false, float BrokenProgress = 0f, bool ShouldShake = false, float ShakeCooldown = 0f, float ShakeCooldown2 = 0f, float Opacity = 0f,
        bool Destroyed = false, float DestroyProgress = 0f);

    private WormSegmentInfo[] _wormData = null!;
    private GeometryUtils.BezierCurve _bezierCurve = null!;
    private bool _drawLights;
    private Player _lightPlayer = null!;

    private record struct PlayerLightInfo(Vector2[] Positions, float[] Rotations);

    private PlayerLightInfo[] _playerLightData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float StartWaveValue => ref Projectile.ai[0];
    public ref float UsedValue => ref Projectile.ai[2];

    public Vector2 GetNextPositionForAdventure(float progress) {
        progress = MathUtils.Clamp01(progress);
        int length = _wormData.Length;
        int index = (int)(length * progress);
        index = length - index;
        index = Math.Clamp(index, 0, length - 1);
        return _wormData[index].Position;
    }

    public List<Vector2> GetPositionsForAdventure(int pointCount) => _bezierCurve.GetPoints(pointCount);
    public List<Vector2> GetPositionsForAdventure2() {
        List<Vector2> result = [];
        foreach (WormSegmentInfo wormSegmentInfo in _wormData) {
            result.Add(wormSegmentInfo.Position);
        }
        return result;
    }

    public Vector2 LastPosition => _wormData[0].Position;
    public Vector2 StartPosition => _wormData[^1].Position;

    public float LastAngle => _wormData[^1].Rotation;
    public float FirstAngle => _wormData[0].Rotation;

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public bool Used {
        get => UsedValue != 0f;
        set => UsedValue = value.ToInt();
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _behindTexture = ModContent.Request<Texture2D>(Texture + "_Behind");
            _tentacleTexture = ModContent.Request<Texture2D>(Texture + "_Tentacle");
        }
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.hide = true;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (Used) {
            overPlayers.Add(index);
        }
    }

    public override bool? CanDamage() => true;
    public override bool? CanCutTiles() => true;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        int length = _wormData.Length;
        for (int i = 0; i < length; i++) {
            ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
            if (wormSegmentInfo.Body) {
                for (int k = 0; k < wormSegmentInfo.TentacleData.Length; k++) {
                    TentacleInfo tentacleInfo = wormSegmentInfo.TentacleData[k];
                    if (tentacleInfo.Progress <= 0f) {
                        continue;
                    }
                    Vector2 to = wormSegmentInfo.Position + Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation + tentacleInfo.Angle) * tentacleInfo.Length * 3.25f * tentacleInfo.Progress;
                    Vector2 start = wormSegmentInfo.Position;
                    if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, to)) {
                        return true;
                    }
                }
            }
        }

        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void AI() {
        Projectile.hide = Used;

        if (Used) {
            Projectile.timeLeft += 5;
        }

        void init() {
            if (!Init) {

                Init = true;

                Player owner = Projectile.GetOwnerAsPlayer();

                Projectile.SetDirection(owner.direction);

                if (owner.IsLocal()) {
                    StartWaveValue = Main.rand.NextFloatDirection() * MathHelper.TwoPi;
                }

                void initSegments() {
                    int index = 0;
                    Vector2 mousePosition = owner.GetViableMousePosition();
                    Vector2 playerCenter = owner.GetPlayerCorePoint();
                    float spawnOffset3 = MathF.Max(600f, mousePosition.Distance(playerCenter));
                    Vector2 startPosition = playerCenter + playerCenter.DirectionTo(mousePosition) * spawnOffset3,
                            endPosition = playerCenter;
                    List<(Vector2, float)> segmentPositions = [];
                    Vector2 velocity = startPosition.DirectionTo(endPosition);
                    Vector2 startPosition2 = startPosition;
                    Vector2 velocity2 = velocity;
                    float waveWidth = MathHelper.Lerp(0.25f, 0.375f, 0.5f) * Main.rand.NextBool().ToDirectionInt();
                    int maxCount = 10;
                    while (index < maxCount) {
                        float step = STEP_BASEDONTEXTUREWIDTH;
                        float distance = startPosition.Distance(endPosition);
                        if (distance < step) {
                            break;
                        }

                        float rotation = velocity.ToRotation();

                        float waveFactor = 0f + MathHelper.Lerp(0f, 1f, 1f - MathUtils.Clamp01(distance / (step * 5f)));
                        waveFactor = Ease.CubeOut(waveFactor);
                        velocity = velocity.RotatedBy(MathF.Sin(StartWaveValue + index) * waveWidth * Projectile.direction * (1f - waveFactor));
                        velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), waveFactor);

                        waveWidth = Helper.Approach(waveWidth, 0f, 0.025f);

                        segmentPositions.Add((startPosition, rotation));

                        startPosition += velocity * step;

                        startPosition2 += velocity2 * step;

                        index++;
                    }
                    int length = segmentPositions.Count;
                    _wormData = new WormSegmentInfo[length];
                    bool body = false;
                    for (int i = 0; i < length; i++) {
                        Vector2 position = segmentPositions[i].Item1;
                        float rotation = segmentPositions[i].Item2;
                        bool last = i == length - 1;
                        if (last) {
                            body = false;
                        }

                        TentacleInfo[] tentacleData = new TentacleInfo[10];
                        int half = tentacleData.Length / 2;
                        float sectorAngle = MathHelper.Pi * 0.25f;
                        float halfSector = sectorAngle / 2;
                        float offset = sectorAngle / half * 0.5f;
                        float lengthProgress = 0.625f;
                        int increaseIndex = 0;
                        for (int k = 0; k < half; k++) {
                            float angle = -halfSector + (k * sectorAngle / half + offset);
                            float tentacleLength = lengthProgress * 100f;
                            tentacleData[k] = new TentacleInfo(angle, tentacleLength, -Main.rand.NextFloat() * 0.5f);
                            lengthProgress += 0.2f * (increaseIndex < half / 2).ToDirectionInt();
                            increaseIndex++;
                        }
                        lengthProgress = 0.625f;
                        increaseIndex = 0;
                        for (int k = half; k < tentacleData.Length; k++) {
                            float angle = MathHelper.Pi - halfSector + ((k - half) * sectorAngle / half + offset);
                            float tentacleLength = lengthProgress * 100f;
                            tentacleData[k] = new TentacleInfo(angle, tentacleLength, -Main.rand.NextFloat() * 0.5f);
                            lengthProgress += 0.2f * (increaseIndex < half / 2).ToDirectionInt();
                            increaseIndex++;
                        }

                        _wormData[i] = new WormSegmentInfo(position, 0, rotation, tentacleData, Body: body);
                        body = true;
                    }
                    List<Vector2> segmentPositions2 = [];
                    foreach ((Vector2, float) segmentPosition in segmentPositions) {
                        segmentPositions2.Add(segmentPosition.Item1);
                    }
                    _bezierCurve = new GeometryUtils.BezierCurve([.. segmentPositions2]);
                }
                void initLights() {
                    _playerLightData = new PlayerLightInfo[3];
                    for (int i = 0; i < _playerLightData.Length; i++) {
                        ref PlayerLightInfo playerLightInfo = ref _playerLightData[i];
                        int max = 10;
                        playerLightInfo.Positions = new Vector2[max];
                        playerLightInfo.Rotations = new float[max];
                    }
                }

                initSegments();
                initLights();
            }
        }
        void initLights(bool hardInit = false) {
            for (int i = 0; i < _playerLightData.Length; i++) {
                ref PlayerLightInfo playerLightInfo = ref _playerLightData[i];
                if (!hardInit) {
                    for (int num28 = playerLightInfo.Positions.Length - 1; num28 > 0; num28--) {
                        playerLightInfo.Positions[num28] = Vector2.Lerp(playerLightInfo.Positions[num28], playerLightInfo.Positions[num28 - 1], 0.5f);
                        playerLightInfo.Rotations[num28] = Utils.AngleLerp(playerLightInfo.Rotations[num28], playerLightInfo.Rotations[num28 - 1], 0.5f);
                    }
                }

                float y = 15f;
                Player player = _lightPlayer;
                Vector2 angle = Vector2.UnitY.RotatedBy(player.velocity.ToRotation());
                Vector2 position = player.Center + angle * Helper.Wave(-y, y, 10f, player.whoAmI + i * 2f);
                position -= angle.RotatedBy(MathHelper.PiOver2) * -i * (i == 0 ? 15f : 10f);
                position += angle.RotatedBy(MathHelper.PiOver2) * -10f;
                float rotation = 0f;

                if (!hardInit) {
                    playerLightInfo.Positions[0] = Vector2.Lerp(playerLightInfo.Positions[0], position, 0.5f);
                    playerLightInfo.Rotations[0] = Utils.AngleLerp(playerLightInfo.Rotations[0], rotation, 0.5f);
                }
                else {
                    for (int num28 = 0; num28 < playerLightInfo.Positions.Length; num28++) {
                        playerLightInfo.Positions[num28] = position;
                        playerLightInfo.Rotations[num28] = rotation;
                    }
                }
            }
        }
        void processSegments() {
            int length = _wormData.Length;
            for (int i = length - 1; i >= 0; i--) {
                ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                Vector3 lightColor = new Color(127, 153, 22).ToVector3() * 0.75f * wormSegmentInfo.Opacity * (1f - wormSegmentInfo.DestroyProgress);
                if (wormSegmentInfo.Body) {
                    for (int k = 0; k < wormSegmentInfo.TentacleData.Length; k++) {
                        TentacleInfo tentacleInfo = wormSegmentInfo.TentacleData[k];
                        Vector2 to = wormSegmentInfo.Position + Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation + tentacleInfo.Angle) * tentacleInfo.Length * 2f * tentacleInfo.Progress;
                        DelegateMethods.v3_1 = lightColor * tentacleInfo.Progress;
                        Vector2 start = wormSegmentInfo.Position;
                        Utils.PlotTileLine(start, to, 20f, DelegateMethods.CastLight);
                    }
                }
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Min(length - 1, i + 1);
                ref WormSegmentInfo currentSegmentData = ref _wormData[currentSegmentIndex],
                                    previousSegmentData = ref _wormData[previousSegmentIndex];
                if (wormSegmentInfo.Broken) {
                    for (int k = 0; k < currentSegmentData.TentacleData.Length; k++) {
                        float to = 1f;
                        bool down = false;
                        if (wormSegmentInfo.BrokenProgress >= 1.8f) {
                            to = 0f;
                            down = true;
                        }
                        currentSegmentData.TentacleData[k].Progress = Helper.Approach(currentSegmentData.TentacleData[k].Progress, to, down ? 0.2f : currentSegmentData.TentacleData[k].Progress < 0.4f ? 0.1f : 0.05f);
                    }
                }
                Lighting.AddLight(wormSegmentInfo.Position, lightColor * 1.5f);
                if (i != length - 1 && previousSegmentData.Opacity < 0.2f) {
                    continue;
                }
                currentSegmentData.Opacity = Helper.Approach(currentSegmentData.Opacity, 1f, 0.05f);
            }
            float allDeathProgress = 0f;
            for (int i = 0; i < length; i++) {
                ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                allDeathProgress += wormSegmentInfo.DestroyProgress;

                if (wormSegmentInfo.Opacity >= 0.5f) {
                    float angle = 0f;
                    if (i == 0) {
                        angle = MathHelper.Pi;
                    }
                    if (!wormSegmentInfo.Body && wormSegmentInfo.DestroyProgress <= 0f) {
                        if (Main.rand.NextBool(15)) {
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.625f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f)
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation - MathHelper.PiOver2 + MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.25f) * 85f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(Main.dust[num492].position.AngleTo(wormSegmentInfo.Position) - MathHelper.PiOver2) * Main.rand.NextFloat(2.5f, 5f);
                            }
                        }
                        if (Main.rand.NextBool(30)) {
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.5f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f)
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation - MathHelper.PiOver2) * 30f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation - MathHelper.PiOver2) * Main.rand.NextFloat(2.5f, 5f);
                            }
                        }
                    }
                    if (Main.rand.NextBool(50)) {
                        Dust dust = Dust.NewDustPerfect(wormSegmentInfo.Position + Main.rand.RandomPointInArea(40f), ModContent.DustType<FilamentDust>());
                        dust.scale = 2.7f * 0.625f;
                        dust.noGravity = true;
                        dust.fadeIn = 0.5f;
                        dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
                    }
                    if (wormSegmentInfo.Body) {
                        if (Main.rand.NextBool(75)) {
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.5f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(4f)
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation + MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.25f) * 60f
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation + MathHelper.PiOver2 - MathHelper.Pi + MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.25f) * 40f * Main.rand.NextFloatDirection();
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= Main.rand.NextFloat(0f, 0.5f);
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation - MathHelper.PiOver2) * Main.rand.NextFloat(2.5f, 5f) * 0.5f;
                            }
                        }
                        if (Main.rand.NextBool(75)) {
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.5f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(4f)
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation - MathHelper.Pi + MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.25f) * 60f
                                    + Vector2.UnitY.RotatedBy(angle + wormSegmentInfo.Rotation - MathHelper.PiOver2 - MathHelper.Pi + MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.25f) * 40f * Main.rand.NextFloatDirection();
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= Main.rand.NextFloat(0f, 0.5f);
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation + MathHelper.PiOver2) * Main.rand.NextFloat(2.5f, 5f) * 0.5f;
                            }
                        }
                    }
                }
            }
            allDeathProgress /= length;
            if (allDeathProgress >= 1f) {
                Projectile.Kill();
            }
            if (!Used) {
                return;
            }
            for (int i = 0; i < length; i++) {
                ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                if (wormSegmentInfo.Broken) {
                    float to = 2f;
                    wormSegmentInfo.BrokenProgress = Helper.Approach(wormSegmentInfo.BrokenProgress, to, 0.015f);
                    if (((wormSegmentInfo.BrokenProgress > 0.05f && wormSegmentInfo.BrokenProgress < 0.25f) || wormSegmentInfo.BrokenProgress >= 1.8f) && wormSegmentInfo.BrokenProgress < to && !wormSegmentInfo.ShouldShake && wormSegmentInfo.ShakeCooldown <= 0f) {
                        if (!wormSegmentInfo.ShouldShake && wormSegmentInfo.Body && wormSegmentInfo.BrokenProgress < 0.1f) {
                            for (int num491 = 0; num491 < 7; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f)
                                    + Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation) * 35f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation) * Main.rand.NextFloat(2.5f, 5f);
                            }
                            for (int num491 = 0; num491 < 7; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f) +
                                    Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation - MathHelper.Pi) * 35f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation - MathHelper.Pi) * Main.rand.NextFloat(2.5f, 5f);
                            }
                        }
                        wormSegmentInfo.ShouldShake = true;
                        wormSegmentInfo.ShakeCooldown = 4f;
                        wormSegmentInfo.ShakeCooldown2 = wormSegmentInfo.ShakeCooldown;
                    }
                    if (wormSegmentInfo.Body && wormSegmentInfo.BrokenProgress >= 0.25f && wormSegmentInfo.DestroyProgress <= 0f) {
                        if (Main.rand.NextChance(Ease.CubeOut((wormSegmentInfo.BrokenProgress - 0.25f) * 0.0325f))) {
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.75f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f)
                                    + Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation) * 35f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation) * Main.rand.NextFloat(2.5f, 5f);
                            }
                            for (int num491 = 0; num491 < 1; num491++) {
                                int num492 = Dust.NewDust(wormSegmentInfo.Position, 6, 6, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 0.75f);
                                Main.dust[num492].position = wormSegmentInfo.Position + Main.rand.RandomPointInArea(14f) +
                                    Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation - MathHelper.Pi) * 35f;
                                Main.dust[num492].noGravity = true;
                                Dust dust2 = Main.dust[num492];
                                dust2.velocity *= 1.2f;
                                dust2 = Main.dust[num492];
                                dust2.velocity += Vector2.UnitY.RotatedBy(wormSegmentInfo.Rotation - MathHelper.Pi) * Main.rand.NextFloat(2.5f, 5f);
                            }
                        }
                    }
                    wormSegmentInfo.ShakeCooldown = Helper.Approach(wormSegmentInfo.ShakeCooldown, 0f, 1f);
                    if (wormSegmentInfo.ShakeCooldown <= 0f) {
                        wormSegmentInfo.ShouldShake = false;
                    }
                    if (wormSegmentInfo.BrokenProgress >= to && wormSegmentInfo.ShakeCooldown <= 0f) {
                        if (!wormSegmentInfo.Destroyed) {
                            int count = 10;
                            for (int k = 0; k < count; k++) {
                                if (Main.rand.NextBool(3)) {
                                    continue;
                                }
                                Dust dust = Dust.NewDustPerfect(wormSegmentInfo.Position + Main.rand.RandomPointInArea(40f), ModContent.DustType<FilamentDust>());
                                dust.scale = 2.7f * 0.625f;
                                dust.noGravity = true;
                                dust.fadeIn = 0.5f;
                                dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
                            }

                            int count2 = 4;
                            if (!Main.dedServ) {
                                for (int k = 0; k < count2; k++) {
                                    int gore = Gore.NewGore(Projectile.GetSource_Death(),
                                                            wormSegmentInfo.Position +
                                                            Main.rand.RandomPointInArea(35f),
                                                            Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + $"/StarwayWormholeGore{Main.rand.Next(4) + 1}").Type, 1f);
                                    Main.gore[gore].velocity.Y *= 0.5f;
                                    Main.gore[gore].velocity.Y = MathF.Abs(Main.gore[gore].velocity.Y);
                                    Main.gore[gore].position -= new Vector2(Main.gore[gore].Width, Main.gore[gore].Height) / 2f;
                                    Main.gore[gore].rotation = MathHelper.TwoPi * Main.rand.NextFloat();
                                }
                            }
                        }
                        wormSegmentInfo.DestroyProgress = Helper.Approach(wormSegmentInfo.DestroyProgress, 1f, 0.275f);
                        wormSegmentInfo.Destroyed = true;
                    }
                }
            }
            bool hasActivePlayer = false;
            foreach (Player player in Main.ActivePlayers) {
                if (player.GetCommon().StarwayWormholeICollidedWith == Projectile && (player.GetCommon().WormholeAdventureProgress <= 1.1f || player.GetCommon().CollidedWithStarwayWormhole)) {
                    for (int i = 0; i < length; i++) {
                        ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                        if (wormSegmentInfo.Broken) {
                            continue;
                        }
                        if (player.Distance(wormSegmentInfo.Position) < 40f) {
                            wormSegmentInfo.Broken = true;
                            wormSegmentInfo.ShouldShake = true;
                            wormSegmentInfo.ShakeCooldown = 4f;
                            wormSegmentInfo.ShakeCooldown2 = wormSegmentInfo.ShakeCooldown;
                        }
                    }
                    hasActivePlayer = true;
                    _lightPlayer = player;
                    if (!_drawLights) {
                        initLights(true);
                    }
                    break;
                }
            }
            if (_drawLights && !hasActivePlayer) {
                for (int i = 0; i < 15; i++) {
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    int size = 15;
                    int num492 = Dust.NewDust(_lightPlayer.position - Vector2.One * size / 2f, _lightPlayer.width + size, _lightPlayer.height + size, ModContent.DustType<FilamentDust>(), 0f, 0f, 0, default(Color), 2.7f * 1f);
                    Main.dust[num492].noGravity = true;
                    Main.dust[num492].velocity += _lightPlayer.velocity * 0.25f * Main.rand.NextFloat(1f, 2f);
                }
            }
            _drawLights = hasActivePlayer;
        }
        void processLights() {
            if (_drawLights) {
                Lighting.AddLight(_lightPlayer.GetPlayerCorePoint(), new Color(255, 217, 37).ToVector3() * GetLightOpacity());
                initLights();
            }
        }

        init();
        processSegments();
        processLights();
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        if (!Init) {
            return false;
        }

        SpriteBatch batch = Main.spriteBatch;

        DrawWorm(batch, true);

        if (!(!Used || !_drawLights)) {
            var graphicsDevice = Main.instance.GraphicsDevice;
            var sb = batch;

            sb.End();

            SpriteBatchSnapshot snapshot = sb.CaptureSnapshot();

            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            sb.End();

            graphicsDevice.SetRenderTarget(TargetLoader.LowResTarget);
            graphicsDevice.Clear(Color.Transparent);

            float scaleX = (float)TargetLoader.LowResTarget.Width / Main.screenWidth;
            float scaleY = (float)TargetLoader.LowResTarget.Height / Main.screenHeight;
            Matrix scaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1f);
            sb.Begin(SpriteSortMode.Deferred,
                     BlendState.AlphaBlend,
                     SamplerState.PointClamp,
                     null, null, null,
                     scaleMatrix);

            DrawLights(batch);

            sb.End();

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            sb.End();

            sb.Begin(snapshot with { samplerState = SamplerState.PointClamp });
            sb.Draw(TargetLoader.LowResTarget,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.White);
            sb.End();

            sb.Begin(snapshot);
        }

        DrawWorm(batch, false);

        return false;
    }

    private void DrawWorm(SpriteBatch batch, bool drawBehind) {
        Texture2D texture = Projectile.GetTexture();
        Texture2D behindTexture = _behindTexture.Value;

        int length = _wormData.Length;
        bool first = true;
        for (int i = 0; i < length; i++) {
            WormSegmentInfo wormSegmentInfo = _wormData[i];
            int frameX = (!wormSegmentInfo.Body).ToInt(),
                frameY = wormSegmentInfo.Frame;
            if (wormSegmentInfo.BrokenProgress > 0.05f) {
                frameY++;
            }
            if (wormSegmentInfo.BrokenProgress > 1.8f) {
                frameY++;
            }
            float shakeProgress = wormSegmentInfo.ShakeCooldown / wormSegmentInfo.ShakeCooldown2;
            if (float.IsNaN(shakeProgress)) {
                shakeProgress = 0f;
            }
            float shakeIncrease = 0f;
            float disappearOpacity = Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);
            float opacity = MathF.Min(wormSegmentInfo.Opacity, disappearOpacity);
            Color baseColor = Color.White;
            baseColor = baseColor.MultiplyAlpha(1f - Utils.GetLerpValue(1f, 0.25f, opacity, true));
            float mainOpacity = Utils.GetLerpValue(0f, 0.5f, opacity, true) * (1f - wormSegmentInfo.DestroyProgress);
            mainOpacity *= Ease.CubeIn(disappearOpacity);
            baseColor *= mainOpacity;
            Color color = baseColor * Helper.Wave(0.5f + shakeIncrease * shakeProgress, 0.75f + shakeIncrease * shakeProgress, 5f, Projectile.identity);
            Rectangle clip = Utils.Frame(texture, 3, 3, frameX: frameX, frameY: frameY);
            Vector2 origin = clip.Centered();
            int mouthFrame = (int)((TimeSystem.TimeForVisualEffects * 10 + i + Projectile.identity) % 3);
            Rectangle clip2 = Utils.Frame(texture, 3, 3, frameX: 2, frameY: mouthFrame);
            float rotation = wormSegmentInfo.Rotation;
            SpriteEffects flip = (-Projectile.spriteDirection).ToSpriteEffects2();
            if (first) {
                flip |= SpriteEffects.FlipHorizontally;
            }
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                ImageFlip = flip,
                Color = color
            };
            Vector2 position = wormSegmentInfo.Position;
            Vector2 shakePosition = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f;
            if (wormSegmentInfo.ShouldShake) {
                shakePosition *= shakeProgress;
                position += shakePosition;
            }

            void drawSelf(Texture2D texture, bool glow = true) {
                batch.Draw(texture, position, drawInfo);
                if (!wormSegmentInfo.Body) {
                    batch.Draw(texture, position, drawInfo with {
                        Clip = clip2
                    });
                }
                if (glow) {
                    float num184 = Helper.Wave(2f, 6f, 1f, Projectile.identity);
                    for (int num185 = 0; num185 < 4; num185++) {
                        batch.Draw(texture, position + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                            Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * mainOpacity
                        });
                        if (!wormSegmentInfo.Body) {
                            batch.Draw(texture, position + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                                Clip = clip2,
                                Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * mainOpacity
                            });
                        }
                    }
                }
            }

            if (drawBehind) {
                drawSelf(behindTexture);

                color *= 1f;

                if (wormSegmentInfo.ShouldShake) {
                    position -= shakePosition;
                }

                Texture2D tentacleTexture = _tentacleTexture.Value;
                if (wormSegmentInfo.Broken && wormSegmentInfo.Body) {
                    int tentacleLength = wormSegmentInfo.TentacleData.Length;
                    int halfTentacleLength = tentacleLength / 2;
                    for (int k = 0; k < tentacleLength; k++) {
                        TentacleInfo tentacleInfo = wormSegmentInfo.TentacleData[k];
                        float progress = Ease.CubeInOut(MathUtils.Clamp01(tentacleInfo.Progress));
                        ShaderLoader.WormholeTentacleShader.WaveTime = TimeSystem.TimeForVisualEffects * 5f + k * 2 + i * 2;
                        ShaderLoader.WormholeTentacleShader.WaveAmplitude = 0.75f * progress * mainOpacity;
                        ShaderLoader.WormholeTentacleShader.WaveFrequency = 2.5f * progress * mainOpacity;
                        ShaderLoader.WormholeTentacleShader.WaveSpeed = 1f;

                        ShaderLoader.WormholeTentacleShader.BendDirection = 0f;
                        ShaderLoader.WormholeTentacleShader.BendStrength = 1f;
                        ShaderLoader.WormholeTentacleShader.BaseStability = 0f;
                        ShaderLoader.WormholeTentacleShader.TipWiggle = 0f;

                        ShaderLoader.WormholeTentacleShader.Apply(batch, () => {
                            float startOffset = 37.5f * -2f;
                            Vector2 from = Vector2.UnitY.RotatedBy(rotation + tentacleInfo.Angle) * (startOffset + tentacleInfo.Length * 0.55f);
                            Vector2 to = Vector2.UnitY.RotatedBy(rotation + tentacleInfo.Angle) * (startOffset + tentacleInfo.Length + tentacleInfo.Length * 0.5f);
                            Vector2 start = position + from;
                            Vector2 end = position + to;
                            start += start.DirectionTo(position) * 20f;
                            Vector2 tentacleScale = new Vector2(1f, Vector2.Distance(start, end) * 0.1f) * progress;
                            batch.Draw(tentacleTexture, start - Main.screenPosition, null,
                                color, Utils.AngleTo(start, end) - MathHelper.PiOver2,
                                tentacleTexture.Bounds.BottomCenter(), tentacleScale, SpriteEffects.None, 0f);
                            float num184 = Helper.Wave(2f, 6f, 1f, Projectile.identity);
                            for (int num185 = 0; num185 < 4; num185++) {
                                batch.Draw(tentacleTexture, start + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184 - Main.screenPosition, null,
                                   new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * mainOpacity, Utils.AngleTo(start, end) - MathHelper.PiOver2,
                                   tentacleTexture.Bounds.BottomCenter(), tentacleScale, SpriteEffects.None, 0f);
                            }
                        });
                    }
                }
            }
            else {
                drawSelf(texture);
            }

            first = false;
        }
    }

    private float GetLightOpacity() {
        float progress2 = Utils.GetLerpValue(0f, 0.1f, _lightPlayer.GetCommon().WormholeAdventureProgress, true);
        progress2 = Ease.QuartOut(progress2);
        float progress3 = Ease.QuintIn(Utils.GetLerpValue(1f, 1.1f, _lightPlayer.GetCommon().WormholeAdventureProgress, true));
        progress2 -= progress3;
        if (_lightPlayer.GetCommon().WormholeAdventureReversed2) {
            progress2 = 1f;
        }
        return progress2;
    }

    private void DrawLights(SpriteBatch batch) {
        float progress2 = GetLightOpacity();
        //ShaderLoader.PixellateShader.BufferSize = new Vector2(320, 180);
        //ShaderLoader.PixellateShader.ScreenSize = new Vector2(640, 320);
        //ShaderLoader.PixellateShader.PixelDensity = 16f;
        //ShaderLoader.PixellateShader.Apply(batch, () => {
        for (int i = 0; i < _playerLightData.Length; i++) {
            PlayerLightInfo playerLightInfo = _playerLightData[i];
            int length2 = playerLightInfo.Positions.Length;
            Color color = new Color(255, 217, 37).ModifyRGB(1f);
            float lerpValue = 0.25f;
            float scale = 1f;
            float scaleLerp = 0.75f;
            for (int i2 = length2 - 1; i2 > 0; i2--) {
                Vector2 position = playerLightInfo.Positions[i2];
                Texture2D texture2 = ResourceManager.Circle2;

                float num3 = 0f;
                //float y = 0f;
                Vector2 vector6 = playerLightInfo.Positions[i2];
                Vector2 vector7 = playerLightInfo.Positions[i2 - 1];
                if (vector6 == Vector2.Zero || vector7 == Vector2.Zero) {
                    continue;
                }
                vector7.Y -= num3 / 2f;
                vector6.Y -= num3 / 2f;
                int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
                if (Vector2.Distance(vector6, vector7) % 3f != 0f)
                    num5++;

                if (i2 < length2 * 0.25f) {
                    color = Color.Lerp(color, new Color(255, 238, 166), 0.75f);
                }

                for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
                    batch.Draw(texture2, Vector2.Lerp(vector7, vector6, num6 / (float)num5) - Main.screenPosition, texture2.Bounds,
                        color.MultiplyAlpha(1f) * 1f * progress2, 0f, texture2.Bounds.Centered(), scale * 0.01f * 0.75f * Helper.Wave(1f, 1.1f, 5f, i2 + Projectile.identity + 3f), 0, 0);
                }

                scale += scaleLerp;
                scaleLerp *= 0.975f;
                lerpValue = Helper.Approach(lerpValue, 0.15f, 0.025f);
            }
        }
        //});
        //ShaderLoader.PixellateShader.Apply(batch, () => {
        for (int i = 0; i < _playerLightData.Length; i++) {
            PlayerLightInfo playerLightInfo = _playerLightData[i];
            Vector2 position = playerLightInfo.Positions[0];
            Texture2D texture2 = ResourceManager.Circle2;
            float rotation = playerLightInfo.Positions[1].AngleTo(playerLightInfo.Positions[0]);

            batch.Draw(texture2, position - Main.screenPosition, texture2.Bounds,
                new Color(255, 238, 166).MultiplyAlpha(1f) * 1f * progress2, rotation, texture2.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.185f * 0.25f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 2f), 0, 0);
            batch.Draw(texture2, position - Main.screenPosition, texture2.Bounds,
                Color.White.MultiplyAlpha(1f) * 1f * progress2, rotation, texture2.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.155f * 0.125f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 1f), 0, 0);

            Texture2D texture3 = ResourceManager.Bloom2;
            batch.Draw(texture3, position - Main.screenPosition, texture3.Bounds,
                new Color(255, 217, 37).MultiplyAlpha(0f) * 0.5f * progress2, rotation, texture3.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.185f * 1f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 2f), 0, 0);
        }
        //});
    }
}
