using Microsoft.Xna.Framework;

using RoA.Common.Druid.Claws;
using RoA.Common.Players;
using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Items.Weapons.Druidic.Rods;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using static RoA.Common.Druid.Wreath.WreathHandler;
using static tModPorter.ProgressUpdate;

namespace RoA.Common.Druid.Wreath;

sealed class WreathHandler : ModPlayer {
    public enum WreathType : byte {
        Normal,
        Phoenix
    }

    private const float BASEADDVALUE = 0.115f;
    private const float DRAWCOLORINTENSITY = 3f;
    private const byte MAXBOOSTINCREMENT = 7;
    private const float STAYTIMEMAX = 5f;

    private ushort _currentResource, _tempResource;
    private float _addExtraValue;
    private float _keepBonusesForTime;
    private byte _boost;
    private ushort _increaseValue;
    private float _currentChangingTime, _currentChangingMult, _stayTime;
    private bool _shouldDecrease, _shouldDecrease2;

    public ushort MaxResource { get; private set; } = 100;
    public ushort ExtraResource { get; private set; } = 0;
    public float ChangingTimeValue { get; private set; }
    public WreathType CurrentType { get; private set; } = WreathType.Normal;

    public ushort CurrentResource {
        get => _currentResource;
        private set {
            if (value < 1) {
                value = 0;
            }
            if (value > TotalResource) {
                value = TotalResource;
            }
            _currentResource = value;
        }
    }

    public ushort TotalResource => (ushort)(MaxResource + ExtraResource);

    public float Max => TotalResource / MaxResource;
    public float ActualProgress => (float)CurrentResource / TotalResource;
    public float ActualProgress2 => (float)CurrentResource / MaxResource;
    public float GetActualProgress2(ushort currentResource) => (float)currentResource / MaxResource;
    public float ActualProgress3 => SoulOfTheWoods ? (ActualProgress2 - 1f) : ActualProgress2;
    public float Progress => HasKeepTime ? Math.Max(1f, ActualProgress2) : ActualProgress2;
    public float GetProgress(ushort currentResource) => HasKeepTime ? Math.Max(1f, GetActualProgress2(currentResource)) : GetActualProgress2(currentResource);
    public float Progress2 => HasKeepTime ? Math.Max(1f, ActualProgress) : ActualProgress;
    public float ChangingProgress {
        get {
            float value = MathHelper.Clamp(ChangingTimeValue - _currentChangingTime, 0f, 1f);
            return _shouldDecrease2 ? value : Ease.CircOut(value);
        }
    }
    public bool IsEmpty => ActualProgress2 <= 0.01f;
    public bool IsFull => Progress > 0.95f;
    public bool GetIsFull(ushort currentResource) => GetProgress(currentResource) > 0.95f;
    public bool IsFull2 => Progress > 1.95f;
    public bool IsFull3 => Progress > 0.95f && Progress <= 1f;
    public bool IsMinCharged => ActualProgress2 > 0.1f;

    public float AddValue => BASEADDVALUE + _addExtraValue;
    public bool IsChangingValue => _currentChangingTime > 0f;

    public bool ShouldDraw => !IsEmpty || Player.IsHoldingNatureWeapon();
    public float PulseIntensity => _stayTime <= 0.35f ? 0f : _stayTime > 0.35f && _stayTime <= 1.35f ? Ease.CubeInOut(_stayTime - 0.35f) : 1f;

    public bool HasKeepTime => _keepBonusesForTime > 0f;

    public float DrawColorOpacity {
        get {
            float progress = ActualProgress;
            float opacity = Math.Clamp(progress * 2f, 1f, DRAWCOLORINTENSITY);
            return opacity;
        }
    }
    public Color BaseColor => new(255, 255, 200, 200);
    public Color DrawColor => Utils.MultiplyRGB(BaseColor, Lighting.GetColor(new Point((int)LightingPosition.X / 16, (int)LightingPosition.Y / 16)) * DrawColorOpacity);
    public Vector2 LightingPosition => Utils.Floor(Player.Top - Vector2.UnitY * 15f);
    public float LightingIntensity => (float)Math.Min(Ease.CircOut(ActualProgress3), 0.35f);

    public ClawsHandler ClawsStats => Player.GetModPlayer<ClawsHandler>();
    public ClawsHandler.SpecialAttackSpawnInfo SpecialAttackData => ClawsStats.SpecialAttackData;
    public DruidStats DruidPlayerStats => Player.GetModPlayer<DruidStats>();

    public bool SoulOfTheWoods => DruidPlayerStats.SoulOfTheWoods && Progress > 1f && Progress <= 2f;
    public bool IsPhoenixWreath => CurrentType == WreathType.Phoenix;

    public ushort AddResourceValue() => (ushort)(AddValue * MaxResource);

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        if (!Player.IsLocal()) {
            return;
        }
        if (!proj.IsDruidic(out NatureProjectile natureProjectile)) {
            return;
        }
        if (!natureProjectile.ShouldIncreaseWreathPoints) {
            return;
        }

        Item selectedItem = Player.GetSelectedItem();
        bool playerUsingClaws = selectedItem.ModItem is BaseClawsItem;
        if (playerUsingClaws && GetIsFull((ushort)(CurrentResource + GetIncreaseValue(natureProjectile.WreathPointsFine) / 2))) {
            if (SpecialAttackData.Owner == selectedItem) {
                Reset();

                Projectile.NewProjectile(Player.GetSource_ItemUse(selectedItem), SpecialAttackData.SpawnPosition, SpecialAttackData.StartVelocity, SpecialAttackData.ProjectileTypeToSpawn, selectedItem.damage, selectedItem.knockBack, Player.whoAmI);
                SoundEngine.PlaySound(SpecialAttackData.PlaySoundStyle, SpecialAttackData.SpawnPosition);
            }
        }
        //else {

        //}

        IncreaseResourceValue(natureProjectile.WreathPointsFine);
        MakeDustsOnHit();
    }

    public override void PostUpdateEquips() {
        ApplyBuffs();
        GetWreathType();
        MakeDusts();

        if (_stayTime <= 0f && !_shouldDecrease) {
            Reset(true);
        }
        else {
            BaseRodProjectile? rodProjectile = null;
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner == Player.whoAmI && projectile.ModProjectile is BaseRodProjectile baseRodProjectile) {
                    rodProjectile = baseRodProjectile;
                    break;
                }
            }
            bool flag = rodProjectile == null;
            bool flag2 = !flag && !rodProjectile.PreparingAttack;
            if (flag || flag2) {
                _stayTime -= TimeSystem.LogicDeltaTime;
            }
        }
        if (HasKeepTime && ActualProgress2 <= 1f) {
            _keepBonusesForTime -= 1f;
        }
    }

    private void ApplyBuffs() {
        int buff = ModContent.BuffType<WreathCharged>();
        int buff2 = ModContent.BuffType<WreathFullCharged>();
        int buff3 = ModContent.BuffType<WreathFullCharged2>();
        if (IsMinCharged) {
            int buffIndex;
            if (DruidPlayerStats.SoulOfTheWoods && IsFull2) {
                Player.AddBuff(buff3, 10);
                if (Player.FindBuff(buff2, out buffIndex)) {
                    Player.DelBuff(buffIndex);
                }

                return;
            }
            if (IsFull) {
                Player.AddBuff(buff2, 10);
                if (Player.FindBuff(buff, out buffIndex)) {
                    Player.DelBuff(buffIndex);
                }
                if (Player.FindBuff(buff3, out buffIndex)) {
                    Player.DelBuff(buffIndex);
                }

                return;
            }
            Player.AddBuff(buff, 10);
            if (Player.FindBuff(buff2, out buffIndex)) {
                Player.DelBuff(buffIndex);
            }
        }
    }

    private void GetWreathType() {
        bool phoenixWreathEquipped = WreathSlot.GetFunctionalItem(Player).type == ModContent.ItemType<FenethsBlazingWreath>();
        CurrentType = phoenixWreathEquipped ? WreathType.Phoenix : WreathType.Normal;
    }

    public override void PreUpdate() {
        if (!Player.IsLocal()) {
            return;
        }

        AddLight();

        MaxResource = 100;
        ExtraResource = 0;
        if (DruidPlayerStats.SoulOfTheWoods) {
            ExtraResource += 100;
        }
        if (CurrentResource > TotalResource) {
            CurrentResource = TotalResource;
        }

        ChangingHandler();
    }

    private void IncreaseResourceValue(float fine = 0f) {
        if (_shouldDecrease2) {
            _shouldDecrease = _shouldDecrease2 = false;
        }

        if (_shouldDecrease) {
            return;
        }

        if (IsChangingValue) {
            if (_boost < MAXBOOSTINCREMENT) {
                _boost++;
            }
            _addExtraValue += BASEADDVALUE / _boost * BASEADDVALUE;
        }

        _stayTime = STAYTIMEMAX;
        ChangeItsValue();
        _increaseValue = GetIncreaseValue(fine);
    }

    private ushort GetIncreaseValue(float fine) => (ushort)(AddResourceValue() - AddResourceValue() * fine);

    private void ChangingHandler() {
        if (IsFull && _addExtraValue > 0f) {
            _addExtraValue = 0f;
        }

        if (!IsChangingValue) {
            _boost = 0;

            return;
        }

        float num = 1.75f, num2 = 0.5f;
        float mult = num;
        if (!_shouldDecrease2) {
            _currentChangingMult = mult;
        }
        else {
            float progressForThis = (float)_tempResource / MaxResource;
            if (progressForThis > 1f) {
                float value = progressForThis - 1f;
                if (!float.IsNaN(value)) {
                    num2 -= num2 * 0.5f * value;
                }
            }
            mult = num2 * DruidPlayerStats.DischargeTimeDecreaseMultiplier;
            if (_currentChangingMult < mult) {
                _currentChangingMult += TimeSystem.LogicDeltaTime;
            }
            else {
                _currentChangingMult = mult;
            }
        }
        _currentChangingTime -= TimeSystem.LogicDeltaTime * _currentChangingMult * Math.Max((byte)1, _boost);

        if (_shouldDecrease) {
            CurrentResource = (ushort)(_tempResource - _tempResource * ChangingProgress);
            if (IsEmpty) {
                _shouldDecrease = false;
                _increaseValue = _tempResource = 0;
            }

            return;
        }
        CurrentResource = (ushort)(_tempResource + _increaseValue * ChangingProgress);
    }

    private void Reset(bool resetAfterStaying = false) {
        if (resetAfterStaying) {
            _shouldDecrease2 = true;
            _currentChangingMult = TimeSystem.LogicDeltaTime;
        }

        if (IsFull && !HasKeepTime) {
            _keepBonusesForTime = DruidPlayerStats.KeepBonusesForTime;
            //VisualCurrentResource = CurrentResource;
        }

        _shouldDecrease = true;

        _boost = 0;

        ChangeItsValue();
    }

    private void ChangeItsValue() {
        _tempResource = CurrentResource;
        ChangingTimeValue = TimeSystem.LogicDeltaTime * 60f;
        _currentChangingTime = ChangingTimeValue;
    }

    private void MakeDusts() {
        ushort dustType = GetDustType();
        if (PulseIntensity > 0f && (IsFull2 || IsFull3)) {
            if (Player.miscCounter % 6 == 0 && Main.rand.NextChance(0.5)) {
                Dust dust = Dust.NewDustPerfect(LightingPosition - new Vector2(0, 23) + Main.rand.NextVector2CircularEdge(20, 24) * (0.3f + Main.rand.NextFloat() * 0.5f), dustType, new Vector2(0f, (0f - Main.rand.NextFloat()) * 0.3f - 0.5f), newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.45f, 0.8f, Main.rand.NextFloat()) * 1.25f);
                dust.fadeIn = Main.rand.Next(0, 17) * 0.1f;
                dust.alpha = (int)(DrawColorOpacity * 255f);
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
                dust.customData = DrawColorOpacity;
            }
        }
    }

    private void MakeDustsOnHit() {
        float actualProgress = ActualProgress3;
        ushort dustType = GetDustType();
        if (actualProgress >= 0.1f && actualProgress <= 0.95f) {
            float progress = actualProgress * 1.25f + 0.1f;
            int count = Math.Min((int)(15 * progress), 10);
            for (int i = 0; i < count; i++) {
                if (Main.rand.NextChance(0.5)) {
                    Dust dust = Dust.NewDustDirect(LightingPosition - new Vector2(13, 23), 20, 20, dustType, newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.45f, 0.8f, progress));
                    dust.velocity *= 1.25f * progress;
                    if (i >= (int)(count * 0.8f)) {
                        dust.velocity *= 2f * progress;
                    }
                    else if (i >= count / 2) {
                        dust.velocity *= 1.5f * progress;
                    }
                    dust.fadeIn = Main.rand.Next(0, 17) * 0.1f;
                    dust.noGravity = true;
                    dust.position += dust.velocity * 0.75f;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                    dust.alpha = (int)(DrawColorOpacity * 255f);
                    dust.customData = DrawColorOpacity;
                }
            }
        }
    }

    private ushort GetDustType() {
        ushort basicDustType = (ushort)(IsPhoenixWreath ? ModContent.DustType<Content.Dusts.WreathDust3>() : ModContent.DustType<Content.Dusts.WreathDust>());
        ushort dustType = (ushort)(SoulOfTheWoods ? ModContent.DustType<Content.Dusts.WreathDust2>() : basicDustType);
        return dustType;
    }

    private void AddLight() {
        float progress = ActualProgress2;
        float progress2 = MathHelper.Clamp(progress, 0f, 1f);
        float value = 0.5f;
        float value2 = ActualProgress2 - 1f;
        if (value2 > 0f) {
            progress2 *= MathHelper.Clamp(1f - value2 * 1.5f, 0f, 1f);
        }
        Lighting.AddLight(LightingPosition, (IsPhoenixWreath ? new Color(251, 234, 94) : new Color(170, 252, 134)).ToVector3() * 0.35f * progress2 * (1.35f + value));
        if (SoulOfTheWoods) {
            progress = value2;
            Lighting.AddLight(LightingPosition, new Color(248, 119, 119).ToVector3() * 0.35f * (progress * (2f + value)));
        }
    }
}
