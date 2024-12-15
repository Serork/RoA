﻿using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.World.Generations;

sealed class MercuriumOreGen : ModSystem {
    private static int _tileCount;

    public override void Load() {
        On_WorldGen.TileRunner += On_WorldGen_TileRunner;
    }

    private void On_WorldGen_TileRunner(On_WorldGen.orig_TileRunner orig, int i, int j, double strength, int steps, int type, bool addTile, double speedX, double speedY, bool noYChange, bool overRide, int ignoreTileType) {
        IReadOnlyList<int> oresType = [7, 166, 6, 167, 9, 168, 8, 169, 22, 204];
        orig(i, j, strength, steps, type, addTile, speedX, speedY, noYChange, overRide, ignoreTileType);
        if (oresType.Contains(type)) {
            for (int x = i - 10; x < i + 11; x++) {
                for (int y = j - 10; y < j + 11; y++) {
                    Tile tile = WorldGenHelper.GetTileSafely(x, y);
                    if (tile.HasTile && tile.TileType == type) {
                        _tileCount++;
                    }
                }
            }
            if (_tileCount > 50) {
                _tileCount = 0;
                return;
            }
            else if (_tileCount < 15) {
                _tileCount = 15;
            }
            double chance = 0.85;
            float sizeMult = 1f;
            switch (type) {
                case 6 or 167:
                    chance = 0.75;
                    sizeMult = 1.1f;
                    break;
                case 7 or 166:
                    chance = 0.85;
                    sizeMult = WorldGen.genRand.NextFloat(1.5f, 2f) * 0.75f;
                    break;
                case 8 or 169:
                    chance = 1.0;
                    break;
                case 9 or 168:
                    chance = 1.0;
                    break;
            }
            if (WorldGen.genRand.NextChance(chance) && !WorldGen.genRand.NextBool(3)) {
                Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                velocity *= WorldGen.genRand.NextFloat(_tileCount / 2) * 0.15f;
                if (!WorldGen.genRand.NextBool(4)) {
                    velocity = Vector2.Zero;
                }
                WorldGenHelper.ModifiedTileRunner(i + (int)WorldGen.genRand.NextFloat(-velocity.X, velocity.X), j + (int)WorldGen.genRand.NextFloat(-velocity.Y, velocity.Y), _tileCount * 0.25f * sizeMult, (int)(_tileCount * 0.1f), ModContent.TileType<MercuriumOre>(), ignoreTileTypes: [.. oresType]);
            }
            _tileCount = 0;
        }
    }
}
