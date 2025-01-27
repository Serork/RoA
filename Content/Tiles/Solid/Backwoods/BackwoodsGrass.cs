using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGrass : ModTile {
    public override void Load() {
        On_Player.DoBootsEffect_PlaceFlowersOnTile += On_Player_DoBootsEffect_PlaceFlowersOnTile;
    }

    private bool On_Player_DoBootsEffect_PlaceFlowersOnTile(On_Player.orig_DoBootsEffect_PlaceFlowersOnTile orig, Player self, int X, int Y) {
        Tile tile = WorldGenHelper.GetTileSafely(X, Y);
        if (!tile.HasTile && tile.LiquidAmount == 0 && Main.tile[X, Y + 1] != null && WorldGen.SolidTile(X, Y + 1)) {
            tile.TileFrameY = 0;
            tile.Slope = 0;
            tile.IsHalfBlock = false;

            if (Main.tile[X, Y + 1].TileType == ModContent.TileType<BackwoodsGrass>()) {
                tile.HasTile = true;
                tile.TileType = (ushort)ModContent.TileType<BackwoodsPlants>();
                tile.TileFrameX = (short)(18 * Main.rand.Next(20));
                tile.TileFrameY = 0;
                tile.CopyPaintAndCoating(Main.tile[X, Y + 1]);
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendTileSquare(-1, X, Y);
                }

                return true;
            }
        }

        return orig(self, X, Y);
    }

    public override void SetStaticDefaults() {
        TileHelper.Solid(Type);

        TileID.Sets.Grass[Type] = true;
		TileID.Sets.CanBeDugByShovel[Type] = true;
		TileID.Sets.NeedsGrassFraming[Type] = true;
		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<BackwoodsDirt>();
        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        TileID.Sets.Conversion.Grass[Type] = true;

        TransformTileSystem.OnKillActNormal[Type] = false;
        TransformTileSystem.ReplaceToTypeOnKill[Type] = TileID.Dirt;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(38, 107, 57));
	}

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => EmitDusts(i, j);

    public static void EmitDusts(int i, int j) {
        if (Main.rand.NextBool(300)) {
            Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<BackwoodsDust>());
        }
    }
}