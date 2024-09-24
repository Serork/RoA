using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed class NatureWeaponStats : ModPlayer {
    private float _druidBaseDamageMultiplier = 1f;

    public float DruidBaseDamageMultiplier {
        get => _druidBaseDamageMultiplier;
        set {
            _druidBaseDamageMultiplier = MathHelper.Clamp(_druidBaseDamageMultiplier, -2f, 2f);
        }
    }

    public override void ResetEffects() {
        DruidBaseDamageMultiplier = 1f;
    }
}
