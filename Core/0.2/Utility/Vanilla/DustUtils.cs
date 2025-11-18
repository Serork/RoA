using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;

namespace RoA.Core.Utility.Vanilla;

static class DustUtils {
    public static void QuickDraw(this Dust dust, Texture2D texture, Color? color = null) {
        Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, dust.frame,
            color ?? dust.GetAlpha(Lighting.GetColor(dust.position.ToTileCoordinates())), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
    }
}
