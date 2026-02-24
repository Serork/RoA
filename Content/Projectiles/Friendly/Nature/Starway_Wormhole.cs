using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Cache;
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
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    private static float STEP_BASEDONTEXTUREWIDTH => 90f * 0.85f;

    private record struct WormSegmentInfo(Vector2 Position, byte Frame, float Rotation, bool Body = true, bool Broken = false, float BrokenProgress = 0f, bool ShouldShake = false, float ShakeCooldown = 0f, float ShakeCooldown2 = 0f);

    private WormSegmentInfo[] _wormData = null!;
    private GeometryUtils.BezierCurve _bezierCurve = null!;
    private bool _drawLights;
    private Player _lightPlayer = null!;

    private static RenderTarget2D lowResTarget = null!;

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

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public bool Used {
        get => UsedValue != 0f;
        set => UsedValue = value.ToInt();
    }

    public override void SetStaticDefaults() {

    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (Used) {
            overPlayers.Add(index);
        }
    }

    public override void AI() {
        Projectile.hide = Used;

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
                        _wormData[i] = new WormSegmentInfo(position, 0, rotation, body);
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
                Vector2 position = player.Center + angle * Helper.Wave(-y, y, 10f, i * 2f);
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
        void playerEnter() {
            if (!Used) {
                return;
            }
            int length = _wormData.Length;
            for (int i = 0; i < length; i++) {
                ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                if (wormSegmentInfo.Broken) {
                    wormSegmentInfo.BrokenProgress = Helper.Approach(wormSegmentInfo.BrokenProgress, 1f, 0.015f);
                    if ((wormSegmentInfo.BrokenProgress > 0f || wormSegmentInfo.BrokenProgress > 0.5f) && wormSegmentInfo.BrokenProgress < 0.75f && !wormSegmentInfo.ShouldShake && wormSegmentInfo.ShakeCooldown <= 0f) {
                        wormSegmentInfo.ShouldShake = true;
                        wormSegmentInfo.ShakeCooldown = 2f;
                        wormSegmentInfo.ShakeCooldown2 = wormSegmentInfo.ShakeCooldown;
                    }
                    wormSegmentInfo.ShakeCooldown = Helper.Approach(wormSegmentInfo.ShakeCooldown, 0f, 1f);
                    if (wormSegmentInfo.ShakeCooldown <= 0f) {
                        wormSegmentInfo.ShouldShake = false;
                    }
                }
            }
            bool hasActivePlayer = false;
            foreach (Player player in Main.ActivePlayers) {
                if (player.GetCommon().CollidedWithStarwayWormhole && player.GetCommon().StarwayWormholeICollidedWith == Projectile) {
                    for (int i = 0; i < length; i++) {
                        ref WormSegmentInfo wormSegmentInfo = ref _wormData[i];
                        if (wormSegmentInfo.Broken) {
                            continue;
                        }
                        if (player.Distance(wormSegmentInfo.Position) < 40f) {
                            wormSegmentInfo.Broken = true;
                            wormSegmentInfo.ShouldShake = true;
                            wormSegmentInfo.ShakeCooldown = 2f;
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
            _drawLights = hasActivePlayer;
        }
        void processLights() {
            if (_drawLights) {
                initLights();
            }
        }

        init();
        playerEnter();
        processLights();

        Projectile.timeLeft = 2;
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        if (!Init) {
            return false;
        }

        SpriteBatch batch = Main.spriteBatch;

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

        DrawWorm(batch);

        return false;
    }

    private void DrawWorm(SpriteBatch batch) {
        Texture2D texture = Projectile.GetTexture();

        int length = _wormData.Length;
        bool first = true;
        for (int i = 0; i < length; i++) {
            WormSegmentInfo wormSegmentInfo = _wormData[i];
            int frameX = (!wormSegmentInfo.Body).ToInt(),
                frameY = wormSegmentInfo.Frame;
            if (wormSegmentInfo.BrokenProgress > 0f) {
                frameY++;
            }
            if (wormSegmentInfo.BrokenProgress > 0.5f) {
                frameY++;
            }
            float shakeProgress = wormSegmentInfo.ShakeCooldown / wormSegmentInfo.ShakeCooldown2;
            if (float.IsNaN(shakeProgress)) {
                shakeProgress = 0f;
            }
            Color color = Color.White * Helper.Wave(0.5f, 0.75f, 5f, Projectile.identity);
            Rectangle clip = Utils.Frame(texture, 2, 3, frameX: frameX, frameY: frameY);
            Vector2 origin = clip.Centered();
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
            batch.Draw(texture, position, drawInfo);
            float num184 = Helper.Wave(2f, 6f, 1f, Projectile.identity);
            for (int num185 = 0; num185 < 4; num185++) {
                batch.Draw(texture, position + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                    Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f
                });
            }
            first = false;
        }
    }

    private void DrawLights(SpriteBatch batch) {
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
                        color.MultiplyAlpha(1f) * 1f, 0f, texture2.Bounds.Centered(), scale * 0.01f * 0.75f * Helper.Wave(1f, 1.1f, 5f, i2 + Projectile.identity + 3f), 0, 0);
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
                new Color(255, 238, 166).MultiplyAlpha(1f) * 1f, rotation, texture2.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.185f * 0.25f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 2f), 0, 0);
            batch.Draw(texture2, position - Main.screenPosition, texture2.Bounds,
                Color.White.MultiplyAlpha(1f) * 1f, rotation, texture2.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.155f * 0.125f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 1f), 0, 0);

            Texture2D texture3 = ResourceManager.Bloom2;
            batch.Draw(texture3, position - Main.screenPosition, texture3.Bounds,
                new Color(255, 217, 37).MultiplyAlpha(0f) * 0.5f, rotation, texture3.Bounds.Centered(), new Vector2(1f, 0.75f) * 0.185f * 1f * Helper.Wave(1f, 1.5f, 5f, Projectile.identity + 2f), 0, 0);
        }
        //});
    }
}
