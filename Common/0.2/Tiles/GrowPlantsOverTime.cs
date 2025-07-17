using RoA.Content.Tiles.Miscellaneous;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.Tiles;

interface IGrowLikeTulip {
    Predicate<Point16> ShouldGrow { get; }
}

sealed class GrowPlantsOverTime : IPostSetupContent {
    private static readonly Dictionary<ushort, IGrowLikeTulip> _growLikeTulips = [];

    void ILoadable.Load(Mod mod) {
        On_WorldGen.plantDye += On_WorldGen_plantDye;
    }

    void IPostSetupContent.PostSetupContent() {
        foreach (ModTile modTile in ModContent.GetContent<ModTile>()) {
            if (modTile is IGrowLikeTulip growLikeTulip) {
                _growLikeTulips.Add(modTile.Type, growLikeTulip);
            }
        }
    }

    void ILoadable.Unload() { }

    private void On_WorldGen_plantDye(On_WorldGen.orig_plantDye orig, int i, int j, bool exoticPlant) {
        orig(i, j, exoticPlant);

        if (!exoticPlant) {
            UnifiedRandom unifiedRandom = (WorldGen.gen ? WorldGen.genRand : Main.rand);
            if (!Main.tile[i, j].HasTile || i < 95 || i > Main.maxTilesX - 95 || j < 95 || j > Main.maxTilesY - 95)
                return;

            if (((double)j < Main.worldSurface && !WorldGen.remixWorldGen) && (!Main.tile[i, j - 1].HasTile ||
                Main.tileCut[Main.tile[i, j - 1].TileType])) {
                foreach (ushort growLikeTulipTileType in _growLikeTulips.Keys) {
                    TryToGrowAPlant(growLikeTulipTileType, i, j);
                }
            }
        }
    }

    public static bool TryToGrowAPlant(ushort growLikeTulipTileType, int i, int j) {
        if (CanGrowAPlant(growLikeTulipTileType, i, j)) {
            GrowAPlant(growLikeTulipTileType, i, j);
            return true;
        }

        return false;
    }

    public static bool CanGrowAPlant(ushort growLikeTulipTileType, int i, int j) {
        IGrowLikeTulip growLikeTulip = _growLikeTulips[growLikeTulipTileType];
        ushort tileType = growLikeTulipTileType;
        if (!growLikeTulip.ShouldGrow(new(i, j))) {
            return false;
        }
        int num = 90;
        num = 120;
        bool flag = false;
        int num2 = Utils.Clamp(i - num, 1, Main.maxTilesX - 1 - 1);
        int num3 = Utils.Clamp(i + num, 1, Main.maxTilesX - 1 - 1);
        int num4 = Utils.Clamp(j - num, 1, Main.maxTilesY - 1 - 1);
        int num5 = Utils.Clamp(j + num, 1, Main.maxTilesY - 1 - 1);
        for (int k = num2; k < num3; k++) {
            for (int l = num4; l < num5; l++) {
                if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileType)
                    flag = true;
            }
        }

        return !flag;
    }

    public static void GrowAPlant(ushort growLikeTulipTileType, int i, int j) {
        Tile tile = Main.tile[i, j - 1];
        if (tile.TileType != growLikeTulipTileType && Main.tileCut[tile.TileType]) {
            tile.HasTile = false;
        }
        WorldGen.PlaceTile(i, j - 1, growLikeTulipTileType, mute: true, forced: true, style: 0);
    }
}
