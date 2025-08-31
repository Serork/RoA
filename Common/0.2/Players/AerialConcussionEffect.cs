using RoA.Content.Buffs;
using RoA.Content.Items.Weapons.Summon.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class AerialConcussionEffect : ModPlayer {
    private static ushort TIMEFORSTACK => 300;
    private static ushort INBATTLETIMER => 900;

    private ushort _timer;
    private byte _enduranceTier;
    private bool _buffApplied;

    public bool CanGainStack => _timer >= TIMEFORSTACK;

    public bool IsEffectActive => Player.HasItem(ModContent.ItemType<AerialConcussion>());

    public override void PostUpdate() {
        if (!IsEffectActive) {
            _timer = TIMEFORSTACK;

            return;
        }

        if (!CanGainStack) {
            _timer++;
        }

        if (_enduranceTier <= 0) {
            return;
        }
        if (_buffApplied) {
            return;
        }
        switch (_enduranceTier) {
            case 1:
                Player.DelBuff<EnduranceCloud2>();
                Player.DelBuff<EnduranceCloud3>();
                Player.AddBuff<EnduranceCloud1>(INBATTLETIMER);
                break;
            case 2:
                Player.DelBuff<EnduranceCloud1>();
                Player.DelBuff<EnduranceCloud3>();
                Player.AddBuff<EnduranceCloud2>(INBATTLETIMER);
                break;
            case >= 3:
                Player.DelBuff<EnduranceCloud2>();
                Player.DelBuff<EnduranceCloud1>();
                Player.AddBuff<EnduranceCloud3>(INBATTLETIMER);
                break;
        }
        _buffApplied = true;
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
        ResetBattleTimer();

        if (!CanGainStack) {
            return;
        }

        _enduranceTier++;

        _timer = 0;
    }

    public void ResetBattleTimer() => _buffApplied = false;

    public void Reset(bool onlyTier = false) {
        _enduranceTier = 0;
        if (!onlyTier) {
            _timer = 0;
        }

        ResetBattleTimer();

        Player.DelBuff<EnduranceCloud3>();
        Player.DelBuff<EnduranceCloud2>();
        Player.DelBuff<EnduranceCloud1>();

        //foreach (Projectile trackedCloud in TrackedEntitiesSystem.GetTrackedProjectile<EnduranceCloud>(checkProjectile => checkProjectile.owner != Player.whoAmI)) {
        //    trackedCloud.Kill();
        //}
    }
}
