using Microsoft.Xna.Framework;

using RoA.Common;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts.Backwoods;

sealed class Fog : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 12 * Main.rand.Next(4), 42, 12);
        dust.alpha = 255;
        dust.noGravity = true;
        dust.fadeIn = 3f;
    }

    public override bool Update(Dust dust) {
        dust.fadeIn -= TimeSystem.LogicDeltaTime;
        if (dust.fadeIn <= 0) {
            dust.active = false;
            return false;
        }

        bool flag = false;
        Point point = (dust.position + new Vector2(15f, 0f)).ToTileCoordinates();
        Tile tile = Main.tile[point.X, point.Y];
        Tile tile2 = Main.tile[point.X, point.Y + 1];
        Tile tile3 = Main.tile[point.X, point.Y + 2];
        if (tile == null || tile2 == null || tile3 == null) {
            dust.active = false;
            return false;
        }

        if (WorldGen.SolidTile(tile) || (!WorldGen.SolidTile(tile2) && !WorldGen.SolidTile(tile3)))
            flag = true;

        if (dust.fadeIn <= 0.3f)
            flag = true;

        dust.velocity.X = 0.2f * Main.WindForVisuals;
        dust.velocity.Y = 0f;
        if (!flag) {
            if (dust.alpha > 200)
                dust.alpha--;
        }
        else {
            dust.alpha++;
            if (dust.alpha >= 255) {
                dust.active = false;
                return false;
            }
        }

        dust.position += dust.velocity;

        return false;
    }
}
