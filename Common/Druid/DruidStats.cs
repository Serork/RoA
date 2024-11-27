using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed class DruidStats : ModPlayer {
    private float _druidBaseDamageMultiplier = 1f;
    private float _druidPotentialDamageMultiplier = 1f;
    private float _druidDamageExtraIncreaseValueMultiplier = 1f;
    private float _keepBonusesForTime = 0f;
    private float _dischargeTimeDecreaseMultiplier = 1f;
    private float _druidPotentialUseTimeMultiplier = 1f;

    public float DruidBaseDamageMultiplier {
        get => _druidBaseDamageMultiplier;
        set {
            _druidBaseDamageMultiplier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public float DruidPotentialDamageMultiplier {
        get => _druidPotentialDamageMultiplier;
        set {
            _druidPotentialDamageMultiplier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public float DruidDamageExtraIncreaseValueMultiplier {
        get => _druidDamageExtraIncreaseValueMultiplier;
        set {
            _druidDamageExtraIncreaseValueMultiplier = MathHelper.Clamp(value, 0f, 2f);
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

    public bool SoulOfTheWoods { get; set; }

    public override void ResetEffects() {
        DruidBaseDamageMultiplier = 1f;
        DruidPotentialDamageMultiplier = 1f;
        DruidDamageExtraIncreaseValueMultiplier = 1f;
        DischargeTimeDecreaseMultiplier = 1f;
        DruidPotentialUseTimeMultiplier = 1f;

        KeepBonusesForTime = 0f;

        SoulOfTheWoods = false;
    }
}
