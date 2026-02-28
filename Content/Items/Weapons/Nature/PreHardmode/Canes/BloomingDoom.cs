using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class BloomingDoom : TulipBaseItem<BloomingDoom.BloomingDoomBase> {
    protected override void SafeSetDefaults() {
        Item.SetWeaponValues(12, 1.5f);
        Item.SetUsableValues(-1, 60, useSound: SoundID.Item7);
        Item.SetSizeValues(36, 38);

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 2, 0, 0);

        NatureWeaponHandler.SetPotentialDamage(Item, 20);

        base.SafeSetDefaults();
    }

    public sealed class BloomingDoomBase : TulipBase {
        protected override Vector2 CorePositionOffsetFactor() => new(0.145f, 0.135f);

        protected override ushort CoreDustType() => (ushort)ModContent.DustType<BloomingDoomDust>();

        protected override byte DustFrameXUsed() => 3;
    }
}

abstract class TulipBaseItem<T> : CaneBaseItem<T> where T : CaneBaseProjectile {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<TulipFlower>();

    protected override void SafeSetDefaults() {
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.12f);
    }
}

abstract class TulipBase : CaneBaseProjectile {
    private readonly WeightedRandom<int> _random = new();

    private Vector2 _mousePosition = Vector2.Zero;

    private bool IsWeepingTulip => DustFrameXUsed() == 2;

    public sealed class TulipBaseExtraData : ModPlayer {
        public Vector2 TempMousePosition { get; internal set; } = Vector2.Zero;

        public Vector2 SpawnPositionRandomlySelected => GetTilePosition(Player, TempMousePosition).ToWorldCoordinates();
        public Vector2 SpawnPositionMid => GetTilePosition(Player, TempMousePosition, false).ToWorldCoordinates();
    }

    private TulipBaseExtraData TulipBaseData => Main.player[Projectile.owner].GetModPlayer<TulipBaseExtraData>();

    protected abstract ushort CoreDustType();

    protected abstract byte DustFrameXUsed();

    protected override Vector2 CorePositionOffsetFactor() => new(0.08f, 0.125f);

    //protected override bool ShouldStopUpdatingRotationAndDirection() => false;

    protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
        spawnPosition = Main.MouseWorld;
        ai0 = DustFrameXUsed();
    }

    protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
        float offset = 10f;
        Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition - Vector2.One * 1f + randomOffset;
        ushort dustType = CoreDustType();

        bool flag = false/*Main.rand.NextBool()*/;
        if (flag) {
            Point tileCoords = spawnPosition.ToTileCoordinates();
            dustType = (ushort)TileHelper.GetKillTileDust(tileCoords.X, tileCoords.Y, WorldGenHelper.GetTileSafely(tileCoords.X, tileCoords.Y));
        }

        float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 1.25f * Ease.ExpoInOut(Math.Max(step, 0.25f)) + 0.25f;
        Dust dust = Dust.NewDustPerfect(spawnPosition, dustType,
            Scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f));
        dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
        dust.velocity *= 0.9f;
        // hardcoded for now
        //_random.Clear();
        //_random.needsRefresh = true;
        //_random.Add(0);
        //_random.Add(1, 0.25);
        //_random.Add(2, 0.25);
        dust.customData = player;
        dust.noGravity = true;

        if (flag) {
            dust.scale *= 0.65f;
        }

        if (player.whoAmI == Main.myPlayer) {
            if (step <= 0.5f) {
                TulipBaseData.TempMousePosition = GetCappedMousePosition(player);
            }
            Projectile.netUpdate = true;
        }

        SpawnGroundDusts(player, dustType, TulipBaseData, velocityFactor);
    }

    private Vector2 GetCappedMousePosition(Player player) => IsWeepingTulip ? player.GetViableMousePosition(480f, 300f) : player.GetViableMousePosition(240f, 150f);

    protected override void SpawnCoreDustsWhileShotProjectileIsActive(float step, Player player, Vector2 corePosition) {
        SpawnGroundDusts(player, CoreDustType(), TulipBaseData, 1f);
    }

    public override void SafePostAI() {
        if (Projectile.owner == Main.myPlayer) {
            Vector2 to = GetCappedMousePosition(Main.player[Projectile.owner]);
            if (_mousePosition == Vector2.Zero) {
                _mousePosition = to;
            }
            _mousePosition = Vector2.SmoothStep(_mousePosition, to, 0.1f);
            Projectile.netUpdate = true;
        }
    }

    protected override void SpawnDustsWhenReady(Player player, Vector2 corePosition) {
        for (int i = 0; i < 10; i++) {
            float offset = 10f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset),
                    spawnPosition = corePosition + randomOffset - Vector2.One * 1f;

            //ushort dustType = CoreDustType();
            byte frameX = DustFrameXUsed();
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - corePosition).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
                                            Alpha: frameX);
            dust.customData = Main.rand.NextFloatRange(50f);
        }
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteVector2(TulipBaseData.TempMousePosition);
        writer.WriteVector2(_mousePosition);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        TulipBaseData.TempMousePosition = reader.ReadVector2();
        _mousePosition = reader.ReadVector2();
    }

    public void SpawnGroundDusts(Player player, ushort dustType, TulipBaseExtraData tulipBaseExtraData, float velocityFactor) {
        Vector2 position = tulipBaseExtraData.SpawnPositionRandomlySelected,
                velocity = (_mousePosition + Main.rand.NextVector2Circular(8f, 8f) - position).SafeNormalize(Vector2.Zero) * 3f * velocityFactor;
        Vector2 dustPos = position - Vector2.UnitY * 8f + Main.rand.NextVector2Circular(8f, 8f);
        //int x = (int)dustPos.X / 16, y = (int)dustPos.Y / 16;

        bool flag = Main.rand.NextBool();
        if (flag) {
            Point tileCoords = position.ToTileCoordinates();
            dustType = (ushort)TileHelper.GetKillTileDust(tileCoords.X, tileCoords.Y, WorldGenHelper.GetTileSafely(tileCoords.X, tileCoords.Y));
        }

        Dust dust = Dust.NewDustPerfect(dustPos,
                                        dustType,
                                        Scale: Main.rand.NextFloat(1.5f, 2f));
        dust.velocity = velocity;
        // hardcoded for now
        dust.customData = 0;
        //dust.scale *= 0.75f;
        dust.velocity *= 0.9f;
        dust.noGravity = true;

        if (flag) {
            dust.scale *= 0.65f;
        }
    }

    // adapted vanilla
    public static Point GetTilePosition(Player player, Vector2 targetSpot, bool randomlySelected = true) {
        Point point;
        Vector2 center = player.GetPlayerCorePoint();
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
        while (!WorldGen.SolidTile3(point.X, point.Y)) {
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
                if (!WorldGen.SolidTile3(j, k)) {
                    continue;
                }
                list.Add(new Point(j, k));
            }
        }
        int index = Main.rand.Next(list.Count);
        return randomlySelected ? list[index] : list[list.Count / 2];
    }
}

//sealed class SweetTulip : TulipBaseItem<SweetTulip.SweepTulipBase> {
//    protected override void SafeSetDefaults() {
//        Item.SetWeaponValues(6, 1.5f);
//        Item.SetUsableValues(-1, 60, useSound: SoundID.Item65);

//        NatureWeaponHandler.SetPotentialDamage(Item, 20);

//        base.SafeSetDefaults();
//    }

//    public sealed class SweepTulipBase : TulipBase {
//        protected override ushort CoreDustType() => (ushort)DustID.YellowTorch;

//        protected override byte DustFrameXUsed() => 1;
//    }
//}

//sealed class WeepingTulip : TulipBaseItem<WeepingTulip.WeepingTulipBase> {
//    protected override void SafeSetDefaults() {
//        Item.SetWeaponValues(6, 1.5f);
//        Item.SetUsableValues(-1, 45, useSound: SoundID.Item65);

//        NatureWeaponHandler.SetPotentialDamage(Item, 20);

//        base.SafeSetDefaults();
//    }

//    public sealed class WeepingTulipBase : TulipBase {
//        protected override ushort CoreDustType() => (ushort)DustID.BlueTorch;

//        protected override byte DustFrameXUsed() => 2;
//    }
//}