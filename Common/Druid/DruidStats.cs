using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class DruidStats : ModPlayer {
    public override void Load() {
        WreathHandler.OnHitByAnythingEvent += WreathHandler_OnHitByAnythingEvent;
    }

    private void WreathHandler_OnHitByAnythingEvent(Player player, Player.HurtInfo hurtInfo) {
        if (!player.GetDruidStats().IsStarfruitCharmEffectActive) {
            return;
        }

        if (!player.GetWreathHandler().IsFull1) {
            return;
        }

        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        player.GetWreathHandler().ForcedHardReset(makeDusts: true);
    }

    private float _druidPotentialDamageMultiplier = 1f;
    private float _druidDamageExtraIncreaseValueMultiplier = 1f;
    private float _keepBonusesForTime = 0f;
    private float _dischargeTimeDecreaseMultiplier = 1f;
    private float _druidPotentialUseTimeMultiplier = 1f;
    private int _druidBaseDamage = 0, _druidPotentialDamage = 0;
    private float _clawsAttacKSpeedModifier;
    private float _clawsResetDecreaseModifier;

    public enum DruidEyesType : byte {
        PricklenutCharm,
        PinCushion,
        DruidsEyes,
        StarfruitCharm
    }

    public (bool, DruidEyesType) IsDruidsEyesEffectActive;

    public bool IsCrystallineNeedleEffectActive;

    public bool IsStarfruitCharmEffectActive;

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

    public float ClawsAttackSpeedModifier {
        get => _clawsAttacKSpeedModifier;
        set {
            _clawsAttacKSpeedModifier = MathHelper.Clamp(value, 0f, 2f);
        }
    }

    public float ClawsResetDecreaseModifier {
        get => _clawsResetDecreaseModifier;
        set {
            if (Player.GetFormHandler().IsInADruidicForm) {
                value = 1f;
            }
            _clawsResetDecreaseModifier = MathUtils.Clamp01(value);
        }
    }

    public bool SoulOfTheWoods { get; set; }

    public override void ResetEffects() {
        IsCrystallineNeedleEffectActive = false;
        IsStarfruitCharmEffectActive = false;

        DruidPotentialDamageMultiplier = 1f;
        WreathChargeRateMultiplier = 1f;
        DischargeTimeDecreaseMultiplier = 1f;
        DruidPotentialUseTimeMultiplier = 1f;
        ClawsAttackSpeedModifier = 1f;

        ClawsResetDecreaseModifier = 1f;

        KeepBonusesForTime = 0f;

        DruidBaseDamage = DruidPotentialDamage = 0;

        SoulOfTheWoods = false;

        ResetEquippableWreathStats();

        IsDruidsEyesEffectActive = (false, DruidEyesType.DruidsEyes);
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

    public override void OnHurt(Player.HurtInfo info) {
        bool empowered = Player.GetWreathHandler().IsFull1;

        if (Player.whoAmI == Main.myPlayer) {
            int cooldownCounter = info.CooldownCounter;
            if (Player.starCloakItem == null && IsStarfruitCharmEffectActive /*&& (cooldownCounter == -1 || cooldownCounter == 1)*/) {
                for (int num15 = 0; num15 < 3; num15++) {
                    float x = Player.position.X + (float)Main.rand.Next(-400, 400);
                    float y = Player.position.Y - (float)Main.rand.Next(500, 800);
                    Vector2 vector = new Vector2(x, y);
                    float num16 = Player.position.X + (float)(Player.width / 2) - vector.X;
                    float num17 = Player.position.Y + (float)(Player.height / 2) - vector.Y;
                    num16 += (float)Main.rand.Next(-100, 101);
                    float num18 = (float)Math.Sqrt(num16 * num16 + num17 * num17);
                    num18 = 23f / num18;
                    num16 *= num18;
                    num17 *= num18;

                    int type = ModContent.ProjectileType<StarfruitCharmStar>();
                    //Item item = starCloakItem;
                    //if (starCloakItem_starVeilOverrideItem != null) {
                    //    item = starCloakItem_starVeilOverrideItem;
                    //    type = 725;
                    //}

                    //if (starCloakItem_beeCloakOverrideItem != null) {
                    //    item = starCloakItem_beeCloakOverrideItem;
                    //    type = 724;
                    //}

                    //if (starCloakItem_manaCloakOverrideItem != null) {
                    //    item = starCloakItem_manaCloakOverrideItem;
                    //    type = 723;
                    //}

                    int num19 = 75;
                    if (Main.masterMode)
                        num19 *= 3;
                    else if (Main.expertMode)
                        num19 *= 2;

                    Projectile.NewProjectile(Player.GetSource_OnHurt(info.DamageSource), x, y, num16, num17, type, num19, 5f, Player.whoAmI, 0f, Player.position.Y, 2f * empowered.ToInt());
                }
            }
        }
    }
}
