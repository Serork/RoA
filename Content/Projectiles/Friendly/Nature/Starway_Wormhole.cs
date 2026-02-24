using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class StarwayWormhole : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    private static float STEP_BASEDONTEXTUREWIDTH => 90f * 0.85f;

    private readonly record struct WormSegmentInfo(Vector2 Position, byte Frame, float Rotation, bool Body = true);

    private WormSegmentInfo[] _wormData = null!;
    private GeometryUtils.BezierCurve _bezierCurve = null!;

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

                initSegments();
            }
        }
        void playerEnter() {

        }

        init();
        playerEnter();
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        if (!Init) {
            return false;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();

        Color color = Color.White * Helper.Wave(0.5f, 0.75f, 5f, Projectile.identity);

        int length = _wormData.Length;
        bool first = true;
        for (int i = 0; i < length; i++) {
            WormSegmentInfo wormSegmentInfo = _wormData[i];
            int frameX = (!wormSegmentInfo.Body).ToInt(),
                frameY = wormSegmentInfo.Frame;
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
            batch.Draw(texture, position, drawInfo);
            float num184 = Helper.Wave(2f, 6f, 1f, Projectile.identity);
            for (int num185 = 0; num185 < 4; num185++) {
                batch.Draw(texture, position + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                    Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f
                });
            }
            first = false;
        }

        return false;
    }
}
