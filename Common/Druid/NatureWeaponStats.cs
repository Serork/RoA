using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed class NatureWeaponStats : ModPlayer {
    private float _druidBaseDamageMultiplier = 1f;
    private float _druidPotentialDamageMultiplier = 1f;
    private float _druidDamageExtraIncreaseValueMultiplier = 1f;

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

    public override void ResetEffects() {
        DruidBaseDamageMultiplier = 1f;
        DruidPotentialDamageMultiplier = 1f;
        DruidDamageExtraIncreaseValueMultiplier = 1f;
    }
}
