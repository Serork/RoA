using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Players;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class HallowedGryphonHandler : ModPlayer, IDoubleTap {
    public static float MOVESPEEDBUFFTIMEINTICKS => 120f;

    public bool JustStartedDoingLoopAttack;
    public bool LoopAttackIsDone;
    public Vector2 SavedVelocity;
    public ushort CanDoLoopAttackTimer;
    public float AttackFactor, AttackFactor2;
    public byte AttackCount;
    public float MoveSpeedBuffTime;

    public bool CanDoLoopAttack {
        get => CanDoLoopAttackTimer <= 0;
        set => CanDoLoopAttackTimer = (ushort)(value ? 0 : 30);
    }

    public bool IncreasedMoveSpeed => MoveSpeedBuffTime > 0f;
    public bool CanIncreaseMoveSpeed => MoveSpeedBuffTime == 0f;

    public bool IsInLoopAttack => AttackFactor > 0;

    public override void PostUpdate() {
        if (!Player.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>()) {
            return;
        }
        if (LoopAttackIsDone && CanDoLoopAttackTimer > 0) {
            CanDoLoopAttackTimer--;
            if (CanDoLoopAttackTimer <= 0) {
                LoopAttackIsDone = false;
            }
        }
        if (IncreasedMoveSpeed) {
            MoveSpeedBuffTime--;
            if (CanIncreaseMoveSpeed) {
                MoveSpeedBuffTime = -480f;
            }
        }
        else if (!CanIncreaseMoveSpeed) {
            MoveSpeedBuffTime++;
        }
    }

    public override void PostUpdateRunSpeeds() {
    }

    public override void Load() {
        On_Player.GetImmuneAlpha += On_Player_GetImmuneAlpha;
        On_Player.GetImmuneAlphaPure += On_Player_GetImmuneAlphaPure;
    }

    private Color On_Player_GetImmuneAlphaPure(On_Player.orig_GetImmuneAlphaPure orig, Player self, Color newColor, float alphaReduction) {
        if (self.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>() && HallowedGryphon.GetHandler(self).IncreasedMoveSpeed) {
            float num = (float)(255 - self.immuneAlpha) / 255f;
            if (alphaReduction > 0f)
                num *= 1f - alphaReduction;

            float shimmerTransparency = 0.15f * HallowedGryphon.GetMoveSpeedFactor(self);
            if (shimmerTransparency > 0f) {
                if ((double)shimmerTransparency >= 0.8)
                    return Color.Transparent;

                num *= 1f - shimmerTransparency;
                num *= 1f - shimmerTransparency;
                num *= 1f - shimmerTransparency;

                newColor.A = 0;
            }

            if (self.immuneAlpha > 125)
                return Color.Transparent;

            newColor.A = 0;

            Color result = Color.Multiply(newColor.MultiplyRGB(Color.Lerp(Color.LightYellow, new Color(255, 224, 224), Helper.Wave(0f, 1f, speed: 15f))), num);
            //result.A = (byte)Math.Max(result.A - 25, 0);
            return result;
        }

        return orig(self, newColor, alphaReduction);
    }

    private Color On_Player_GetImmuneAlpha(On_Player.orig_GetImmuneAlpha orig, Player self, Color newColor, float alphaReduction) {
        if (self.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>() && HallowedGryphon.GetHandler(self).IncreasedMoveSpeed) {
            float num = (float)(255 - self.immuneAlpha) / 255f;
            float shimmerTransparency = 0.15f * HallowedGryphon.GetMoveSpeedFactor(self);
            if (alphaReduction > 0f)
                num *= 1f - alphaReduction;

            if (shimmerTransparency > 0f)
                num *= 1f - shimmerTransparency;

            newColor.A = 0;

            Color result = Color.Multiply(newColor.MultiplyRGB(Color.Lerp(Color.LightYellow, new Color(255, 224, 224), Helper.Wave(0f, 1f, speed: 15f))), num);
            //result.A = (byte)Math.Max(result.A - 25, 0);
            return result;
        }

        return orig(self, newColor, alphaReduction);
    }

    public void Reset() {
        JustStartedDoingLoopAttack = LoopAttackIsDone = false;
        SavedVelocity = Vector2.Zero;
        CanDoLoopAttackTimer = 0;
        AttackFactor = 0f;
        AttackCount = 0;
        MoveSpeedBuffTime = 0f;
        AttackFactor2 = 0f;
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
        if (!flag) {
            return;
        }
        if (!player.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>()) {
            return;
        }

        player.GetModPlayer<HallowedGryphonHandler>().GainMoveSpeedBuff();
    }

    public void GainMoveSpeedBuff() {
        if (!CanIncreaseMoveSpeed) {
            return;
        }
        MoveSpeedBuffTime = MOVESPEEDBUFFTIMEINTICKS;
    }
}
