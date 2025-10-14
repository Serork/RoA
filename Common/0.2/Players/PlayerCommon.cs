using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

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

    public bool ApplyBoneArmorVisuals;

    public bool DoingBackflip => _backflipTimer > 0f;
    public float BackflipProgress => Ease.CubeIn(_backflipTimer / BACKFLIPTIME);

    public void DoBackflip(float time = 0f) {
        if (DoingBackflip) {
            return;
        }

        _backflipTimer = time == 0f ? BACKFLIPTIME : time;
    }

    private void ApplySkullEffect() {
        if (Player.HasEquipped<CarcassChestguard>(EquipType.Body) &&
            Player.HasEquipped<CarcassSandals>(EquipType.Legs) &&
            (Player.HasEquipped<HornetSkull>(EquipType.Head) || Player.HasEquipped<DeerSkull>(EquipType.Head) || Player.HasEquipped<DeerSkull>(EquipType.Head) ||
            Player.HasEquipped(ArmorIDs.Head.Skull, EquipType.Head))) {
            ApplyBoneArmorVisuals = true;
        }
    }

    public bool Fell { get; private set; }

    public bool LockHorizontalMovement;

    public Vector2 SavedPosition;
    public Vector2 SavedVelocity;
    public float DashTime;
    public bool Dashed;

    public override void Load() {
        On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
        On_Player.DryCollision += On_Player_DryCollision;

        DrawPlayerFullEvent += PlayerCommon_DrawPlayerFullEvent;

        On_Player.HorizontalMovement += On_Player_HorizontalMovement;
    }

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
        PostUpdateEquipsEvent?.Invoke(Player);
    }

    public delegate void UpdateEquipsDelegate(Player player);
    public static event UpdateEquipsDelegate UpdateEquipsEvent;
    public override void UpdateEquips() {
        UpdateEquipsEvent?.Invoke(Player);

        ApplySkullEffect();
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

    public delegate void ResetEffectsDelegate(Player player);
    public static event ResetEffectsDelegate ResetEffectsEvent;

    public override void ResetEffects() {
        ResetEffectsEvent?.Invoke(Player);

        ApplyBoneArmorVisuals = false;
        LockHorizontalMovement = false;
        ApplyVanillaSkullSetBonus = false;
    }

    public override void PostUpdate() {
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
