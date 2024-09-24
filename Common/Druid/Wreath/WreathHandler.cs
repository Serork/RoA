using Microsoft.Xna.Framework;

using RoA.Common.Druid.Claws;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Wreath;

sealed class WreathHandler : ModPlayer {
    private const float BASEADDVALUE = 0.115f;
    private const float DRAWCOLORINTENSITY = 3f;
    private const byte MAXBOOSTINCREMENT = 7;
    private const float STAYTIMEMAX = 5f;

    private ushort _currentResource, _tempResource, _tempResource2;
    private float _addExtraValue;

    private byte _boost;
    private ushort _increaseValue;
    private float _currentChangingTime, _currentChangingMult, _stayTime;
    private bool _shouldDecrease, _shouldDecrease2;

    private Color _lightingColor;

    public ushort MaxResource { get; private set; } = 100;
    public ushort ExtraResource { get; private set; } = 0;

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

    public float Progress => (float)CurrentResource / TotalResource;
    public float ChangingProgress {
        get {
            float value = ChangingTimeValue - _currentChangingTime;
            return _shouldDecrease2 ? value : Ease.CircOut(value);
        }
    }
    public bool IsEmpty => Progress <= 0.01f;
    public bool IsFull => Progress > 0.95f;

    public float AddValue => BASEADDVALUE + _addExtraValue;
    public bool IsChangingValue => _currentChangingTime > 0f;
    public float ChangingTimeValue => TimeSystem.LogicDeltaTime * 60f;

    public bool ShouldDraw => !IsEmpty || Player.IsHoldingNatureWeapon();
    public float PulseIntensity => _stayTime <= 1f ? Ease.CubeInOut(_stayTime) : 1f;

    public float DrawColorOpacity {
        get {
            float progress = Progress;
            float opacity = Math.Clamp(progress * 2f, 1f, DRAWCOLORINTENSITY);
            return opacity;
        }
    }
    public Color BaseColor => new(255, 255, 200, 200);
    public Color DrawColor => Utils.MultiplyRGB(BaseColor, Lighting.GetColor(new Point((int)LightingPosition.X / 16, (int)LightingPosition.Y / 16)) * DrawColorOpacity);
    public Color LightingColor => _lightingColor;
    public Vector2 LightingPosition => Utils.Floor(Player.Top - Vector2.UnitY * 15f);
    public float LightingIntensity => (float)Math.Min(Ease.CircOut(Progress), 0.35f);

    public ClawsHandler ClawsStats => Player.GetModPlayer<ClawsHandler>();
    public ClawsHandler.SpecialAttackSpawnInfo SpecialAttackData => ClawsStats.SpecialAttackData;

    public ushort AddResourceValue() => (ushort)(AddValue * TotalResource);

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
        if (playerUsingClaws && IsFull) {
            if (SpecialAttackData.Owner == selectedItem) {
                Reset();

                Projectile.NewProjectile(Player.GetSource_ItemUse(selectedItem), SpecialAttackData.SpawnPosition, SpecialAttackData.StartVelocity, SpecialAttackData.ProjectileTypeToSpawn, selectedItem.damage, selectedItem.knockBack, Player.whoAmI);
                SoundEngine.PlaySound(SpecialAttackData.PlaySoundStyle, SpecialAttackData.SpawnPosition);
            }
        }

        IncreaseResourceValue(natureProjectile.WreathPointsFine);
        MakeDusts();
    }

    public override void PostUpdateEquips() {
        if (_stayTime <= 0f && !_shouldDecrease) {
            Reset(true);
        }
        else {
            _stayTime -= TimeSystem.LogicDeltaTime;
        }
    }

    public override void PreUpdate() {
        if (!Player.IsLocal()) {
            return;
        }

        ChangingHandler();
        AddLight();
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
        _currentChangingTime = ChangingTimeValue;
        _tempResource = CurrentResource;
        _increaseValue = (ushort)(AddResourceValue() - AddResourceValue() * fine);
    }

    private void ChangingHandler() {
        if (IsFull && _addExtraValue > 0f) {
            _addExtraValue = 0f;
        }

        if (!IsChangingValue) {
            _boost = 0;

            return;
        }

        float mult = 1.75f;
        if (!_shouldDecrease2) {
            _currentChangingMult = mult;
        }
        else {
            mult = 1f;
            if (_currentChangingMult < mult) {
                _currentChangingMult += TimeSystem.LogicDeltaTime;
            }
        }
        _currentChangingTime -= TimeSystem.LogicDeltaTime * _currentChangingMult * Math.Max((byte)1, _boost);

        if (_shouldDecrease) {
            CurrentResource = (ushort)(_tempResource - _tempResource2 * ChangingProgress);
            if (IsEmpty) {
                _shouldDecrease = false;
                _increaseValue = _tempResource = _tempResource2 = 0;
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

        _shouldDecrease = true;

        _boost = 0;

        _tempResource = CurrentResource;
        _currentChangingTime = ChangingTimeValue;
        _tempResource2 = _tempResource;
    }

    private void MakeDusts() {
        if (Progress >= 0.1f && Progress <= 0.97f) {
            if (Main.netMode != NetmodeID.Server) {
                float progress = Progress * 1.25f + 0.1f;
                int count = Math.Min((int)(15 * progress), 10);
                if (Main.netMode != NetmodeID.Server) {
                    for (int i = 0; i < count; i++) {
                        if (Main.rand.NextChance(0.5)) {
                            Dust dust = Dust.NewDustDirect(LightingPosition - new Vector2(13, 23), 20, 20, ModContent.DustType<Content.Dusts.WreathDust>(), newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.45f, 0.8f, progress));
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
                            dust.noLightEmittence = true;
                            dust.alpha = (int)(DrawColorOpacity * 255f);
                            dust.customData = DrawColorOpacity;
                        }
                    }
                }
            }
        }
    }

    private void AddLight() {
        //_lightingColor = Color.LightGreen;
        //if (!Main.dedServ) {
        //    Lighting.AddLight(LightingPosition, _lightingColor.ToVector3() * LightingIntensity);
        //}
    }
}
