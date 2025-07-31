using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ClingerHideway : ModDust {
    public const byte COLUMNCOUNT = 1;
    public const byte ROWCOUNT = 2;

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        dust.rotation *= 0.75f;
        if (dust.scale <= 0.9f) {
            dust.scale *= 0.98f;
        }

        return false;
    }

    public override bool PreDraw(Dust dust) {
        dust.frame = (Texture2D?.Value?.Frame(COLUMNCOUNT, ROWCOUNT, frameY: dust.customData is byte frame ? frame : 0)).GetValueOrDefault();

        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 screenPosition = Main.screenPosition;
        Vector2 origin = dust.frame.Size() / 2f;
        float scale = dust.scale;
        Color newColor = Lighting.GetColor((int)((double)dust.position.X + origin.X) / 16, (int)((double)dust.position.Y + origin.Y) / 16);
        newColor = dust.GetAlpha(newColor);
        spriteBatch.Draw(Texture2D.Value, dust.position - screenPosition, dust.frame, newColor, dust.GetVisualRotation(), origin, scale, SpriteEffects.None, 0f);
        if (dust.color.PackedValue != 0) {
           Color color6 = dust.GetColor(newColor);
            if (color6.PackedValue != 0)
                spriteBatch.Draw(Texture2D.Value, dust.position - screenPosition, dust.frame, color6, dust.GetVisualRotation(), origin, scale, SpriteEffects.None, 0f);
        }


        return false;
    }
}
