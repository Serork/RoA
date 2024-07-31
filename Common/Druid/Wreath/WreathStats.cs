using Microsoft.Xna.Framework;

using RoA.Common.Druid.Claws;
using RoA.Content;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Wreath;

sealed class WreathStats : ModPlayer {
    private const float BASEADDVALUE = 0.115f;
    private const float INCREASINGTIME = TimeSystem.LogicDeltaTime * 60f;
    private const float DRAWCOLORINTENSITY = 3f;

    private ushort _currentResource, _tempResource;
    private float _addExtraValue;

    private byte _increasingFactor;
    private ushort _increaseValue;
    private float _currentIncreasingTime;

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
    public float IncreasingProgress => Ease.CircOut(Math.Min(1f - _currentIncreasingTime, 0.999f));
    public bool IsFull => Progress > 0.998f;

    public float AddValue => BASEADDVALUE + _addExtraValue;
    public bool IsIncreasingValue => _currentIncreasingTime > 0f;

    public float DrawColorOpacity {
        get {
            float progress = Progress;
            float opacity = Math.Clamp(progress * 2f, 1f, DRAWCOLORINTENSITY);
            return opacity;
        }
    }
    public Color DrawColor => Utils.MultiplyRGB(new Color(255, 255, 200, 200), Lighting.GetColor(new Point((int)LightingPosition.X / 16, (int)LightingPosition.Y / 16)) * DrawColorOpacity);
    public Color LightingColor => _lightingColor;
    public Vector2 LightingPosition => Utils.Floor(Player.Top - Vector2.UnitY * 15f);
    public float LightingIntensity => (float)Math.Min(Ease.CircOut(Progress), 0.35f);

    public ClawsStats ClawsStats => Player.GetModPlayer<ClawsStats>();
    public ClawsStats.SpecialAttackSpawnInfo SpecialAttackData => ClawsStats.SpecialAttackData;

    public ushort AddResourceValue() => (ushort)(AddValue * TotalResource);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool hitByNatureDamage = hit.DamageType.CountsAsClass(DruidClass.NatureDamage);
        if (!hitByNatureDamage) {
            return;
        }

        Item selectedItem = Player.GetSelectedItem();
        bool playerUsingClaws = selectedItem.ModItem is BaseClawsItem;
        if (playerUsingClaws && IsFull && Main.myPlayer == Player.whoAmI) {
            Projectile.NewProjectile(Player.GetSource_ItemUse(selectedItem), SpecialAttackData.SpawnPosition, SpecialAttackData.StartVelocity, SpecialAttackData.ProjectileTypeToSpawn, selectedItem.damage, selectedItem.knockBack, Player.whoAmI);
        }

        IncreaseCurrentResourceValue();
        MakeDusts();
    }

    public override void PostUpdateEquips() {
        if (!Player.IsHoldingNatureWeapon()) {
            Reset();
        }
    }

    public override void PostUpdateMiscEffects() {
        HandleIncreasing();
        AddLight();
    }

    private void IncreaseCurrentResourceValue() {
        _currentIncreasingTime = INCREASINGTIME;
        _tempResource = CurrentResource;

        if (IsIncreasingValue) {
            _increasingFactor++;
            _addExtraValue += BASEADDVALUE / _increasingFactor * BASEADDVALUE;
        }

        _increaseValue = AddResourceValue();
    }

    private void HandleIncreasing() {
        if (!IsIncreasingValue) {
            _increasingFactor = 0;

            return;
        }

        _currentIncreasingTime -= TimeSystem.LogicDeltaTime * 1.75f * Math.Max((byte)1, _increasingFactor);

        CurrentResource = (ushort)(_tempResource + _increaseValue * IncreasingProgress);
    }

    private void Reset() {
        _currentIncreasingTime = _addExtraValue = 0f;
        _increasingFactor = 0;
        CurrentResource = _tempResource = _increaseValue = 0;
    }

    private void MakeDusts() {
        if (!IsFull) {
            if (Main.netMode != NetmodeID.Server) {
                float progress = Progress * 1.25f + 0.1f;
                int count = (int)(15 * progress);
                if (Main.netMode != NetmodeID.Server) {
                    for (int i = 0; i < count; i++) {
                        if (Main.rand.NextChance(0.5)) {
                            Dust dust = Dust.NewDustDirect(LightingPosition - new Vector2(13, 23), 20, 20, ModContent.DustType<Content.Dusts.WreathDust>(), newColor: DrawColor * Math.Max(DRAWCOLORINTENSITY - DrawColorOpacity, 0f), Scale: MathHelper.Lerp(0.45f, 0.8f, progress));
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
                        }
                    }
                }
            }
        }
    }

    private void AddLight() {
        _lightingColor = Color.LightGreen;
        if (!Main.dedServ) {
            Lighting.AddLight(LightingPosition, _lightingColor.ToVector3() * LightingIntensity);
        }
    }
}
