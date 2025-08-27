using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Core;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA;

interface IPostLiquidDraw { }

sealed class IPostLiquidDraw_DrawDusts : IInitializer {
    void ILoadable.Load(Mod mod) {
        On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
    }

    private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self) {
        orig(self);
        DrawGoresAndDusts();
    }

    private void DrawGoresAndDusts() {
        Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)Main.screenPosition.X - 500, (int)Main.screenPosition.Y - 50, Main.screenWidth + 1000, Main.screenHeight + 100);
        rectangle = new Microsoft.Xna.Framework.Rectangle((int)Main.screenPosition.X - 1000, (int)Main.screenPosition.Y - 1050, Main.screenWidth + 2000, Main.screenHeight + 2100);
        Microsoft.Xna.Framework.Rectangle rectangle2 = rectangle;
        ArmorShaderData armorShaderData = null;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(Main.spriteBatch);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        for (int i = 0; i < 600; i++) {
            if (!Main.gore[i].active || Main.gore[i].type <= 0)
                continue;

            if (Main.gore[i].ModGore is not IPostLiquidDraw) {
                continue;
            }

            /*
			if (((gore[i].type >= 706 && gore[i].type <= 717) || gore[i].type == 943 || gore[i].type == 1147 || (gore[i].type >= 1160 && gore[i].type <= 1162)) && (gore[i].frame < 7 || gore[i].frame > 9)) {
			*/
            if (GoreID.Sets.DrawBehind[Main.gore[i].type] || (GoreID.Sets.LiquidDroplet[Main.gore[i].type] && Main.gore[i].frame is < 7 or > 9)) {
                continue;
            }

            //TML: Added '+ gore[i].drawOffset' to draw calls below
            if (Main.gore[i].Frame.ColumnCount > 1 || Main.gore[i].Frame.RowCount > 1) {
                Microsoft.Xna.Framework.Rectangle sourceRectangle = Main.gore[i].Frame.GetSourceRectangle(TextureAssets.Gore[Main.gore[i].type].Value);
                Vector2 vector = new Vector2(0f, 0f);
                if (Main.gore[i].type == 1217)
                    vector.Y += 4f;

                vector += Main.gore[i].drawOffset;
                Microsoft.Xna.Framework.Color alpha = Main.gore[i].GetAlpha(Lighting.GetColor((int)((double)Main.gore[i].position.X + (double)sourceRectangle.Width * 0.5) / 16, (int)(((double)Main.gore[i].position.Y + (double)sourceRectangle.Height * 0.5) / 16.0)));
                Main.spriteBatch.Draw(TextureAssets.Gore[Main.gore[i].type].Value, new Vector2(Main.gore[i].position.X - Main.screenPosition.X + (float)(sourceRectangle.Width / 2), Main.gore[i].position.Y - Main.screenPosition.Y + (float)(sourceRectangle.Height / 2) - 2f) + vector, sourceRectangle, alpha, Main.gore[i].rotation, new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2), Main.gore[i].scale, SpriteEffects.None, 0f);
            }
            else {
                Microsoft.Xna.Framework.Color alpha2 = Main.gore[i].GetAlpha(Lighting.GetColor((int)((double)Main.gore[i].position.X + (double)TextureAssets.Gore[Main.gore[i].type].Width() * 0.5) / 16, (int)(((double)Main.gore[i].position.Y + (double)TextureAssets.Gore[Main.gore[i].type].Height() * 0.5) / 16.0)));
                Main.spriteBatch.Draw(TextureAssets.Gore[Main.gore[i].type].Value, new Vector2(Main.gore[i].position.X - Main.screenPosition.X + (float)(TextureAssets.Gore[Main.gore[i].type].Width() / 2), Main.gore[i].position.Y - Main.screenPosition.Y + (float)(TextureAssets.Gore[Main.gore[i].type].Height() / 2)) + Main.gore[i].drawOffset, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Gore[Main.gore[i].type].Width(), TextureAssets.Gore[Main.gore[i].type].Height()), alpha2, Main.gore[i].rotation, new Vector2(TextureAssets.Gore[Main.gore[i].type].Width() / 2, TextureAssets.Gore[Main.gore[i].type].Height() / 2), Main.gore[i].scale, SpriteEffects.None, 0f);
            }
        }
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
        for (int i = 0; i < Main.maxDustToDraw; i++) {
            Dust dust = Main.dust[i];

            if (!dust.active)
                continue;

            ModDust modDust = DustLoader.GetDust(dust.type);

            if (modDust is not IPostLiquidDraw) {
                continue;
            }

            if (new Microsoft.Xna.Framework.Rectangle((int)dust.position.X, (int)dust.position.Y, 4, 4).Intersects(rectangle)) {
                float scale = dust.GetVisualScale();
                if (dust.shader != armorShaderData) {
                    Main.spriteBatch.End();
                    armorShaderData = dust.shader;
                    if (armorShaderData == null) {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                    }
                    else {
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                        dust.shader.Apply(null);
                    }
                }

                Microsoft.Xna.Framework.Color newColor = Lighting.GetColor((int)((double)dust.position.X + 4.0) / 16, (int)((double)dust.position.Y + 4.0) / 16);
                if (dust.type == 6 || dust.type == 15 || (dust.type >= 59 && dust.type <= 64))
                    newColor = Microsoft.Xna.Framework.Color.White;

                newColor = dust.GetAlpha(newColor);
                if (dust.type == 213)
                    scale = 1f;

                Main.spriteBatch.Draw(modDust.Texture2D.Value, dust.position - Main.screenPosition, dust.frame, newColor, dust.GetVisualRotation(), new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
                if (dust.color.PackedValue != 0) {
                    Microsoft.Xna.Framework.Color color6 = dust.GetColor(newColor);
                    if (color6.PackedValue != 0)
                        Main.spriteBatch.Draw(modDust.Texture2D.Value, dust.position - Main.screenPosition, dust.frame, color6, dust.GetVisualRotation(), new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
                }

                if (newColor == Microsoft.Xna.Framework.Color.Black)
                    dust.active = false;
            }
            else {
                dust.active = false;
            }
        }
        Main.spriteBatch.Begin(snapshot, true);
    }
}
