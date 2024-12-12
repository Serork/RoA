using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Content.Biomes.Backwoods;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
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
        if (!player.GetModPlayer<WreathHandler>().IsEmpty2) {
            player.mount.SetMount(Parent.Type, player);
            player.buffTime[buffIndex] = 10;
        }
    }
}

abstract class BaseForm : ModMount {
    private delegate void ExtraJumpLoader_UpdateHorizontalSpeeds_orig(Player player);
    private static object Hook_ExtraJumpLoader_UpdateHorizontalSpeeds;

    private readonly struct MovementSpeedInfo(float maxRunSpeed, float accRunSpeed, float runAcceleration) {
        public readonly float MaxRunSpeed = maxRunSpeed;
        public readonly float AccRunSpeed = accRunSpeed;
        public readonly float RunAcceleration = runAcceleration;
    }

    private static MovementSpeedInfo _playerMovementSpeedInfo;

    protected float _attackCharge;

    public float AttackCharge {
        get => Ease.QuintOut(_attackCharge);
        protected set => _attackCharge = value;
    }

    public BaseFormBuff MountBuff { get; init; }

    public BaseForm() => MountBuff = new BaseFormBuff(this);

    public override void Load() {
        Mod.AddContent(MountBuff);

        On_Player.HorizontalMovement += On_Player_HorizontalMovement;
        Hook_ExtraJumpLoader_UpdateHorizontalSpeeds = RoA.Detour(typeof(ExtraJumpLoader).GetMethod(nameof(ExtraJumpLoader.UpdateHorizontalSpeeds), BindingFlags.Public | BindingFlags.Static),
            typeof(BaseForm).GetMethod(nameof(ExtraJumpLoader_UpdateHorizontalSpeeds), BindingFlags.NonPublic | BindingFlags.Static));
    }

    public override void Unload() => Hook_ExtraJumpLoader_UpdateHorizontalSpeeds = null;

    private static void ExtraJumpLoader_UpdateHorizontalSpeeds(ExtraJumpLoader_UpdateHorizontalSpeeds_orig self, Player player) {
        self(player);

        if (player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed) {
            _playerMovementSpeedInfo = new MovementSpeedInfo(player.maxRunSpeed, player.accRunSpeed, player.runAcceleration);
        }
    }

    private void On_Player_HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player self) {
        if (self.GetModPlayer<BaseFormHandler>().UsePlayerSpeed && self.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            BaseForm mountData = MountLoader.GetMount(self.mount._type) as BaseForm;
            self.maxRunSpeed = _playerMovementSpeedInfo.MaxRunSpeed * mountData.GetMaxSpeedMultiplier(self);
            self.accRunSpeed = _playerMovementSpeedInfo.AccRunSpeed * mountData.GetAccRunSpeedMultiplier(self);
            self.runAcceleration = _playerMovementSpeedInfo.RunAcceleration * mountData.GetRunAccelerationMultiplier(self);
        }
        orig(self);
    }

    protected virtual float GetMaxSpeedMultiplier(Player player) => 1f;
    protected virtual float GetAccRunSpeedMultiplier(Player player) => 1f;
    protected virtual float GetRunAccelerationMultiplier(Player player) => 1f;

    protected static bool IsInAir(Player player) {
        bool flag = false;
        for (int i = -1; i < 2; i++) {
            if (WorldGenHelper.SolidTile((int)(player.Center.X + i * 16f) / 16, (int)(player.Bottom.Y + 10f) / 16)) {
                flag = true;
                break;
            }
        }
        bool onTile = Math.Abs(player.velocity.Y) < 1.25f && flag;
        return !player.sliding && !onTile && player.gfxOffY == 0f;
    }

    public sealed override void SetStaticDefaults() {
        MountData.buff = MountBuff.Type;

        MountData.xOffset = 0;
        MountData.bodyFrame = 0;
        MountData.yOffset = -10;
        MountData.playerHeadOffset = -10;

        SafeSetDefaults();

        MountData.playerYOffsets = Enumerable.Repeat(0, MountData.totalFrames).ToArray();

        MountData.frontTextureGlow = ModContent.Request<Texture2D>(Texture + "_Glow");

        if (!Main.dedServ) {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeUpdateEffects(Player player) { }
    protected virtual bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) => true;
    protected virtual void SafeSetMount(Player player, ref bool skipDust) { }

    protected virtual Vector2 GetLightingPos(Player player) => Vector2.Zero;
    protected virtual Color LightingColor { get; } = Color.White;

    public sealed override void SetMount(Player player, ref bool skipDust) {
        int buffType = MountBuff.Type;
        player.ClearBuff(buffType);
        player.AddBuffInStart(buffType, 3600);

        SafeSetMount(player, ref skipDust);
    }

    public sealed override void UpdateEffects(Player player) {
        MountData.buff = MountBuff.Type;

        SafeUpdateEffects(player);

        SpawnRunDusts(player);

        if (_attackCharge > 0f) {
            _attackCharge -= TimeSystem.LogicDeltaTime;
        }

        if (LightingColor == Color.White) {
            return;
        }
        WreathHandler wreathHandler = player.GetModPlayer<WreathHandler>();
        float value = Math.Max(MathHelper.Clamp(_attackCharge, 0f, 1f), wreathHandler.ActualProgress4);
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

        return SafeUpdateFrame(mountedPlayer, ref frameCounter, ref frame);
    }

    public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        GetSpriteEffects(drawPlayer, ref spriteEffects);
        DrawData item = new(texture, drawPosition, frame, drawColor, rotation, drawOrigin, drawScale, spriteEffects);
        playerDrawData.Add(item);
        DrawGlowMask(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);

        return false;
    }

    protected virtual void DrawGlowMask(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        WreathHandler wreathHandler = drawPlayer.GetModPlayer<WreathHandler>();
        if (glowTexture != null) {
            float value = Math.Max(MathHelper.Clamp(_attackCharge, 0f, 1f), wreathHandler.ActualProgress4);
            DrawData item = new(glowTexture, drawPosition, frame, Color.White * ((float)(int)drawColor.A / 255f) * value, rotation, drawOrigin, drawScale, spriteEffects);
            playerDrawData.Add(item);
        }
    }

    protected virtual void GetSpriteEffects(Player player, ref SpriteEffects spriteEffects) { }

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
