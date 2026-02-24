using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class StarwayWormhole : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    private static float STEP_BASEDONTEXTUREWIDTH => 90f * 0.85f;

    private readonly record struct WormSegmentInfo(Vector2 Position, byte Frame, float Rotation, bool Body = true);

    private WormSegmentInfo[] _wormData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float StartWaveValue => ref Projectile.ai[0];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public override void SetStaticDefaults() {

    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;
    }

    public override void AI() {
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
                    float spawnOffset = TileHelper.TileSize * 3;
                    //float spawnOffset2 = TileHelper.TileSize * 0;
                    //startPosition += startPosition.DirectionFrom(playerCenter) * spawnOffset2;
                    endPosition += endPosition.DirectionTo(mousePosition) * spawnOffset;
                    List<(Vector2, float)> segmentPositions = [];
                    Vector2 velocity = startPosition.DirectionTo(endPosition);
                    Vector2 startPosition2 = startPosition;
                    Vector2 velocity2 = velocity;
                    float waveWidth = 0.25f * Main.rand.NextBool().ToDirectionInt();
                    int maxCount = 11;
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
                }

                initSegments();
            }
        }

        init();
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();

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
                ImageFlip = flip
            };
            Vector2 position = wormSegmentInfo.Position;
            batch.Draw(texture, position, drawInfo);
            first = false;
        }

        return false;
    }
}
