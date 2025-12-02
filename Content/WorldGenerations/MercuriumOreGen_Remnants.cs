using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class MercuriumOreGen_Remnants : ModSystem {
    private static int _tileCount;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        if (!ModLoader.HasMod("Remnants")) {
            return;
        }

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
        if (genIndex != -1) {
            tasks.Insert(genIndex + 30, new MercuriumOrePass("Mercurium Ore", 237.4298f));
        }
    }

    private class MercuriumOrePass(string name, float loadWeight) : GenPass(name, loadWeight) {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
            for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-03); k++) {
                int i = WorldGen.genRand.Next(0, Main.maxTilesX);
                int j = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, Main.maxTilesY);
                int type = Main.tile[i, j].TileType;
                if (!MercuriumOreGen.OresType.Contains(type)) {
                    continue;
                }

                for (int x = i - 10; x < i + 11; x++) {
                    for (int y = j - 10; y < j + 11; y++) {
                        Tile tile = WorldGenHelper.GetTileSafely(x, y);
                        if (tile.HasTile && tile.TileType == type) {
                            _tileCount++;
                        }
                    }
                }
                if (_tileCount > 50) {
                    _tileCount = 50;
                }
                else if (_tileCount < 10) {
                    _tileCount = 10;
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
                    WorldGenHelper.ModifiedTileRunner(i + (int)WorldGen.genRand.NextFloat(-velocity.X, velocity.X), j + (int)WorldGen.genRand.NextFloat(-velocity.Y, velocity.Y), _tileCount * 0.25f * sizeMult, (int)(_tileCount * 0.1f), ModContent.TileType<MercuriumOre>(), ignoreTileTypes: [.. MercuriumOreGen.OresType]);
                }
                _tileCount = 0;
            }
        }
    }
}
