using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Wreath;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;

[Autoload(false)]
sealed class BaseFormBuff(BaseForm parent) : ModBuff {
    public BaseForm Parent { get; private set; } = parent;

    public override string Name => Parent.Name;
    public override string Texture => Parent.NamespacePath() + $"/{Parent.Name.Replace("Mount", "")}Buff";

    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (!player.GetWreathHandler().IsEmpty3) {
            player.mount.SetMount(Parent.Type, player);
            player.buffTime[buffIndex] = 10;
        }
    }
}

abstract class BaseForm : ModMount {
    internal sealed class BaseFormDataStorage : ModPlayer {
        internal float _attackCharge, _attackCharge2;

        public float AttackCharge {
            get => Ease.QuintOut(_attackCharge);
            private set => _attackCharge = value;
        }

        public static float GetAttackCharge(Player player) => MathHelper.Clamp(Math.Max(player.GetModPlayer<BaseFormDataStorage>()._attackCharge2, player.GetModPlayer<BaseFormDataStorage>()._attackCharge), 0f, 1f);

        internal static void ChangeAttackCharge1(Player player, float value, bool net = true) {
            player.GetModPlayer<BaseFormDataStorage>().AttackCharge = value;
            if (net && Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new BaseFormPacket1(player, value));
            }
        }

        internal static void ChangeAttackCharge2(Player player, float value, bool net = true) {
            player.GetModPlayer<BaseFormDataStorage>()._attackCharge2 = value;
            if (net && Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new BaseFormPacket2(player, value));
            }
        }

        public override void PostUpdateMiscEffects() {
            if (_attackCharge > 0f) {
                _attackCharge -= TimeSystem.LogicDeltaTime;
            }
            if (_attackCharge2 > 0f) {
                _attackCharge2 -= TimeSystem.LogicDeltaTime;
            }
        }
    }
    
    public Asset<Texture2D> HeadTexture { get; private set; }
    public Asset<Texture2D> SolidTexture { get; private set; }

    public BaseFormBuff MountBuff { get; init; }

    public BaseForm() => MountBuff = new BaseFormBuff(this);

    public virtual Vector2 SetWreathOffset(Player player) => Vector2.Zero;
    public virtual Vector2 SetWreathOffset2(Player player) => Vector2.Zero;

    public virtual SoundStyle ApplySound { get; } = SoundID.Item25;
    public virtual SoundStyle ReleaseSound { get; } = SoundID.Item25;

    public virtual ushort SetHitboxWidth(Player player) => Player.defaultWidth;
    public virtual ushort SetHitboxHeight(Player player) => Player.defaultHeight;

    public sealed override void Load() {
        Mod.AddContent(MountBuff);

        if (!Main.dedServ) {
            HeadTexture = ModContent.Request<Texture2D>(Texture + "_Head");
            SolidTexture = ModContent.Request<Texture2D>(Texture + "_Solid");
        }

        SafeLoad();
    }

    protected virtual void SafeLoad() { }

    public virtual bool ShouldApplyUpdateJumpHeightLogic { get; }

    public virtual float GetMaxSpeedMultiplier(Player player) => 1f;
    public virtual float GetAccRunSpeedMultiplier(Player player) => 1f;
    public virtual float GetRunAccelerationMultiplier(Player player) => 1f;

    public static bool IsInAir(Player player) {
        bool flag = false;
        for (int i = -1; i < 2; i++) {
            if (WorldGenHelper.SolidTile2((int)(player.Center.X + i * 16f) / 16, (int)(player.Bottom.Y + 10f) / 16)) {
                flag = true;
                break;
            }
        }
        bool onTile = Math.Abs(player.velocity.Y) < 1.25f && flag;
        return !player.sliding && !onTile && player.gfxOffY == 0f;
    }

    protected virtual void OnJump(Player player) { }
    protected virtual void OnJumping(Player player, bool firstJump) { }

    protected virtual void SetJumpSettings(Player player, ref int jumpHeight, ref float jumpSpeed, ref float jumpSpeedBoost, ref float extraFall,
        ref bool hasDoubleJump, ref int jumpOnFirst, ref int jumpOnSecond) { }

    protected virtual void ActivateExtraJumps(Player player) {
        var plr = player.GetFormHandler();
        Helper.GetJumpSettings(player, out int jumpHeight, out float jumpSpeed, out float jumpSpeedBoost, out float extraFall);
        bool hasDoubleJump = false;
        int jumpOnFirst = jumpHeight;
        int jumpOnSecond = jumpHeight;
        SetJumpSettings(player, ref jumpHeight, ref jumpSpeed, ref jumpSpeedBoost, ref extraFall, ref hasDoubleJump, ref jumpOnFirst, ref jumpOnSecond);
        void jump() {
            player.velocity.Y = -jumpSpeed * player.gravDir;
            plr.Jump = jumpOnFirst;
            plr.JumpCD = 15;
            plr.JustJumped = plr.JustJumpedForAnimation = true;

            OnJump(player);

            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
        }

        if (player.controlJump && !plr.IsPreparing) {
            if (plr.Jump > 0) {
                if (player.velocity.Y == 0f) {
                    plr.Jump = 0;
                }
                else {
                    player.velocity.Y = -jumpSpeed * player.gravDir;
                    plr.Jump--;
                    OnJumping(player, plr.Jumped);
                }
            }
            else {
                if ((player.sliding || player.velocity.Y == 0f || plr.Jumped || (hasDoubleJump && plr.Jumped2)) && player.releaseJump && plr.JumpCD == 0) {
                    bool justJumped = false;
                    bool justJumped2 = false;
                    if (plr.Jumped) {
                        justJumped = true;
                        plr.Jumped = false;
                    }
                    else if (hasDoubleJump && plr.Jumped2) {
                        justJumped2 = true;
                        plr.Jumped2 = false;
                    }
                    if (player.velocity.Y == 0f || player.sliding) {
                        plr.Jumped = true;
                        plr.Jumped2 = true;
                    }
                    if (player.velocity.Y == 0f || player.sliding) {
                        player.velocity.Y = -jumpSpeed * player.gravDir;
                        plr.Jump = jumpOnSecond;
                        plr.JumpCD = 15;
                    }
                    else {
                        if (justJumped) {
                            jump();
                        }
                        else if (justJumped2) {
                            jump();
                        }
                    }
                }
            }
            bool flag = plr.Jumped || (hasDoubleJump && plr.Jumped2);
            if (flag || plr.JumpCD > 0) {
                player.releaseJump = false;
            }
            else if (player.controlJump && player.releaseJump) {
                plr.JustJumped = plr.JustJumpedForAnimation = true;
            }
        }
        else {
            plr.Jump = 0;
        }
        if (plr.JumpCD > 0) {
            plr.JumpCD--;
        }
    }

    public sealed override void SetStaticDefaults() {
        MountData.buff = MountBuff.Type;

        MountData.xOffset = 0;
        MountData.bodyFrame = 0;
        MountData.yOffset = -10;
        MountData.playerHeadOffset = -10;

        MountData.blockExtraJumps = false;

        SafeSetDefaults();

        MountData.playerYOffsets = [.. Enumerable.Repeat(0, MountData.totalFrames)];

        MountData.frontTextureGlow = ModContent.Request<Texture2D>(Texture + "_Glow");

        if (!Main.dedServ) {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    protected virtual bool ForcedChangeDirectionIfNeeded(Player player) => true;

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafePostUpdate(Player player) { }
    protected virtual bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) => true;
    protected virtual void SafeSetMount(Player player, ref bool skipDust) { }
    protected virtual void SafeDismountMount(Player player, ref bool skipDust) { }

    protected virtual Vector2 GetLightingPos(Player player) => Vector2.Zero;
    protected virtual Color LightingColor { get; } = Color.White;
    public virtual SoundStyle? HurtSound { get; } = null;

    private ushort _drawFor = ushort.MaxValue;
    public bool IsDrawing { get; private set; }

    public sealed override void SetMount(Player player, ref bool skipDust) {
        int buffType = MountBuff.Type;
        player.ClearBuff(buffType);
        player.AddBuffInStart(buffType, 3600);

        BaseFormDataStorage.ChangeAttackCharge2(player, 1.5f);

        SafeSetMount(player, ref skipDust);
    }

    public sealed override void Dismount(Player player, ref bool skipDust) {
        player.GetWreathHandler().Dusts_ResetStayTime();

        SafeDismountMount(player, ref skipDust);

        IsDrawing = false;
        _drawFor = ushort.MaxValue;
    }

    public sealed override void UpdateEffects(Player player) {
        MountData.buff = MountBuff.Type;

        if (!IsInAir(player)) {
            player.GetFormHandler().JustJumpedForAnimation = player.GetFormHandler().JustJumpedForAnimation2 = false;
        }

        SafePostUpdate(player);

        SpawnRunDusts(player);

        if (LightingColor == Color.White) {
            return;
        }
        float value = MathHelper.Clamp(player.GetModPlayer<BaseFormDataStorage>()._attackCharge, 0f, 1f);
        Lighting.AddLight(GetLightingPos(player) == Vector2.Zero ? player.Center : GetLightingPos(player), LightingColor.ToVector3() * value);
    }

    private static void SpawnRunDusts(Player player) {
        float num = (player.accRunSpeed + player.maxRunSpeed) / 2f;
        if (player.controlLeft && player.velocity.X > 0f - player.maxRunSpeed) {
        }
        else if (player.controlRight && player.velocity.X < player.maxRunSpeed) {
        }
        else if (player.controlLeft && player.velocity.X > 0f - player.accRunSpeed && player.dashDelay >= 0) {
            if (player.velocity.X < 0f - num && player.velocity.Y == 0f) {
                SpawnFastRunParticles(player);
            }
        }
        else if (player.controlRight && player.velocity.X < player.accRunSpeed && player.dashDelay >= 0) {
            if (player.velocity.X > num && player.velocity.Y == 0f) {
                SpawnFastRunParticles(player);
            }
        }
    }

    private static void SpawnFastRunParticles(Player player) {
        int num = 0;
        if (player.gravDir == -1f)
            num -= player.height;

        if (player.runSoundDelay == 0 && player.velocity.Y == 0f) {
            SoundEngine.PlaySound(player.hermesStepSound.Style, player.position);
            player.runSoundDelay = player.hermesStepSound.IntendedCooldown;
        }

        if (player.fairyBoots) {
            int type = Main.rand.NextFromList(new short[6] {
                61,
                61,
                61,
                242,
                64,
                63
            });

            int alpha = 0;
            for (int k = 1; k < 3; k++) {
                float scale = 1.5f;
                if (k == 2)
                    scale = 1f;

                int num5 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)player.height + (float)num), player.width + 8, 4, type, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, alpha, default(Color), scale);
                Main.dust[num5].velocity *= 1.5f;
                if (k == 2)
                    Main.dust[num5].position += Main.dust[num5].velocity;

                Main.dust[num5].noGravity = true;
                Main.dust[num5].noLightEmittence = true;
                Main.dust[num5].shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
            }
        }
        else if (player.hellfireTreads) {
            int num6 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)player.height + (float)num), player.width + 8, 4, 6, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 50, default(Color), 2f);
            Main.dust[num6].velocity.X = Main.dust[num6].velocity.X * 0.2f;
            Main.dust[num6].velocity.Y = -1.5f - Main.rand.NextFloat() * 0.5f;
            Main.dust[num6].fadeIn = 0.5f;
            Main.dust[num6].noGravity = true;
            Main.dust[num6].shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
        }
        else {
            int num7 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)player.height + (float)num), player.width + 8, 4, 16, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 50, default(Color), 1.5f);
            Main.dust[num7].velocity.X = Main.dust[num7].velocity.X * 0.2f;
            Main.dust[num7].velocity.Y = Main.dust[num7].velocity.Y * 0.2f;
            Main.dust[num7].shader = GameShaders.Armor.GetSecondaryShader(player.cShoe, player);
        }
    }

    public sealed override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
        ref float frameCounter = ref mountedPlayer.mount._frameCounter;
        ref int frame = ref mountedPlayer.mount._frame;

        UpdateDrawingState();

        return SafeUpdateFrame(mountedPlayer, ref frameCounter, ref frame);
    }

    public sealed override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        if (IsDrawing) {
            GetSpriteEffects(drawPlayer, ref spriteEffects);
            drawPosition.X -= drawPlayer.mount.XOffset * drawPlayer.direction;
            drawPosition.X += drawPlayer.mount.XOffset * (spriteEffects == SpriteEffects.None).ToDirectionInt();

            if (drawType == 0 && drawPlayer.GetCommon().ApplyDeerSkullSetBonus) {
                if (drawPlayer.GetWreathHandler().ChargedBySlowFill && shadow == 0) {
                    float hornsOpacity = drawPlayer.GetCommon().DeerSkullHornsOpacity;
                    float hornsBorderOpacity = MathUtils.Clamp01(drawPlayer.GetCommon().DeerSkullHornsBorderOpacity);
                    float hornsBorderOpacity2 = MathUtils.Clamp01(drawPlayer.GetCommon().DeerSkullHornsBorderOpacity2);
                    for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                        Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 2f * drawScale;
                        Color color = hornsBorderOpacity2 >= 0.925f ? Color.White : new(35, 193, 179);
                        color *= 0.75f;
                        DrawData item2 = new(SolidTexture.Value, drawPosition + vector2, frame, color * hornsBorderOpacity, rotation, drawOrigin, drawScale, spriteEffects);
                        item2.shader = drawPlayer.cHead;
                        playerDrawData.Add(item2);

                        color.A /= 2;
                        item2 = new(SolidTexture.Value, drawPosition + vector2 + Main.rand.RandomPointInArea(2f) * hornsBorderOpacity2, frame, color * 0.5f * hornsBorderOpacity, rotation, drawOrigin, drawScale, spriteEffects);
                        item2.shader = drawPlayer.cBody;
                        playerDrawData.Add(item2);
                    }
                }
            }

            DrawData item = new(texture, drawPosition, frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cBody;
            playerDrawData.Add(item);
            DrawGlowMask(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);
        }

        return false;
    }

    protected void UpdateDrawingState() {
        if (!IsDrawing) {
            if (_drawFor == ushort.MaxValue) {
                _drawFor = 1;
            }
            else {
                if (_drawFor <= 0) {
                    IsDrawing = true;
                }
                else {
                    _drawFor--;
                }
            }
        }
    }

    protected virtual void DrawGlowMask(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        if (glowTexture != null) {
            float value = BaseFormDataStorage.GetAttackCharge(drawPlayer);
            DrawData item = new(glowTexture, drawPosition, frame, GlowColor(drawPlayer, drawColor, ((float)(int)drawColor.A / 255f) * value), rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cBody;
            playerDrawData.Add(item);
        }
    }

    protected virtual Color GlowColor(Player player, Color drawColor, float progress) => Color.White * progress;

    protected virtual void GetSpriteEffects(Player player, ref SpriteEffects spriteEffects) {
        if (!ForcedChangeDirectionIfNeeded(player)) {
            return;
        }

        bool? variable = player.GetFormHandler().FacedRight;
        if (!variable.HasValue) {
            return;
        }
        spriteEffects = variable.Value ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    }

    //public static void TestHook(Player player) => Main.NewText(123);

    //private void IL_Player_Update(ILContext context) {
    //    var il = new ILCursor(context);

    //    il.GotoNext(
    //        MoveType.Before,
    //        i => i.MatchLdarg(0),
    //        i => i.MatchCall(typeof(PlayerLoader), "PostUpdateRunSpeeds")
    //    );

    //    il.GotoPrev(
    //        MoveType.Before,
    //        i => i.MatchLdarg(0),
    //        i => i.MatchLdfld(typeof(Player), nameof(Player.mount)),
    //        i => i.MatchCallvirt(typeof(Mount), $"get_{nameof(Mount.Active)}"),
    //        i => i.MatchBrfalse(out _)
    //    );

    //    il.Emit(OpCodes.Ldarg_0);
    //    il.EmitDelegate(TestHook);

    //    MonoModHooks.DumpIL(ModContent.GetInstance<RoA>(), context);
    //}
}
