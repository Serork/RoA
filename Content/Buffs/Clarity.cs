using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Clarity : ModBuff {
    private static bool _isDrawingLiquid, _isDrawingWaterfalls;

    public static float APPLIEDLIQUIDOPACITY => 0.375f;

    public override void Update(Player player, ref int buffIndex) => player.GetCommon().IsClarityEffectActive = true;

    public override void Load() {
        On_Main.DrawLiquid += On_Main_DrawLiquid;
        On_Lighting.GetCornerColors += On_Lighting_GetCornerColors;
        On_WaterfallManager.GetAlpha += On_WaterfallManager_GetAlpha;
        On_WaterfallManager.Draw += On_WaterfallManager_Draw;
    }

    private void On_WaterfallManager_Draw(On_WaterfallManager.orig_Draw orig, WaterfallManager self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
        _isDrawingWaterfalls = true;

        orig(self, spriteBatch);

        _isDrawingWaterfalls = false;
    }

    private float On_WaterfallManager_GetAlpha(On_WaterfallManager.orig_GetAlpha orig, float Alpha, int maxSteps, int waterfallType, int y, int s, Tile tileCache) {
        float result = orig(Alpha, maxSteps, waterfallType, y, s, tileCache);
        if (_isDrawingWaterfalls && Main.LocalPlayer.GetCommon().IsClarityEffectActive) {
            result *= APPLIEDLIQUIDOPACITY;
        }
        return result;
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
