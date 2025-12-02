using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.Items;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;

sealed class ElderwoodClaws : ClawsBaseItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26);
        Item.SetWeaponValues(14, 4f);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<ClawsSlash>(), 1.2f);

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 22, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 16);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override (Color, Color) SetSlashColors(Player player) => (new(62, 86, 80), new(94, 110, 102));

    private Vector2 GetPos(Player player, bool leftSided) {
        int direction = leftSided ? -1 : 1;
        GetPoints(player, direction, out Point point1, out Point point2);
        Vector2 position = GetPoint(point1, point2).ToWorldCoordinates();
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            int attempts = 10;
            while (projectile.owner == player.whoAmI && projectile.type == Type &&
                   position.X < projectile.position.X + 30 && position.X > projectile.position.X - 30) {
                position.X -= (30 + 2) * (position - player.position).X.GetDirection();
                if (--attempts <= 0) {
                    break;
                }
            }
        }
        return position;
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        ushort type = (ushort)ModContent.ProjectileType<ElderwoodWallProjectile>();
        args.SpawnProjectile = () => {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "ClawsRoot") { Volume = 2.5f }, player.Center);
            for (int k = 0; k < 2; k++) {
                switch (k) {
                    case 0:
                        for (int k2 = 0; k2 < 3; k2++) {
                            Vector2 position = GetPos(player, false);
                            Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                                   position,
                                   Vector2.Zero,
                                   type,
                                   NatureWeaponHandler.GetNatureDamage(Item, player),
                                   player.GetTotalKnockback(DruidClass.Nature).ApplyTo(Item.knockBack),
                                   player.whoAmI,
                                   5f - k2 + Main.rand.Next(-k2 / 2, k2 / 2),
                                   2f);
                        }
                        break;
                    case 1:
                        for (int k2 = 0; k2 < 3; k2++) {
                            Vector2 position = GetPos(player, true);
                            Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                                   position,
                                   Vector2.Zero,
                                   type,
                                   NatureWeaponHandler.GetNatureDamage(Item, player),
                                   player.GetTotalKnockback(DruidClass.Nature).ApplyTo(Item.knockBack),
                                   player.whoAmI,
                                   5f - k2 + Main.rand.Next(-k2 / 2, k2 / 2),
                                   2f);
                        }
                        break;
                }
            }
        };
        args.OnAttack = () => {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 3, player.Center));
            }
        };
    }

    private static void GetPoints(Player player, int direction, out Point point1, out Point point2) {
        Vector2 startPoint = player.Center - Vector2.UnitY * player.height * 3f;
        Vector2 dir = new(player.direction * direction, 0f);
        Vector2 destination = startPoint + dir * 75f;
        while (Vector2.Distance(startPoint, destination) > 32f) {
            if (WorldGenHelper.CustomSolidCollision(startPoint, 0, 0, TileID.Sets.Platforms)) {
                break;
            }
            startPoint = Vector2.Lerp(startPoint, destination, 1f);
        }
        Vector2 endPoint = player.Center;
        destination = endPoint + dir * 125f;
        while (Vector2.Distance(endPoint, destination) > 32f) {
            if (WorldGenHelper.CustomSolidCollision(endPoint, 0, 0, TileID.Sets.Platforms)) {
                break;
            }
            endPoint = Vector2.Lerp(endPoint, destination, 1f);
        }
        point1 = startPoint.ToTileCoordinates();
        while (!WorldGen.SolidTile(point1)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point1.X, point1.Y).TileType]) {
                break;
            }
            point1.Y++;
            if (Vector2.Distance(startPoint, player.Center) > player.height * 3f) {
                break;
            }
        }
        point2 = endPoint.ToTileCoordinates();
        while (!WorldGen.SolidTile(point2)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point2.X, point2.Y).TileType]) {
                break;
            }
            point2.Y++;
            if (Vector2.Distance(endPoint, player.Center) > player.height * 3f) {
                break;
            }
        }
    }

    private static bool Handle(int i, int j, float strength = 1f, float maxDist = 0f, Vector2? center = null, bool spawnDust = false) {
        float num4 = strength / 3;
        if (center != null && Vector2.Distance(center.Value, new Vector2(i * 16, j * 16)) > maxDist)
            return false;
        Tile tileSafely = Framing.GetTileSafely(i, j);
        if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType]) {
            if (!TileID.Sets.Platforms[tileSafely.TileType]) {
                return false;
            }
        }
        Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
        if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType]) {
            return false;
        }

        if (spawnDust) {
            int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j) / 2;
            for (int k = 0; k < num5; k++) {
                Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                obj.velocity.Y -= 3f + num4 * 1.5f;
                obj.velocity.Y *= Main.rand.NextFloat();
                obj.velocity.Y *= 0.75f;
                obj.velocity *= 0.5f;
                obj.scale += num4 * 0.03f;
            }
            if (num4 >= 2) {
                for (int m = 0; m < num5 - 1; m++) {
                    Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                    obj2.velocity.Y -= 1f + num4;
                    obj2.velocity.Y *= Main.rand.NextFloat();
                    obj2.velocity.Y *= 0.75f;
                    obj2.velocity *= 0.5f;
                }
            }
            if (num4 >= 2) {
                for (int m = 0; m < num5 - 1; m++) {
                    Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                    obj2.velocity.Y -= 1f + num4;
                    obj2.velocity.Y *= Main.rand.NextFloat();
                    obj2.velocity.Y *= 0.75f;
                    obj2.velocity *= 0.5f;
                }
            }
            if (num5 <= 0 || Main.rand.Next(3) == 0) {
                return false;
            }
        }
        return true;
    }

    private static Point GetPoint(Point startPoint, Point endPoint) {
        if (startPoint.X > endPoint.X) {
            for (int i = endPoint.X; i <= startPoint.X; i++) {
                for (int j = startPoint.Y; j <= endPoint.Y; j++) {
                    if (Handle(i, j)) {
                        return new Point(i + (startPoint.X - endPoint.X) / 2 + 1, j + 1);
                    }
                }
            }
        }
        else {
            for (int i = startPoint.X; i <= endPoint.X; i++) {
                for (int j = startPoint.Y; j <= endPoint.Y; j++) {
                    if (Handle(i, j)) {
                        return new Point(i + (endPoint.X - startPoint.X) / 2 + 1, j + 1);
                    }
                }
            }
        }

        return Point.Zero;
    }

    public static void SpawnGroundDusts(Point startPoint, Point endPoint, float strength, float maxDist = 0f, Vector2? center = null) {
        if (startPoint.X > endPoint.X) {
            for (int i = endPoint.X; i <= startPoint.X; i++) {
                for (int j = startPoint.Y; j <= endPoint.Y; j++) {
                    if (!Handle(i, j, strength, maxDist, center, true)) {
                        continue;
                    }
                }
            }
        }
        else {
            for (int i = startPoint.X; i <= endPoint.X; i++) {
                for (int j = startPoint.Y; j <= endPoint.Y; j++) {
                    if (!Handle(i, j, strength, maxDist, center, true)) {
                        continue;
                    }
                }
            }
        }
    }
}
