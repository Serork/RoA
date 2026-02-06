using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Clarity : ModBuff {
    private static bool _isDrawingLiquid, _isDrawingWaterfalls;

    public static float APPLIEDLIQUIDOPACITY => 0.5f;

    public override void Update(Player player, ref int buffIndex) => player.GetCommon().IsClarityEffectActive = true;

    public override void Load() {
        On_Main.DrawLiquid += On_Main_DrawLiquid;
        On_Lighting.GetCornerColors += On_Lighting_GetCornerColors;
        On_WaterfallManager.GetAlpha += On_WaterfallManager_GetAlpha;
        On_WaterfallManager.Draw += On_WaterfallManager_Draw;
        On_LiquidRenderer.SetShimmerVertexColors += On_LiquidRenderer_SetShimmerVertexColors;
        On_LiquidRenderer.GetShimmerGlitterOpacity += On_LiquidRenderer_GetShimmerGlitterOpacity;
    }

    private float On_LiquidRenderer_GetShimmerGlitterOpacity(On_LiquidRenderer.orig_GetShimmerGlitterOpacity orig, bool top, float worldPositionX, float worldPositionY) {
        float result = orig(top, worldPositionX, worldPositionY);

        if (Main.LocalPlayer.GetCommon().IsClarityEffectActive) {
            float num = APPLIEDLIQUIDOPACITY;
            result *= num;
        }

        return result;
    }

    private void On_LiquidRenderer_SetShimmerVertexColors(On_LiquidRenderer.orig_SetShimmerVertexColors orig, ref Terraria.Graphics.VertexColors colors, float opacity, int x, int y) {
        orig(ref colors, opacity, x, y);

        if (Main.LocalPlayer.GetCommon().IsClarityEffectActive) {
            float num = APPLIEDLIQUIDOPACITY;
            colors.BottomLeftColor *= num;
            colors.BottomRightColor *= num;
            colors.TopLeftColor *= num;
            colors.TopRightColor *= num;
        }
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
