using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    private readonly record struct RootAirInfo(Vector2 Position, float Rotation, byte TextureIndex, float Progress = 0f);

    private List<RootAirInfo> _rootAirData = null!;
    private Vector2 _previousPosition;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.velocity = Projectile.velocity.SafeNormalize() * 5f;

            _rootAirData = [];
        }

        bool flag = true;
        if (Projectile.ai[2] == 1f) {
            Projectile.velocity *= 0f;
            flag = false;
        }

        Vector2 center = Projectile.Center;
        ref float checkFactor = ref Projectile.ai[0];
        if (checkFactor == 0f) {
            _previousPosition = center;
        }

        checkFactor++;
        if (Vector2.Distance(_previousPosition, center + Projectile.velocity) > 20) {
            byte textureIndex = 0;
            if (Projectile.ai[1] > 0f) {
                textureIndex = (byte)(1 + Main.rand.NextBool().ToInt());
            }
            _rootAirData.Add(new RootAirInfo(center, Projectile.velocity.ToRotation() - MathHelper.PiOver2, textureIndex));
            Projectile.ai[1]++;
            checkFactor = 0f;
        }

        if (!flag) {
            return;
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;
    }

    private List<RootAirInfo> GetRootAirData() {
        int index = 0;
        int count = _rootAirData.Count;
        List<RootAirInfo> rootAirData = [];
        Vector2 position = _rootAirData[0].Position;
        while (true) {
            if (index >= count) {
                break;
            }
            int nextIndex = Math.Min(index + 1, count - 1);
            Vector2 velocity = _rootAirData[index].Position.DirectionTo(_rootAirData[nextIndex].Position);
            Vector2 position2 = position + velocity * 22;
            rootAirData.Add(new RootAirInfo(position2, velocity.ToRotation() - MathHelper.PiOver2, _rootAirData[index].TextureIndex));
            position = position2;
            index++;
        }
        return rootAirData;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_rootAirData.Count < 1) {
            return false;
        }

        int index = 0;
        Texture2D texture = Projectile.GetTexture();
        var data = GetRootAirData();
        foreach (RootAirInfo rootAirInfo in data) {
            byte textureIndex = rootAirInfo.TextureIndex;
            if (index == data.Count - 2) {
                textureIndex = 3;
            }
            Rectangle clip = Utils.Frame(texture, 1, Projectile.GetFrameCount(), frameY: textureIndex);
            Vector2 origin = clip.Centered();
            float rotation = rootAirInfo.Rotation;
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation
            };
            Vector2 position = rootAirInfo.Position;
            Main.spriteBatch.Draw(texture, position, drawInfo);
            index++;
        }

        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[2] != 1f && Projectile.IsOwnerLocal()) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeRoot>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromThis()) {
                Position = Projectile.Center,
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack,
                AI1 = Projectile.velocity.X.GetDirection()
            });
        }

        Projectile.ai[2] = 1f;

        return false;
    }
}
