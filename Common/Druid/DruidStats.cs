﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class DruidStats : ModPlayer {
    private float _druidPotentialDamageMultiplier = 1f;
    private float _druidDamageExtraIncreaseValueMultiplier = 1f;
    private float _keepBonusesForTime = 0f;
    private float _dischargeTimeDecreaseMultiplier = 1f;
    private float _druidPotentialUseTimeMultiplier = 1f;
    private int _druidBaseDamage = 0, _druidPotentialDamage = 0;

    public float DruidPotentialDamageMultiplier {
        get => _druidPotentialDamageMultiplier;
        set {
            _druidPotentialDamageMultiplier = MathHelper.Clamp(value, 0f, 3f);
        }
    }

    public float DruidDamageExtraIncreaseValueMultiplier {
        get => _druidDamageExtraIncreaseValueMultiplier;
        set {
            _druidDamageExtraIncreaseValueMultiplier = MathHelper.Clamp(value, 0f, 3f);
        }
    }

    public float KeepBonusesForTime {
        get => _keepBonusesForTime;
        set {
            _keepBonusesForTime = MathHelper.Clamp(value, 0f, 300f);
        }
    }

    public float DischargeTimeDecreaseMultiplier {
        get => _dischargeTimeDecreaseMultiplier;
        set {
            _dischargeTimeDecreaseMultiplier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public float DruidPotentialUseTimeMultiplier {
        get => _druidPotentialUseTimeMultiplier;
        set {
            _druidPotentialUseTimeMultiplier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public int DruidBaseDamage {
        get => _druidBaseDamage;
        set {
            _druidBaseDamage = (int)MathHelper.Clamp(value, -100, 100);
        }
    }

    public int DruidPotentialDamage {
        get => _druidPotentialDamage;
        set {
            _druidPotentialDamage = (int)MathHelper.Clamp(value, -100, 100);
        }
    }

    public bool SoulOfTheWoods { get; set; }

    public override void ResetEffects() {
        DruidPotentialDamageMultiplier = 1f;
        DruidDamageExtraIncreaseValueMultiplier = 1f;
        DischargeTimeDecreaseMultiplier = 1f;
        DruidPotentialUseTimeMultiplier = 1f;

        KeepBonusesForTime = 0f;

        DruidBaseDamage = DruidPotentialDamage = 0;

        SoulOfTheWoods = false;

        ResetEquippableWreathStats();
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        ApplyVenomToAttackerAndDamageIt(Player, npc, hurtInfo);
    }
}
