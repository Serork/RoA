using Microsoft.Xna.Framework;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TransformTileSystem : ILoadable {
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "stopDrops")]
    public extern static ref bool WorldGen_stopDrops(WorldGen self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "KillTile_DropBait")]
    public extern static void WorldGen_KillTile_DropBait(WorldGen self, int i, int j, Tile tileCache);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "KillTile_DropItems")]
    public extern static void WorldGen_KillTile_DropItems(WorldGen self, int x, int y, Tile tileCache, bool includeLargeObjectDrops = false, bool includeAllModdedLargeObjectDrops = false);

    //public static bool[] OnKillActNormal = TileID.Sets.Factory.CreateBoolSet(true);
    public static ushort[] ReplaceToTypeOnKill = TileID.Sets.Factory.CreateUshortSet(TileID.Count);

    public void Load(Mod mod) {
        //On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill;
        //On_Player.PlaceThing_ValidTileForReplacement += On_Player_PlaceThing_ValidTileForReplacement;

        On_WorldGen.KillTile += On_WorldGen_KillTile;
        On_Player.GetPickaxeDamage += On_Player_GetPickaxeDamage;
    }

    private int On_Player_GetPickaxeDamage(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget) {
        int num = 0;
        if (Main.tileNoFail[tileTarget.TileType])
            num = 100;

        if (TileLoader.GetTile(tileTarget.TileType) is ModTile modTile) {
            num += (int)(pickPower / modTile.MineResist);
            goto skipVanillaPickPower;
        }

        num = ((!Main.tileDungeon[tileTarget.TileType] && tileTarget.TileType != 25 && tileTarget.TileType != 58 && tileTarget.TileType != 117 && tileTarget.TileType != 203) ? ((tileTarget.TileType == 85) ? ((!Main.getGoodWorld) ? (num + pickPower / 3) : (num + pickPower / 4)) : ((tileTarget.TileType != 48 && tileTarget.TileType != 232) ? ((tileTarget.TileType == 226) ? (num + pickPower / 4) : ((tileTarget.TileType != 107 && tileTarget.TileType != 221) ? ((tileTarget.TileType != 108 && tileTarget.TileType != 222) ? ((tileTarget.TileType == 111 || tileTarget.TileType == 223) ? (num + pickPower / 4) : ((tileTarget.TileType != 211) ? (num + pickPower) : (num + pickPower / 5))) : (num + pickPower / 3)) : (num + pickPower / 2))) : (num + pickPower * 2))) : (num + pickPower / 2));
    skipVanillaPickPower:

        if (tileTarget.TileType == 211 && pickPower < 200)
            num = 0;

        if ((tileTarget.TileType == 25 || tileTarget.TileType == 203) && pickPower < 65) {
            num = 0;
        }
        else if (tileTarget.TileType == 117 && pickPower < 65) {
            num = 0;
        }
        else if (tileTarget.TileType == 37 && pickPower < 50) {
            num = 0;
        }
        else if ((tileTarget.TileType == 22 || tileTarget.TileType == 204) && (double)y > Main.worldSurface && pickPower < 55) {
            num = 0;
        }
        else if (tileTarget.TileType == 56 && pickPower < 55) {
            num = 0;
        }
        else if (tileTarget.TileType == 77 && pickPower < 65 && y >= Main.UnderworldLayer) {
            num = 0;
        }
        else if (tileTarget.TileType == 58 && pickPower < 65) {
            num = 0;
        }
        else if ((tileTarget.TileType == 226 || tileTarget.TileType == 237) && pickPower < 210) {
            num = 0;
        }
        else if (tileTarget.TileType == 137 && pickPower < 210) {
            int num2 = tileTarget.TileFrameY / 18;
            if ((uint)(num2 - 1) <= 3u)
                num = 0;
        }
        else if (Main.tileDungeon[tileTarget.TileType] && pickPower < 100 && (double)y > Main.worldSurface) {
            if ((double)x < (double)Main.maxTilesX * 0.35 || (double)x > (double)Main.maxTilesX * 0.65)
                num = 0;
        }
        else if (tileTarget.TileType == 107 && pickPower < 100) {
            num = 0;
        }
        else if (tileTarget.TileType == 108 && pickPower < 110) {
            num = 0;
        }
        else if (tileTarget.TileType == 111 && pickPower < 150) {
            num = 0;
        }
        else if (tileTarget.TileType == 221 && pickPower < 100) {
            num = 0;
        }
        else if (tileTarget.TileType == 222 && pickPower < 110) {
            num = 0;
        }
        else if (tileTarget.TileType == 223 && pickPower < 150) {
            num = 0;
        }
        else {
            TileLoader.PickPowerCheck(tileTarget, pickPower, ref num);
        }

        if (tileTarget.TileType == 147 || tileTarget.TileType == 0 || tileTarget.TileType == 40 || tileTarget.TileType == 53 || tileTarget.TileType == 57 || tileTarget.TileType == 59 || tileTarget.TileType == 123 || tileTarget.TileType == 224 || tileTarget.TileType == 397)
            num += pickPower;

        if (tileTarget.TileType == 404)
            num += 5;

        ushort replaceTo = ReplaceToTypeOnKill[tileTarget.TileType];
        if (tileTarget.TileType == 165 || Main.tileRope[tileTarget.TileType] || tileTarget.TileType == 199)
            num = 100;

        if (replaceTo != TileID.Count) {
            num = 0;
        }

        if (tileTarget.TileType == 128 || tileTarget.TileType == 269) {
            if (tileTarget.TileFrameX == 18 || tileTarget.TileFrameX == 54) {
                x--;
                tileTarget = Main.tile[x, y];
                self.hitTile.UpdatePosition(hitBufferIndex, x, y);
            }

            if (tileTarget.TileFrameX >= 100) {
                num = 0;
                Main.blockMouse = true;
            }
        }

        if (tileTarget.TileType == 334) {
            if (tileTarget.TileFrameY == 0) {
                y++;
                tileTarget = Main.tile[x, y];
                self.hitTile.UpdatePosition(hitBufferIndex, x, y);
            }

            if (tileTarget.TileFrameY == 36) {
                y--;
                tileTarget = Main.tile[x, y];
                self.hitTile.UpdatePosition(hitBufferIndex, x, y);
            }

            int frameX = tileTarget.TileFrameX;
            bool flag = frameX >= 5000;
            bool flag2 = false;
            if (!flag) {
                int num3 = frameX / 18;
                num3 %= 3;
                x -= num3;
                tileTarget = Main.tile[x, y];
                if (tileTarget.TileFrameX >= 5000)
                    flag = true;
            }

            if (flag) {
                frameX = tileTarget.TileFrameX;
                int num4 = 0;
                while (frameX >= 5000) {
                    frameX -= 5000;
                    num4++;
                }

                if (num4 != 0)
                    flag2 = true;
            }

            if (flag2) {
                num = 0;
                Main.blockMouse = true;
            }
        }

        return num;
    }

    private void On_WorldGen_KillTile(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem) {
        if (WorldGen.gen) {
            orig(i, j, fail, effectOnly, noItem);
            return;
        }

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

                //if (tile.TileType == 2 || tile.TileType == 23 || tile.TileType == 109 || tile.TileType == 199 || tile.TileType == 477 || tile.TileType == 492)
                //    tile.TileType = 0;

                //if (tile.TileType == 633)
                //    tile.TileType = 57;

                //if (tile.TileType == 60 || tile.TileType == 661 || tile.TileType == 662 || tile.TileType == 70)
                //    tile.TileType = 59;

                //if (Main.tileMoss[tile.TileType])
                //    tile.TileType = 1;

                //if (TileID.Sets.tileMossBrick[tile.TileType])
                //    tile.TileType = 38;

                WorldGen.SquareTileFrame(i, j);
                return;
            }

            if (WorldGen.CheckTileBreakability2_ShouldTileSurvive(i, j))
                return;

            if (!noItem && !WorldGen_stopDrops(null) && Main.netMode != NetmodeID.MultiplayerClient) {
                WorldGen_KillTile_DropBait(null, i, j, tile);
                WorldGen_KillTile_DropItems(null, i, j, tile);
            }

            if (Main.netMode != NetmodeID.Server)
                AchievementsHelper.NotifyTileDestroyed(Main.player[Main.myPlayer], tile.TileType);

            tile.HasTile = false;
            tile.IsHalfBlock = false;
            tile.TileFrameX = -1;
            tile.TileFrameY = -1;
            tile.ClearBlockPaintAndCoating();
            tile.TileFrameNumber = 0;

            tile.TileType = 0;
            tile.IsActuated = false;
            WorldGen.SquareTileFrame(i, j);
            while (!WorldGen.destroyObject && WorldGen.ExploitDestroyQueue.Count > 0) {
                Point point = WorldGen.ExploitDestroyQueue.Dequeue();
                if (Framing.GetTileSafely(point.X, point.Y).HasTile) {
                    WorldGen.SquareTileFrame(point.X, point.Y);
                    NetMessage.SendTileSquare(-1, point.X, point.Y);
                }
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
