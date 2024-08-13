using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Staffs;

sealed class ExoticTulip : BaseRodItem<ExoticTulip.ExoticTulipBase> {
    protected override void SafeSetDefaults() {
        Item.SetSize(34);
        Item.SetDefaultToUsable(-1, 40, useSound: SoundID.Item65);
        Item.SetWeaponValues(6, 1.5f);

        NatureWeaponHandler.SetPotentialDamage(Item, 20);
        NatureWeaponHandler.SetFillingRate(Item, 0.75f);
    }

    public sealed class ExoticTulipBase : TulipBase {
        protected override ushort CoreDustType() => (ushort)DustID.PurpleTorch;
    }
}

sealed class SweetTulip : BaseRodItem<SweetTulip.SweepTulipBase> {
    protected override void SafeSetDefaults() {
        Item.SetSize(32, 34);
        Item.SetDefaultToUsable(-1, 40, useSound: SoundID.Item65);
        Item.SetWeaponValues(6, 1.5f);

        NatureWeaponHandler.SetPotentialDamage(Item, 20);
        NatureWeaponHandler.SetFillingRate(Item, 0.75f);
    }

    public sealed class SweepTulipBase : TulipBase {
        protected override ushort CoreDustType() => (ushort)DustID.YellowTorch;
    }
}

sealed class WeepingTulip : BaseRodItem<WeepingTulip.WeepingTulipBase> {
    protected override void SafeSetDefaults() {
        Item.SetSize(34);
        Item.SetDefaultToUsable(-1, 40, useSound: SoundID.Item65);
        Item.SetWeaponValues(6, 1.5f);

        NatureWeaponHandler.SetPotentialDamage(Item, 20);
        NatureWeaponHandler.SetFillingRate(Item, 0.75f);
    }

    public sealed class WeepingTulipBase : TulipBase {
        protected override ushort CoreDustType() => (ushort)DustID.BlueTorch;
    }
}

abstract class TulipBase : BaseRodProjectile {
    private Vector2 TempMousePosition { get; set; }

    protected abstract ushort CoreDustType();

    protected override Vector2 CorePositionOffsetFactor() => new(0.08f, 0.13f);

    protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count) { }

    protected override void SpawnCoreDusts(float step, Player player, Vector2 corePosition) {
        float offset = 10f;
        Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition + randomOffset;
        ushort dustType = CoreDustType();
        float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 1.25f * Ease.ExpoInOut(Math.Max(step, 0.25f)) + 0.25f;
        Dust dust = Dust.NewDustPerfect(spawnPosition, dustType, Scale: MathHelper.Clamp(velocityFactor * 1.25f, 1f, 1.75f));
        dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
        dust.velocity *= 0.9f;
        dust.noGravity = true;

        if (player.whoAmI == Main.myPlayer) {
            if (step <= 0.5f) {
                TempMousePosition = player.GetViableMousePosition();
            }
            Projectile.netUpdate = true;
        }

        Vector2 pointPosition = TempMousePosition;
        Vector2 position = GetTilePosition(player, pointPosition).ToWorldCoordinates(), velocity = (pointPosition + Main.rand.NextVector2Circular(8f, 8f) - position).SafeNormalize(-Vector2.UnitY) * 3f * velocityFactor;
        dust = Dust.NewDustPerfect(position - Vector2.UnitY * 8f + Main.rand.NextVector2Circular(8f, 8f),
                                    dustType,
                                    Scale: Main.rand.NextFloat(1.5f, 2f));
        dust.velocity = velocity;
        dust.velocity *= 0.9f;
        dust.noGravity = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.WriteVector2(TempMousePosition);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        TempMousePosition = reader.ReadVector2();
    }

    // adapted vanilla
    private static Point GetTilePosition(Player player, Vector2 targetSpot) {
        Point point;
        Vector2 center = player.Center;
        Vector2 endPoint = targetSpot;
        int samplesToTake = 3;
        float samplingWidth = 4f;
        Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out Vector2 vectorTowardsTarget, out float[] samples);
        float num = float.PositiveInfinity;
        for (int i = 0; i < samples.Length; i++) {
            if (samples[i] < num) {
                num = samples[i];
            }
        }

        targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
        point = targetSpot.ToTileCoordinates();
        while (!WorldGen.SolidTile2(point.X, point.Y)) {
            point.Y++;
        }
        point.Y -= 1;
        Rectangle value = new Rectangle(point.X, point.Y, 1, 1);
        value.Inflate(1, 1);
        Rectangle value2 = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
        value2.Inflate(-40, -40);
        value = Rectangle.Intersect(value, value2);
        List<Point> list = new List<Point>();
        for (int j = value.Left; j <= value.Right; j++) {
            for (int k = value.Top; k <= value.Bottom; k++) {
                if (!WorldGen.SolidTile2(j, k)) {
                    continue;
                }
                list.Add(new Point(j, k));
            }
        }
        int index = Main.rand.Next(list.Count);
        return list[index];
    }
}