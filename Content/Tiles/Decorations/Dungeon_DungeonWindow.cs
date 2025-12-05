using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class DungeonWindow : ModTile, IPostSetupContent {
    public static float OPACITY => 1f;

    public override void SetStaticDefaults() {
        TileID.Sets.IsBeam[Type] = true;

        AddMapEntry(new Microsoft.Xna.Framework.Color(46, 31, 38));

        HitSound = SoundID.Shatter;

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        type = ModContent.DustType<FlamingFabricDust>();

        return base.CreateDust(i, j, ref type);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        height = 18;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        drawData.finalColor *= OPACITY;
    }

    void IPostSetupContent.PostSetupContent() {
        AutomaticallyMakeOtherTilesAnchorToThisTile();
    }

    private void AutomaticallyMakeOtherTilesAnchorToThisTile() {
        for (int i = 0; i < TileLoader.TileCount; i++) {
            TileObjectData objData = TileObjectData.GetTileData(i, 0);
            if (objData == null || objData.AnchorAlternateTiles == null || objData.AnchorAlternateTiles.Length == 0) {
                continue;
            }

            if (objData.AnchorAlternateTiles.Any(tileId => tileId == TileID.BorealBeam)) {
                lock (objData) {
                    int[] anchorAlternates = objData.AnchorAlternateTiles;
                    Array.Resize(ref anchorAlternates, anchorAlternates.Length + 1);
                    anchorAlternates[^1] = ModContent.TileType<FlamingFabric>();
                    objData.AnchorAlternateTiles = anchorAlternates;
                }
            }
        }

        for (int i = TileID.Count; i < TileLoader.TileCount; i++) {
            ModTile modTile = TileLoader.GetTile(i);

            if (modTile == null || modTile.AdjTiles == null || modTile.AdjTiles.Length == 0 || !Main.tileTable[i] || !Main.tileSolidTop[i]) {
                continue;
            }

            if (modTile.AdjTiles.Any(tileId => tileId == TileID.BorealBeam)) {
                Main.tileMerge[ModContent.TileType<FlamingFabric>()][i] = true;
            }
        }
    }
}
