using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeRootAir : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(2);
    private static ushort TIMELEFT2 => MathUtils.SecondsToFrames(2);

    private record struct RootAirInfo(Vector2 Position, float Rotation, byte TextureIndex, float Progress = 0f, ushort TimeLeft = 0) {
        public readonly bool IsDestroyed => TimeLeft >= TIMELEFT2;
    }

    private List<RootAirInfo> _rootAirData = null!,
                              _rootAirData2 = null!;
    private Vector2 _previousPosition;
    private byte _currentIndex;
    private byte _lastTextureIndex;
    private bool _shouldDisappear;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;

        Projectile.extraUpdates = 1;

        Projectile.manualDirectionChange = true;
    }

    public override void AI() {
        if (Projectile.GetOwnerAsPlayer().GetCommon().IsBrambleMazePlaced && !_shouldDisappear) {
            Projectile.extraUpdates = Projectile.numUpdates = 0;
            _shouldDisappear = true;
        }
        if (!_shouldDisappear) {
            Projectile.timeLeft++;
        }

        bool flag3 = false;
        if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
            if (Projectile.velocity.Length() < 1f && Projectile.localAI[2] >= 1f) {
                if (Projectile.ai[2] != 1f && Projectile.IsOwnerLocal()) {
                    Vector2 position = _rootAirData2.Last().Position;
                    ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeRoot>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromThis()) {
                        Position = position,
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack,
                        AI1 = Projectile.velocity.X.GetDirection()
                    });
                }

                Projectile.ai[2] = 1f;
            }

            Projectile.velocity *= 0.8f;
            flag3 = true;
        }

        Vector2 center = Projectile.Center;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.SetDirection(Projectile.GetOwnerAsPlayer().direction);

            Projectile.velocity = Projectile.velocity.SafeNormalize() * 5f;

            _rootAirData = [];
            _rootAirData2 = [];

            var root = new RootAirInfo(center, Projectile.velocity.ToRotation() - MathHelper.PiOver2, 0);
            _rootAirData.Add(root);
            _rootAirData2.Add(root);
        }

        bool flag = true;
        if (Projectile.ai[2] == 1f) {
            Projectile.velocity *= 0f;
            flag = false;
        }

        ref float checkFactor = ref Projectile.ai[0];
        if (checkFactor == 0f) {
            _previousPosition = center;
        }

        checkFactor++;
        if (Vector2.Distance(_previousPosition, center + Projectile.velocity) > 20) {
            _lastTextureIndex++;
            if (_lastTextureIndex > 2) {
                _lastTextureIndex = 1;
            }
            _rootAirData.Add(new RootAirInfo(center, Projectile.velocity.ToRotation() - MathHelper.PiOver2, _lastTextureIndex));
            Projectile.ai[1]++;
            checkFactor = 0f;

            Vector2 position = _rootAirData2[_currentIndex].Position;
            int nextIndex = _currentIndex + 1;
            Vector2 velocity = position.DirectionTo(_rootAirData[nextIndex].Position);
            Vector2 position2 = position + velocity * 20;
            _rootAirData2.Add(new RootAirInfo(position2, velocity.ToRotation() - MathHelper.PiOver2, _rootAirData[_currentIndex].TextureIndex, _rootAirData[_currentIndex].Progress));
            _currentIndex++;
        }
        if (_rootAirData2.Count > 0) {
            float allProgress = 0f;
            for (int i = 0; i < _rootAirData2.Count; i++) {
                int currentSegmentIndex = i;
                float lerpValue = 0.2f;
                RootAirInfo rootAirInfo = _rootAirData2[currentSegmentIndex];
                if (i == 0 || _rootAirData2[i - 1].Progress >= 1f) {
                    rootAirInfo.Progress = Helper.Approach(rootAirInfo.Progress, 1f, lerpValue);
                    //rootAirInfo.TimeLeft = (ushort)Helper.Approach(rootAirInfo.TimeLeft, TIMELEFT2, 1);
                }

                //if (!_shouldDisappear) {
                //    rootAirInfo.TimeLeft = (ushort)Helper.Approach(rootAirInfo.TimeLeft, 0, 1);
                //}

                _rootAirData2[currentSegmentIndex] = rootAirInfo;

                allProgress += _rootAirData2[currentSegmentIndex].Progress;
            }
            allProgress /= _rootAirData2.Count;
            Projectile.localAI[2] = allProgress;
        }

        if (!flag || flag3) {
            return;
        }
        float minX = 3f;
        if (Projectile.SpeedX() < minX) {
            Projectile.velocity.X = minX * Projectile.velocity.X.GetDirection();
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;
    }

    public override bool PreDraw(ref Color lightColor) {
        int index = 0;
        Texture2D texture = Projectile.GetTexture();
        var data = _rootAirData2;
        foreach (RootAirInfo rootAirInfo in data) {
            if (index == 0) {
                index++;
                continue;
            }
            if (rootAirInfo.IsDestroyed) {
                index++;
                continue;
            }
            byte textureIndex = rootAirInfo.TextureIndex;
            if (Projectile.ai[2] == 1f && index >= data.Count - 1) {
                textureIndex = 3;
            }
            Rectangle clip = Utils.Frame(texture, 1, Projectile.GetFrameCount(), frameY: textureIndex);
            float rotation = rootAirInfo.Rotation;
            Vector2 origin = clip.Centered();
            float opacity = MathUtils.Clamp01(rootAirInfo.Progress);
            float opacity3 = opacity + 0.075f;
            float borderColorRGBFactor = 0.5f;
            Vector2 position = rootAirInfo.Position;

            Color color = Lighting.GetColor(position.ToTileCoordinates());

            float opacity2 = Ease.QuadOut(opacity);
            opacity2 *= Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);

            SpriteEffects flip = Projectile.spriteDirection.ToSpriteEffects();

            int height = clip.Height;
            clip.Height = (int)(height * opacity3);
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color.ModifyRGB(borderColorRGBFactor) * opacity2,
                ImageFlip = flip
            };
            if (opacity != 1f) {
                Main.spriteBatch.Draw(texture, position, drawInfo);
            }

            clip.Height = (int)(height * opacity);
            drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color * opacity2,
                ImageFlip = flip
            };
            Main.spriteBatch.Draw(texture, position, drawInfo);

            index++;
        }

        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }
}
