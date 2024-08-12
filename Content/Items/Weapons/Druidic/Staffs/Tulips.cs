using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Staffs;

abstract class TulipBase : BaseRodProjectile {
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
    }
}

sealed class ExoticTulip : BaseRodItem<ExoticTulip.ExoticTulipBase> {
    protected override ushort ShootType() => 1;

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
    protected override ushort ShootType() => 1;

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
    protected override ushort ShootType() => 1;

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