using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Bonerose : PlantBase, TileHooks.IGlobalRandomUpdate {
    protected override int PlantDrop => DropItem;

    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new(178, 178, 137), CreateMapEntryName());

        DustType = DustID.Bone;
        HitSound = SoundID.NPCHit2;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.Bonerose>();

        RootsDrawing.ShouldDraw[Type] = true;
    }

    void TileHooks.IGlobalRandomUpdate.OnGlobalRandomUpdate(int i, int j) {
        if (j < Main.worldSurface) {
            return;
        }

        if (!Main.wallDungeon[Main.tile[i, j].WallType]/* || !WorldGen.genRand.NextBool(150)*/) {
            return;
        }

        if (WorldGen.genRand.NextBool()) {
            return;
        }

        int style = WorldGen.gen ? WorldGen.genRand.Next(WorldGen.genRand.NextBool() ? 0 : WorldGen.genRand.NextBool() ? 1 : 2) : 0;
        int[] validTiles = AnchorValidTiles;
        ushort tileTypeToGrow = Type;
        for (int y = j - 1; y > 20; y--) {
            if (!WorldGen.InWorld(i, y, 30) || Main.tile[i, y].HasTile || !Main.tile[i, y + 1].HasUnactuatedTile || Main.tile[i, y + 1].Slope != SlopeType.Solid || Main.tile[i, y + 1].IsHalfBlock) {
                continue;
            }

            for (int k = 0; k < validTiles.Length; k++) {
                if (Main.tile[i, y + 1].TileType != validTiles[k]) {
                    continue;
                }

                if (j < Main.worldSurface) {
                    continue;
                }

                int num3 = 15;
                int num4 = 2;
                int num5 = 0;
                num3 = (int)((double)num3 * ((double)Main.maxTilesX / 4200.0));
                int num6 = Utils.Clamp(i - num3, 4, Main.maxTilesX - 4);
                int num7 = Utils.Clamp(i + num3, 4, Main.maxTilesX - 4);
                int num8 = Utils.Clamp(y - num3, 4, Main.maxTilesY - 4);
                int num9 = Utils.Clamp(y + num3, 4, Main.maxTilesY - 4);
                for (int i2 = num6; i2 <= num7; i2++) {
                    for (int j2 = num8; j2 <= num9; j2++) {
                        int chectTileType = Main.tile[i2, j2].TileType;
                        if (Main.tileAlch[chectTileType] || (chectTileType >= TileID.Count && TileLoader.GetTile(chectTileType) is PlantBase)) {
                            num5++;
                        }
                    }
                }

                if (num5 < num4) {
                    Tile tile = Main.tile[i, y];
                    tile.ClearTile();
                    tile.TileType = tileTypeToGrow;
                    tile.HasTile = true;
                    tile.TileFrameX = (short)(FrameWidth * style);
                    tile.CopyPaintAndCoating(Main.tile[i, y + 1]);
                    if (Main.tile[i, y].HasTile && Main.netMode == NetmodeID.Server) {
                        NetMessage.SendTileSquare(-1, i, y);
                    }
                }
            }
        }
    }
}
