using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ModLiquidLib.ModLoader;

using ReLogic.Content;

using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace RoA.Content.Liquids;

sealed class TarFall : ModLiquidFall {
    private static int _wFallFrCounter2;
    private static int _slowFrame;
    private Asset<Texture2D>? _waterFallTexture;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _waterFallTexture = ModContent.Request<Texture2D>(Texture);
        }
    }

    public override void Load() {
        On_WaterfallManager.StylizeColor += On_WaterfallManager_StylizeColor;
        On_WaterfallManager.UpdateFrame += On_WaterfallManager_UpdateFrame;
        On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += On_WaterfallManager_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects;
        On_WaterfallManager.AddLight += On_WaterfallManager_AddLight;
    }

    public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
        int j = s;
        int num22 = maxSteps;
        float num25 = 1f;
        float num26 = 0.3f;
        if (j > num22 - 8) {
            float num27 = (float)(num22 - j) / 8f;
            num25 *= num27;
            num26 *= num27;
        }
        return num25;
    }

    public override void AnimateWaterfall(ref int frame, ref int frameBackground, ref int frameCounter) {
        frame = _slowFrame;
    }

    private void On_WaterfallManager_AddLight(On_WaterfallManager.orig_AddLight orig, int waterfallType, int x, int y) {
        if (waterfallType == ModContent.GetInstance<TarFall>().Slot) {
            return;
        }

        orig(waterfallType, x, y);
    }

    private void On_WaterfallManager_UpdateFrame(On_WaterfallManager.orig_UpdateFrame orig, WaterfallManager self) {
        orig(self);

        _wFallFrCounter2++;
        if (_wFallFrCounter2 > 8) {
            _wFallFrCounter2 = 0;
            _slowFrame++;
            if (_slowFrame > 15)
                _slowFrame = 0;
        }
    }

    private void On_WaterfallManager_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects) {
        if (_waterFallTexture?.IsLoaded == true && waterfallType == ModContent.GetInstance<TarFall>().Slot) {
            Texture2D value = _waterFallTexture!.Value;
            Lighting.GetCornerColors(x, y, out VertexColors vertices);
            Tar.SetTarVertexColors(ref vertices, opacity, x, y);
            Main.tileBatch.Draw(value, position + new Vector2(0f, 0f), sourceRect, vertices, default, 1f, effects);

            return;
        }

        orig(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
    }

    private Microsoft.Xna.Framework.Color On_WaterfallManager_StylizeColor(On_WaterfallManager.orig_StylizeColor orig, float alpha, int maxSteps, int waterfallType, int y, int s, Tile tileCache, Microsoft.Xna.Framework.Color aColor) {
        if (waterfallType == ModContent.GetInstance<TarFall>().Slot) {
            float num = (float)(int)aColor.R * alpha;
            float num2 = (float)(int)aColor.G * alpha;
            float num3 = (float)(int)aColor.B * alpha;
            float num4 = (float)(int)aColor.A * alpha;

            if (num < 190f * alpha)
                num = 190f * alpha;
            if (num2 < 190f * alpha)
                num2 = 190f * alpha;
            if (num3 < 190f * alpha)
                num3 = 190f * alpha;

            aColor = new Color((int)num, (int)num2, (int)num3, (int)num4);
            return aColor;
        }

        return orig(alpha, maxSteps, waterfallType, y, s, tileCache, aColor);
    }
}
