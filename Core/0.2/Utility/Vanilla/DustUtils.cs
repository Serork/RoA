using Microsoft.Xna.Framework.Graphics;

using Terraria;

namespace RoA.Core.Utility.Vanilla;

static class DustUtils {
    public static void QuickDraw(this Dust dust, Texture2D texture) {
        Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
    }
}
