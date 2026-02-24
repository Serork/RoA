using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;

using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Wreaths.Hardmode;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

using static RoA.Content.Projectiles.Friendly.Ranged.DistilleryOfDeathGust;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    private static byte MAXCOPIES => 10;

    public struct CopyInfo {
        private float _opacity;

        public Vector2 Position;
        public float Rotation;
        public Vector2 Origin;
        public int Direction;
        public int GravityDirection;
        public float Scale;
        public int BodyFrameIndex;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);
        }
    }

    private static float BACKFLIPTIME => 25f;
    private static float MAXFALLSPEEDMODIFIERFORFALL => 0.75f;

    private static ushort OBSIDIANSTOPWATCHCOOLDOWNINTICKS => MathUtils.SecondsToFrames(5);
    private static ushort OBSIDIANSTOPWATCHBETWEENINTICKS => (ushort)(OBSIDIANSTOPWATCHCOOLDOWNINTICKS / 7f);

    private static ushort CONJURERSEYEATTACKTIME => MathUtils.SecondsToFrames(0.5f);

    public static ushort CONTROLUSEITEMTIMECHECKBASE => 10;

    private static bool _drawingTempBufferCopies, _drawingObsidianStopwatchCopies;
    private static List<float> _tempBufferCopiesHueShift = null!;
    private static List<ushort> _obsidianStopwatchCopiesHueShift = null!;
    private static byte _currentTempBufferCopyIndex, _currentObsidianStopwatchCopyIndex;
    private static byte _copyIndexIAmDrawing;
    private static bool _naturePriestCapeDrawing;

    private const int MaxAdvancedShadows = 60;
    public int availableAdvancedShadowsCount;
    private EntityShadowInfo[] _advancedShadows = new EntityShadowInfo[60];
    private int _lastAddedAvancedShadow;

    private bool _fell;
    private float _fellTimer;
    private ushort _controlUseItemTimer;
    private float _backflipTimer;
    private bool _isTeleportingBackViaObisidianStopwatch;
    private ushort _currentTeleportPointIndex;
    private float _obsidianStopwatchTeleportCooldown;
    private float _obsidianStopwatchTeleportLerpValue, _obsidianStopwatchTeleportLerpValue2;

    private CopyInfo[] _copyData = new CopyInfo[MAXCOPIES];
    private byte _currentCopyIndex;

    public ushort ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
    public bool ControlUseItem;
    public bool StopFaceDrawing, StopHeadDrawing;

    public bool ApplyBoneArmorVisuals;

    public bool IsAetherInvincibilityActive;
    public float AetherShimmerAlpha;

    public float ExtraManaFromStarsModifier;
    public float ExtraLifeFromHeartsModifier;
    public int ManaIncrease, LifeIncrease;
    public bool DontShowHealEffect, DontShowManaEffect;

    public bool ElderSnailSlow;
    public bool IsElderShellEffectActive, IsElderShieldEffectActive;
    public int DefenseLastTick;

    public bool IsFriarLanternBuffEffectActive;
    public float FriarLanternEffectStrength;

    public ushort ArchiveUseCooldownInTicks;

    public bool DrawJokeVisor;

    public bool IsFallenLeavesEffectActive;
    public float FallenLeavesCounter;

    public float StandingStillTimer;

    public Player.CompositeArmStretchAmount TempStretchAmount;
    public bool ItemUsed;

    public bool IsSeedOfWisdomEffectActive;

    public bool IsMaidensBracersEffectActive;

    public bool IsFossilizedSpiralEffectActive;

    public ushort TempBufferDodgeAnimationCounter;

    public bool IsObsidianStopwatchEffectActive, IsObsidianStopwatchEffectActive_Hidden;

    public bool IsHereticsVeilEffectActive;
    public float HereticVeilEffectOpacity = 1f;

    public bool IsBansheesGuardEffectActive;

    public bool IsSnakeIdolEffectActive;

    public bool IsGobletOfPainEffectActive;

    public bool IsChromaticScarfEffectActive;

    public bool IsConjurersEyeEffectActive, IsConjurersEyeEffectActive_Hidden, ConjurersEyeVanity;
    public ushort ConjurersEyeAttackCounter;

    public bool IsDoubleGogglesEffectActive;
    public bool IsDoubleGogglesEffectActive_Hidden;
    public bool DoubleGogglesActivated;
    public float DoubleGogglesEffectOpacity;

    public bool IsBrawlerMaskEffectActive;
    public HashSet<(int, int)> BeforeHitActiveDebuffs = [];

    public bool DistilleryOfDeathInitialized, DistilleryOfDeathInitialized2;
    public GustType DistilleryOfDeathLastShootType_Current, DistilleryOfDeathLastShootType_Next, DistilleryOfDeathLastShootType_Next_Next;
    public byte DistilleryOfDeathShootCount;
    public GustType DistilleryOfDeathLastShootType_Back1, DistilleryOfDeathLastShootType_Back1_2, DistilleryOfDeathLastShootType_Back2, DistilleryOfDeathLastShootType_Back2_2;

    public bool ShouldDrawVanillaBackpacks = true;

    public bool IsNaturePriestCapeEffectActive;
    public ushort NaturePriestCapeImmunityFrames;

    public bool IsFermentedSpiderEyeEffectActive;

    public bool IsDuskStagEffectActive, IsDuskStagEffectActive_Vanity;
    public Vector2 DuskStagPosition, DuskStagVelocity;
    public float DuskStagVelocityFactor;
    public int DustStagDirection;

    public bool IsAriesActive;
    public bool IsGardeningGlovesEffectActive;
    public bool ShouldResetClawsOnNextAttack;

    public bool IsChainedCloudEffectActive;
    public bool IsThunderKingsGraceEffectActive;

    public bool IsFeathersInABottleEffectActive;
    public bool IsFeathersInABalloonEffectActive;

    public bool IsScrapRingEffectActive;
    public bool ScrapRingWet;
    public float ScrapRingStrength;
    public Color ScrapRingLiquidColor;

    public bool IsEyePatchEffectActive, IsEyePatchEffectActive_Hidden;

    public bool IsBlindFoldEffectActive;

    public bool IsBadgeOfHonorEffectActive;

    public bool CollidedWithFungalMushroom;
    public Projectile FungalMushroomICollidedWith = null!;

    public bool CollidedWithCottonBoll;
    public Projectile CottonBollICollidedWith = null!;

    public bool ZoneFilament;

    public bool IsHoneyPunchEffectActive;

    //public bool IsBrambleMazePlaced, IsBrambleMazeUsed;

    public readonly record struct ControlsCache(
        bool ControlUp,
        bool ControlLeft,
        bool ControlRight,
        bool ControlDown,
        bool ControlJump);

    public bool IsFilamentBindingEffectActive, IsFilamentBindingEffectActive2;
    public ControlsCache FilamentBindingControlsCache { get; private set; }

    public bool ShouldDrawProjectileOverArm;

    public enum EyePatchMode : byte {
        LeftEye = 0,
        RightEye = 1,
        BothEyes = 2,
        Count
    }
    public EyePatchMode _currentEyePatchMode;
    public EyePatchMode CurrentEyePatchMode {
        get => _currentEyePatchMode;
        set => _currentEyePatchMode = (EyePatchMode)Utils.Clamp((byte)value, (byte)EyePatchMode.LeftEye, (byte)EyePatchMode.Count);
    }

    public bool CollidedWithStarwayWormhole;
    public Projectile StarwayWormholeICollidedWith = null!;
    public float WormholeAdventureProgress;
    public float WormholeCooldown;
    public bool WormholeAdventureReversed;

    public override void OnEnterWorld() {

    }

    public byte CrystallineNeedleIndexToBeAdded { get; private set; }
    public (ushort, ushort)[] CrystallineNeedleTime { get; private set; } = new (ushort, ushort)[5];
    public float[] CrystallineNeedleRotation { get; private set; } = new float[5];
    public Vector2[] CrystallineNeedleExtraPosition { get; private set; } = new Vector2[5];

    public void AddCrystallineNeedle(ushort time, float rotation, Vector2 extraPosition) {
        while (MathF.Abs(rotation) < 2f) {
            rotation += 0.25f;
        }
        for (int i = 0; i < CrystallineNeedleTime.Length; i++) {
            if (CrystallineNeedleTime[i].Item1 <= 0) {
                continue;
            }
            while (MathF.Abs(rotation - CrystallineNeedleRotation[i]) < 1f) {
                rotation += 0.25f;
            }
        }
        while (MathF.Abs(rotation) < 1f) {
            rotation += 0.25f;
        }

        bool searchForFreeSlot() {
            for (int i = 0; i < CrystallineNeedleTime.Length; i++) {
                if (CrystallineNeedleTime[i].Item1 <= 0) {
                    CrystallineNeedleIndexToBeAdded = (byte)i;
                    return true;
                }
            }
            return false;
        }
        if (CrystallineNeedleIndexToBeAdded >= 5) {
            if (!searchForFreeSlot()) {
                return;
            }
        }
        CrystallineNeedleTime[CrystallineNeedleIndexToBeAdded] = (time, time);
        CrystallineNeedleRotation[CrystallineNeedleIndexToBeAdded] = rotation;
        CrystallineNeedleExtraPosition[CrystallineNeedleIndexToBeAdded] = extraPosition;
        if (searchForFreeSlot()) {
            return;
        }
        CrystallineNeedleIndexToBeAdded = 5;
    }

    public void UpdateCrystallineNeedles() {
        for (int i = 0; i < CrystallineNeedleTime.Length; i++) {
            if (CrystallineNeedleTime[i].Item1 > 0) {
                CrystallineNeedleTime[i].Item1--;
            }
        }
    }

    public override void UpdateLifeRegen() {
        for (int i = 0; i < CrystallineNeedleTime.Length; i++) {
            if (CrystallineNeedleTime[i].Item1 > 0) {
                Player.GetCritChance(DruidClass.Nature) += 15;
            }
        }
    }

    public override void NaturalLifeRegen(ref float regen) {
        for (int i = 0; i < CrystallineNeedleTime.Length; i++) {
            if (CrystallineNeedleTime[i].Item1 > 0) {
                regen *= 1.25f;
                return;
            }
        }
    }

    public override void UpdateDead() {
        DeerSkullReset();

        IsHereticsVeilEffectActive = false;
    }

    public bool ShouldUpdateAdvancedShadows;

    public bool IsClarityEffectActive;

    public float DistilleryOfDeathShootProgress => (float)DistilleryOfDeathShootCount / DistilleryOfDeath.DistilleryOfDeath_Use.SHOOTCOUNTPERTYPE;

    public bool ConjurersEyeCanShoot => Player.manaRegenDelay <= 0 && Player.statMana < Player.statManaMax2;
    public float ConjurersEyeShootOpacity => Utils.GetLerpValue(CONJURERSEYEATTACKTIME * 0.75f, CONJURERSEYEATTACKTIME, ConjurersEyeAttackCounter, true);

    public bool StandingStill => StandingStillTimer > 0;

    public bool CanSpawnFallenLeavesBranch => FallenLeavesCounter >= FallenLeaves.ATTACKTIME;

    public bool DoingBackflip => _backflipTimer > 0f;
    public float BackflipProgress => Ease.CubeIn(_backflipTimer / BACKFLIPTIME);

    public bool IsObsidianStopwatchTeleportAvailable => IsObsidianStopwatchEffectActive && _obsidianStopwatchTeleportCooldown <= 0;
    public bool IsObsidianStopwatchTeleportAvailable2 => IsObsidianStopwatchEffectActive && (_obsidianStopwatchTeleportCooldown <= OBSIDIANSTOPWATCHBETWEENINTICKS || _obsidianStopwatchTeleportCooldown >= OBSIDIANSTOPWATCHCOOLDOWNINTICKS - OBSIDIANSTOPWATCHBETWEENINTICKS * 5);
    public float ObsidianStopwatchEffectOpacity => 1f - Utils.GetLerpValue(0, OBSIDIANSTOPWATCHBETWEENINTICKS, _obsidianStopwatchTeleportCooldown, true) * Utils.GetLerpValue(OBSIDIANSTOPWATCHCOOLDOWNINTICKS, OBSIDIANSTOPWATCHCOOLDOWNINTICKS - OBSIDIANSTOPWATCHBETWEENINTICKS, _obsidianStopwatchTeleportCooldown, true);

    public void DoBackflip(float time = 0f) {
        if (DoingBackflip) {
            return;
        }

        _backflipTimer = time == 0f ? BACKFLIPTIME : time;
    }

    private static void ApplySkullEffect(Player self, Player drawPlayer) {
        if (drawPlayer.HasEquipped<CarcassChestguard>(EquipType.Body) &&
            (drawPlayer.HasEquipped<CarcassSandals>(EquipType.Legs) || drawPlayer.legs == CarcassSandals.FemaleLegs) &&
            (drawPlayer.HasEquipped<HornetSkull>(EquipType.Head) || drawPlayer.HasEquipped<DevilSkull>(EquipType.Head) || drawPlayer.HasEquipped<DeerSkull>(EquipType.Head) || drawPlayer.HasEquipped<CrystallizedSkull>(EquipType.Head) ||
            drawPlayer.HasEquipped(ArmorIDs.Head.Skull, EquipType.Head))) {
            self.GetCommon().ApplyBoneArmorVisuals = true;
        }
    }

    public bool Fell { get; private set; }

    public bool LockHorizontalMovement;
    public bool PerfectClotActivated;

    public Vector2 SavedPosition;
    public Vector2 SavedVelocity;
    public float DashTime;
    public bool Dashed;

    public Vector2[] OldUseItemPos = null!;
    public float[] OldUseItemRot = null!;

    public void KeepOldUseItemInfo() {
        if (OldUseItemPos == null) {
            return;
        }

        for (int num28 = OldUseItemPos.Length - 1; num28 > 0; num28--) {
            OldUseItemPos[num28] = OldUseItemPos[num28 - 1];
            OldUseItemRot[num28] = OldUseItemRot[num28 - 1];
        }

        OldUseItemPos[0] = Player.Center;
        OldUseItemRot[0] = 0f;
    }

    public void UpdateOldUseItemInfo(int length, Vector2 position, float rotation) {
        int num = length;
        if (OldUseItemPos == null || num != OldUseItemPos.Length) {
            OldUseItemPos = new Vector2[num];
            OldUseItemRot = new float[num];
            for (int i = 0; i < OldUseItemPos.Length; i++) {
                OldUseItemPos[i].X = 0f;
                OldUseItemPos[i].Y = 0f;
                OldUseItemRot[i] = 0f;
            }
        }

        KeepOldUseItemInfo();

        OldUseItemPos[0] = position;
        OldUseItemRot[0] = rotation;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
        drawInfo.colorHead *= HereticVeilEffectOpacity;
        drawInfo.colorHair *= HereticVeilEffectOpacity;
        drawInfo.colorEyes *= HereticVeilEffectOpacity;
        drawInfo.colorEyeWhites *= HereticVeilEffectOpacity;
        drawInfo.colorArmorHead *= HereticVeilEffectOpacity;
    }

    public override bool FreeDodge(Player.HurtInfo info) {
        return base.FreeDodge(info);
    }

    public override bool ConsumableDodge(Player.HurtInfo info) {
        if (Player.HasBuff<TempBuffer>()) {
            Player.DelBuff<TempBuffer>();

            Player.SetImmuneTimeForAllTypes(Player.longInvince ? 120 : 80);

            TempBufferDodgeAnimationCounter = 300;

            return true;
        }

        return base.ConsumableDodge(info);
    }

    public delegate void CatchFishDelegate(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);
    public static event CatchFishDelegate CatchFishEvent = null!;
    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
        CatchFishEvent?.Invoke(Player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
    }

    public override void SaveData(TagCompound tag) {
        if (PerfectClotActivated) {
            tag[RoA.ModName + nameof(PerfectClotActivated)] = true;
        }

        tag[RoA.ModName + nameof(CurrentEyePatchMode)] = (byte)CurrentEyePatchMode;
        if (IsEyePatchEffectActive) {
            tag[RoA.ModName + nameof(IsEyePatchEffectActive)] = true;
        }
        if (IsEyePatchEffectActive_Hidden) {
            tag[RoA.ModName + nameof(IsEyePatchEffectActive_Hidden)] = true;
        }
    }

    public override void LoadData(TagCompound tag) {
        PerfectClotActivated = tag.ContainsKey(RoA.ModName + nameof(PerfectClotActivated));

        CurrentEyePatchMode = (EyePatchMode)tag.GetByte(RoA.ModName + nameof(CurrentEyePatchMode));
        IsEyePatchEffectActive = tag.ContainsKey(RoA.ModName + nameof(IsEyePatchEffectActive));
        IsEyePatchEffectActive_Hidden = tag.ContainsKey(RoA.ModName + nameof(IsEyePatchEffectActive_Hidden));
    }

    public override void Load() {
        On_Player.PickupItem += On_Player_PickupItem;
        On_Player.HealEffect += On_Player_HealEffect;
        On_Player.ManaEffect += On_Player_ManaEffect;

        On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
        On_Player.DryCollision += On_Player_DryCollision;

        DrawPlayerFullEvent += PlayerCommon_DrawPlayerFullEvent;

        On_Player.HorizontalMovement += On_Player_HorizontalMovement;
        On_Player.SetArmorEffectVisuals += On_Player_SetArmorEffectVisuals;
        On_PlayerDrawLayers.DrawPlayer_21_Head_TheFace += On_PlayerDrawLayers_DrawPlayer_21_Head_TheFace;
        On_PlayerDrawLayers.DrawPlayer_21_Head += On_PlayerDrawLayers_DrawPlayer_21_Head;

        On_ItemSlot.PickItemMovementAction += On_ItemSlot_PickItemMovementAction;
        On_ItemSlot.ArmorSwap += On_ItemSlot_ArmorSwap;

        On_PlayerDrawSet.BoringSetup_2 += On_PlayerDrawSet_BoringSetup_21;

        DevilSkullLoad();
        CrystallizedSkullLoad();
        WiresLoad();
        CursorEffectsLoad();

        On_Player.AddBuff_ActuallyTryToAddTheBuff += On_Player_AddBuff_ActuallyTryToAddTheBuff;
        On_Player.AddBuff_TryUpdatingExistingBuffTime += On_Player_AddBuff_TryUpdatingExistingBuffTime;

        On_Player.GetImmuneAlpha += On_Player_GetImmuneAlpha;
        On_Player.GetImmuneAlphaPure += On_Player_GetImmuneAlphaPure;

        On_Player.UpdateAdvancedShadows += On_Player_UpdateAdvancedShadows;

        On_Player.CheckMana_int_bool_bool += On_Player_CheckMana_int_bool_bool1;
        On_Player.ItemCheck_PayMana += On_Player_ItemCheck_PayMana;

        On_Player.ApplyVanillaHurtEffectModifiers += On_Player_ApplyVanillaHurtEffectModifiers;

        On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull1;

        On_Player.RotatedRelativePoint += On_Player_RotatedRelativePoint;
    }

    private Vector2 On_Player_RotatedRelativePoint(On_Player.orig_RotatedRelativePoint orig, Player self, Vector2 pos, bool reverseRotation, bool addGfxOffY) {
        Vector2 result = orig(self, pos, reverseRotation, addGfxOffY);
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatform>(checkProjectile => !checkProjectile.SameOwnerAs(self))) {
            result += projectile.velocity;
        }
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatformAngry>(checkProjectile => !checkProjectile.SameOwnerAs(self))) {
            result += projectile.velocity;
        }
        return result;
    }

    private void On_LegacyPlayerRenderer_DrawPlayerFull1(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer) {
        orig(self, camera, drawPlayer);
    }

    private void On_Player_ApplyVanillaHurtEffectModifiers(On_Player.orig_ApplyVanillaHurtEffectModifiers orig, Player self, ref Player.HurtModifiers modifiers) {
        orig(self, ref modifiers);

        if (self.GetCommon().NaturePriestCapeImmunityFrames > 0) {
            float decrease = MathUtils.Clamp01(1f - Ease.QuintOut(self.GetCommon().NaturePriestCapeImmunityFrames / 20f));
            decrease = MathF.Max(0.01f, decrease);
            modifiers.FinalDamage *= decrease;
        }
    }

    private bool On_Player_ItemCheck_PayMana(On_Player.orig_ItemCheck_PayMana orig, Player self, Item sItem, bool canUse) {
        if (self.GetCommon().DoubleGogglesActivated) {
            self.GetCommon().DoubleGogglesActivated = false;
            return true;
        }

        return orig(self, sItem, canUse);
    }

    // TODO: net sync
    private bool On_Player_CheckMana_int_bool_bool1(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana) {
        if (self.GetCommon().IsDoubleGogglesEffectActive && Main.rand.NextBool(2)) {
            pay = false;
            self.GetCommon().DoubleGogglesActivated = true;
        }

        return orig(self, amount, pay, blockQuickMana);
    }

    public void MakeCopy(float opacity = 1.25f, float scale = 1f) {
        if (_currentCopyIndex >= MAXCOPIES) {
            _currentCopyIndex = 0;
        }
        Player player = Player;
        _copyData[_currentCopyIndex++] = new CopyInfo() {
            Position = player.position,
            Rotation = player.fullRotation,
            Origin = player.fullRotationOrigin,
            Direction = player.direction,
            GravityDirection = (int)player.gravDir,
            BodyFrameIndex = player.bodyFrame.Y / player.bodyFrame.Height,
            Opacity = opacity,
            Scale = scale
        };
    }

    public Vector2[] GetAdvancedShadowPositions(int length = 0) {
        int pickedLength = length == 0 ? availableAdvancedShadowsCount : length;
        Vector2[] positions = new Vector2[pickedLength];
        for (int i = 0; i < pickedLength; i++) {
            positions[i] = GetAdvancedShadow(i).Position;
        }
        return positions;
    }

    public float[] GetAdvancedShadowRotations(int length = 0) {
        int pickedLength = length == 0 ? availableAdvancedShadowsCount : length;
        float[] rotations = new float[pickedLength];
        int num30 = 1;
        for (int num31 = 0; num31 < num30; num31++) {
            for (int num32 = pickedLength - 1; num32 > 0; num32--) {
                rotations[num32] = (GetAdvancedShadow(num32 - 1).Position - GetAdvancedShadow(num32).Position).SafeNormalize(Vector2.Zero).ToRotation();
            }
        }
        return rotations;
    }

    public EntityShadowInfo GetAdvancedShadow(int shadowIndex) {
        if (shadowIndex > availableAdvancedShadowsCount)
            shadowIndex = availableAdvancedShadowsCount;

        int num = (_lastAddedAvancedShadow - shadowIndex).ModulusPositive(60);
        return _advancedShadows[num];
    }

    public void UpdateAdvancedShadows() {
        availableAdvancedShadowsCount++;
        if (availableAdvancedShadowsCount > 60)
            availableAdvancedShadowsCount = 60;

        if (++_lastAddedAvancedShadow >= 60)
            _lastAddedAvancedShadow = 0;

        _advancedShadows[_lastAddedAvancedShadow].CopyPlayer(Player);
        //_advancedShadows[_lastAddedAvancedShadow].Positions.Y += Player.gfxOffY;
    }

    public void ResetAdvancedShadows() {
        _lastAddedAvancedShadow = 0;
        for (int i = 0; i < availableAdvancedShadowsCount; i++) {
            _advancedShadows[i].Position = Vector2.Zero;
        }
        availableAdvancedShadowsCount = 0;
    }

    public override void SetControls() {
        var handler = Player.GetCommon();
        if (handler._isTeleportingBackViaObisidianStopwatch) {
            ResetControls();
        }

        if (CollidedWithFungalMushroom) {
            ResetControls();
        }
        if (CollidedWithCottonBoll) {
            ResetControls();
        }

        if (IsFilamentBindingEffectActive && !IsFilamentBindingEffectActive2) {
            FilamentBindingControlsCache = new ControlsCache(Player.controlUp, Player.controlLeft, Player.controlRight, Player.controlDown, Player.controlJump);
            IsFilamentBindingEffectActive2 = true;
        }

        if (IsFilamentBindingEffectActive) {
            Player.controlUp = FilamentBindingControlsCache.ControlUp;
            Player.controlLeft = FilamentBindingControlsCache.ControlLeft;
            Player.controlDown = FilamentBindingControlsCache.ControlDown;
            Player.controlRight = FilamentBindingControlsCache.ControlRight;
            Player.controlJump = FilamentBindingControlsCache.ControlJump;
        }
        else {
            IsFilamentBindingEffectActive2 = false;
        }
    }

    public void ResetControls() {
        Player.controlUp = false;
        Player.controlLeft = false;
        Player.controlDown = false;
        Player.controlRight = false;
        Player.controlJump = false;
        Player.controlUseItem = false;
        Player.controlUseTile = false;
        Player.controlThrow = false;
        Player.controlHook = false;
        Player.controlTorch = false;
        Player.controlSmart = false;
        Player.controlMount = false;
    }

    private void On_Player_UpdateAdvancedShadows(On_Player.orig_UpdateAdvancedShadows orig, Player self) {
        orig(self);

        var handler = self.GetCommon();
        if (handler._isTeleportingBackViaObisidianStopwatch) {
            for (int i = 0; i < 2; i++) {
                int shadowIndex = handler._currentTeleportPointIndex;
                if (shadowIndex > handler.availableAdvancedShadowsCount)
                    shadowIndex = handler.availableAdvancedShadowsCount;

                int num = (handler._lastAddedAvancedShadow - shadowIndex).ModulusPositive(60);
                Vector2 lastPosition = handler._advancedShadows[num].Position;
                if (lastPosition != Vector2.Zero) {
                    self.position = Vector2.Lerp(self.position, lastPosition, 0.5f);
                    self.velocity = Vector2.One * 0.5f * new Vector2(-self.direction, 1f);
                    self.velocity.Y *= 0f;
                    self.fallStart = (int)(self.position.Y / 16f);

                    self.gravity = 0f;

                    self.SetImmuneTimeForAllTypes(self.longInvince ? 40 : 20);
                    self.immuneNoBlink = true;
                }
                else {
                    handler._isTeleportingBackViaObisidianStopwatch = false;
                    handler._obsidianStopwatchTeleportCooldown = OBSIDIANSTOPWATCHCOOLDOWNINTICKS;
                    handler.ResetAdvancedShadows();
                    handler._obsidianStopwatchTeleportLerpValue2 = 0f;

                    self.AddBuff<Rewind>(MathUtils.SecondsToFrames(5));
                }
                if (handler._currentTeleportPointIndex < 60) {
                    if (handler._currentTeleportPointIndex >= 53) {
                        if (handler._obsidianStopwatchTeleportLerpValue2 > 0f) {
                            handler._obsidianStopwatchTeleportLerpValue2 -= 0.05f;
                        }
                    }
                    else {
                        if (handler._obsidianStopwatchTeleportLerpValue2 < 1f) {
                            handler._obsidianStopwatchTeleportLerpValue2 += 0.05f;
                        }
                    }
                    if (handler._currentTeleportPointIndex % 10 == 0) {
                        self.GetCommon().MakeCopy(0.625f);
                    }
                    handler._obsidianStopwatchTeleportLerpValue += handler._obsidianStopwatchTeleportLerpValue2;
                    if (handler._obsidianStopwatchTeleportLerpValue > 1f) {
                        handler._currentTeleportPointIndex++;
                        handler._advancedShadows[num].Position = Vector2.Zero;
                        handler._obsidianStopwatchTeleportLerpValue = 0f;
                    }
                }
            }

            return;
        }

        if (handler.IsObsidianStopwatchTeleportAvailable || handler.ShouldUpdateAdvancedShadows) {
            handler.UpdateAdvancedShadows();
        }
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
        if (CollidedWithStarwayWormhole) {
            foreach (var layer in PlayerDrawLayerLoader.Layers) {
                layer.Hide();
            }
        }
    }

    public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
        int count = drawInfo.DrawDataCache.Count;

        if (CollidedWithStarwayWormhole) {
            for (int i = 0; i < count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.color *= 0f;
                drawInfo.DrawDataCache[i] = value;
            }
        }

        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatform>(checkProjectile => !checkProjectile.SameOwnerAs(Player))) {
            for (int i = 0; i < count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.position += projectile.velocity;
                drawInfo.DrawDataCache[i] = value;
            }
        }
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CloudPlatformAngry>(checkProjectile => !checkProjectile.SameOwnerAs(Player))) {
            for (int i = 0; i < count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.position += projectile.velocity;
                drawInfo.DrawDataCache[i] = value;
            }
        }

        if (drawInfo.drawPlayer.active && _copyIndexIAmDrawing != 255) {
            for (int i = 0; i < count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.color *= MathUtils.Clamp01(_copyData[_copyIndexIAmDrawing].Opacity);
                drawInfo.DrawDataCache[i] = value;
            }
        }

        if (_naturePriestCapeDrawing) {
            for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.color = value.color.MultiplyRGBA(WreathHandler.GetCurrentColor(drawInfo.drawPlayer).MultiplyAlpha(0f));
                drawInfo.DrawDataCache[i] = value;
            }
        }

        if (!_drawingObsidianStopwatchCopies) {
            return;
        }

        drawInfo.drawPlayer.armorEffectDrawOutlines = false;
        drawInfo.drawPlayer.armorEffectDrawShadow = false;
        drawInfo.drawPlayer.armorEffectDrawShadowSubtle = false;

        for (int i = 0; i < count; i++) {
            float progress = i / (float)count;
            DrawData value = drawInfo.DrawDataCache[i];
            float offset = drawInfo.drawPlayer.whoAmI + _obsidianStopwatchCopiesHueShift[_currentObsidianStopwatchCopyIndex] * 0.1f * 0.5f;
            float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
            Color color = Main.hslToRgb(hue, 1f, 0.5f);
            if (drawInfo.drawPlayer.GetModPlayer<SmallMoonPlayer>().HasContributor) {
                color = Color.Lerp(drawInfo.drawPlayer.GetModPlayer<SmallMoonPlayer>().smallMoonColor, drawInfo.drawPlayer.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
            }
            color.A = 25;
            color *= 0.5f;
            value.color = value.color.MultiplyRGBA(color) * drawInfo.drawPlayer.GetCommon().ObsidianStopwatchEffectOpacity;
            value.scale *= Helper.Wave(1.1f, 1.2f, 5f, offset);
            value.scale *= 1.5f * progress;
            value.color *= 0.5f;
            value.shader = 0;
            drawInfo.DrawDataCache[i] = value;
        }
    }

    public override void DrawPlayer(Camera camera) {
        if (NaturePriestCapeImmunityFrames > 0) {
            _naturePriestCapeDrawing = true;

            Vector2 vector2 = Player.position;
            float factor = 1f - NaturePriestCapeImmunityFrames / 20f;
            Main.PlayerRenderer.DrawPlayer(camera, Player, vector2, Player.fullRotation, Player.fullRotationOrigin, factor, 1f + factor * 0.5f);
        }

        _naturePriestCapeDrawing = false;

        for (int i = 0; i < MAXCOPIES; i++) {
            CopyInfo copyInfo = _copyData[i];
            Vector2 drawPosition = Player.position;
            if (copyInfo.Opacity <= 0f) {
                continue;
            }
            if (MathUtils.Approximately(copyInfo.Position, drawPosition, 2f)) {
                continue;
            }

            int direction = Player.direction;
            float gravDir = Player.gravDir;
            Rectangle bodyFrame = Player.bodyFrame;
            float rotation = Player.fullRotation;

            Player player = Player;

            player.direction = copyInfo.Direction;
            player.gravDir = copyInfo.GravityDirection;
            player.UseBodyFrame((Core.Data.PlayerFrame)copyInfo.BodyFrameIndex);
            player.fullRotation = copyInfo.Rotation;

            _copyIndexIAmDrawing = (byte)i;

            Main.PlayerRenderer.DrawPlayer(camera, player, copyInfo.Position, copyInfo.Rotation, copyInfo.Origin, 0f, copyInfo.Scale);

            player.direction = direction;
            player.gravDir = gravDir;
            player.bodyFrame = bodyFrame;
            player.fullRotation = rotation;
        }

        _copyIndexIAmDrawing = 255;

        void drawObsidianStopwatchEffect() {
            if (!IsObsidianStopwatchEffectActive) {
                return;
            }

            if (!IsObsidianStopwatchTeleportAvailable2) {
                return;
            }

            if (IsObsidianStopwatchEffectActive_Hidden && !_isTeleportingBackViaObisidianStopwatch) {
                return;
            }

            int totalShadows = Math.Min(availableAdvancedShadowsCount, 60);

            totalShadows = Math.Clamp(totalShadows, 0, 100);

            _obsidianStopwatchCopiesHueShift = [];
            _currentObsidianStopwatchCopyIndex = 0;
            _drawingObsidianStopwatchCopies = true;

            int skip = 1;
            for (int i = 0; i < totalShadows; i += skip) {
                _obsidianStopwatchCopiesHueShift.Add((ushort)i);
            }

            int direction = Player.direction;
            float gravDir = Player.gravDir;
            Rectangle bodyFrame = Player.bodyFrame;
            float rotation = Player.fullRotation;
            Rectangle legFrame = Player.legFrame;
            for (int i = totalShadows - totalShadows % skip; i > 0; i -= skip) {
                EntityShadowInfo advancedShadow = GetAdvancedShadow(i);
                float shadow = Utils.Remap((float)i / totalShadows, 0f, 1f, 0.15f, 0.5f, clamped: true);

                if (advancedShadow.Position != Vector2.Zero) {
                    Player player = Player;
                    player.direction = advancedShadow.Direction;
                    player.gravDir = advancedShadow.GravityDirection;
                    player.UseBodyFrame((Core.Data.PlayerFrame)advancedShadow.BodyFrameIndex);
                    player.fullRotation = advancedShadow.Rotation;
                    player.legFrame.Y *= 0;

                    Main.PlayerRenderer.DrawPlayer(camera, player, advancedShadow.Position, advancedShadow.Rotation, advancedShadow.Origin, shadow, 1f);
                }

                _currentObsidianStopwatchCopyIndex++;
            }
            Player.direction = direction;
            Player.gravDir = gravDir;
            Player.bodyFrame = bodyFrame;
            Player.fullRotation = rotation;
            Player.legFrame = legFrame;

            _drawingObsidianStopwatchCopies = false;
        }

        void drawTempBufferEffect() {
            if (TempBufferDodgeAnimationCounter <= 0) {
                return;
            }

            Player drawPlayer = Player;

            _drawingTempBufferCopies = true;
            _tempBufferCopiesHueShift = [];
            _currentTempBufferCopyIndex = 0;

            Vector2 vector2 = drawPlayer.position + new Vector2(0f, drawPlayer.gfxOffY);
            float lerpValue = Utils.GetLerpValue(300f, 270f, TempBufferDodgeAnimationCounter);
            float y = MathHelper.Lerp(2f, 100f, lerpValue);
            if (lerpValue >= 0f && lerpValue <= 1f) {
                for (float num12 = 0f; num12 < (float)Math.PI * 0.5f; num12 += (float)Math.PI / 10f) {
                    _tempBufferCopiesHueShift.Add(num12 * 2f);
                }
                for (float num12 = 0f; num12 < (float)Math.PI * 0.5f; num12 += (float)Math.PI / 10f) {
                    Vector2 position = vector2 + Vector2.UnitY * y * 0.5f + new Vector2(0f, y).RotatedBy((float)Math.PI * 0.8f + num12);
                    Main.PlayerRenderer.DrawPlayer(camera, drawPlayer, position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, lerpValue);

                    _currentTempBufferCopyIndex++;
                }
            }

            _drawingTempBufferCopies = false;
        }

        drawTempBufferEffect();
        if (!Common.ReflectionTarget.isDrawReflectablesThisFrame) {
            drawObsidianStopwatchEffect();
        }
    }

    private Color On_Player_GetImmuneAlphaPure(On_Player.orig_GetImmuneAlphaPure orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, result.MultiplyRGB(Color.Gray * 1f), self.GetCommon().FriarLanternEffectStrength);
        }
        if (self.GetCommon().ScrapRingStrength > 0f) {
            result = Color.Lerp(result, result.MultiplyRGB(self.GetCommon().ScrapRingLiquidColor * 1f), self.GetCommon().ScrapRingStrength * 0.5f);
        }
        if (_drawingTempBufferCopies) {
            float opacity = Utils.GetLerpValue(270f, 300f, self.GetCommon().TempBufferDodgeAnimationCounter, true);
            Color color = Main.hslToRgb(0.7f + (float)Math.Sin((float)Math.PI * 2f * TimeSystem.TimeForVisualEffects * 0.16f
                + _tempBufferCopiesHueShift[_currentTempBufferCopyIndex]), 1f, 0.5f);
            color.A /= 4;
            result = Color.Lerp(result, color, 0.25f) * opacity;
        }
        //if (!self.GetCommon().IsObsidianStopwatchEffectActive_Hidden || self.GetCommon()._isTeleportingBackViaObisidianStopwatch)
        {
            if (self.GetCommon()._isTeleportingBackViaObisidianStopwatch || self.GetCommon().IsObsidianStopwatchTeleportAvailable2) {
                float offset = self.whoAmI + (!_drawingObsidianStopwatchCopies ? 0f : (_obsidianStopwatchCopiesHueShift[0] * 0.1f * 0.5f));
                float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
                Color color = Main.hslToRgb(hue, 1f, 0.5f);
                if (self.GetModPlayer<SmallMoonPlayer>().HasContributor) {
                    color = Color.Lerp(self.GetModPlayer<SmallMoonPlayer>().smallMoonColor, self.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
                }
                color.A = 25;
                //color *= 0.5f;
                Color color2 = result.MultiplyRGB(color);
                result = Color.Lerp(result, color2, (!self.GetCommon()._isTeleportingBackViaObisidianStopwatch ? 0.25f : 0.5f) * self.GetCommon().ObsidianStopwatchEffectOpacity);
            }
        }
        return result;
    }

    private Color On_Player_GetImmuneAlpha(On_Player.orig_GetImmuneAlpha orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, result.MultiplyRGB(Color.Gray * 1f), self.GetCommon().FriarLanternEffectStrength);
        }
        if (self.GetCommon().ScrapRingStrength > 0f) {
            result = Color.Lerp(result, result.MultiplyRGB(self.GetCommon().ScrapRingLiquidColor * 1f), self.GetCommon().ScrapRingStrength * 0.25f);
        }
        if (_drawingTempBufferCopies) {
            float opacity = Utils.GetLerpValue(270f, 300f, self.GetCommon().TempBufferDodgeAnimationCounter, true);
            Color color = Main.hslToRgb(0.7f + (float)Math.Sin((float)Math.PI * 2f * TimeSystem.TimeForVisualEffects * 0.16f 
                + _tempBufferCopiesHueShift[_currentTempBufferCopyIndex]), 1f, 0.5f);
            color.A /= 4;
            result = Color.Lerp(result, color, 0.25f) * opacity;
        }
        //if (!self.GetCommon().IsObsidianStopwatchEffectActive_Hidden || self.GetCommon()._isTeleportingBackViaObisidianStopwatch)
        {
            if (self.GetCommon()._isTeleportingBackViaObisidianStopwatch || self.GetCommon().IsObsidianStopwatchTeleportAvailable2) {
                float offset = self.whoAmI + (!_drawingObsidianStopwatchCopies ? 0f : (_obsidianStopwatchCopiesHueShift[0] * 0.1f * 0.5f));
                float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
                Color color = Main.hslToRgb(hue, 1f, 0.5f);
                if (self.GetModPlayer<SmallMoonPlayer>().HasContributor) {
                    color = Color.Lerp(self.GetModPlayer<SmallMoonPlayer>().smallMoonColor, self.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
                }
                color.A = 25;
                //color *= 0.5f;
                Color color2 = result.MultiplyRGB(color);
                result = Color.Lerp(result, color2, (!self.GetCommon()._isTeleportingBackViaObisidianStopwatch ? 0.25f : 0.5f) * self.GetCommon().ObsidianStopwatchEffectOpacity);
            }
        }
        return result;
    }

    private bool On_Player_AddBuff_TryUpdatingExistingBuffTime(On_Player.orig_AddBuff_TryUpdatingExistingBuffTime orig, Player self, int type, int time) {
        bool result = orig(self, type, time);
        if (result) {
            if (self.GetCommon().IsBrawlerMaskEffectActive) {
                self.GetCommon().BeforeHitActiveDebuffs.Add((type, time));
            }
        }
        return result;
    }

    private bool On_Player_AddBuff_ActuallyTryToAddTheBuff(On_Player.orig_AddBuff_ActuallyTryToAddTheBuff orig, Player self, int type, int time) {
        bool result = orig(self, type, time);
        if (result && type == BuffID.PotionSickness) {
            foreach (Player player in Main.ActivePlayers) {
                if (player.whoAmI == self.whoAmI) {
                    continue;
                }
                if (player.GetCommon().IsElderShieldEffectActive) {
                    if (self.team == player.team && player.team != 0) {
                        float num = player.position.X - self.position.X;
                        float num2 = player.position.Y - self.position.Y;
                        if ((float)Math.Sqrt(num * num + num2 * num2) < 800f)
                            player.AddBuff<SnailBuff>(ElderShell.BUFFTIME);
                    }
                }
            }
            if (self.GetCommon().IsElderShellEffectActive) {
                self.AddBuff<SnailBuff>(ElderShell.BUFFTIME);
            }
            if (self.GetCommon().IsFossilizedSpiralEffectActive) {
                self.AddBuff<TempBuffer>(ElderShell.BUFFTIME);
            }
        }
        if (self.GetCommon().IsBrawlerMaskEffectActive) {
            self.GetCommon().BeforeHitActiveDebuffs.Add((type, time));
        }
        return result;
    }

    private void On_Player_ManaEffect(On_Player.orig_ManaEffect orig, Player self, int manaAmount) {
        if (self.GetCommon().DontShowManaEffect) {
            return;
        }

        if (self.GetCommon().ManaIncrease > 0) {
            manaAmount = self.GetCommon().ManaIncrease;
            self.GetCommon().ManaIncrease = 0;
        }

        orig(self, manaAmount);
    }

    private void On_Player_HealEffect(On_Player.orig_HealEffect orig, Player self, int healAmount, bool broadcast) {
        if (self.GetCommon().DontShowHealEffect) {
            return;
        }

        if (self.GetCommon().LifeIncrease > 0) {
            healAmount = self.GetCommon().LifeIncrease;
            self.GetCommon().LifeIncrease = 0;
        }

        orig(self, healAmount, broadcast);
    }

    private Item On_Player_PickupItem(On_Player.orig_PickupItem orig, Player self, int playerIndex, int worldItemArrayIndex, Item itemToPickUp) {
        int healthBefore = self.statLife;
        int manaBefore = self.statMana;
        int[] hearts = [ItemID.Heart, ItemID.CandyApple, ItemID.CandyCane];
        int[] manaStars = [ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum, 4143];
        Item itemToPickUp2 = itemToPickUp;
        if (self.GetCommon().ExtraLifeFromHeartsModifier != 1f) {
            self.GetCommon().DontShowHealEffect = true;
        }
        if (self.GetCommon().ExtraManaFromStarsModifier != 1f) {
            self.GetCommon().DontShowManaEffect = true;
        }
        Item result = orig(self, playerIndex, worldItemArrayIndex, itemToPickUp);
        if (self.GetCommon().DontShowHealEffect || self.GetCommon().DontShowManaEffect) {
            if (!itemToPickUp2.IsEmpty()) {
                if (self.GetCommon().DontShowHealEffect && hearts.Contains(itemToPickUp2.type)) {
                    int increase = self.statLife - healthBefore;
                    self.statLife -= increase;
                    self.GetCommon().LifeIncrease = (int)(increase * self.GetCommon().ExtraLifeFromHeartsModifier);
                    self.statLife += self.GetCommon().LifeIncrease;
                    self.GetCommon().DontShowHealEffect = false;
                    if (Main.myPlayer == self.whoAmI && self.GetCommon().LifeIncrease > 0) {
                        self.HealEffect(self.GetCommon().LifeIncrease - increase);
                    }
                    if (self.statLife > self.statLifeMax2) {
                        self.statLife = self.statLifeMax2;
                    }
                    if (self.GetCommon().IsHoneyPunchEffectActive) {
                        self.AddBuff(BuffID.Honey, MathUtils.SecondsToFrames(10));
                    }
                }
                if (self.GetCommon().DontShowManaEffect && manaStars.Contains(itemToPickUp2.type)) {
                    int increase = self.statMana - manaBefore;
                    self.statMana -= increase;
                    self.GetCommon().ManaIncrease = (int)(increase * self.GetCommon().ExtraManaFromStarsModifier);
                    self.statMana += self.GetCommon().ManaIncrease;
                    self.GetCommon().DontShowManaEffect = false;
                    if (Main.myPlayer == self.whoAmI && self.GetCommon().ManaIncrease > 0) {
                        self.ManaEffect(self.GetCommon().ManaIncrease - increase);
                    }
                    if (self.statMana > self.statManaMax2) {
                        self.statMana = self.statManaMax2;
                    }
                }
            }
        }
        return result;
    }

    private void On_PlayerDrawSet_BoringSetup_21(On_PlayerDrawSet.orig_BoringSetup_2 orig, ref PlayerDrawSet self, Player player, System.Collections.Generic.List<DrawData> drawData, System.Collections.Generic.List<int> dust, System.Collections.Generic.List<int> gore, Vector2 drawPosition, float shadowOpacity, float rotation, Vector2 rotationOrigin) {
        bool reset = false;
        if (player.GetCommon().IsAetherInvincibilityActive) {
            player.noItems = false;
            reset = true;
        }

        orig(ref self, player, drawData, dust, gore, drawPosition, shadowOpacity, rotation, rotationOrigin);

        if (reset) {
            player.noItems = true;
        }
    }


    public delegate void AlwaysHeadDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event AlwaysHeadDrawDelegate AlwaysHeadDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_21_Head(On_PlayerDrawLayers.orig_DrawPlayer_21_Head orig, ref PlayerDrawSet drawinfo) {
        if (!(drawinfo.drawPlayer.GetCommon().StopHeadDrawing ||
            drawinfo.drawPlayer.face == EquipLoader.GetEquipSlot(RoA.Instance, nameof(HornetSkull), EquipType.Face))) {
            orig(ref drawinfo);
        }

        AlwaysHeadDrawEvent?.Invoke(ref drawinfo);
    }

    public partial void CursorEffectsLoad();

    private Item On_ItemSlot_ArmorSwap(On_ItemSlot.orig_ArmorSwap orig, Item item, out bool success) {
        bool hornetSkull = item.type == ModContent.ItemType<HornetSkull>() && Main.LocalPlayer.GetCommon().ApplyHornetSkullSetBonus;
        bool devilSkull = item.type == ModContent.ItemType<DevilSkull>() && Main.LocalPlayer.GetCommon().ApplyDevilSkullSetBonus;
        bool vanillaSkull = item.type == ItemID.Skull && Main.LocalPlayer.GetCommon().ApplyVanillaSkullSetBonus;
        bool crystallizedSkull = item.type == ModContent.ItemType<CrystallizedSkull>() && Main.LocalPlayer.GetCommon().ApplyCrystallizedSkullSetBonus;
        bool deerSkull = item.type == ModContent.ItemType<DeerSkull>() && Main.LocalPlayer.GetCommon().ApplyDeerSkullSetBonus;
        if (hornetSkull || devilSkull || vanillaSkull || crystallizedSkull || deerSkull) {
            success = false;
            return item;
        }

        return orig(item, out success);
    }


    private int On_ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
        int result = orig(inv, context, slot, checkItem);
        bool hornetSkull = checkItem.type == ModContent.ItemType<HornetSkull>() && Main.LocalPlayer.GetCommon().ApplyHornetSkullSetBonus;
        bool devilSkull = checkItem.type == ModContent.ItemType<DevilSkull>() && Main.LocalPlayer.GetCommon().ApplyDevilSkullSetBonus;
        bool vanillaSkull = checkItem.type == ItemID.Skull && Main.LocalPlayer.GetCommon().ApplyVanillaSkullSetBonus;
        bool crystallizedSkull = checkItem.type == ModContent.ItemType<CrystallizedSkull>() && Main.LocalPlayer.GetCommon().ApplyCrystallizedSkullSetBonus;
        bool deerSkull = checkItem.type == ModContent.ItemType<DeerSkull>() && Main.LocalPlayer.GetCommon().ApplyDeerSkullSetBonus;
        if (result == 1 &&
            (hornetSkull || devilSkull || vanillaSkull || crystallizedSkull || deerSkull)) {
            result = -1;
        }
        return result;
    }

    private void On_PlayerDrawLayers_DrawPlayer_21_Head_TheFace(On_PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo) {
        if (drawinfo.drawPlayer.GetCommon().StopFaceDrawing) {
            return;
        }

        orig(ref drawinfo);
    }

    private void On_Player_SetArmorEffectVisuals(On_Player.orig_SetArmorEffectVisuals orig, Player self, Player drawPlayer) {
        orig(self, drawPlayer);
        ApplySkullEffect(self, drawPlayer);
    }

    public partial void WiresLoad();

    public partial void DevilSkullLoad();
    public partial void CrystallizedSkullLoad();

    private void On_Player_HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player self) {
        if (self.GetCommon().LockHorizontalMovement) {
            return;
        }

        orig(self);
    }

    private void PlayerCommon_DrawPlayerFullEvent(LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
        if (!drawPlayer.GetCommon().ApplyBoneArmorVisuals) {
            return;
        }

        drawPlayer.armorEffectDrawOutlines = false;
        drawPlayer.armorEffectDrawShadow = false;
        drawPlayer.armorEffectDrawShadowSubtle = false;

        _ = drawPlayer.position;
        if (!Main.gamePaused)
            drawPlayer.ghostFade += drawPlayer.ghostDir * 0.025f;

        if ((double)drawPlayer.ghostFade < 0.75f) {
            drawPlayer.ghostDir = 1f;
            drawPlayer.ghostFade = 0.75f;
        }
        else if ((double)drawPlayer.ghostFade > 0.9) {
            drawPlayer.ghostDir = -1f;
            drawPlayer.ghostFade = 0.9f;
        }

        float num2 = 5f;
        for (int l = 0; l < 4; l++) {
            float num3;
            float num4;
            switch (l) {
                default:
                    num3 = num2;
                    num4 = 0f;
                    break;
                case 1:
                    num3 = 0f - num2;
                    num4 = 0f;
                    break;
                case 2:
                    num3 = 0f;
                    num4 = 0f;
                    break;
                case 3:
                    num3 = 0f;
                    num4 = 0f;
                    break;
            }

            float num165 = (1.2f + 0.2f * (float)Math.Cos(TimeSystem.TimeForVisualEffects % 30f / 2.5f * ((float)Math.PI * 2f) * 3f)) * 0.8f;
            Vector2 position = new Vector2(drawPlayer.position.X + num3 * num165, drawPlayer.position.Y + drawPlayer.gfxOffY + num4);
            self.DrawPlayer(camera, drawPlayer, position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, drawPlayer.ghostFade, MathF.Max(1.1f, (num165 + 0.2f) * 0.9f));
        }
    }

    public delegate void PreUpdateMovementDelegate(Player player);
    public static event PreUpdateMovementDelegate PreUpdateMovementEvent;
    public override void PreUpdateMovement() {
        PreUpdateMovementEvent?.Invoke(Player);

        ScrapRingWet = false;
        if (IsScrapRingEffectActive) {
            if (Player.wet) {
                if (Player.GetModdedWetArray()[LiquidLoader.LiquidType<Content.Liquids.Tar>() - LiquidID.Count]) {
                    ScrapRingLiquidColor = new Color(48, 37, 49);
                }
                else if (Player.honeyWet) {
                    ScrapRingLiquidColor = new Color(250, 155, 16);
                }
                else if (Player.shimmerWet) {
                    ScrapRingLiquidColor = WreathHandler.AetherColor;
                }
                else if (Player.lavaWet) {
                    ScrapRingLiquidColor = new Color(245, 33, 7);
                }
                else {
                    ScrapRingLiquidColor = new Color(13, 80, 195);
                }
                ScrapRingWet = true;
            }

            Player.wetCount = 10;

            Player.lavaWet = false;
            Player.shimmerWet = false;
            Player.honeyWet = false;
            Player.wet = false;
        }

        {
            if (!CollidedWithFungalMushroom && Player.velocity.Y > Player.gravity) {
                foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<FungalCaneMushroom>()) {
                    if (Player.getRect().Intersects(projectile.getRect())) {
                        if (projectile.ai[0] < 0f || projectile.ai[1] < 1f) {
                            continue;
                        }
                        CollidedWithFungalMushroom = true;
                        FungalMushroomICollidedWith = projectile;
                        break;
                    }
                }
            }
            if (CollidedWithFungalMushroom) {
                bool flag = FungalMushroomICollidedWith.ai[0] < 0f;
                if (!FungalMushroomICollidedWith.active || flag) {
                    if (flag) {
                        Player.velocity.Y = -10f;
                    }
                    CollidedWithFungalMushroom = false;
                    return;
                }
                FungalMushroomICollidedWith.ai[2] = 1f;
                Player.position.Y = MathHelper.Lerp(Player.position.Y, FungalMushroomICollidedWith.Center.Y - Player.height - Player.height * 0.5f, 0.25f);
                Player.fallStart = (int)(Player.position.Y / 16f);
                Player.gravity = 0f;
            }
        }
        {
            if (CollidedWithCottonBoll) {
                if (!CottonBollICollidedWith.active || CottonBollICollidedWith.ai[1] <= 0f || CottonBollICollidedWith.ai[2] >= 1f) {
                    CollidedWithCottonBoll = false;
                    Player.shimmering = false;
                    Player.velocity.Y = -10f;
                    return;
                }

                if (Player.IsLocal()) {
                    Main.SetCameraLerp(0.25f, 0);
                }

                Player.shimmering = true;
                Player.shimmerTransparency = 0f;

                Player.velocity.Y = 1f;

                Player.position = Vector2.Lerp(Player.position, CottonBollICollidedWith.Center - Player.Size / 2f + Vector2.UnitY * 10f, MathUtils.Clamp01(0.01f + (CottonBollICollidedWith.velocity.Y < 0f ? MathF.Abs(CottonBollICollidedWith.velocity.Y * 0.1f) : 0f)));
                Player.fallStart = (int)(Player.position.Y / 16f);
                Player.gravity = 0f;
            }
        }
        {
            int checkWidth = Player.width * 4;
            if (!CollidedWithStarwayWormhole) {
                if (WormholeCooldown <= 0f) {
                    foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<StarwayWormhole>()) {
                        StarwayWormhole starwayWormhole = projectile.As<StarwayWormhole>();
                        if (!starwayWormhole.Init || starwayWormhole.Used) {
                            continue;
                        }
                        Rectangle getRect = GeometryUtils.CenteredSquare(Player.GetPlayerCorePoint(), checkWidth);
                        if (getRect.Contains(starwayWormhole.StartPosition.ToPoint())) {
                            CollideWithStarwayWormhole(starwayWormhole);
                            break;
                        }
                        if (getRect.Contains(starwayWormhole.LastPosition.ToPoint())) {
                            CollideWithStarwayWormhole(starwayWormhole, true);
                            break;
                        }
                    }
                }
                else {
                    WormholeCooldown = Helper.Approach(WormholeCooldown, 0f, 1f);
                }
            }
            if (CollidedWithStarwayWormhole) {
                Player.controlUseItem = false;

                float maxProgress = 1.5f;
                float progress = WormholeAdventureProgress;
                if (!WormholeAdventureReversed) {
                    progress = 1f - progress;
                }
                progress = MathUtils.Clamp01(progress);
                StarwayWormhole starwayWormhole = StarwayWormholeICollidedWith.As<StarwayWormhole>();
                List<Vector2> wormholePositions = starwayWormhole.GetPositionsForAdventure2();
                Vector2 targetPosition;
                float exactIndex = (wormholePositions.Count - 1) * progress;
                int index1 = (int)Math.Floor(exactIndex);
                int index2 = Math.Min(index1 + 1, wormholePositions.Count - 1);
                float lerpFactor = exactIndex - index1;
                targetPosition = Vector2.Lerp(
                    wormholePositions[index1],
                    wormholePositions[index2],
                    lerpFactor
                );
                Vector2 to = targetPosition - Player.Size / 2;

                bool completed = WormholeAdventureProgress >= maxProgress * 0.7f && Player.Distance(WormholeAdventureReversed ? wormholePositions[^1] : wormholePositions[0]) < checkWidth;

                if (!StarwayWormholeICollidedWith.active || WormholeAdventureProgress >= maxProgress || completed) {
                    CollidedWithStarwayWormhole = false;
                    Player.shimmering = false;
                    WormholeCooldown = 10f;
                    Player.velocity = Player.position.DirectionTo(to) * 10f;
                    return;
                }

                Player.velocity = Player.position.DirectionTo(to) * 10f;

                ShouldDrawProjectileOverArm = false;

                //if (Player.IsLocal()) {
                //    Main.SetCameraLerp(0.15f, 0);
                //}

                Player.shimmering = true;
                Player.shimmerTransparency = 0f;

                Player.position = Vector2.Lerp(Player.position, to, 0.25f);

                Player.fallStart = (int)(Player.position.Y / 16f);
                Player.gravity = 0f;

                WormholeAdventureProgress = Helper.Approach(WormholeAdventureProgress, maxProgress, 0.025f);
            }
        }
    }

    public void CollideWithCottonBall(CottonBoll cottonBoll) {
        CottonBollICollidedWith = cottonBoll.Projectile;
        CollidedWithCottonBoll = true;
    }

    public void CollideWithStarwayWormhole(StarwayWormhole starwayWormhole, bool reversed = false) {
        StarwayWormholeICollidedWith = starwayWormhole.Projectile;
        CollidedWithStarwayWormhole = true;
        WormholeAdventureProgress = 0f;
        WormholeAdventureReversed = reversed;
        if (Player.IsLocal()) {
            StarwayWormholeICollidedWith.ai[2] = 1f;
            StarwayWormholeICollidedWith.netUpdate = true;
        }
    }

    public delegate void PostUpdateEquipsDelegate(Player player);
    public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
    public override void PostUpdateEquips() {
        ScrapRingStrength = Helper.Approach(ScrapRingStrength, ScrapRingWet.ToInt(), 0.2f);

        UpdateCrystallineNeedles();

        ArchiveUseCooldownInTicks = (ushort)Helper.Approach(ArchiveUseCooldownInTicks, 0, 1);

        PostUpdateEquipsEvent?.Invoke(Player);

        DeerSkullPostUpdateEquips();

        if (TempBufferDodgeAnimationCounter > 0) {
            TempBufferDodgeAnimationCounter--;
        }

        if (ElderSnailSlow) {
            Player.moveSpeed /= 3f;
            if (Player.velocity.Y == 0f && Math.Abs(Player.velocity.X) > 1f) {
                Player.velocity.X /= 2f;
            }

            Player.slowOgreSpit = Player.dazed = Player.slow = Player.chilled = false;
        }

        if (IsFallenLeavesEffectActive) {
            FallenLeavesCounter += 1f * Player.GetWreathHandler().ActualProgress4;
            if (FallenLeavesCounter > FallenLeaves.ATTACKTIME) {
                FallenLeavesCounter = FallenLeaves.ATTACKTIME;
            }
        }
        else {
            FallenLeavesCounter = 0f;
        }

        if (Player.IsStandingStillForSpecialEffects && Player.IsGrounded() && WorldGenHelper.CustomSolidCollision(Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms)) {
            StandingStillTimer++;
        }
        else {
            StandingStillTimer = 0f;
        }

        if (IsObsidianStopwatchEffectActive) {
            if (_obsidianStopwatchTeleportCooldown > 0) {
                _obsidianStopwatchTeleportCooldown--;
            }
        }
        else if (!ShouldUpdateAdvancedShadows) {
            ResetAdvancedShadows();
        }

        Vector2 position = Player.Top + Vector2.UnitY * Player.height * 0.2f;
        if (!IsHereticsVeilEffectActive) {
            HereticVeilEffectOpacity = Helper.Approach(HereticVeilEffectOpacity, 1f, 0.15f);

            if (HereticVeilEffectOpacity < 1f) {
                for (int i = 0; i < 20; i++) {
                    Player player = Player;
                    Vector2 velocity = -Vector2.UnitY.RotatedBy(player.fullRotation);
                    int num1020 = Math.Sign(velocity.Y);
                    int num1021 = ((num1020 != -1) ? 1 : 0);
                    int num1030 = DustID.Smoke;
                    float num127 = Main.rand.NextFloat(0.75f, 1.25f);
                    num127 *= Main.rand.NextFloat(1.25f, 1.5f);
                    int width = 20;
                    int num131 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, num1030, 0f, 0f, 235, default, Main.rand.NextFloat(3f, 6f) * 1.375f * (1f - HereticVeilEffectOpacity));
                    Main.dust[num131].position = position + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(velocity.ToRotation()) * width / 3f;
                    Main.dust[num131].customData = num1021;
                    if (num1020 == -1 && Main.rand.Next(4) != 0)
                        Main.dust[num131].velocity.Y -= 0.2f;
                    Main.dust[num131].noGravity = true;
                    Dust dust2 = Main.dust[num131];
                    dust2.velocity *= 0.5f;
                    dust2 = Main.dust[num131];
                    dust2.velocity += position.DirectionTo(Main.dust[num131].position) * Main.rand.NextFloat(2f, 5f) * 0.8f;
                    dust2.velocity.Y += velocity.Y * Main.rand.NextFloat(2f, 5f) * 0.625f * 0.8f;
                    dust2.velocity *= 0.1f;
                }
            }
        }
        else if (Player.statLife <= 100) {
            HereticVeilEffectOpacity = Helper.Approach(HereticVeilEffectOpacity, 0f, 0.1f);
            Lighting.AddLight(position, Color.Lerp(new Color(255, 247, 147), new Color(255, 165, 53), 0.5f).ToVector3() * 1f);
            if (Main.rand.NextChance(1f - HereticVeilEffectOpacity) && Main.rand.NextBool(7)) {
                Dust dust = Dust.NewDustDirect(position, 4, 4, 6, 0f, 0f, 100);
                dust.position = position + new Vector2(Main.rand.NextFloatDirection() * 14f, Main.rand.NextFloatDirection() * 4f - 8f);
                if (Main.rand.Next(2) == 0) {
                    dust.noGravity = true;
                    dust.fadeIn = 1.15f;
                }
                else {
                    dust.scale = 0.6f;
                }

                dust.velocity *= 0.6f;
                dust.velocity.Y -= 1.2f;
                dust.position.Y -= 4f;
            }
        }

        if (IsConjurersEyeEffectActive && ConjurersEyeCanShoot) {
            if (ConjurersEyeAttackCounter++ > CONJURERSEYEATTACKTIME) {
                ConjurersEyeAttackCounter = 0;
                if (Player.IsLocal() && Player.MouthPosition.HasValue) {
                    Vector2 eyePosition = Player.MouthPosition.Value;
                    eyePosition -= Vector2.One * 4f * new Vector2(Player.direction * 1.25f, 1f);
                    Vector2 velocity = eyePosition.DirectionTo(Player.GetViableMousePosition()) * 27.5f;
                    int damage = 70;
                    float knockBack = 1.5f;
                    ProjectileUtils.SpawnPlayerOwnedProjectile<ConjurersEyeLaser>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("conjurerseye")) {
                        Position = eyePosition,
                        Velocity = velocity,
                        AI0 = eyePosition.X - Player.GetPlayerCorePoint().X,
                        AI1 = eyePosition.Y - Player.GetPlayerCorePoint().Y,
                        Damage = damage,
                        KnockBack = knockBack
                    });
                }
            }
        }

        if (IsDoubleGogglesEffectActive) {
            DoubleGogglesEffectOpacity = Helper.Approach(DoubleGogglesEffectOpacity, 1f, 0.1f);
        }
        else {
            DoubleGogglesEffectOpacity = Helper.Approach(DoubleGogglesEffectOpacity, 0f, 0.1f);
        }

        if (NaturePriestCapeImmunityFrames > 0) {
            NaturePriestCapeImmunityFrames--;
        }

        if (IsDuskStagEffectActive_Vanity) {
            DuskStagPosition += DuskStagVelocity;
            DuskStagVelocityFactor++;
            if (DuskStagVelocityFactor > 1) {
                DuskStagVelocity.X += (float)Main.rand.Next(-100, 101) * 0.0025f;
                DuskStagVelocity.Y += (float)Main.rand.Next(-100, 101) * 0.0025f;
                DuskStagVelocityFactor = 0;
            }
            float x = DuskStagPosition.X;
            float y = DuskStagPosition.Y;
            float num6 = (float)Math.Sqrt(x * x + y * y);
            if (num6 > 100f) {
                num6 = 10f / num6;
                x *= 0f - num6;
                y *= 0f - num6;
                int num7 = 5;
                DuskStagVelocity.X = MathHelper.Lerp(DuskStagVelocity.X, (DuskStagVelocity.X * (float)(num7 - 1) + x) / (float)num7, 0.25f);
                DuskStagVelocity.Y = MathHelper.Lerp(DuskStagVelocity.Y, (DuskStagVelocity.Y * (float)(num7 - 1) + y) / (float)num7, 0.25f);
            }
            else if (num6 > 30f) {
                num6 = 5f / num6;
                x *= 0f - num6;
                y *= 0f - num6;
                int num8 = 10;
                DuskStagVelocity.X = MathHelper.Lerp(DuskStagVelocity.X, (DuskStagVelocity.X * (float)(num8 - 1) + x) / (float)num8, 0.25f);
                DuskStagVelocity.Y = MathHelper.Lerp(DuskStagVelocity.Y, (DuskStagVelocity.Y * (float)(num8 - 1) + y) / (float)num8, 0.25f);
            }

            x = DuskStagVelocity.X;
            y = DuskStagVelocity.Y;
            num6 = (float)Math.Sqrt(x * x + y * y);
            if (num6 > 2f)
                DuskStagVelocity *= 0.95f;

            DuskStagPosition -= Player.velocity * 0.25f;

            if (MathF.Abs(DuskStagVelocity.X) > 0.1f) {
                DustStagDirection = DuskStagVelocity.X.GetDirection();
            }
        }
        else {
            DuskStagPosition = DuskStagVelocity = Vector2.Zero;
        }
    }

    public partial void DeerSkullPostUpdateEquips();

    public delegate void UpdateEquipsDelegate(Player player);
    public static event UpdateEquipsDelegate UpdateEquipsEvent;
    public override void UpdateEquips() {
        UpdateEquipsEvent?.Invoke(Player);
    }

    public delegate void FrameEffectsDelegate(Player player);
    public static event FrameEffectsDelegate FrameEffectsEvent;
    public override void FrameEffects() {
        FrameEffectsEvent?.Invoke(Player);
    }

    public delegate void PostUpdateRunSpeedsDelegate(Player player);
    public static event PostUpdateRunSpeedsDelegate PostUpdateRunSpeedsEvent;
    public override void PostUpdateRunSpeeds() {
        PostUpdateRunSpeedsEvent?.Invoke(Player);

        HandleBackflip();
        HandleHornetDash();

        FriarLanternEffectStrength = Helper.Approach(FriarLanternEffectStrength, IsFriarLanternBuffEffectActive.ToInt() * 0.625f, !IsFriarLanternBuffEffectActive ? 0.1f : 0.05f);
    }

    public void ResetSocialShadows() {
        for (int i = 0; i < Player.shadowDirection.Length; i++) {
            Player.shadowDirection[i] = 0;
        }
        Player.shadowCount = 0;
        for (int i = 0; i < Player.shadowPos.Length; i++) {
            Player.shadowPos[i] = Vector2.Zero;
        }
        for (int i = 0; i < Player.shadowOrigin.Length; i++) {
            Player.shadowOrigin[i] = Vector2.Zero;
        }
        for (int i = 0; i < Player.shadowRotation.Length; i++) {
            Player.shadowRotation[i] = 0f;
        }
    }

    private void HandleBackflip() {
        if (!DoingBackflip) {
            return;
        }

        if (Player.velocity.Y == 0f) {
            _backflipTimer = 0f;
        }

        _backflipTimer = Helper.Approach(_backflipTimer, 0f, 1f);

        Player.controlJump = false;

        Player.fullRotation = MathHelper.ToRadians(360f * BackflipProgress * -Player.direction);
        Player.fullRotationOrigin = Player.Size / 2f;
    }

    public delegate void DrawPlayerFullDelegate(LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer);
    public static event DrawPlayerFullDelegate DrawPlayerFullEvent;

    private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
        DrawPlayerFullEvent?.Invoke(self, camera, drawPlayer);

        orig(self, camera, drawPlayer);
    }

    public delegate void PreUpdateDelegate(Player player);
    public static event PreUpdateDelegate PreUpdateEvent;
    public override void PreUpdate() {
        PreUpdateEvent?.Invoke(Player);

        if (!DistilleryOfDeathInitialized) {
            DistilleryOfDeathInitialized = true;

            DistilleryOfDeathLastShootType_Next = DistilleryOfDeathLastShootType_Next_Next;
            DistilleryOfDeathLastShootType_Next_Next = Main.rand.GetRandomEnumValue<GustType>(1);
            while (DistilleryOfDeathLastShootType_Next_Next == DistilleryOfDeathLastShootType_Next ||
                   DistilleryOfDeathLastShootType_Next_Next == DistilleryOfDeathLastShootType_Current) {
                DistilleryOfDeathLastShootType_Next_Next = Main.rand.GetRandomEnumValue<GustType>(1);
            }

            DistilleryOfDeathLastShootType_Back1 = Main.rand.GetRandomEnumValue<GustType>(1);
            DistilleryOfDeathLastShootType_Back1_2 = DistilleryOfDeathLastShootType_Back1;
            while (DistilleryOfDeathLastShootType_Back1 == DistilleryOfDeathLastShootType_Back1_2) {
                DistilleryOfDeathLastShootType_Back1 = Main.rand.GetRandomEnumValue<GustType>(1);
            }
            DistilleryOfDeathLastShootType_Back2 = Main.rand.GetRandomEnumValue<GustType>(1);
            DistilleryOfDeathLastShootType_Back2_2 = DistilleryOfDeathLastShootType_Back2;
            while (DistilleryOfDeathLastShootType_Back2 == DistilleryOfDeathLastShootType_Back2_2) {
                DistilleryOfDeathLastShootType_Back2 = Main.rand.GetRandomEnumValue<GustType>(1);
            }
        }
    }

    public void ActivateNaturePriestCapeEffect() {
        if (NaturePriestCapeImmunityFrames > 0) {
            return;
        }
        NaturePriestCapeImmunityFrames = 20;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
        if (IsAetherInvincibilityActive) {
            modifiers.Cancel();
        }
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) {
        int? sourceProjectileType = damageSource.SourceProjectileType;
        if (sourceProjectileType.HasValue) {
            if (sourceProjectileType == ModContent.ProjectileType<SatchelChargeProjectile>()) {
                damageSource = PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.SatchelCharge{Main.rand.Next(2)}").ToNetworkText(Player.name));
            }
            if (sourceProjectileType == ModContent.ProjectileType<ConjurersEyeLaser>()) {
                damageSource = PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.ConjurersEyeLaser{Main.rand.Next(4)}").ToNetworkText(Player.name));
            }
        }

        return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
    }

    public delegate void OnHurtDelegate(Player player, Player.HurtInfo info);
    public static event OnHurtDelegate OnHurtEvent;
    public override void OnHurt(Player.HurtInfo info) {
        OnHurtEvent?.Invoke(Player, info);

        // need sync
        if (IsObsidianStopwatchEffectActive && IsObsidianStopwatchTeleportAvailable) {
            _isTeleportingBackViaObisidianStopwatch = true;
            _currentTeleportPointIndex = 0;

            ProjectileUtils.SpawnPlayerOwnedProjectile<ObsidianStopwatchClock>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_OnHurt(info.DamageSource)) with {
                Position = Player.GetPlayerCorePoint()
            });
        }

        // TODO: net sync
        if (IsBrawlerMaskEffectActive && Main.rand.NextBool(2)) {
            foreach (NPC nPC in Main.ActiveNPCs) {
                float num = TileHelper.TileSize * 31;
                if (nPC.CanBeChasedBy(Player) && !(Player.Distance(nPC.Center) > num)/* && Collision.CanHitLine(Player.position, Player.width, Player.height, nPC.position, nPC.width, nPC.height)*/) {
                    int num2 = (nPC.Center.X - Player.Center.X).GetDirection();
                    if (Player.whoAmI == Main.myPlayer)
                        Player.ApplyDamageToNPC(nPC, (int)info.Damage, info.Knockback, num2, crit: false);
                    foreach ((int, int) buffInfo in BeforeHitActiveDebuffs) {
                        nPC.AddBuff(buffInfo.Item1, buffInfo.Item2);
                    }
                }
            }
        }
        BeforeHitActiveDebuffs.Clear();
    }

    public delegate void OnHitNPCDelegate(Player player, NPC target, NPC.HitInfo hit, int damageDone);
    public static event OnHitNPCDelegate OnHitNPCEvent;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        OnHitNPCEvent?.Invoke(Player, target, hit, damageDone);

        if (IsGobletOfPainEffectActive && hit.Crit && Main.rand.NextBool(4)) {
            Player.AddBuff(BuffID.Poisoned, MathUtils.SecondsToFrames(3), quiet: false);

            ProjectileUtils.SpawnPlayerOwnedProjectile<GobletOfPainSplash>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Misc("gobletofpain")) {
                Position = Player.GetPlayerCorePoint()
            });
        }

        if (IsChromaticScarfEffectActive && hit.Crit) {
            Item heldItem = Player.GetSelectedItem();
            if (!heldItem.IsEmpty() && heldItem.IsAWeapon()) {
                if (heldItem.TryGetGlobalItem(out ChromaticScarfDebuffPicker modItem)) {
                    target.AddBuff(modItem.CurrentDebuff, Main.rand.Next(MathUtils.SecondsToFrames(3), MathUtils.SecondsToFrames(5) + 1));
                }
            }
        }
    }

    public override void OnHitAnything(float x, float y, Entity victim) {
        if (IsMaidensBracersEffectActive) {
            if (Player.IsLocal() && Main.rand.NextBool(10)) {
                Item sItem = Player.GetSelectedItem();
                if (!sItem.IsATool()) {
                    int num = sItem.damage;
                    num = Main.DamageVar(num, Player.luck);
                    int direction = 0;
                    PlayerDeathReason playerDeathReason = PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.MaidensBracers{Main.rand.Next(2)}").ToNetworkText(Player.name));
                    int result = (int)Player.Hurt(playerDeathReason, num, direction);
                    if (result > 0) {
                        ProjectileUtils.SpawnPlayerOwnedProjectile<MaidensBracersSpike>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_OnHurt(playerDeathReason)) with {
                            Position = Player.GetPlayerCorePoint()
                        });
                    }
                }
            }
        }
    }

    public delegate void ResetEffectsDelegate(Player player);
    public static event ResetEffectsDelegate ResetEffectsEvent;
    public override void ResetEffects() {
        ShouldDrawProjectileOverArm = true;

        IsFilamentBindingEffectActive = false;

        IsHoneyPunchEffectActive = false;

        IsBadgeOfHonorEffectActive = false;

        IsBlindFoldEffectActive = false;

        if (!Main.gameMenu) {
            IsEyePatchEffectActive_Hidden = false;
            IsEyePatchEffectActive = false;
        }

        IsScrapRingEffectActive = false;

        IsFeathersInABalloonEffectActive = false;
        IsFeathersInABottleEffectActive = false;

        IsThunderKingsGraceEffectActive = false;
        IsChainedCloudEffectActive = false;

        IsGardeningGlovesEffectActive = false;

        IsAriesActive = false;

        IsClarityEffectActive = false;

        ShouldUpdateAdvancedShadows = false;

        IsDuskStagEffectActive = false;
        IsDuskStagEffectActive_Vanity = false;

        IsFermentedSpiderEyeEffectActive = false;

        //IsNaturePriestCapeEffectActive = false;

        ShouldDrawVanillaBackpacks = true;

        IsBrawlerMaskEffectActive = false;

        IsDoubleGogglesEffectActive = false;
        IsDoubleGogglesEffectActive_Hidden = false;

        ConjurersEyeVanity = false;
        IsConjurersEyeEffectActive = IsConjurersEyeEffectActive_Hidden = false;

        IsChromaticScarfEffectActive = false;

        IsGobletOfPainEffectActive = false;

        IsSnakeIdolEffectActive = false;

        IsBansheesGuardEffectActive = false;

        IsHereticsVeilEffectActive = false;

        IsObsidianStopwatchEffectActive = IsObsidianStopwatchEffectActive_Hidden = false;

        IsSeedOfWisdomEffectActive = false;

        IsFossilizedSpiralEffectActive = false;

        IsMaidensBracersEffectActive = false;

        IsFallenLeavesEffectActive = false;

        DrawJokeVisor = false;

        IsFriarLanternBuffEffectActive = false;

        IsElderShellEffectActive = false;
        ElderSnailSlow = false;
        IsElderShieldEffectActive = false;

        if (IsAetherInvincibilityActive) {
            Player.shimmerTransparency += 0.03f;
            if (Player.shimmerTransparency > 0.5f)
                Player.shimmerTransparency = 0.5f;
        }
        else if (!Player.shimmering && Player.shimmerTransparency > 0f) {
            Player.shimmerTransparency -= 0.015f;
            if (Player.shimmerTransparency < 0f)
                Player.shimmerTransparency = 0f;
        }

        ResetEffectsEvent?.Invoke(Player);

        ApplyBoneArmorVisuals = false;
        LockHorizontalMovement = false;
        ApplyVanillaSkullSetBonus = false;
        ApplyDevilSkullSetBonus = false;
        ApplyCrystallizedSkullSetBonus = false;
        ApplyHornetSkullSetBonus = false;
        ApplyDeerSkullSetBonus = false;

        StopFaceDrawing = StopHeadDrawing = false;

        IsAetherInvincibilityActive = false;

        ExtraManaFromStarsModifier = ExtraLifeFromHeartsModifier = 1f;

        CursorEffectsResetEffects();
        DeerSkullResetEffects();
    }

    public partial void CursorEffectsResetEffects();
    public partial void DeerSkullResetEffects();

    public delegate void PreItemCheckDelegate(Player player);
    public static event PreItemCheckDelegate PreItemCheckEvent;
    public override bool PreItemCheck() {
        PreItemCheckEvent?.Invoke(Player);

        RodOfTheBifrostItemCheck(Player);

        if (Player.ItemAnimationEndingOrEnded && ItemUsed) {
            ItemUsed = false;
        }

        if (IsAetherInvincibilityActive) {
            Player.noItems = true;
        }

        //if (IsBrambleMazeUsed) {
        //    //Player.itemTime = Player.itemTimeMax - 1;
        //    //Player.itemAnimation = Player.itemAnimationMax - 1;

        //    if (IsBrambleMazePlaced || !Player.HasProjectile<BrambleMazeRootAir>()) {
        //        IsBrambleMazeUsed = false;
        //    }
        //}

        if (CollidedWithStarwayWormhole) {
            Player.channel = false;
            Player.itemAnimation = (Player.itemAnimationMax = 0);
            return false;
        }

        return base.PreItemCheck();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (!IsSnakeIdolEffectActive) {
            return;
        }

        int debuffCount = 0;
        for (int i = 0; i < NPC.maxBuffs; i++) {
            if (target.buffTime[i] >= 1 && target.buffType[i] > 0) {
                if (Main.debuff[target.buffType[i]] || Main.pvpBuff[target.buffType[i]]) {
                    debuffCount++;
                }
            }
        }
        int armorPenetration = 5 * debuffCount;
        modifiers.ArmorPenetration += armorPenetration;
    }

    public partial void RodOfTheBifrostItemCheck(Player player);

    public override void PostUpdateMiscEffects() {
        DevilSkullSetBonusPostMiscEffects();
    }

    public partial void DevilSkullSetBonusPostMiscEffects();

    public override void PostUpdate() {
        DefenseLastTick = Player.statDefense;

        if (Player.controlUseItem) {
            ControlUseItem = true;
        }
        else if (ControlUseItem) {
            if (_controlUseItemTimer++ > ControlUseItemTimeCheck) {
                ControlUseItem = false;
                _controlUseItemTimer = 0;
                ControlUseItemTimeCheck = CONTROLUSEITEMTIMECHECKBASE;
            }
        }

        TouchGround();

        for (int i = 0; i < MAXCOPIES; i++) {
            ref CopyInfo copyData = ref _copyData![i];
            if (copyData.Opacity > 0f) {
                //copyData.Scale -= 0.05f;
                copyData.Opacity -= 0.05f;
                copyData.Opacity = MathF.Max(0f, copyData.Opacity);
            }
        }

        if (IsAetherInvincibilityActive) {
            return;
        }

        float num7 = AetherShimmerAlpha;
        if (num7 > 0f) {
             num7 -= 0.05f;

            if (num7 < 0f)
                num7 = 0f;
        }

        AetherShimmerAlpha = num7;
    }

    private void TouchGround() {
        if (Player.velocity.Y >= Player.maxFallSpeed * MAXFALLSPEEDMODIFIERFORFALL && !_fell) {
            _fell = true;
        }
        if (_fell && Player.velocity.Y == 0f && !Fell) {
            Fell = true;
            _fell = false;
            _fellTimer = 10f;
            return;
        }
        if (MathF.Abs(Player.velocity.Y) > 1f) {
            Fell = false;
        }
        if (_fellTimer > 0f) {
            _fellTimer--;
        }
        else if (!_fell) {
            Fell = false;
        }
    }
}
