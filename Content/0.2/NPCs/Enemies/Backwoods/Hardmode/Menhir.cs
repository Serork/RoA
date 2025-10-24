using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Emotes;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class Menhir : ModNPC, IRequestAssets {
    private static byte FRAMECOUNT => 9;
    private static byte MAXENEMYCOUNTTOLOCK => 3;
    private static ushort PICKAXEEMOTESPAWNTIME => 300;
    private static float MINDISTANCETOENEMY => 800f;

    private static ushort TELEPORTTIME_JOURNEY => 9 * 60;
    private static ushort TELEPORTTIME_NORMAL => 7 * 60;
    private static ushort TELEPORTTIME_EXPERT => 5 * 60;
    private static ushort TELEPORTTIME_MASTER => 5 * 60;
    private static ushort TELEPORTTIME_LEGENDARY => 3 * 60;

    private static float TELEPORTANIMATIONTIMEINTICKS => 40f;

    public static LerpColor LerpColor { get; private set; } = new();
    public static Color GlowColor {
        get {
            Color result = LerpColor.GetLerpColor([new Color(79, 172, 211), new Color(49, 75, 188)]);
            result.A = 225;
            return result;
        }
    }

    public enum MenhirRequstedTextureType : byte {
        Glow,
        Chain,
        Lock
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture 
        => [((byte)MenhirRequstedTextureType.Glow, Texture + "_Glow"),
            ((byte)MenhirRequstedTextureType.Chain, Texture + "_Chain"),
            ((byte)MenhirRequstedTextureType.Lock, Texture + "_Lock")];

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

        NPCID.Sets.ImmuneToAllBuffs[Type] = true;
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
    public ref float PickaxeEmoteTimer => ref NPC.localAI[3];

    public bool IsTeleporting => IsDisappearing || IsAppearing;
    public bool IsDisappearing => State == MenhirState.Disappearing;
    public bool IsAppearing => State == MenhirState.Appearing;
    public bool IsInIdle => State == MenhirState.Idle;
    public bool IsCasting => State == MenhirState.Casting;

    public float FrameTime => TELEPORTANIMATIONTIMEINTICKS / ((int)MenhirFrame.Teleport7 - (int)MenhirFrame.Teleport1 + 1);
    public bool CanTeleport => State >= MenhirState.Casting;
    public Vector2 ChainCenter => IsTeleporting ? NPC.Center + Vector2.UnitY * 20f : NPC.Center;

    public override void SetDefaults() {
        NPC.SetSizeValues(30, 60);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.HitSound = SoundID.Dig;

        NPC.aiStyle = -1;
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override bool PreAI() {
        UnlockEnemies();

        return base.PreAI();
    }

    public override void AI() {
        LerpColor.Update();
        LerpColor.Update();

        if (IsCasting && PickaxeEmoteTimer++ >= PICKAXEEMOTESPAWNTIME) {
            PickaxeEmoteTimer = 0f;

            foreach (Player player in Main.ActivePlayers) {
                if (player.Distance(NPC.Center) > 16f * 10f || !Collision.CanHit(player.Center, 0, 0, NPC.position, NPC.width, NPC.height)) {
                    continue;
                }

                EmoteBubble.NewBubble(EmoteID.ItemPickaxe, new WorldUIAnchor(player), 180);
            }
        }

        NPC.velocity.X *= 0.8f;

        LockEnemies();

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
        if (GlowOpacityFactor < 0.01f) {
            GlowOpacityFactor = 0.01f;
        }

        Frame = MenhirFrame.Casting;
    }

    private void UnlockEnemies() {
        foreach (NPC checkNPC in Main.ActiveNPCs) {
            if (checkNPC.whoAmI == NPC.whoAmI) {
                continue;
            }
            var handler = checkNPC.GetCommon();
            if (!(handler.IsMenhirEffectActive && handler.MenhirEffectAppliedBy == NPC.whoAmI)) {
                continue;
            }

            handler.IsMenhirEffectActive = false;
            if (handler.DontTakeDamagePrevious is not null) {
                checkNPC.dontTakeDamage = handler.DontTakeDamagePrevious.Value;
                handler.DontTakeDamagePrevious = null;
            }
        }
    }

    private void LockEnemies() {
        GlowOpacityFactor = MathUtils.Clamp01(GlowOpacityFactor);

        if (GlowOpacityFactor <= 0.01f) {
            return;
        }

        List<int> taken = [];
        var npcs = Main.npc.Where(checkNPC => checkNPC.active && checkNPC.Distance(NPC.Center) < MINDISTANCETOENEMY).OrderBy(x => x.Distance(NPC.Center));
        foreach (NPC checkNPC in npcs) {
            if (checkNPC.type == Type || checkNPC.whoAmI == NPC.whoAmI) {
                continue;
            }
            if (checkNPC.GetCommon().IsMenhirEffectActive) {
                continue;
            }
            if (!checkNPC.CanActivateOnHitEffect() || checkNPC.friendly || checkNPC.boss) {
                continue;
            }
            if (taken.Contains(checkNPC.whoAmI) || taken.Count >= MAXENEMYCOUNTTOLOCK) {
                continue;
            }
            ApplyEffect(checkNPC, NPC.whoAmI);
            taken.Add(checkNPC.whoAmI);
        }
    }

    private void ApplyEffect(NPC npc, int whoAmI) {
        var handler = npc.GetCommon();
        handler.IsMenhirEffectActive = true;
        handler.MenhirEffectAppliedBy = whoAmI;
        HandleMenhirEffect(npc);
    }

    private void HandleMenhirEffect(NPC npc) {
        ref bool dontTakeDamage = ref npc.dontTakeDamage;
        npc.GetCommon().DontTakeDamagePrevious ??= dontTakeDamage;
        dontTakeDamage = true;
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
        if ((IsTeleporting || IsInIdle) && NPC.Opacity > 0f) {
            if (Main.rand.NextBool(5)) {
                int dust = Dust.NewDust(NPC.Bottom - new Vector2(NPC.width / 2f, 20), NPC.width, 20, (ushort)ModContent.DustType<Dusts.Backwoods.Stone>());
                Main.dust[dust].velocity.Y -= 2f * Main.rand.NextFloat();
                Main.dust[dust].scale *= 1f + 0.1f * Main.rand.NextFloatDirection();
            }
        }

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
            StateValue = 0f;
        }
        else if (StateValue >= FrameTime * 1.25f) {
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

    // see MenhirEffect
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        return false;
    }
}
