using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Forms;

sealed class HallowEnt : BaseForm {
    private static ushort TIMEBEFORELEAFATTACK => 30;
    private static float STARTATTACKTIME => 30f;
    private static float ENDATTACKTIME => 10f;
    private static float STARTANGLE => MathHelper.PiOver4 / 1.75f;
    private static float ENDANGLE => STARTANGLE * 0.25f;

    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor_HallowEnt(player, drawColor, progress);

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1.85f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, -36f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 18f);

    protected override void SafeSetDefaults() {
        MountData.fallDamage = 0.1f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 15;
        MountData.jumpSpeed = 6f;
        MountData.totalFrames = 1;
        MountData.constantJump = false;
        MountData.usesHover = false;
        MountData.xOffset = -6;
        MountData.yOffset = 3;
        MountData.playerHeadOffset = -45;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetFormHandler().UsePlayerSpeed = true;

        player.statDefense += 30;

        HandleLeafAttack(player);
    }

    private void HandleLeafAttack(Player player) {
        var handler = player.GetFormHandler();
        ref int shootCounter = ref handler.ShootCounter;
        ref byte attackCount = ref handler.AttackCount;
        ref float timeForAttack = ref handler.AttackFactor;

        if (player.HoldingLMB(true)) {
            shootCounter++;
            void leafAttack(float attackProgress) {
                if (player.IsLocal()) {
                    int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(35f);
                    float knockBack = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(0f);
                    Vector2 position = player.Center;
                    Vector2 velocity = position.DirectionTo(player.GetWorldMousePosition());
                    float angle = STARTANGLE;
                    float progress = MathUtils.Clamp01(attackProgress);
                    angle = MathHelper.Lerp(angle, ENDANGLE, progress);
                    ref Vector2 savedVelocity = ref handler.SavedVelocity;
                    velocity = velocity.RotatedBy(angle * Main.rand.NextFloatDirection());
                    progress = Utils.Remap(1f - progress, 0.5f, 1f, 0.25f, 1f, true);
                    savedVelocity = Vector2.Lerp(savedVelocity, velocity, progress);
                    velocity = savedVelocity;
                    ProjectileUtils.SpawnPlayerOwnedProjectile<HallowLeaf>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("hallowentattack")) {
                        Damage = damage,
                        KnockBack = knockBack,
                        Position = position,
                        Velocity = velocity
                    });
                }
            }
            if (shootCounter > TIMEBEFORELEAFATTACK) {
                if (shootCounter > TIMEBEFORELEAFATTACK + (int)timeForAttack) {
                    float attackProgress = 1f - timeForAttack / STARTATTACKTIME;
                    leafAttack(attackProgress);
                    shootCounter = TIMEBEFORELEAFATTACK;

                    BaseFormHandler.ForcedDirectionChange(player, 0.5f + 0.5f * attackProgress, true);
                }
            }
            timeForAttack -= 0.05f;
            timeForAttack = Utils.Clamp(timeForAttack, ENDATTACKTIME, STARTATTACKTIME);
            float baseForEffect = MathHelper.Lerp(STARTATTACKTIME, ENDATTACKTIME, 0.75f), 
                  base2ForEffect = MathHelper.Lerp(STARTATTACKTIME, ENDATTACKTIME, 1f);
            if (timeForAttack <= baseForEffect) {
                float value = Ease.SineOut(Utils.GetLerpValue(baseForEffect, base2ForEffect, timeForAttack, true)) * 0.35f;
                BaseFormDataStorage.ChangeAttackCharge1(player, value, false);
            }
        }
        else {

            Set(player);
        }
    }

    private void Set(Player player) {
        var handler = player.GetFormHandler();
        ref int shootCounter = ref handler.ShootCounter;
        ref byte attackCount = ref handler.AttackCount;
        ref float timeForAttack = ref handler.AttackFactor;
        ref Vector2 savedVelocity = ref handler.SavedVelocity;

        shootCounter = TIMEBEFORELEAFATTACK;
        attackCount = 0;
        timeForAttack = STARTATTACKTIME;
        savedVelocity = Vector2.Zero;
    }

    private void Reset(Player player) {
        var handler = player.GetFormHandler();
        ref int shootCounter = ref handler.ShootCounter;
        ref byte attackCount = ref handler.AttackCount;
        ref float timeForAttack = ref handler.AttackFactor;
        ref Vector2 savedVelocity = ref handler.SavedVelocity;

        shootCounter = 0;
        attackCount = 0;
        timeForAttack = 0;
        savedVelocity = Vector2.Zero;
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        frameCounter = 0;
        frame = 0;

        return true;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        Set(player);

        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        Reset(player);

        skipDust = true;
    }
}
