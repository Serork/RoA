using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Common.Tiles;
using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class Acalypha : CollectableFlower, IGrowLikeTulip {
    private static bool _hallowOnLeft = true;

    public override void Load() {
        On_WorldGen.GERunner += On_WorldGen_GERunner;
        On_WorldGen.smCallback_End += On_WorldGen_smCallback_End;
    }

    private void On_WorldGen_GERunner(On_WorldGen.orig_GERunner orig, int i, int j, double speedX, double speedY, bool good) {
        orig(i, j, speedX, speedY, good);
        if (good) {
            _hallowOnLeft = Math.Sign(speedX) > 0;
        }
    }
    private void On_WorldGen_smCallback_End(On_WorldGen.orig_smCallback_End orig, System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> hardmodeTasks) {
        orig(hardmodeTasks);

        ushort acalyphaTileType = (ushort)ModContent.TileType<Acalypha>();
        bool flag = false;
        bool tryToGrowAcalypha(int k, int l) {
            if (Main.tile[k, l].HasTile && GrowPlantsOverTime.CanGrowAPlant(acalyphaTileType, k, l)) {
                GrowPlantsOverTime.GrowAPlant(acalyphaTileType, k, l);
                if (Main.tile[k, l - 1].TileType == acalyphaTileType) {
                    Main.LocalPlayer.position = new Point(k, l).ToWorldCoordinates();
                    return true;
                }
            }
            return false;
        }
        if (_hallowOnLeft) {
            for (int k = 20; k < Main.maxTilesX - 20; k++) {
                if (flag) {
                    break;
                }
                for (int l = 20; l < Main.maxTilesY - 20; l++) {
                    if (tryToGrowAcalypha(k, l)) {
                        flag = true;
                        break;
                    }
                }
            }
        }
        else {
            for (int k = Main.maxTilesX - 20; k > 20; k--) {
                if (flag) {
                    break;
                }
                for (int l = 20; l < Main.maxTilesY - 20; l++) {
                    if (tryToGrowAcalypha(k, l)) {
                        flag = true;
                        break;
                    }
                }
            }
        }
    }

    protected override ushort DropItemType => (ushort)ModContent.ItemType<SacredAcalypha>();
    protected override Color MapColor => new(246, 73, 112);
    protected override int[] AnchorValidTileTypes => [TileID.HallowedGrass, TileID.GolfGrassHallowed, TileID.Pearlsand];
    protected override ushort HitDustType => (ushort)DustID.HallowedPlants;

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        width = height = 30;
        offsetY -= 12;
    }

    Predicate<Point16> IGrowLikeTulip.ShouldGrow => (tilePosition) => {
        int i = tilePosition.X, j = tilePosition.Y;
        Tile tile = Main.tile[i, j];
        if (!(AnchorValidTileTypes.Contains(tile.TileType) && !Main.tile[i, j - 1].AnyWall())) {
            return false;
        }
        bool flag = true;
        if (Main.tile[i, j - 1].LiquidAmount > 0 && Main.tile[i, j - 1].LiquidType != LiquidID.Water) {
            flag = false;
        }
        return flag;
    };
}
