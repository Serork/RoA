using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TransformTileSystem : ILoadable {
    public static bool[] OnKillActNormal = TileID.Sets.Factory.CreateBoolSet(true);
    public static ushort[] ReplaceToTypeOnKill = TileID.Sets.Factory.CreateUshortSet(0);

    public void Load(Mod mod) {
        On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill;
        On_Player.PlaceThing_ValidTileForReplacement += On_Player_PlaceThing_ValidTileForReplacement;
    }

    private bool On_Player_PlaceThing_ValidTileForReplacement(On_Player.orig_PlaceThing_ValidTileForReplacement orig, Player self) {
        bool result = orig(self);
        int createTile = self.HeldItem.createTile;
        Tile tile = WorldGenHelper.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
        if (!OnKillActNormal[tile.TileType] && createTile == ReplaceToTypeOnKill[tile.TileType]) {
            return false;
        }

        return result;
    }

    public void Unload() { }

    private bool DoesPickTargetTransformOnKill(On_Player.orig_DoesPickTargetTransformOnKill original, Player self, HitTile hitCounter, int damage, int x, int y, int pickPower, int bufferIndex, Tile tileTarget) {
        if (!OnKillActNormal[tileTarget.TileType]) {
            return true;
        }
        
        return original(self, hitCounter, damage, x, y, pickPower, bufferIndex, tileTarget);
    }

     private class TileReplacement : GlobalTile {
        public override bool CanExplode(int i, int j, int type) {
            if (!OnKillActNormal[type] || WorldGen.gen) {
                WorldGen.KillTile(i, j);

                return true;
            }

            return base.CanExplode(i, j, type);
        }

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
            bool flag = !fail && !effectOnly && !noItem;
            if (!flag) {
                if (OnKillActNormal[type] || WorldGen.gen) {
                    return;
                }
                if (!WorldGen.CanKillTile(i, j)) {
                    return;
                }
                fail = true;

                if (fail && !effectOnly) {
                    TileReplacementSystem.SetReplacementData(i, j, ReplaceToTypeOnKill[type]);
                }
            }
        }

        public override bool CreateDust(int i, int j, int type, ref int dustType) {
            if (!(OnKillActNormal[type] || WorldGen.gen)) {
                if (!(!WorldGen.CanKillTile(i, j))) {
                    int dustType2 = TileHelper.GetKillTileDust(i, j, ReplaceToTypeOnKill[type], 0, 0);
                    if (Main.rand.NextBool(3)) {
                        dustType = dustType2;
                    }
                }
            }

            return base.CreateDust(i, j, type, ref dustType);
        }

         private class TileReplacementSystem : ModPlayer {
            readonly struct ReplacementData(Point positionInWorld, ushort replaceToType) {
                public readonly Point PositionInWorld = positionInWorld;
                public readonly ushort ReplaceToType = replaceToType;
            }

            static ReplacementData _tileToReplaceData;
            static bool _replaced;

            public static void SetReplacementData(int i, int j, ushort type) {
                _tileToReplaceData = new(new Point(i, j), type);
                _replaced = false;
            }

            public override void PostItemCheck() {
                if (!_replaced) {
                    _replaced = true;
                    WorldGenHelper.GetTileSafely(_tileToReplaceData.PositionInWorld).TileType = _tileToReplaceData.ReplaceToType;
                    WorldGen.SquareTileFrame(_tileToReplaceData.PositionInWorld.X, _tileToReplaceData.PositionInWorld.Y);
                    int pickPower = Player.GetSelectedItem().pick;
                    if (pickPower < 50) {
                        Player.PickTile(_tileToReplaceData.PositionInWorld.X, _tileToReplaceData.PositionInWorld.Y, pickPower / 2);
                    }
                }
            }
        }
    }
}
