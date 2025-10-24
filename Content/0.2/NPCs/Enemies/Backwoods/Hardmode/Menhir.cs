using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class Menhir : ModNPC, IRequestAssets {
    private static byte FRAMECOUNT => 9;

    private static ushort TELEPORTTIME_JOURNEY => 9 * 60;
    private static ushort TELEPORTTIME_NORMAL => 7 * 60;
    private static ushort TELEPORTTIME_EXPERT => 5 * 60;
    private static ushort TELEPORTTIME_MASTER => 5 * 60;
    private static ushort TELEPORTTIME_LEGENDARY => 3 * 60;

    private static float TELEPORTANIMATIONTIMEINTICKS => 60f;

    public enum MenhirRequstedTextureType : byte {
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture 
        => [((byte)MenhirRequstedTextureType.Glow, Texture + "_Glow")];

    public enum MenhirState : byte {
        Idle,
        Casting,
        Disappearing,
        Appearing,
        Count
    }

    public enum MenhirFrame : byte {
        Idle,
        Casting,
        Teleport1,
        Teleport2,
        Teleport3,
        Teleport4,
        Teleport5,
        Teleport6,
        Teleport7,
        Count
    }

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public MenhirState State {
        get => (MenhirState)NPC.ai[0];
        set => NPC.ai[0] = Utils.Clamp((byte)value, (byte)MenhirState.Idle, (byte)MenhirState.Count);
    }

    public MenhirFrame Frame {
        get => (MenhirFrame)NPC.localAI[1];
        set => NPC.localAI[1] = Utils.Clamp((byte)value, (byte)MenhirFrame.Idle, (byte)MenhirFrame.Count);
    }
    public bool FacedLeft {
        get => NPC.ai[2] == 0f;
        set => NPC.ai[2] = value.ToInt();
    }

    public ref float TeleportTime => ref NPC.localAI[0];
    public ref float StateValue => ref NPC.ai[1];
    public ref float GlowOpacityFactor => ref NPC.localAI[2];

    public bool IsTeleporting => IsDisappearing || IsAppearing;
    public bool IsDisappearing => State == MenhirState.Disappearing;
    public bool IsAppearing => State == MenhirState.Appearing;
    public bool IsInIdle => State == MenhirState.Idle;
    public bool IsCasting => State == MenhirState.Casting;

    public float FrameTime => TELEPORTANIMATIONTIMEINTICKS / ((int)MenhirFrame.Teleport7 - (int)MenhirFrame.Teleport1 + 1);
    public bool CanTeleport => State >= MenhirState.Casting;

    public override void SetDefaults() {
        NPC.SetSizeValues(30, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.HitSound = SoundID.Dig;

        NPC.aiStyle = -1;

    }

    public override void AI() {
        NPC.velocity.X *= 0.8f;

        HandleIdle();
        HandleCasting();
        TeleportOverTime();
    }

    private void HandleCasting() {
        if (!IsCasting) {
            if (!IsInIdle) {
                GlowOpacityFactor = MathHelper.Lerp(GlowOpacityFactor, 0f, 0.1f);
            }

            return;
        }

        GlowOpacityFactor = MathHelper.Lerp(GlowOpacityFactor, 1f, 0.15f);

        Frame = MenhirFrame.Casting;
    }

    private void HandleIdle() {
        if (!IsInIdle || NPC.velocity.Y != 0f) {
            return;
        }

        MakeVisible();

        if (Frame == MenhirFrame.Idle) {
            State = MenhirState.Casting;
            return;
        }
        ref double frameCounter = ref NPC.frameCounter;
        if (frameCounter++ > FrameTime) {
            frameCounter = 0;
            Frame--;
        }
    }

    private void TeleportOverTime() {
        if (!CanTeleport) {
            return;
        }

        if (IsTeleporting) {
            HandleTeleporting();

            return;
        }

        ushort timeToTeleport = TELEPORTTIME_NORMAL;
        if (Main.GameModeInfo.IsJourneyMode) {
            timeToTeleport = TELEPORTTIME_JOURNEY;
        }
        else if (Main.expertMode) {
            timeToTeleport = TELEPORTTIME_EXPERT;
        }
        else if (Main.masterMode) {
            timeToTeleport = TELEPORTTIME_MASTER;
        }
        else if (Main.getGoodWorld) {
            timeToTeleport = TELEPORTTIME_LEGENDARY;
        }
        if (TeleportTime++ < timeToTeleport) {
            return;
        }

        Teleport();
        TeleportTime = 0f;
    }

    private void Teleport() {
        State = MenhirState.Disappearing;
        Frame = MenhirFrame.Teleport1;
    }

    private void HandleTeleporting() {
        if (IsDisappearing) {
            ref double frameCounter = ref NPC.frameCounter;
            if (frameCounter++ > FrameTime) {
                frameCounter = 0;
                Frame++;
                if (Frame >= MenhirFrame.Teleport7) {
                    State = MenhirState.Appearing;
                }
            }

            return;
        }

        if (!IsAppearing) {
            return;
        }

        StateValue++;
        if (StateValue >= FrameTime + 60f) {
            NPC.TargetClosest(false);
            ActuallyTeleport();
            StateValue = FrameTime;
        }
        else if (StateValue >= FrameTime) {
            MakeInvisible();
        }
    }

    private void MakeInvisible() {
        NPC.Opacity = 0f;
        NPC.dontTakeDamage = true;
    }

    private void MakeVisible() {
        if (NPC.Opacity != 1f) {
            if (Helper.SinglePlayerOrServer) {
                FacedLeft = Main.rand.NextBool();
                NPC.netUpdate = true;
            }
        }

        NPC.Opacity = 1f;
        NPC.dontTakeDamage = false;
    }

    private void ActuallyTeleport() {
        Player target = NPC.GetTargetPlayer();
        if (Helper.SinglePlayerOrServer) {
            Point point14 = NPC.Center.ToTileCoordinates();
            Point point15 = target.Center.ToTileCoordinates();
            Vector2 chosenTile2 = Vector2.Zero;
            if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile2, point15.X, point15.Y, 20, 12, 1, solidTileCheckCentered: true, teleportInAir: true)) {
                NPC.Center = chosenTile2.ToWorldCoordinates();
                State = MenhirState.Idle;
            }
            NPC.netUpdate = true;
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.SetDirection(FacedLeft.ToDirectionInt());

        NPC.SetFrame((byte)Frame, (ushort)frameHeight);
    }

    public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
        if (item.pick <= 0) {
            modifiers.FinalDamage *= 0.1f;
        }
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
        modifiers.FinalDamage *= 0.1f;
    }

    public override void OnKill() {
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Menhir>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Vector2 position = NPC.position;
        NPC.position.Y += 3;
        drawColor *= NPC.Opacity;
        NPC.QuickDraw(spriteBatch, screenPos, drawColor);
        Color glowColor = Color.Lerp(drawColor, Color.White * NPC.Opacity, 0.9f) * GlowOpacityFactor;
        int max = 2;
        for (int i = -max; i < max + 1; i++) {
            float scaleFactor = 1f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * ((float)Math.PI * 2f) * 3f + 1f * i);
            NPC.QuickDraw_Vector2Scale(spriteBatch, screenPos, glowColor * (1f - MathF.Abs(i) / (float)max) * 0.5f, scale: new Vector2(scaleFactor, 1f) * NPC.scale, texture: indexedTextureAssets[(byte)MenhirRequstedTextureType.Glow].Value);
        }
        NPC.QuickDraw(spriteBatch, screenPos, glowColor, texture: indexedTextureAssets[(byte)MenhirRequstedTextureType.Glow].Value);
        NPC.position = position;

        return false;
    }
}
