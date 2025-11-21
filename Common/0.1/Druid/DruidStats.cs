using Microsoft.Xna.Framework;

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
    private float _clawsAttacKSpeedModifier;

    public bool IsDruidsEyesEffectActive;

    public float DruidPotentialDamageMultiplier {
        get => _druidPotentialDamageMultiplier;
        set {
            _druidPotentialDamageMultiplier = MathHelper.Clamp(value, 0f, 3f);
        }
    }

    public float WreathChargeRateMultiplier {
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

    public float ClawsAttacKSpeedModifier {
        get => _clawsAttacKSpeedModifier;
        set {
            _clawsAttacKSpeedModifier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public bool SoulOfTheWoods { get; set; }

    public override void ResetEffects() {
        DruidPotentialDamageMultiplier = 1f;
        WreathChargeRateMultiplier = 1f;
        DischargeTimeDecreaseMultiplier = 1f;
        DruidPotentialUseTimeMultiplier = 1f;
        ClawsAttacKSpeedModifier = 1f;

        KeepBonusesForTime = 0f;

        DruidBaseDamage = DruidPotentialDamage = 0;

        SoulOfTheWoods = false;

        ResetEquippableWreathStats();

        IsDruidsEyesEffectActive = false;
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        ApplyVenomToAttackerAndDamageIt(Player, npc, hurtInfo);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        ApplyPoisonOnNatureDamage(Player, target, proj);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
        ApplyPoisonOnNatureDamage(Player, target, item);
    }
}
