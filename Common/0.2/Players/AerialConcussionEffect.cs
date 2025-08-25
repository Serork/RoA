using RoA.Content.Buffs;
using RoA.Content.Items.Weapons.Summon.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class AerialConcussionEffect : ModPlayer {
    private static ushort TIMEFORSTACK => 300;

    private int _timer;
    private ushort _enduranceTier;

    public bool CanGainStack => _timer >= TIMEFORSTACK;

    public bool IsEffectActive => Player.HasItem(ModContent.ItemType<AerialConcussion>());

    public override void PostUpdate() {
        if (!IsEffectActive) {
            return;
        }

        if (!CanGainStack) {
            _timer++;
        }

        if (_enduranceTier <= 0) {
            return;
        }
        switch (_enduranceTier) {
            case 1:
                Player.DelBuff<EnduranceCloud2>();
                Player.DelBuff<EnduranceCloud3>();
                Player.AddBuff<EnduranceCloud1>(5);
                break;
            case 2:
                Player.DelBuff<EnduranceCloud1>();
                Player.DelBuff<EnduranceCloud3>();
                Player.AddBuff<EnduranceCloud2>(5);
                break;
            case >= 3:
                Player.DelBuff<EnduranceCloud2>();
                Player.DelBuff<EnduranceCloud1>();
                Player.AddBuff<EnduranceCloud3>(5);
                break;
        }
    }

    public override void OnHurt(Player.HurtInfo info) {
        if (!IsEffectActive) {
            return;
        }

        Reset();
    }

    public override void OnEnterWorld() {
        if (!IsEffectActive) {
            return;
        }

        Reset();
    }

    public void ConsumeStack() {
        if (!CanGainStack) {
            return;
        }

        _enduranceTier++;

        _timer = 0;
    }

    public void Reset() {
        _enduranceTier = 0;
        _timer = 0;

        Player.DelBuff<EnduranceCloud3>();
        Player.DelBuff<EnduranceCloud2>();
        Player.DelBuff<EnduranceCloud1>();

        foreach (Projectile trackedCloud in TrackedEntitiesSystem.GetTrackedProjectile<EnduranceCloud>(checkProjectile => checkProjectile.owner != Player.whoAmI)) {
            trackedCloud.Kill();
        }
    }
}
