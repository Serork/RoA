using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Wreaths.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

    public static ushort CONTROLUSEITEMTIMECHECKBASE => 10;

    private static bool _drawingTempBufferCopies, _drawingObsidianStopwatchCopies;
    private static List<float> _tempBufferCopiesHueShift = null!;
    private static List<ushort> _obsidianStopwatchCopiesHueShift = null!;
    private static byte _currentTempBufferCopyIndex, _currentObsidianStopwatchCopyIndex;
    private static byte _copyIndexIAmDrawing;

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

    public bool IsObsidianStopwatchEffectActive;

    public bool StandingStill => StandingStillTimer > 0;

    public bool CanSpawnFallenLeavesBranch => FallenLeavesCounter >= FallenLeaves.ATTACKTIME;

    public bool DoingBackflip => _backflipTimer > 0f;
    public float BackflipProgress => Ease.CubeIn(_backflipTimer / BACKFLIPTIME);

    public bool IsObsidianStopwatchTeleportAvailable => IsObsidianStopwatchEffectActive && _obsidianStopwatchTeleportCooldown <= 0;
    public bool IsObsidianStopwatchTeleportAvailable2 => IsObsidianStopwatchEffectActive && (_obsidianStopwatchTeleportCooldown <= OBSIDIANSTOPWATCHBETWEENINTICKS || _obsidianStopwatchTeleportCooldown >= OBSIDIANSTOPWATCHCOOLDOWNINTICKS - OBSIDIANSTOPWATCHBETWEENINTICKS);
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
    }

    public override void LoadData(TagCompound tag) {
        PerfectClotActivated = tag.GetBool(RoA.ModName + nameof(PerfectClotActivated));
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

        On_Player.GetImmuneAlpha += On_Player_GetImmuneAlpha;
        On_Player.GetImmuneAlphaPure += On_Player_GetImmuneAlphaPure;

        On_Player.UpdateAdvancedShadows += On_Player_UpdateAdvancedShadows;
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
        //_advancedShadows[_lastAddedAvancedShadow].Position.Y += Player.gfxOffY;
    }

    public void ResetAdvancedShadows() {
        for (int i = 0; i < availableAdvancedShadowsCount; i++) {
            _advancedShadows[i].Position = Vector2.Zero;
        }
        availableAdvancedShadowsCount = 0;
    }

    public override void SetControls() {
        var handler = Player.GetCommon();
        if (handler._isTeleportingBackViaObisidianStopwatch) {
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

        if (handler.IsObsidianStopwatchTeleportAvailable) {
            handler.UpdateAdvancedShadows();
        }
    }

    public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
        int count = drawInfo.DrawDataCache.Count;
        if (drawInfo.drawPlayer.active && _copyIndexIAmDrawing != 255) {
            for (int i = 0; i < count; i++) {
                DrawData value = drawInfo.DrawDataCache[i];
                value.color *= MathUtils.Clamp01(_copyData[_copyIndexIAmDrawing].Opacity);
                drawInfo.DrawDataCache[i] = value;
            }
        }

        if (!_drawingObsidianStopwatchCopies) {
            return;
        }

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
            value.color = value.color.MultiplyRGBA(color)/* * drawInfo.drawPlayer.GetCommon().ObsidianStopwatchEffectOpacity*/;
            value.scale *= Helper.Wave(1.1f, 1.2f, 5f, offset);
            value.scale *= 1.5f * progress;
            drawInfo.DrawDataCache[i] = value;
        }
    }

    public override void DrawPlayer(Camera camera) {
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
                float shadow = Utils.Remap((float)i / totalShadows, 0, 1, 0.15f, 0.5f, clamped: true);

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
        drawObsidianStopwatchEffect();
    }

    private Color On_Player_GetImmuneAlphaPure(On_Player.orig_GetImmuneAlphaPure orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, Color.Gray * 0.25f, self.GetCommon().FriarLanternEffectStrength);
        }
        if (_drawingTempBufferCopies) {
            float opacity = Utils.GetLerpValue(270f, 300f, self.GetCommon().TempBufferDodgeAnimationCounter, true);
            Color color = Main.hslToRgb(0.7f + (float)Math.Sin((float)Math.PI * 2f * Main.GlobalTimeWrappedHourly * 0.16f
                + _tempBufferCopiesHueShift[_currentTempBufferCopyIndex]), 1f, 0.5f);
            color.A /= 4;
            result = Color.Lerp(result, color, 0.25f) * opacity;
        }
        if (self.GetCommon()._isTeleportingBackViaObisidianStopwatch || self.GetCommon().IsObsidianStopwatchTeleportAvailable2) {
            float offset = self.whoAmI + (!_drawingObsidianStopwatchCopies ? 0f : (_obsidianStopwatchCopiesHueShift[0] * 0.1f * 0.5f));
            float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
            Color color = Main.hslToRgb(hue, 1f, 0.5f);
            if (self.GetModPlayer<SmallMoonPlayer>().HasContributor) {
                color = Color.Lerp(self.GetModPlayer<SmallMoonPlayer>().smallMoonColor, self.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
            }
            color.A = 25;
            color *= 0.5f;
            Color color2 = result.MultiplyRGBA(color);
            result = Color.Lerp(result, color2, (!self.GetCommon()._isTeleportingBackViaObisidianStopwatch ? 0.375f : 0.75f) * self.GetCommon().ObsidianStopwatchEffectOpacity);
        }
        return result;
    }

    private Color On_Player_GetImmuneAlpha(On_Player.orig_GetImmuneAlpha orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, Color.Gray * 0.25f, self.GetCommon().FriarLanternEffectStrength);
        }
        if (_drawingTempBufferCopies) {
            float opacity = Utils.GetLerpValue(270f, 300f, self.GetCommon().TempBufferDodgeAnimationCounter, true);
            Color color = Main.hslToRgb(0.7f + (float)Math.Sin((float)Math.PI * 2f * Main.GlobalTimeWrappedHourly * 0.16f 
                + _tempBufferCopiesHueShift[_currentTempBufferCopyIndex]), 1f, 0.5f);
            color.A /= 4;
            result = Color.Lerp(result, color, 0.25f) * opacity;
        }
        if (self.GetCommon()._isTeleportingBackViaObisidianStopwatch || self.GetCommon().IsObsidianStopwatchTeleportAvailable2) {
            float offset = self.whoAmI + (!_drawingObsidianStopwatchCopies ? 0f : (_obsidianStopwatchCopiesHueShift[0] * 0.1f * 0.5f));
            float hue = 0f + Helper.Wave(60f / 255f, 165f / 255f, 5f, offset);
            Color color = Main.hslToRgb(hue, 1f, 0.5f);
            if (self.GetModPlayer<SmallMoonPlayer>().HasContributor) {
                color = Color.Lerp(self.GetModPlayer<SmallMoonPlayer>().smallMoonColor, self.GetModPlayer<SmallMoonPlayer>().smallMoonColor2, Helper.Wave(0f, 1f, 5f, offset));
            }
            color.A = 25;
            color *= 0.5f;
            Color color2 = result.MultiplyRGBA(color);
            result = Color.Lerp(result, color2, (!self.GetCommon()._isTeleportingBackViaObisidianStopwatch ? 0.375f : 0.75f) * self.GetCommon().ObsidianStopwatchEffectOpacity);
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

    private void On_PlayerDrawLayers_DrawPlayer_21_Head(On_PlayerDrawLayers.orig_DrawPlayer_21_Head orig, ref PlayerDrawSet drawinfo) {
        if (drawinfo.drawPlayer.GetCommon().StopHeadDrawing ||
            drawinfo.drawPlayer.face == (HornetSkull.HornetSkullAsFace + 1)) {
            return;
        }

        orig(ref drawinfo);
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

            float num165 = (1.2f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 2.5f * ((float)Math.PI * 2f) * 3f)) * 0.8f;
            Vector2 position = new Vector2(drawPlayer.position.X + num3 * num165, drawPlayer.position.Y + drawPlayer.gfxOffY + num4);
            self.DrawPlayer(camera, drawPlayer, position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, drawPlayer.ghostFade, MathF.Max(1.1f, (num165 + 0.2f) * 0.9f));
        }
    }

    public delegate void PreUpdateMovementDelegate(Player player);
    public static event PreUpdateMovementDelegate PreUpdateMovementEvent;
    public override void PreUpdateMovement() {
        PreUpdateMovementEvent?.Invoke(Player);
    }

    public delegate void PostUpdateEquipsDelegate(Player player);
    public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
    public override void PostUpdateEquips() {
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
        else {
            ResetAdvancedShadows();
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
    }

    public delegate void OnHurtDelegate(Player player, Player.HurtInfo info);
    public static event OnHurtDelegate OnHurtEvent;
    public override void OnHurt(Player.HurtInfo info) {
        OnHurtEvent?.Invoke(Player, info);

        // need sync
        if (IsObsidianStopwatchEffectActive && IsObsidianStopwatchTeleportAvailable) {
            _isTeleportingBackViaObisidianStopwatch = true;
            _currentTeleportPointIndex = 0;
        }
    }

    public delegate void OnHitNPCDelegate(Player player, NPC target, NPC.HitInfo hit, int damageDone);
    public static event OnHitNPCDelegate OnHitNPCEvent;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        OnHitNPCEvent?.Invoke(Player, target, hit, damageDone);
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
                            Position = Player.Center
                        });
                    }
                }
            }
        }
    }

    public delegate void ResetEffectsDelegate(Player player);
    public static event ResetEffectsDelegate ResetEffectsEvent;
    public override void ResetEffects() {
        IsObsidianStopwatchEffectActive = false;

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

        return base.PreItemCheck();
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
        if (IsAetherInvincibilityActive) {
            modifiers.Cancel();
        }
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
