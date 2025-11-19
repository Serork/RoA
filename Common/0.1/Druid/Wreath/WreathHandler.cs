using Microsoft.Xna.Framework;

using RoA.Common.Configs;
using RoA.Common.Druid.Claws;
using RoA.Common.Druid.Forms;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content.Buffs;
using RoA.Content.Items;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Equipables.Wreaths.Hardmode;
using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

using static RoA.Common.Druid.Forms.BaseFormHandler;

namespace RoA.Common.Druid.Wreath;

sealed class WreathHandler : ModPlayer {
    private static ushort SHOWTIMEBEFOREDISAPPEARING => 120;
    public static ushort GETHITEFFECTTIME => 30;

    public override void UpdateDead() {
        CommonUpdate();
        if (_dontPlayStaySound) {
            return;
        }
        ForcedHardReset2();
        _dontPlayStaySound = true;
    }

    public static Color GetArmorGlowColor1(Player player, Color baseColor, float progress = -1f, byte a = 255) {
        if (progress < 0f || progress > 1f) {
            progress = GetWreathChargeProgress_ForArmorGlow(player);
        }
        Color color = Color.Lerp(baseColor, Color.White, 0.25f) * progress;
        color.A = (byte)((a != 255 ? a : 125) * progress);
        return color;
    }

    public static Color GetArmorGlowColor_HallowEnt(Player player, Color baseColor, float progress = -1f) => GetArmorGlowColor1(player, baseColor, progress);

    public static WreathHandler GetWreathStats(Player player) => player.GetWreathHandler();
    public static bool IsWreathCharged(Player player) => GetWreathStats(player).IsFull1;
    public static float GetWreathChargeProgress(Player player) => GetWreathStats(player).ActualProgress4;
    public static float GetWreathChargeProgress_ForArmorGlow(Player player) => GetWreathStats(player).ActualProgress5;

    public short YAdjustAmountInPixels;
    public bool LockWreathPosition;

    public enum WreathType : byte {
        Normal,
        Phoenix,
        Aether
    }

    private const float BASEADDVALUE = 0.115f;
    private const float DRAWCOLORINTENSITY = 3f;
    private const byte MAXBOOSTINCREMENT = 7;
    private const float STAYTIMEMAX = 5f;

    private ushort _currentResource, _tempResource;
    private float _addExtraValue;
    private float _keepBonusesForTime, _keepProgress;
    private byte _boost;
    private ushort _increaseValue;
    private float _currentChangingTime, _currentChangingMult, _stayTime, _extraChangingValueMultiplier;
    private bool _shouldDecrease, _shouldDecrease2;
    private FormInfo _formInfo;
    private ushort _hitEffectTimer;
    private bool _onFullCreated;
    private bool _useAltSounds = true;
    private ushort _showForTime;
    private bool _dontPlayStaySound;

    public ushort GetHitTimer { get; private set; }

    //public static LerpColor AetherLerpColor { get; private set; } = new(0.03f);

    public static Color StandardColor => new(170, 252, 134);
    public static Color PhoenixColor => new(251, 234, 94);
    public static Color SoulOfTheWoodsColor => new(248, 119, 119);
    public static Color AetherColor => Color.Lerp(Color.Lerp(new(233, 191, 255), new(252, 134, 241), 0.5f), (LiquidRenderer.GetShimmerGlitterColor(top: true, 0f, 0f) * 1.5f).MultiplyAlpha(1.25f), 0.25f) with { A = 75 } * 1.075f;
    //public static Color AetherColor => AetherLerpColor.GetLerpColor([new(242, 182, 248), new(155, 196, 156), new(255, 155, 155), new(88, 146, 164)]);

    public static Color GetCurrentColor(Player player) {
        if (Main.gameMenu) {
            return StandardColor;
        }

        var self = player.GetWreathHandler();
        int hoverItemType = Main.HoverItem.type;
        if (hoverItemType == ModContent.ItemType<SoulOfTheWoods>()) {
            return SoulOfTheWoodsColor;
        }
        else if (hoverItemType == ModContent.ItemType<AetherWreath>()) {
            return AetherColor;
        }
        else if (hoverItemType == ModContent.ItemType<FenethsBlazingWreath>()) {
            return PhoenixColor;
        }
        return self.SoulOfTheWoods ? SoulOfTheWoodsColor :
            self.IsAetherWreath ? AetherColor :
            self.IsPhoenixWreath ? PhoenixColor :
            StandardColor;
    }

    public delegate void OnResetedDelegate();

    public OnResetedDelegate OnWreathReset = null;

    public ushort MaxResource { get; private set; } = 100;
    public ushort ExtraResource { get; private set; } = 0;
    public float ChangingTimeValue { get; private set; }
    public WreathType CurrentType { get; private set; } = WreathType.Normal;
    public WreathType CurrentVisualType { get; private set; } = WreathType.Normal;

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

    public bool StartSlowlyIncreasingUntilFull { get; private set; }
    public bool StartSlowlyIncreasingUntilFull2 { get; private set; }
    public bool CannotToggleOrGetWreathCharge { get; private set; }

    public bool ShouldKeepSlowFill2, ChargedBySlowFill;

    public ushort TotalResource => (ushort)(MaxResource + ExtraResource);

    public float Max => TotalResource / MaxResource;
    public float ActualProgress => (float)CurrentResource / TotalResource;
    public float ActualProgress2 => (float)CurrentResource / MaxResource;
    public float GetActualProgress2(ushort currentResource) => (float)currentResource / MaxResource;
    public float ActualProgress3 => SoulOfTheWoods ? (ActualProgress2 - 1f) : ActualProgress2;
    public float ActualProgress4 => MathHelper.Clamp(ActualProgress2, 0f, 1f);
    public float ActualProgress5 => MathHelper.Clamp(ActualProgress4 * 1.5f, 0f, 1f);
    public float Progress => HasKeepTime ? _keepProgress : ActualProgress2;
    public float GetProgress(ushort currentResource) => HasKeepTime ? _keepProgress : GetActualProgress2(currentResource);
    public float Progress2 => HasKeepTime ? _keepProgress : ActualProgress;
    public float ChangingProgress {
        get {
            float value = MathHelper.Clamp(ChangingTimeValue - _currentChangingTime, 0f, 1f);
            return _shouldDecrease2 ? value : (StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2) ? value : Ease.CircOut(value);
        }
    }
    public bool IsEmpty => ActualProgress2 <= 0.01f;
    public bool IsEmpty2 => ActualProgress2 <= 0.05f;
    public bool IsEmpty3 => ActualProgress2 <= 0.15f;
    public bool IsFull1 => Progress >= 0.95f /*|| Player.GetFormHandler().IsInADruidicForm*/;
    public bool IsActualFull1 => ActualProgress2 >= 0.95f;
    public bool WillBeFull(ushort currentResource, bool clawsReset = false) => (clawsReset ? GetActualProgress2(currentResource) : GetProgress(currentResource)) > 0.95f;
    public bool IsFull2 => Progress >= 1.95f;
    public bool IsFull3 => IsFull1 && Progress <= 1.1f;
    public bool IsFull4 => Progress >= 0.85f;
    public bool IsFull6 => Progress >= 0.975f;
    public bool IsActualFull6 => ActualProgress2 >= 0.975f;
    public bool IsFull7 => Progress >= 1.9f;

    public bool IsMinCharged => ActualProgress2 > 0.1f;

    public float AddValue => BASEADDVALUE + _addExtraValue;
    public bool IsChangingValue => _currentChangingTime > 0f;

    public bool ShouldDrawItself { get; private set; }
    public float PulseIntensity { get; private set; }

    public bool HasKeepTime => _keepBonusesForTime > 0f;

    public float DrawColorOpacity {
        get {
            float progress = ActualProgress;
            float opacity = Math.Clamp(progress * 2f, 1f, DRAWCOLORINTENSITY);
            return opacity;
        }
    }
    public static Color BaseColor => new(255, 255, 200, 200);
    public static Color AetherBaseColor => Color.Lerp(new Color(255, 255, 255, 100), (LiquidRenderer.GetShimmerGlitterColor(top: true, 0f, 0f) * 1.5f).MultiplyAlpha(1.25f), 0.25f) with { A = 75 } * 1.075f;
    public static Color SoulOfTheWoodsBaseColor => new(255, 255, 255, 100);
    //public Color DrawColor => Utils.MultiplyRGB(BaseColor, Lighting.GetColor(new Point((int)NormalWreathPosition.X / 16, (int)NormalWreathPosition.Y / 16)) * DrawColorOpacity);
    public Vector2 NormalWreathPosition {
        get {
            var config = ModContent.GetInstance<RoAClientConfig>();
            bool flag = false;
            if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
                config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
                flag = true;
            }
            return (flag || IsNormalNearHPBar() ? Player.Center : Utils.Floor(Player.Top - Vector2.UnitY * (Player.gravDir == -1 ? Player.height * -1f - 15f : 15f))) + Vector2.UnitY * YAdjustAmountInPixels;
        }
    }
    public float LightingIntensity => (float)Math.Min(Ease.CircOut(ActualProgress3), 0.35f);

    public ClawsHandler ClawsStats => Player.GetModPlayer<ClawsHandler>();
    public ClawsHandler.SpecialAttackSpawnInfo SpecialAttackData => ClawsStats.SpecialAttackData;
    public DruidStats DruidPlayerStats => Player.GetModPlayer<DruidStats>();

    public bool SoulOfTheWoods => DruidPlayerStats.SoulOfTheWoods && Progress > 1f && Progress <= 2f;
    public bool IsPhoenixWreath => CurrentVisualType == WreathType.Phoenix || CurrentType == WreathType.Phoenix;
    public bool IsAetherWreath => CurrentVisualType == WreathType.Aether || CurrentType == WreathType.Aether;

    public ushort AddResourceValue() => (ushort)(AddValue * MaxResource);

    public bool HasEnough(float percentOfMax, bool total = false) => CurrentResource >= (ushort)((total ? TotalResource : MaxResource) * percentOfMax);

    public override void Unload() {
        OnWreathReset = null!;
    }

    internal void ReceivePlayerSync(ushort resource/*, ushort tempResource, float changingTimeValue, float currentChangingTime, bool shouldDecrease1, bool shouldDecrease2, float currentChangingMult, ushort increaseValue, float stayTime, bool startSlowlyIncreasingUntilFull, bool startSlowlyIncreasingUntilFull2*/) {
        CurrentResource = resource;
        //_tempResource = tempResource;
        //ChangingTimeValue = changingTimeValue;
        //_currentChangingTime = currentChangingTime;
        //_shouldDecrease = shouldDecrease1;
        //_shouldDecrease2 = shouldDecrease2;
        //_currentChangingMult = currentChangingMult;
        //_increaseValue = increaseValue;
        //_stayTime = stayTime;
        //StartSlowlyIncreasingUntilFull = startSlowlyIncreasingUntilFull;
        //StartSlowlyIncreasingUntilFull2 = startSlowlyIncreasingUntilFull2;
    }

    public override void CopyClientState(ModPlayer targetCopy) {
        WreathHandler source = Player.GetWreathHandler(), target = targetCopy.Player.GetWreathHandler();
        target.CurrentResource = source.CurrentResource;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        => SendPacket(Player, toWho);

    private void SendPacket(Player player, int toClient) {
        WreathHandler source = player.GetWreathHandler();
        MultiplayerSystem.SendPacket(new WreathPointsSyncPacket((byte)player.whoAmI, source.CurrentResource/*, source._tempResource, source.ChangingTimeValue, source._currentChangingTime, source._shouldDecrease, source._shouldDecrease2, source._currentChangingMult, source._increaseValue, source._stayTime, source.StartSlowlyIncreasingUntilFull, source.StartSlowlyIncreasingUntilFull2*/),
            toClient: toClient);
    }

    public override void SendClientChanges(ModPlayer clientPlayer) {
        WreathHandler source = Player.GetWreathHandler(), target = clientPlayer.Player.GetWreathHandler();
        if (source.CurrentResource != target.CurrentResource) {
            SendPacket(Player, -1);
        }
    }

    public delegate void OnHitByAnythingDelegate(Player player, Player.HurtInfo hurtInfo);
    public static event OnHitByAnythingDelegate OnHitByAnythingEvent = null!;

    public override void OnHurt(Player.HurtInfo info) {
        if (GetHitTimer <= 0) {
            GetHitTimer = GETHITEFFECTTIME;
        }

        OnHitByAnythingEvent?.Invoke(Player, info);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        HandleOnHitNPCForNatureProjectile(proj, target: target);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
        if (!target.CanActivateOnHitEffect()) {
            return;
        }
        if (!item.IsANatureWeapon()) {
            return;
        }

        IncreaseResourceValue(0f);

        if (!ChargedBySlowFill) {
            if (_hitEffectTimer <= 0) {
                MakeDustsOnHit();
                _hitEffectTimer = 4;
            }
        }
    }

    internal void HandleOnHitNPCForNatureProjectile(Projectile proj, bool nonDataReset = false, NPC target = null) {
        if (target != null && !target.CanActivateOnHitEffect()) {
            return;
        }

        if (proj.ModProjectile is NatureProjectile natureProjectile) {
            if (!natureProjectile.ShouldChargeWreathOnDamage && !nonDataReset) {
                return;
            }

            ClawsReset_OnHit(proj, nonDataReset);

            IncreaseResourceValue(natureProjectile.WreathFillingFine);

            if (!ChargedBySlowFill) {
                if (_hitEffectTimer <= 0) {
                    MakeDustsOnHit();
                    _hitEffectTimer = 4;
                }
            }

            return;
        }

        try {
            if (CrossmodNatureContent.IsProjectileNature(proj)) {
                CrossmodNatureProjectileHandler handler = proj.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
                if (!(!handler.ShouldChargeWreathOnDamage && !nonDataReset)) {
                    ClawsReset_OnHit(proj, nonDataReset);

                    IncreaseResourceValue(handler.WreathFillingFine);

                    if (!ChargedBySlowFill) {
                        if (_hitEffectTimer <= 0) {
                            MakeDustsOnHit();
                            _hitEffectTimer = 4;
                        }
                    }
                }
            }
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
        }
    }

    internal void OnResetEffects() {
        if (Player.whoAmI == Main.myPlayer && ShouldDrawItself && CurrentResource > 0) {
            if (_useAltSounds) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WreathNewDischarge") { PitchVariance = 0.1f, Volume = 0.6f }, Player.Center);
            else SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WreathDischarge") { PitchVariance = 0.1f, Volume = 0.5f }, Player.Center);
        }
    }

    public override void ResetEffects() {
        ShouldKeepSlowFill2 = false;
    }

    internal void ForcedHardReset() {
        StartSlowlyIncreasingUntilFull = false;
        StartSlowlyIncreasingUntilFull2 = false;
        ChargedBySlowFill = ShouldKeepSlowFill2 = false;
        Reset();
        OnResetEffects();
        OnWreathReset?.Invoke();
        _addExtraValue = _boost = 0;
    }


    internal void ForcedHardReset2() {
        StartSlowlyIncreasingUntilFull = false;
        StartSlowlyIncreasingUntilFull2 = false;
        ChargedBySlowFill = ShouldKeepSlowFill2 = false;
        if (!_dontPlayStaySound) {
            OnResetEffects();
        }
        _addExtraValue = _boost = 0;
        Reset(noEffects: _dontPlayStaySound);
        if (_stayTime != 0f) {
            Dusts_ResetStayTime();
            _stayTime = 0f;
            GetHitTimer = 0;
        }
    }

    public bool ShouldClawsReset() => IsActualFull6 && (SpecialAttackData.ShouldReset || SpecialAttackData.OnlySpawn);

    internal void TryToClawsReset(Item item, bool nonDataReset) {
        Item selectedItem = item;
        bool playerUsingClaws = selectedItem.IsNatureClaws();
        if (playerUsingClaws && Player.ItemAnimationActive) {
            if (!_shouldDecrease && !_shouldDecrease2 && IsActualFull6) {
                if (SpecialAttackData.Owner == selectedItem && (ShouldClawsReset() || nonDataReset)) {
                    if (!SpecialAttackData.OnlySpawn || nonDataReset) {
                        ForcedHardReset();
                    }

                    if (!nonDataReset) {
                        SpecialAttackData.OnSpawn?.Invoke();

                        if (SpecialAttackData.ShouldSpawn) {
                            if (SpecialAttackData.SpawnProjectile != null) {
                                SpecialAttackData.SpawnProjectile.Invoke();
                            }
                            else if (Player.whoAmI == Main.myPlayer) {
                                Projectile.NewProjectile(Player.GetSource_ItemUse(selectedItem), SpecialAttackData.SpawnPosition ?? Player.Center, SpecialAttackData.StartVelocity, SpecialAttackData.ProjectileTypeToSpawn, Player.GetWeaponDamage(selectedItem), Player.GetWeaponKnockback(selectedItem), Player.whoAmI);
                            }

                            SpecialAttackData.OnAttack?.Invoke();

                            SoundEngine.PlaySound(SpecialAttackData.PlaySoundStyle, SpecialAttackData.SpawnPosition);
                        }
                    }
                }
            }
        }
    }

    private void ClawsReset_OnHit(Projectile projectile, bool nonDataReset) {
        Item attachedItem = projectile.ModProjectile is NatureProjectile natureProjectile ? natureProjectile.AttachedNatureWeapon : projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>().AttachedNatureWeapon;
        Item selectedItem = Player.GetSelectedItem();
        bool playerUsingClaws = selectedItem.IsNatureClaws();
        if (playerUsingClaws && Player.ItemAnimationActive && attachedItem == selectedItem && selectedItem.As<ClawsBaseItem>().ResetOnHit) {
            selectedItem.As<ClawsBaseItem>().OnHit(Player, Progress);
            if (!_shouldDecrease && !_shouldDecrease2 && IsActualFull6) {
                if (SpecialAttackData.Owner == selectedItem && (SpecialAttackData.ShouldReset || SpecialAttackData.OnlySpawn || nonDataReset)) {
                    if (!SpecialAttackData.OnlySpawn || nonDataReset) {
                        ForcedHardReset();
                    }

                    if (!nonDataReset) {
                        SpecialAttackData.OnSpawn?.Invoke();

                        if (SpecialAttackData.ShouldSpawn) {
                            if (SpecialAttackData.SpawnProjectile != null) {
                                SpecialAttackData.SpawnProjectile.Invoke();
                            }
                            else if (Player.whoAmI == Main.myPlayer) {
                                Projectile.NewProjectile(Player.GetSource_ItemUse(selectedItem), SpecialAttackData.SpawnPosition ?? Player.Center, SpecialAttackData.StartVelocity, SpecialAttackData.ProjectileTypeToSpawn, Player.GetWeaponDamage(selectedItem), Player.GetWeaponKnockback(selectedItem), Player.whoAmI);
                            }

                            SpecialAttackData.OnAttack?.Invoke();

                            SoundEngine.PlaySound(SpecialAttackData.PlaySoundStyle, SpecialAttackData.SpawnPosition);
                        }
                    }
                }
            }
        }
    }

    internal void Consume(float percentageOfMax, bool total = false) {
        CurrentResource -= (ushort)((total ? TotalResource : MaxResource) * 0.25f);
        _stayTime = STAYTIMEMAX;

        _shouldDecrease = _shouldDecrease2 = false;
        _increaseValue = 0;
        ResetChangingValue();

        StartSlowlyIncreasingUntilFull = false;
        StartSlowlyIncreasingUntilFull2 = false;

        for (int i = 0; i < 5; i++) {
            MakeDustsOnHit(1f);
        }
    }

    internal void SlowlyActivateForm(FormInfo formInfo) {
        if (CannotToggleOrGetWreathCharge) {
            return;
        }
        if (Player.GetFormHandler().IsInADruidicForm) {
            return;
        }
        if (StartSlowlyIncreasingUntilFull) {
            return;
        }
        if (Player.mount.Active) {
            Player.mount.Dismount(Player);
        }
        StartSlowlyIncreasingUntilFull = true;
        StartSlowlyIncreasingUntilFull2 = false;
        _formInfo = formInfo;
        IncreaseResourceValue(increaseUntilFull: true);

        if (Player.whoAmI == Main.myPlayer) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Twinkle") { Pitch = 0.3f, Volume = 0.2f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = -0.4f, Volume = 0.4f }, Player.Center);
        }
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(Player, 16, Player.Center));
        }
    }

    internal void SlowlyFill2() {
        if (Player.GetFormHandler().IsInADruidicForm) {
            return;
        }
        if (StartSlowlyIncreasingUntilFull) {
            return;
        }
        if (CannotToggleOrGetWreathCharge) {
            if (_currentChangingMult < 0.1f) {
                Dusts_ResetStayTime();
                ChargedBySlowFill = false;
                Reset(true);
                OnResetEffects();
            }
            return;
        }
        if (StartSlowlyIncreasingUntilFull2) {
            return;
        }
        StartSlowlyIncreasingUntilFull2 = true;
        IncreaseResourceValue(increaseUntilFull: true);

        if (Player.whoAmI == Main.myPlayer) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Twinkle") { Pitch = 0.3f, Volume = 0.2f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = -0.4f, Volume = 0.4f }, Player.Center);
        }
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(Player, 16, Player.Center));
        }
    }

    internal void Reset1() {
        StartSlowlyIncreasingUntilFull = false;
        StartSlowlyIncreasingUntilFull2 = false;
    }

    public void Dusts_ResetStayTime() {
        bool flag = IsFull1 && (StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2);
        bool flag2 = IsFull1 && !(StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2);
        for (int i = 0; i < (flag2 ? 10 : flag ? 5 : 3); i++) {
            MakeDusts_ActualMaking();
        }
        for (int i = 0; i < (StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2 ? 1 : 3); i++) {
            MakeDustsOnHit();
        }
    }

    private void ResetVisualParametersForNotNormal() {
        if (!IsFull1 || (IsFull1 && !IsFull2)) {
            _onFullCreated = false;
        }
    }

    private bool IsNormalNearHPBar() {
        var config = ModContent.GetInstance<RoAClientConfig>();
        bool flag5 = false;
        if (config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
            flag5 = true;
        }
        return flag5;
    }

    private bool IsNotNormal() => IsNormalNearHPBar() || RoAClientConfig.IsBars || RoAClientConfig.IsFancy;

    private bool IsNotNormalWithStat() => IsNormalNearHPBar() || RoAClientConfig.IsBars2 || RoAClientConfig.IsFancy2;

    private void VisualEffectOnFull() {
        if (IsChangingValue && !_shouldDecrease) {
            ushort dustType = GetDustType();
            var config = ModContent.GetInstance<RoAClientConfig>();
            bool noNormal = IsNotNormal();
            int value = CurrentResource % 100;
            if (CurrentResource > 50 && (value > 90 || value < 5)) {
                if ((IsFull3 || IsFull2) && !_onFullCreated) {
                    if (noNormal) {
                        int count = 20;
                        for (int i = 0; i < count; i++) {
                            float progress2 = 1.35f;
                            Dust dust = Dust.NewDustDirect(NormalWreathPosition - new Vector2(13, (IsNotNormalWithStat() ? 34 : 40) * Player.gravDir), 20, 20, dustType, newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.45f, 0.8f, progress2));
                            if (Player.gravDir == -1f) {
                                dust.position.Y -= 15f;
                            }
                            dust.velocity *= 1.25f * progress2;
                            if (i >= (int)(count * 0.8f)) {
                                dust.velocity *= 2f * progress2;
                            }
                            else if (i >= count / 2) {
                                dust.velocity *= 1.5f * progress2;
                            }
                            dust.fadeIn = Main.rand.Next(0, 17) * 0.1f;
                            dust.noGravity = true;
                            dust.position += dust.velocity * 0.75f;
                            dust.noLight = true;
                            dust.noLightEmittence = true;
                            dust.alpha = (int)(DrawColorOpacity * 255f);
                            dust.customData = DrawColorOpacity * PulseIntensity * 1.6f;
                        }
                    }

                    if (Player.whoAmI == Main.myPlayer && ShouldDrawItself) {
                        if (_useAltSounds) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WreathNewCharge") { Pitch = 0.1f, PitchVariance = 0.1f, Volume = 0.3f }, Player.Center);
                        else SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WreathCharge") { PitchVariance = 0.1f, Volume = 0.5f }, Player.Center);
                    }

                    _onFullCreated = true;
                }
            }
        }
        //else {
        //    ResetVisualParametersForNotNormal();
        //}
    }

    public override void PostUpdate() {
        if (ChargedBySlowFill && !ShouldKeepSlowFill2) {
            Dusts_ResetStayTime();
            ChargedBySlowFill = false;
            Reset(true);
            OnResetEffects();
        }
        if (!IsEmpty || Player.GetFormHandler().IsInADruidicForm || ChargedBySlowFill) {
            _showForTime = SHOWTIMEBEFOREDISAPPEARING;
        }
        CommonUpdate();
    }

    private void CommonUpdate() {
        //AetherLerpColor.Update();
        if (IsEmpty && CannotToggleOrGetWreathCharge) {
            CannotToggleOrGetWreathCharge = false;
            ChargedBySlowFill = false;
        }
        if (_showForTime > 0) {
            _showForTime--;
        }
        GetHitTimer = (ushort)Helper.Approach(GetHitTimer, 0, 1);
        ShouldDrawItself = _showForTime > 0 /* || Player.GetFormHandler().HasDruidArmorSet*/;
        YAdjustAmountInPixels = 0;
    }

    public CaneBaseProjectile? GetHeldCane() {
        CaneBaseProjectile? caneProjectile = null;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner != Player.whoAmI) {
                continue;
            }
            if (projectile.ModProjectile is CaneBaseProjectile baseCaneProjectile) {
                caneProjectile = baseCaneProjectile;
                break;
            }
        }
        return caneProjectile;
    }

    public bool TryGetHeldCane(out CaneBaseProjectile? caneProjectile) {
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner != Player.whoAmI) {
                continue;
            }
            if (projectile.ModProjectile is CaneBaseProjectile baseCaneProjectile) {
                caneProjectile = baseCaneProjectile;
                return true;
            }
        }
        caneProjectile = null;
        return false;
    }

    public override void PostUpdateEquips() {
        bool alt = ModContent.GetInstance<RoAClientConfig>().WreathSoundMode == RoAClientConfig.WreathSoundModes.Alt;
        if (_useAltSounds != alt) {
            _useAltSounds = alt;
        }

        ApplyBuffs();
        GetWreathType();
        GetWreathVisualType();
        MakeDusts();

        VisualEffectOnFull();

        if (_hitEffectTimer > 0) {
            _hitEffectTimer--;
            //_hitEffectTimer = 0;
        }

        if (StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2) {
            void effects() {
                float progress = Ease.QuartIn(Progress);
                if (((float)Main.rand.Next(425) < Progress * 200f || Main.rand.Next(40) == 0)) {
                    int num8 = Dust.NewDust(Player.position, Player.width, Player.height, GetDustType(), 0f, 0f, (int)(DrawColorOpacity * 255f), BaseColor * DrawColorOpacity, MathHelper.Lerp(0.45f, 0.8f, progress));
                    Main.dust[num8].noGravity = true;
                    Main.dust[num8].velocity *= 0.75f;
                    Main.dust[num8].fadeIn = 1.3f;
                    Main.dust[num8].noLight = true;
                    Main.dust[num8].customData = DrawColorOpacity * PulseIntensity * 1.6f;
                    Vector2 vector = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    vector.Normalize();
                    vector *= (float)Main.rand.Next(50, 100) * 0.075f;
                    Main.dust[num8].velocity = vector + Player.velocity;
                    vector.Normalize();
                    vector *= 100f;
                    Main.dust[num8].position = Player.Center - vector;
                }
                if (Main.rand.NextBool(7)) {
                    int num = 0;
                    if (Player.gravDir == -1f)
                        num -= Player.height;
                    int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height + (float)num - 2), Player.width + 8, 4, GetDustType2(), (0f - Player.velocity.X) * 0.5f, Player.velocity.Y * 0.5f, (int)(DrawColorOpacity * 255f), BaseColor * DrawColorOpacity, MathHelper.Lerp(0.45f, 0.8f, progress));
                    Main.dust[num6].velocity.X = Main.dust[num6].velocity.X * 0.2f;
                    Main.dust[num6].velocity.Y = -0.5f - Main.rand.NextFloat() * 1.5f;
                    Main.dust[num6].velocity.Y *= Main.rand.NextFloat();
                    Main.dust[num6].fadeIn = 0.1f;
                    Main.dust[num6].scale *= Main.rand.NextFloat(1.1f, 1.25f);
                    Main.dust[num6].scale *= 1.5f;
                    Main.dust[num6].noGravity = true;
                    Main.dust[num6].noLight = true;
                    Main.dust[num6].customData = DrawColorOpacity * PulseIntensity * 1.6f;
                }
                float value = MathHelper.Clamp(MathHelper.Max(0.2f, progress), 0f, 1f);
                Player.accRunSpeed *= value;
                Player.runAcceleration *= value;
                Player.maxRunSpeed *= value;
                Player.jumpSpeed *= value;
            }
            if (StartSlowlyIncreasingUntilFull2) {
                // should reset
                if (!ShouldKeepSlowFill2) {
                    Dusts_ResetStayTime();
                    StartSlowlyIncreasingUntilFull2 = false;
                    ResetChangingValue();
                    Reset();
                    _dontPlayStaySound = true;
                }

                effects();
                if (_stayTime != 0f) {
                    Dusts_ResetStayTime();
                    _stayTime = 0f;
                }
                if (!IsFull6) {

                }
                else {
                    Dusts_ResetStayTime();
                    StartSlowlyIncreasingUntilFull2 = false;
                    Reset(true, 0.1f);

                    OnSlowCharged();
                    // on full
                }
            }
            else {
                if (!Player.GetFormHandler().HasDruidArmorSet) {
                    StartSlowlyIncreasingUntilFull = false;
                    ResetChangingValue();
                    Reset();
                    _dontPlayStaySound = true;
                }
                effects();
                if (_stayTime != 0f) {
                    Dusts_ResetStayTime();
                    _stayTime = 0f;
                }
                if (!IsFull6) {

                }
                else if (!Player.ItemAnimationActive) {
                    Dusts_ResetStayTime();
                    StartSlowlyIncreasingUntilFull = false;
                    if (!Main.dedServ) {
                        BaseFormHandler.ApplyForm(Player, _formInfo);
                    }

                    if (ShouldKeepSlowFill2) {
                        OnSlowCharged();
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        MultiplayerSystem.SendPacket(new ResetFormPacket(Player));
                    }
                }
            }
        }
        else if (_stayTime <= 0f && !_shouldDecrease && !_shouldDecrease2) {
            Reset(true, noEffects: _dontPlayStaySound);
        }
        else if (!Player.GetFormHandler().IsInADruidicForm && !ChargedBySlowFill) {
            CaneBaseProjectile? caneProjectile = GetHeldCane();
            bool flag = caneProjectile == null;
            bool flag2 = !flag && !caneProjectile!.PreparingAttack;
            _stayTime -= TimeSystem.LogicDeltaTime * (flag2 ? 0f : 1f);
        }
        else {
            if (_stayTime != 0f) {
                Dusts_ResetStayTime();
                _stayTime = 0f;
            }
        }
        if (HasKeepTime) {
            _keepBonusesForTime -= 1f;
        }
        if ((Player.GetFormHandler().IsInADruidicForm/* || ChargedBySlowFill*/) && !IsFull1) {
            _keepBonusesForTime = Math.Max(Math.Max(DruidPlayerStats.KeepBonusesForTime, 10), _keepBonusesForTime);
        }

        PulseIntensity = MathHelper.Lerp(
            PulseIntensity,
            _stayTime <= 0.35f ? 0f : _stayTime > 0.35f && _stayTime <= 1.35f ? Ease.CubeInOut(_stayTime - 0.35f) : MathHelper.Lerp(PulseIntensity, 1f, 0.2f),
            0.25f);

        if (Player.IsLocal() && Player.GetSelectedItem().ModItem is NatureItem natureItem) {
            natureItem.WhileBeingHold(Player, Progress);
        }
    }


    public delegate void OnSlowChargedDelegate(Player player);
    public static event OnSlowChargedDelegate OnSlowChargedEvent;
    private void OnSlowCharged() {
        CannotToggleOrGetWreathCharge = true;
        ChargedBySlowFill = true;

        OnSlowChargedEvent?.Invoke(Player);
    }

    public override void Load() {
        On_Player.AddBuff_ActuallyTryToAddTheBuff += On_Player_AddBuff_ActuallyTryToAddTheBuff;
        On_PlayerDrawLayers.DrawPlayer_36_CTG += On_PlayerDrawLayers_DrawPlayer_36_CTG;
    }

    private void On_PlayerDrawLayers_DrawPlayer_36_CTG(On_PlayerDrawLayers.orig_DrawPlayer_36_CTG orig, ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        float positionY = drawinfo.Position.Y;
        if (player.whoAmI == Main.myPlayer && player.GetWreathHandler().ShouldDrawItself) {
            drawinfo.Position.Y -= 35f + (RoAClientConfig.IsBars || RoAClientConfig.IsFancy ? 20f : 0f);
        }
        orig(ref drawinfo);
        drawinfo.Position.Y = positionY;
    }

    private bool On_Player_AddBuff_ActuallyTryToAddTheBuff(On_Player.orig_AddBuff_ActuallyTryToAddTheBuff orig, Player self, int type, int time) {
        int buff = ModContent.BuffType<WreathCharged>();
        int buff2 = ModContent.BuffType<WreathFullCharged>();
        int buff3 = ModContent.BuffType<WreathFullCharged2>();
        List<int> buffTypes = [buff, buff2, buff3];
        if (buffTypes.Contains(type)) {
            self.AddBuffInStart(type, time);
            return true;
        }

        return orig(self, type, time);
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
            if (IsFull1) {
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
        Item wreathSlotItem = WreathSlot.GetFunctionalItem(Player);
        bool phoenixWreathEquipped = wreathSlotItem.type == ModContent.ItemType<FenethsBlazingWreath>();
        bool aetherWreathEquipped = wreathSlotItem.type == ModContent.ItemType<AetherWreath>();
        CurrentType = aetherWreathEquipped ? WreathType.Aether : phoenixWreathEquipped ? WreathType.Phoenix : WreathType.Normal;
    }

    private void GetWreathVisualType() {
        Item wreathSlotItem = WreathSlot.GetVanityItem(Player);
        bool phoenixWreathEquipped = wreathSlotItem.type == ModContent.ItemType<FenethsBlazingWreath>();
        bool aetherWreathEquipped = wreathSlotItem.type == ModContent.ItemType<AetherWreath>();
        CurrentVisualType = aetherWreathEquipped ? WreathType.Aether : phoenixWreathEquipped ? WreathType.Phoenix : WreathType.Normal;
    }

    public override void PreUpdate() {
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

        LockWreathPosition = false;
    }

    public void CapMax1() {
        if (CurrentResource > MaxResource) {
            CurrentResource = MaxResource;
        }
    }

    public void IncreaseResourceValue(float fine = 0f, bool increaseUntilFull = false, float extra2 = 1f) {
        if (CannotToggleOrGetWreathCharge) {
            return;
        }

        if (_shouldDecrease2) {
            _shouldDecrease = _shouldDecrease2 = false;
        }

        if (_shouldDecrease) {
            _addExtraValue = 0f;

            return;
        }

        if (!increaseUntilFull) {
            if (IsChangingValue) {
                //if (_boost < MAXBOOSTINCREMENT) {
                //    _boost++;
                //}
                _boost = 1;
                _addExtraValue += BASEADDVALUE / _boost * BASEADDVALUE;
            }
        }
        else {
            _addExtraValue = 0f;
        }

        _stayTime = STAYTIMEMAX;
        ChangeItsValue();
        float extra = Player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier * extra2;
        _increaseValue = StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2 ? (ushort)((MaxResource - CurrentResource) * extra) :
            (ushort)(GetIncreaseValue(fine) * extra);
    }

    internal ushort GetIncreaseValue(float fine) => (ushort)Math.Max(2, AddResourceValue() - AddResourceValue() * fine);

    private void ChangingHandler() {
        if ((IsFull3 || (SoulOfTheWoods && IsFull2)) && _addExtraValue > 0f) {
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
            mult *= _extraChangingValueMultiplier;
            if (_currentChangingMult < mult) {
                _currentChangingMult += TimeSystem.LogicDeltaTime;
            }
            else {
                _currentChangingMult = mult;
            }
        }
        float value2 = TimeSystem.LogicDeltaTime * _currentChangingMult * Math.Max((byte)1, _boost);
        if (StartSlowlyIncreasingUntilFull || StartSlowlyIncreasingUntilFull2) {
            value2 *= 0.15f;
        }
        CaneBaseProjectile? caneProjectile = GetHeldCane();
        if ((_shouldDecrease || _shouldDecrease2) && caneProjectile != null && caneProjectile.PreparingAttack) {
            value2 *= 0.15f;
        }
        _currentChangingTime -= value2;
        if (_shouldDecrease) {
            CurrentResource = (ushort)(_tempResource - _tempResource * ChangingProgress);
            if (IsEmpty) {
                _shouldDecrease = false;
                _increaseValue = _tempResource = 0;
            }

            return;
        }
        CurrentResource = (ushort)(_tempResource + _increaseValue * ChangingProgress);

        // floor
        bool flag = CurrentResource > 195 && CurrentResource < 200;
        if (!_shouldDecrease &&
            ((CurrentResource > 95 && CurrentResource < 100) ||
            flag)) {
            CurrentResource = (ushort)(flag ? 200 : 100);
            VisualEffectOnFull();
        }
    }

    internal void Reset(bool slowReset = false, float extraChangingValue = 1f, bool noEffects = false) {
        if (!_shouldDecrease2) {
            if (!_shouldDecrease && !noEffects) {
                OnResetEffects();
            }

            if (IsFull1 && !HasKeepTime) {
                _keepBonusesForTime = DruidPlayerStats.KeepBonusesForTime;
                _keepProgress = ActualProgress2;
            }

            _shouldDecrease = true;

            _boost = 0;

            ChangeItsValue();
        }

        if (slowReset) {
            if (!_shouldDecrease2 && !noEffects) {
                OnResetEffects();
            }

            _shouldDecrease2 = true;
            _currentChangingMult = TimeSystem.LogicDeltaTime;
            _extraChangingValueMultiplier = extraChangingValue;
        }
        else {
            _shouldDecrease2 = false;
        }

        if (_dontPlayStaySound) {
            _dontPlayStaySound = false;
        }
    }

    private void ChangeItsValue() {
        int value = CurrentResource % 100;
        if (_shouldDecrease || !(value > 90 || value < 5)) {
            ResetVisualParametersForNotNormal();
        }

        _tempResource = CurrentResource;
        ChangingTimeValue = TimeSystem.LogicDeltaTime * 60f;
        _currentChangingTime = ChangingTimeValue;
    }

    private void ResetChangingValue() {
        _tempResource = CurrentResource;
        _currentChangingTime = ChangingTimeValue = 0f;
    }

    private void MakeDusts() {
        if (Player.whoAmI == Main.myPlayer) {
            if ((PulseIntensity > 0f || HasKeepTime) && (IsFull2 || IsFull3)) {
                if (Player.miscCounter % 6 == 0 && Main.rand.NextChance(0.5)) {
                    MakeDusts_ActualMaking();
                }
            }
        }
    }

    private void MakeDusts_ActualMaking() {
        bool flag = RoAClientConfig.IsBars;
        if (flag) {
            return;
        }

        var config = ModContent.GetInstance<RoAClientConfig>();
        if (config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
            return;
        }

        if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
            return;
        }

        if (Main.rand.NextChance(MathUtils.Clamp01(GetWreathChargeProgress_ForArmorGlow(Player)))) {
            Dust dust = Dust.NewDustPerfect(NormalWreathPosition - new Vector2(0, (IsNotNormal() ? (IsNotNormalWithStat() ? 34 : 40) : 20) * GetWreathChargeProgress_ForArmorGlow(Player) * Player.gravDir) + Main.rand.NextVector2CircularEdge(26, 20 * GetWreathChargeProgress_ForArmorGlow(Player)) * (0.3f + Main.rand.NextFloat() * 0.5f) + Player.velocity, GetDustType(),
                new Vector2(0f, ((0f - Main.rand.NextFloat()) * 0.3f - 0.4f) * Player.gravDir), newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.65f, 0.8f, Main.rand.NextFloat()) * 1.5f);
            dust.fadeIn = Main.rand.Next(0, 17) * 0.1f;
            dust.alpha = (int)(DrawColorOpacity * PulseIntensity * 255f);
            dust.noGravity = true;
            dust.noLight = true;
            dust.noLightEmittence = true;
            dust.customData = DrawColorOpacity * PulseIntensity * 2f;
        }
    }

    public void MakeDustsOnHit(float progress = -1f) {
        if (Player.whoAmI != Main.myPlayer) {
            return;
        }

        var config = ModContent.GetInstance<RoAClientConfig>();
        if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
            return;
        }

        if (config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathPosition == RoAClientConfig.WreathPositions.Health) {
            return;
        }

        ushort dustType = GetDustType();
        bool flag = RoAClientConfig.IsBars;
        if (flag) {
            return;
        }

        float actualProgress = ActualProgress3;
        if (actualProgress >= 0.1f && actualProgress <= 0.95f) {
            if (progress == -1f) {
                progress = actualProgress * 1.25f + 0.1f;
            }
            int count = Math.Min((int)(15 * progress), 10);
            for (int i = 0; i < count; i++) {
                if (Main.rand.NextChance(0.3)) {
                    Dust dust = Dust.NewDustDirect(NormalWreathPosition - new Vector2(13, (IsNotNormal() ? (IsNotNormalWithStat() ? 34 : 40) : 20) * Player.gravDir), 20, 20, dustType, newColor: BaseColor * DrawColorOpacity, Scale: MathHelper.Lerp(0.45f, 0.8f, progress));
                    if (Player.gravDir == -1f) {
                        dust.position.Y -= 15f;
                    }
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
                    dust.customData = DrawColorOpacity * PulseIntensity * 1.6f;
                }
            }
        }
    }

    private ushort GetDustType() {
        ushort basicDustType = (ushort)(IsAetherWreath ? ModContent.DustType<Content.Dusts.WreathDust4>() :
                                        IsPhoenixWreath ? ModContent.DustType<Content.Dusts.WreathDust3>() :
                                        ModContent.DustType<Content.Dusts.WreathDust>());
        ushort dustType = (ushort)(IsFull1 && !IsFull3 ? ModContent.DustType<Content.Dusts.WreathDust2>() : basicDustType);
        return dustType;
    }

    private ushort GetDustType2() {
        ushort basicDustType = (ushort)(IsAetherWreath ? ModContent.DustType<Content.Dusts.WreathDust4_2>() : 
                                        IsPhoenixWreath ? ModContent.DustType<Content.Dusts.WreathDust3_2>() : 
                                        ModContent.DustType<Content.Dusts.WreathDust_2>());
        ushort dustType = (ushort)(IsFull1 && !IsFull3 ? ModContent.DustType<Content.Dusts.WreathDust2_2>() : basicDustType);
        return dustType;
    }

    private void AddLight() {
        return;

        //if (Player.whoAmI != Main.myPlayer) {
        //    return;
        //}

        //bool flag = RoAClientConfig.IsBars;
        //if (flag) {
        //    return;
        //}

        //float value0 = ActualProgress2;
        //float progress = value0;
        //float progress2 = MathHelper.Clamp(progress, 0f, 1f);
        //float value = 0.5f;
        //float value2 = value0 - 1f;
        //if (value2 > 0f) {
        //    progress2 *= MathHelper.Clamp(1f - value2 * 1.5f, 0f, 1f);
        //}
        //Lighting.AddLight(NormalWreathPosition, (IsPhoenixWreath ? new Color(251, 234, 94) : new Color(170, 252, 134)).ToVector3() * 0.35f * progress2 * (1.35f + value));
        //if (SoulOfTheWoods) {
        //    progress = value2;
        //    Lighting.AddLight(NormalWreathPosition, new Color(248, 119, 119).ToVector3() * 0.35f * (progress * (2f + value)));
        //}
    }
}
