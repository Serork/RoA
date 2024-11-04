using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

abstract class PlantBase : ModTile {
    private class StaffOfRegrowthHerbsHelper : ILoadable {
        public void Load(Mod mod) {
            On_Player.PlaceThing_Tiles_BlockPlacementForAssortedThings += Player_PlaceThing_Tiles_BlockPlacementForAssortedThings;
        }

        public void Unload() { }

        private static bool Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(On_Player.orig_PlaceThing_Tiles_BlockPlacementForAssortedThings orig, Player player, bool canPlace) {
            if (player.HeldItem.type == ItemID.StaffofRegrowth && Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile
                && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType >= TileID.Count && TileLoader.GetTile(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType) is PlantBase plantTile) {
                if (plantTile.IsGrown(Player.tileTargetX, Player.tileTargetY)) {
                    WorldGen.KillTile(Player.tileTargetX, Player.tileTargetY);
                    if (!Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, Player.tileTargetX, Player.tileTargetY);
                    }
                }

                canPlace = true;
            }

            return orig(player, canPlace);
        }
    }

    protected virtual short FrameWidth => 18;

    protected virtual int[] AnchorValidTiles => [];

    protected virtual ushort DropItem { get; set; }

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileSpelunker[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.AnchorAlternateTiles = [TileID.ClayPot, TileID.PlanterBox];
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    protected virtual PlantStage GetStage(int i, int j) => (PlantStage)(WorldGenHelper.GetTileSafely(i, j).TileFrameX / FrameWidth);
    protected virtual bool IsGrowing(int i, int j) => GetStage(i, j) == PlantStage.Growing;
    protected virtual bool IsGrown(int i, int j) => GetStage(i, j) == PlantStage.Grown;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (IsGrown(i, j)) {
            yield return new Item(DropItem);
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsGrown(i, j) ? Main.rand.Next(3, 6) : IsGrowing(i, j) ? Main.rand.Next(2, 5) : Main.rand.Next(1, 3);

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = -4;

    public override bool IsTileSpelunkable(int i, int j) => IsGrown(i, j);

    public override void RandomUpdate(int i, int j) {
        if (!IsGrown(i, j)) {
            WorldGenHelper.GetTileSafely(i, j).TileFrameX += FrameWidth;
            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
    }

    protected bool TryPlacePlant(int i, int j, int style = 0, params int[] validTile){
        ushort tileTypeToGrow = Type;
        for (int y = j - 1; y > 20; y--) {
            if (!WorldGen.InWorld(i, y, 30) || Main.tile[i, y].HasTile || !Main.tile[i, y + 1].HasUnactuatedTile || Main.tile[i, y + 1].Slope != SlopeType.Solid || Main.tile[i, y + 1].IsHalfBlock) {
                continue;
            }

            for (int k = 0; k < validTile.Length; k++) {
                if (Main.tile[i, y + 1].TileType != validTile[k]) {
                    continue;
                }

                int num3 = 15;
                int num4 = 5;
                int num5 = 0;
                num3 = (int)((double)num3 * ((double)Main.maxTilesX / 4200.0));
                int num6 = Utils.Clamp(i - num3, 4, Main.maxTilesX - 4);
                int num7 = Utils.Clamp(i + num3, 4, Main.maxTilesX - 4);
                int num8 = Utils.Clamp(j - num3, 4, Main.maxTilesY - 4);
                int num9 = Utils.Clamp(j + num3, 4, Main.maxTilesY - 4);
                for (int i2 = num6; i2 <= num7; i2++) {
                    for (int j2 = num8; j2 <= num9; j2++) {
                        if (Main.tileAlch[Main.tile[i2, j2].TileType])
                            num5++;
                    }
                }

                if (num5 < num4) {
                    Tile tile = Main.tile[i, y];
                    PlantBase plant = TileLoader.GetTile(tileTypeToGrow) as PlantBase;
                    tile.ClearTile();
                    tile.TileType = tileTypeToGrow;
                    tile.HasTile = true;
                    tile.TileFrameX = (short)(plant.FrameWidth * style);
                    tile.CopyPaintAndCoating(Main.tile[i, y + 1]);
                    if (Main.netMode != NetmodeID.SinglePlayer) {
                        NetMessage.SendTileSquare(-1, i - 1, y - 1, 3, 3);
                    }
                }

                return true;
            }
        }
        return false;
    }
}
