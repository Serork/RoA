using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Gores;

interface ICustomGoreDraw {
    public void Draw(SpriteBatch spriteBatch, Gore gore);
}

sealed class CustomGoreDraw : IInitializer {
    void ILoadable.Load(Mod mod) {
        On_Main.DrawGoreBehind += On_Main_DrawGoreBehind;
        On_Main.DrawGore += On_Main_DrawGore;
    }

    private void On_Main_DrawGore(On_Main.orig_DrawGore orig, Main self) {
        orig(self);

        Main.drawBackGore = false;
        for (int i = 0; i < 600; i++) {
            if (!Main.gore[i].active || Main.gore[i].type <= 0)
                continue;

            if (Main.gore[i].ModGore is not ICustomGoreDraw customGoreDraw) {
                continue;
            }

            /*
			if (((gore[i].type >= 706 && gore[i].type <= 717) || gore[i].type == 943 || gore[i].type == 1147 || (gore[i].type >= 1160 && gore[i].type <= 1162)) && (gore[i].frame < 7 || gore[i].frame > 9)) {
			*/
            if (GoreID.Sets.DrawBehind[Main.gore[i].type] || (GoreID.Sets.LiquidDroplet[Main.gore[i].type] && Main.gore[i].frame is < 7 or > 9)) {
                Main.drawBackGore = true;
                continue;
            }

            Main.instance.LoadGore(Main.gore[i].type);

            customGoreDraw.Draw(Main.spriteBatch, Main.gore[i]);

            ////TML: Added '+ gore[i].drawOffset' to draw calls below
            //if (Main.gore[i].Frame.ColumnCount > 1 || Main.gore[i].Frame.RowCount > 1) {
            //    Microsoft.Xna.Framework.Rectangle sourceRectangle = Main.gore[i].Frame.GetSourceRectangle(TextureAssets.Gore[Main.gore[i].type].Value);
            //    Vector2 vector = new Vector2(0f, 0f);
            //    if (Main.gore[i].type == 1217)
            //        vector.Y += 4f;

            //    vector += Main.gore[i].drawOffset;
            //    Microsoft.Xna.Framework.Color alpha = Main.gore[i].GetAlpha(Lighting.GetColor((int)((double)Main.gore[i].position.X + (double)sourceRectangle.Width * 0.5) / 16, (int)(((double)Main.gore[i].position.Y + (double)sourceRectangle.Height * 0.5) / 16.0)));
            //    Main.spriteBatch.Draw(TextureAssets.Gore[Main.gore[i].type].Value, new Vector2(Main.gore[i].position.X - Main.screenPosition.X + (float)(sourceRectangle.Width / 2), Main.gore[i].position.Y - Main.screenPosition.Y + (float)(sourceRectangle.Height / 2) - 2f) + vector, sourceRectangle, alpha, Main.gore[i].rotation, new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2), Main.gore[i].scale, SpriteEffects.None, 0f);
            //}
            //else {
            //    Microsoft.Xna.Framework.Color alpha2 = Main.gore[i].GetAlpha(Lighting.GetColor((int)((double)Main.gore[i].position.X + (double)TextureAssets.Gore[Main.gore[i].type].Width() * 0.5) / 16, (int)(((double)Main.gore[i].position.Y + (double)TextureAssets.Gore[Main.gore[i].type].Height() * 0.5) / 16.0)));
            //    Main.spriteBatch.Draw(TextureAssets.Gore[Main.gore[i].type].Value, new Vector2(Main.gore[i].position.X - Main.screenPosition.X + (float)(TextureAssets.Gore[Main.gore[i].type].Width() / 2), Main.gore[i].position.Y - Main.screenPosition.Y + (float)(TextureAssets.Gore[Main.gore[i].type].Height() / 2)) + Main.gore[i].drawOffset, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Gore[Main.gore[i].type].Width(), TextureAssets.Gore[gore[i].type].Height()), alpha2, Main.gore[i].rotation, new Vector2(TextureAssets.Gore[Main.gore[i].type].Width() / 2, TextureAssets.Gore[Main.gore[i].type].Height() / 2), Main.gore[i].scale, SpriteEffects.None, 0f);
            //}
        }

        TimeLogger.DetailedDrawTime(24);
    }

    private void On_Main_DrawGoreBehind(On_Main.orig_DrawGoreBehind orig, Main self) {
        orig(self);

        for (int i = 0; i < 600; i++) {
            if (!Main.gore[i].active || Main.gore[i].type <= 0)
                continue;

            if (Main.gore[i].ModGore is not ICustomGoreDraw customGoreDraw) {
                continue;
            }

            bool flag = false;

            /*
			if (((gore[i].type >= 706 && gore[i].type <= 717) || gore[i].type == 943 || gore[i].type == 1147 || (gore[i].type >= 1160 && gore[i].type <= 1162)) && (gore[i].frame < 7 || gore[i].frame > 9))
			*/
            if (GoreID.Sets.DrawBehind[Main.gore[i].type] || (GoreID.Sets.LiquidDroplet[Main.gore[i].type] && Main.gore[i].frame is < 7 or > 9))
                flag = true;

            if (flag) {
                Main.instance.LoadGore(Main.gore[i].type);

                customGoreDraw.Draw(Main.spriteBatch, Main.gore[i]);

                ////TML: Added '+ gore[i].drawOffset' to draw calls below
                //if (gore[i].Frame.ColumnCount > 1 || gore[i].Frame.RowCount > 1) {
                //    Microsoft.Xna.Framework.Rectangle sourceRectangle = gore[i].Frame.GetSourceRectangle(TextureAssets.Gore[gore[i].type].Value);
                //    Microsoft.Xna.Framework.Color alpha = gore[i].GetAlpha(Lighting.GetColor((int)((double)gore[i].position.X + (double)sourceRectangle.Width * 0.5) / 16, (int)(((double)gore[i].position.Y + (double)sourceRectangle.Height * 0.5) / 16.0)));
                //    spriteBatch.Draw(TextureAssets.Gore[gore[i].type].Value, new Vector2(gore[i].position.X - screenPosition.X + (float)(sourceRectangle.Width / 2), gore[i].position.Y - screenPosition.Y + (float)(sourceRectangle.Height / 2) - 2f) + gore[i].drawOffset, sourceRectangle, alpha, gore[i].rotation, new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2), gore[i].scale, SpriteEffects.None, 0f);
                //}
                //else {
                //    Microsoft.Xna.Framework.Color alpha2 = gore[i].GetAlpha(Lighting.GetColor((int)((double)gore[i].position.X + (double)TextureAssets.Gore[gore[i].type].Width() * 0.5) / 16, (int)(((double)gore[i].position.Y + (double)TextureAssets.Gore[gore[i].type].Height() * 0.5) / 16.0)));
                //    spriteBatch.Draw(TextureAssets.Gore[gore[i].type].Value, new Vector2(gore[i].position.X - screenPosition.X + (float)(TextureAssets.Gore[gore[i].type].Width() / 2), gore[i].position.Y - screenPosition.Y + (float)(TextureAssets.Gore[gore[i].type].Height() / 2)) + gore[i].drawOffset, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Gore[gore[i].type].Width(), TextureAssets.Gore[gore[i].type].Height()), alpha2, gore[i].rotation, new Vector2(TextureAssets.Gore[gore[i].type].Width() / 2, TextureAssets.Gore[gore[i].type].Height() / 2), gore[i].scale, SpriteEffects.None, 0f);
                //}
            }
        }
    }
}
