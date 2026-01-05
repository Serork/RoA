using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Tiles.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    private static float BACKFLIPTIME => 25f;
    private static float MAXFALLSPEEDMODIFIERFORFALL => 0.75f;

    public static ushort CONTROLUSEITEMTIMECHECKBASE => 10;

    private bool _fell;
    private float _fellTimer;
    private ushort _controlUseItemTimer;
    private float _backflipTimer;

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

    public bool DoingBackflip => _backflipTimer > 0f;
    public float BackflipProgress => Ease.CubeIn(_backflipTimer / BACKFLIPTIME);

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
    }


    private Color On_Player_GetImmuneAlphaPure(On_Player.orig_GetImmuneAlphaPure orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, Color.Gray * 0.25f, self.GetCommon().FriarLanternEffectStrength);
        }
        return result;
    }

    private Color On_Player_GetImmuneAlpha(On_Player.orig_GetImmuneAlpha orig, Player self, Color newColor, float alphaReduction) {
        Color result = orig(self, newColor, alphaReduction);
        if (self.GetCommon().FriarLanternEffectStrength > 0f) {
            result = Color.Lerp(result, Color.Gray * 0.25f, self.GetCommon().FriarLanternEffectStrength);
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

        if (ElderSnailSlow) {
            Player.moveSpeed /= 3f;
            if (Player.velocity.Y == 0f && Math.Abs(Player.velocity.X) > 1f) {
                Player.velocity.X /= 2f;
            }

            Player.slowOgreSpit = Player.dazed = Player.slow = Player.chilled = false;
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

        FriarLanternEffectStrength = Helper.Approach(FriarLanternEffectStrength, IsFriarLanternBuffEffectActive.ToInt() * 0.625f, !IsFriarLanternBuffEffectActive ? 0.075f : 0.05f);
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
    }

    public delegate void OnHitNPCDelegate(Player player, NPC target, NPC.HitInfo hit, int damageDone);
    public static event OnHitNPCDelegate OnHitNPCEvent;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        OnHitNPCEvent?.Invoke(Player, target, hit, damageDone);
    }

    public delegate void ResetEffectsDelegate(Player player);
    public static event ResetEffectsDelegate ResetEffectsEvent;
    public override void ResetEffects() {
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
