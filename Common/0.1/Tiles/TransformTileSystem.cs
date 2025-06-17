using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TransformTileSystem : ILoadable {
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "stopDrops")]
    public extern static ref bool WorldGen_stopDrops(WorldGen self);

    //public static bool[] OnKillActNormal = TileID.Sets.Factory.CreateBoolSet(true);
    public static ushort[] ReplaceToTypeOnKill = TileID.Sets.Factory.CreateUshortSet(TileID.Count);

    public void Load(Mod mod) {
        //On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill;
        //On_Player.PlaceThing_ValidTileForReplacement += On_Player_PlaceThing_ValidTileForReplacement;

        On_WorldGen.KillTile += On_WorldGen_KillTile;
    }

    private void On_WorldGen_KillTile(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem) {
        Tile tile = Main.tile[i, j];
        ushort replaceTo = ReplaceToTypeOnKill[tile.TileType];
        if (replaceTo != TileID.Count) {
            if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
                return;

            if (!tile.HasTile)
                return;

            int num = WorldGen.CheckTileBreakability(i, j);
            // Extra patch context.
            if (num == 1)
                fail = true;

            if (num == 2)
                return;

            // Placed before gen check on purpose.
            TileLoader.KillTile(i, j, tile.TileType, ref fail, ref effectOnly, ref noItem);

            if (WorldGen.gen)
                noItem = true;

            if (!effectOnly && !WorldGen_stopDrops(null)) {
                if (!noItem && FixExploitManEaters.SpotProtected(i, j))
                    return;

                if (!Main.dedServ && !WorldGen.gen && !Main.gameMenu)
                    WorldGen.KillTile_PlaySounds(i, j, fail, tile);
            }

            if ((tile.TileType == 470 && (WorldGen.CheckTileBreakability2_ShouldTileSurvive(i, j) || fail)) || (tile.TileType == 475 && (WorldGen.CheckTileBreakability2_ShouldTileSurvive(i, j) || fail)))
                return;

            int num16 = WorldGen.KillTile_GetTileDustAmount(fail, tile, i, j);
            for (int k = 0; k < num16; k++) {
                WorldGen.KillTile_MakeTileDust(i, j, tile);
            }

            if (effectOnly)
                return;

            //WorldGen.AttemptFossilShattering(i, j, tile, fail);
            if (fail) {
                tile.TileType = replaceTo;
                //if (tile.type == 2 || tile.type == 23 || tile.type == 109 || tile.type == 199 || tile.type == 477 || tile.type == 492)
                //    tile.type = 0;

                //if (tile.type == 633)
                //    tile.type = 57;

                //if (tile.type == 60 || tile.type == 661 || tile.type == 662 || tile.type == 70)
                //    tile.type = 59;

                //if (Main.tileMoss[tile.type])
                //    tile.type = 1;

                //if (TileID.Sets.tileMossBrick[tile.type])
                //    tile.type = 38;

                WorldGen.SquareTileFrame(i, j);
                return;
            }

            return;
        }

        orig(i, j, fail, effectOnly, noItem);
    }

    public void Unload() { }

    //private bool On_Player_PlaceThing_ValidTileForReplacement(On_Player.orig_PlaceThing_ValidTileForReplacement orig, Player self) {
    //    bool result = orig(self);
    //    int createTile = self.HeldItem.createTile;
    //    Tile tile = WorldGenHelper.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
    //    if (!OnKillActNormal[tile.TileType] && createTile == ReplaceToTypeOnKill[tile.TileType]) {
    //        return false;
    //    }

    //    return result;
    //}

    //private bool DoesPickTargetTransformOnKill(On_Player.orig_DoesPickTargetTransformOnKill original, Player self, HitTile hitCounter, int damage, int x, int y, int pickPower, int bufferIndex, Tile tileTarget) {
    //    if (!OnKillActNormal[tileTarget.TileType]) {
    //        return true;
    //    }

    //    return original(self, hitCounter, damage, x, y, pickPower, bufferIndex, tileTarget);
    //}

    //private class TileReplacement : GlobalTile {
    //    public override bool CanExplode(int i, int j, int type) {
    //        if (!OnKillActNormal[type] || WorldGen.gen) {
    //            WorldGen.KillTile(i, j);

    //            return true;
    //        }

    //        return base.CanExplode(i, j, type);
    //    }

    //    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
    //        bool flag = !fail && !effectOnly && !noItem;
    //        if (!flag) {
    //            if (OnKillActNormal[type] || WorldGen.gen) {
    //                return;
    //            }
    //            if (!WorldGen.CanKillTile(i, j)) {
    //                return;
    //            }
    //            fail = true;

    //            if (fail && !effectOnly) {
    //                TileReplacementSystem.SetReplacementData(i, j, ReplaceToTypeOnKill[type]);
    //            }
    //        }
    //    }

    //    public override bool CreateDust(int i, int j, int type, ref int dustType) {
    //        if (!(OnKillActNormal[type] || WorldGen.gen)) {
    //            if (!(!WorldGen.CanKillTile(i, j))) {
    //                int dustType2 = TileHelper.GetKillTileDust(i, j, ReplaceToTypeOnKill[type], 0, 0);
    //                if (Main.rand.NextBool(3)) {
    //                    dustType = dustType2;
    //                }
    //            }
    //        }

    //        return base.CreateDust(i, j, type, ref dustType);
    //    }

    //    private class TileReplacementSystem : ModPlayer {
    //        readonly struct ReplacementData(Point positionInWorld, ushort replaceToType) {
    //            public readonly Point PositionInWorld = positionInWorld;
    //            public readonly ushort ReplaceToType = replaceToType;
    //        }

    //        static ReplacementData _tileToReplaceData;
    //        static bool _replaced;

    //        public static void SetReplacementData(int i, int j, ushort type) {
    //            _tileToReplaceData = new(new Point(i, j), type);
    //            _replaced = false;
    //        }

    //        public override void PostItemCheck() {
    //            if (!_replaced) {
    //                _replaced = true;
    //                WorldGenHelper.GetTileSafely(_tileToReplaceData.PositionInWorld).TileType = _tileToReplaceData.ReplaceToType;
    //                WorldGen.SquareTileFrame(_tileToReplaceData.PositionInWorld.X, _tileToReplaceData.PositionInWorld.Y);
    //                int pickPower = Player.GetSelectedItem().pick;
    //                if (pickPower < 50) {
    //                    Player.PickTile(_tileToReplaceData.PositionInWorld.X, _tileToReplaceData.PositionInWorld.Y, pickPower / 2);
    //                }
    //            }
    //        }
    //    }
    //}
}
