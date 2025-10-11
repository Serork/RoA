using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using RoA.Common;
using RoA.Common.Tiles;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Projectiles.LiquidsSpecific;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Liquids;

sealed class FishingInTar : ModPlayer {
    public override void Load() {
        TileCommon.RandomUpdateEvent += TileCommon_RandomUpdateEvent;
    }

    private void TileCommon_RandomUpdateEvent(int i, int j, int type) {
        if (!Main.hardMode) {
            return;
        }

        int liquidType = Main.tile[i, j].LiquidType;
        if (liquidType == LiquidLoader.LiquidType<Tar>()) {
            GetFishingPondState(i, j, out int waterTilesCount);
            if (waterTilesCount < 75) {
                return;
            }

            Vector2 worldPosition = new Point16(i, j).ToWorldCoordinates();
            foreach (Projectile boneRemains in TrackedEntitiesSystem.GetTrackedProjectile<FloatingRemains>()) {
                if (boneRemains.Distance(worldPosition) < 320f) {
                    return;
                }
            }
            Projectile.NewProjectile(new EntitySource_TileUpdate(i, j, context: "FloatingRemains"), i * 16, j * 16, 0f, 0f, ModContent.ProjectileType<FloatingRemains>(), 0, 0f, Main.myPlayer);
        }
    }

    private static void GetFishingPondState(int x, int y, out int numWaters) {
        numWaters = 0;
        Point tileCoords = new Point(0, 0);
        GetFishingPondWidth(x, y, out var minX, out var maxX);
        for (int i = minX; i <= maxX; i++) {
            int num = y;
            while (Main.tile[i, num] != null && Main.tile[i, num].LiquidAmount > 0 && !WorldGen.SolidTile(i, num) && num < Main.maxTilesY - 10) {
                numWaters++;
                num++;
                tileCoords.X = i;
                tileCoords.Y = num;
            }
        }

        numWaters = (int)((double)numWaters * 1.5);
    }

    private static void GetFishingPondWidth(int x, int y, out int minX, out int maxX) {
        minX = x;
        maxX = x;
        while (minX > 10 && Main.tile[minX, y] != null && Main.tile[minX, y].LiquidAmount > 0 && !WorldGen.SolidTile(minX, y)) {
            minX--;
        }

        while (maxX < Main.maxTilesX - 10 && Main.tile[maxX, y] != null && Main.tile[maxX, y].LiquidAmount > 0 && !WorldGen.SolidTile(maxX, y)) {
            maxX++;
        }
    }

    public override void ModifyFishingAttempt(ref FishingAttempt attempt) {
        if (!Main.hardMode) {
            return;
        }

        int liquidType = Main.tile[attempt.X, attempt.Y].LiquidType;
        if (liquidType == LiquidLoader.LiquidType<Tar>()) {
            attempt.fishingLevel /= 10;
        }
    }

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
        int liquidType = Main.tile[attempt.X, attempt.Y].LiquidType;
        Vector2 worldPosition = new Point16(attempt.X, attempt.Y).ToWorldCoordinates();
        if (liquidType == LiquidLoader.LiquidType<Tar>()) {
            if (!Main.hardMode) {
                itemDrop = 0;
                return;
            }

            bool canCatchFish = false;
            foreach (Projectile boneRemains in TrackedEntitiesSystem.GetTrackedProjectile<FloatingRemains>()) {
                if (boneRemains.Distance(worldPosition) < 48f && Collision.CanHit(boneRemains.position, boneRemains.width, boneRemains.height, worldPosition + Vector2.One * 8f, 1, 1)) {
                    canCatchFish = true;
                    break;
                }
            }
            if (!canCatchFish) {
                itemDrop = 0;
            }
            else {
                if (Main.rand.NextBool(3)) {
                    itemDrop = Main.rand.Next(2337, 2340);
                    return;
                }

                itemDrop = ModContent.ItemType<CrushedRemains>();
            }
        }
    }
}
