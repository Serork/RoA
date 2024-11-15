using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ElderwoodClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(12, 3f);
    }

    protected override (Color, Color) SlashColors() => (new(72, 86, 214), new(114, 126, 255));

    public override void WhileBeingHold(Player player, float progress) {
        if (progress >= 0.5f) {
            if (player.itemTime > player.itemTimeMax - 5) {
                Vector2 startPoint = player.Center;
                Vector2 destination = startPoint + Vector2.UnitX/* * player.direction*/ * 100f;
                while (Vector2.Distance(startPoint, destination) > 32f) {
                    if (Collision.SolidCollision(startPoint, 0, 0)) {
                        break;
                    }
                    startPoint = Vector2.Lerp(startPoint, destination, 1f);
                }
                Vector2 endPoint = player.Center;
                destination = endPoint + Vector2.UnitX /** player.direction */* 150f;
                while (Vector2.Distance(endPoint, destination) > 32f) {
                    if (Collision.SolidCollision(endPoint, 0, 0)) {
                        break;
                    }
                    endPoint = Vector2.Lerp(endPoint, destination, 1f);
                }
                Point point1 = startPoint.ToTileCoordinates();
                while (!WorldGen.SolidTile(point1)) {
                    point1.Y++;
                    if (Vector2.Distance(startPoint, player.Center) > player.height * 5) {
                        break;
                    }
                }
                Point point2 = endPoint.ToTileCoordinates();
                while (!WorldGen.SolidTile(point2)) {
                    point2.Y++;
                    if (Vector2.Distance(endPoint, player.Center) > player.height * 5) {
                        break;
                    }
                }
                SpawnGroundDusts(point1, point2, progress * 7.5f);
            }
        }
    }

    private void SpawnGroundDusts(Point startPoint, Point endPoint, float strength, float maxDist = 0f, Vector2? center = null) {
        float num4 = strength / 3;
        for (int i = startPoint.X; i <= endPoint.X; i++) {
            for (int j = startPoint.Y; j <= endPoint.Y; j++) {
                if (center != null && Vector2.Distance(center.Value, new Vector2(i * 16, j * 16)) > maxDist)
                    continue;
                Tile tileSafely = Framing.GetTileSafely(i, j);
                if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType]) {
                    continue;
                }
                Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType]) {
                    continue;
                }
                int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
                for (int k = 0; k < num5; k++) {
                    Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                    obj.velocity.Y -= 3f + num4 * 1.5f;
                    obj.velocity.Y *= Main.rand.NextFloat();
                    obj.velocity.Y *= 0.75f;
                    obj.scale += num4 * 0.03f;
                }
                if (num4 >= 2) {
                    for (int m = 0; m < num5 - 1; m++) {
                        Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj2.velocity.Y -= 1f + num4;
                        obj2.velocity.Y *= Main.rand.NextFloat();
                        obj2.velocity.Y *= 0.75f;
                    }
                }
                if (num4 >= 2) {
                    for (int m = 0; m < num5 - 1; m++) {
                        Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj2.velocity.Y -= 1f + num4;
                        obj2.velocity.Y *= Main.rand.NextFloat();
                        obj2.velocity.Y *= 0.75f;
                    }
                }
                if (num5 <= 0 || Main.rand.Next(3) == 0) {
                    continue;
                }
            }
        }
    }
}
