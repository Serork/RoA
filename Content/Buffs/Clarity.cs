using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Clarity : ModBuff {
    private static bool _isDrawingLiquid;

    public static float APPLIEDLIQUIDOPACITY => 0.5f;

    public override void Update(Player player, ref int buffIndex) => player.GetCommon().IsClarityEffectActive = true;

    public override void Load() {
        On_Main.DrawLiquid += On_Main_DrawLiquid;
        On_Lighting.GetCornerColors += On_Lighting_GetCornerColors;
    }

    private void On_Lighting_GetCornerColors(On_Lighting.orig_GetCornerColors orig, int centerX, int centerY, out Terraria.Graphics.VertexColors vertices, float scale) {
        orig(centerX, centerY, out vertices, scale);
        if (_isDrawingLiquid && Main.LocalPlayer.GetCommon().IsClarityEffectActive) {
            float num = APPLIEDLIQUIDOPACITY;
            vertices.BottomLeftColor *= num;
            vertices.BottomRightColor *= num;
            vertices.TopLeftColor *= num;
            vertices.TopRightColor *= num;
        }
    }

    private void On_Main_DrawLiquid(On_Main.orig_DrawLiquid orig, Main self, bool bg, int waterStyle, float Alpha, bool drawSinglePassLiquids) {
        _isDrawingLiquid = true;

        orig(self, bg, waterStyle, Alpha, drawSinglePassLiquids);

        _isDrawingLiquid = false;
    }
}
