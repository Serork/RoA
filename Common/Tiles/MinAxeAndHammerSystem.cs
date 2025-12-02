using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Trees;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class MinAxeAndHammerSystem : ILoadable {

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ClearMiningCacheAt")]
    public extern static void Player_ClearMiningCacheAt(Player player, int x, int y, int hitTileCacheType);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IsBottomOfTreeTrunkNoRoots")]
    public extern static bool Player_IsBottomOfTreeTrunkNoRoots(Player player, int x, int y);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "TryReplantingTree")]
    public extern static void Player_TryReplantingTree(Player player, int x, int y);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ItemCheck_UseMiningTools_TryPoundingTile")]
    public extern static void Player_ItemCheck_UseMiningTools_TryPoundingTilee(Player player, Item sItem, int tileHitId, ref bool hitWall, int x, int y);

    void ILoadable.Load(Mod mod) {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += On_Player_ItemCheck_UseMiningTools_ActuallyUseMiningTool;
        On_Player.ItemCheck_UseMiningTools_TryHittingWall += On_Player_ItemCheck_UseMiningTools_TryHittingWall;
    }

    private void On_Player_ItemCheck_UseMiningTools_TryHittingWall(On_Player.orig_ItemCheck_UseMiningTools_TryHittingWall orig, Player self, Item sItem, int wX, int wY) {
        if (Main.tile[wX, wY].WallType > 0 && (!Main.tile[wX, wY].HasTile || wX != Player.tileTargetX || wY != Player.tileTargetY ||
            (!Main.tileHammer[Main.tile[wX, wY].TileType] && !self.poundRelease)) && self.toolTime == 0 && self.itemAnimation > 0 && self.controlUseItem && sItem.hammer > 0 &&
            Player.CanPlayerSmashWall(wX, wY)) {
            Tile tile = Main.tile[wX, wY];
            if (WallLoader.GetWall(tile.WallType) is not TileHooks.IRequireMinHammerPower) {
                orig(self, sItem, wX, wY);
                return;
            }
            int damage = (int)((float)sItem.hammer * 1.5f);
            if (WallLoader.GetWall(tile.WallType) is TileHooks.IRequireMinHammerPower tileMinHammerPower) {
                if (sItem.hammer < tileMinHammerPower.MinHammer) {
                    damage = 0;
                }
                else {
                    if (WallLoader.GetWall(tile.WallType) is TileHooks.IResistToHammer resistToHammer && resistToHammer.CanBeApplied(wX, wY)) {
                        damage = (int)((double)damage * resistToHammer.ResistToPick);
                    }
                }
            }
            self.PickWall(wX, wY, damage);
            self.itemTime = sItem.useTime / 2;
        }
    }

    private void On_Player_ItemCheck_UseMiningTools_ActuallyUseMiningTool(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (tile.HasTile) {
            if (TileLoader.GetTile(tile.TileType) is not TileHooks.IRequireMinHammerPower &&
                TileLoader.GetTile(tile.TileType) is not TileHooks.IRequireMinAxePower &&
                !PrimordialTree.IsPrimordialTree(x, y)) {
                orig(self, sItem, out canHitWalls, x, y);
                return;
            }
        }

        int num = -1;
        int num2 = 0;
        canHitWalls = true;
        if (!tile.HasTile)
            return;

        if ((sItem.pick > 0 && !Main.tileAxe[tile.TileType] && !Main.tileHammer[tile.TileType]) || (sItem.axe > 0 && Main.tileAxe[tile.TileType]) || (sItem.hammer > 0 && Main.tileHammer[tile.TileType]))
            canHitWalls = false;

        num = self.hitTile.HitObject(x, y, 1);
        if (Main.tileNoFail[tile.TileType])
            num2 = 100;

        if (Main.tileHammer[tile.TileType]) {
            canHitWalls = false;
            if (sItem.hammer > 0) {
                if (TileLoader.GetTile(tile.TileType) is ModTile modTile) {
                    num2 += (int)(sItem.hammer / modTile.MineResist);
                    goto skipVanillaHammerPower;
                }

                num2 += sItem.hammer;
            skipVanillaHammerPower:

                if (!WorldGen.CanKillTile(x, y))
                    num2 = 0;

                if (tile.TileType == 26 && (sItem.hammer < 80 || !Main.hardMode)) {
                    num2 = 0;
                    self.Hurt(PlayerDeathReason.ByOther(4), self.statLife / 2, -self.direction);
                }

                if (TileLoader.GetTile(tile.TileType) is TileHooks.IRequireMinHammerPower tileMinHammerPower) {
                    if (sItem.hammer < tileMinHammerPower.MinHammer) {
                        num2 = 0;
                    }
                    else {
                        if (TileLoader.GetTile(tile.TileType) is TileHooks.IResistToHammer resistToHammer && resistToHammer.CanBeApplied(x, y)) {
                            num2 = (int)((double)num2 * resistToHammer.ResistToPick);
                        }
                        else {
                            num2 = (int)((double)num2 * 0.75);
                        }
                    }
                }

                AchievementsHelper.CurrentlyMining = true;
                if (self.hitTile.AddDamage(num, num2) >= 100) {
                    Player_ClearMiningCacheAt(self, x, y, 1);
                    WorldGen.KillTile(x, y);
                    if (Main.netMode == 1)
                        NetMessage.SendData(17, -1, -1, null, 0, x, y);
                }
                else {
                    WorldGen.KillTile(x, y, fail: true);
                    if (Main.netMode == 1)
                        NetMessage.SendData(17, -1, -1, null, 0, x, y, 1f);
                }

                if (num2 != 0)
                    self.hitTile.Prune();

                self.ApplyItemTime(sItem);
                AchievementsHelper.CurrentlyMining = false;
            }
        }
        else if (Main.tileAxe[tile.TileType]) {
            if (TileLoader.GetTile(tile.TileType) is ModTile modTile) {
                num2 += (int)(sItem.axe / modTile.MineResist);
                goto skipVanillaAxePower;
            }

            num2 = ((tile.TileType != 80) ? (num2 + (int)((float)sItem.axe * 1.2f)) : (num2 + (int)((float)(sItem.axe * 3) * 1.2f)));
            if (Main.getGoodWorld)
                num2 = (int)((double)num2 * 1.3);

            skipVanillaAxePower:

            if (sItem.axe > 0) {
                AchievementsHelper.CurrentlyMining = true;
                if (!WorldGen.CanKillTile(x, y))
                    num2 = 0;

                if (PrimordialTree.IsPrimordialTree(x, y)) {
                    if (sItem.axe * 5 < PrimordialTree.MINAXEREQUIRED) {
                        num2 = 0;
                    }
                    else {
                        num2 = (int)((double)num2 * 0.75);
                    }
                }

                if (TileLoader.GetTile(tile.TileType) is TileHooks.IRequireMinAxePower tileMinAxePower) {
                    if (sItem.axe * 5 < tileMinAxePower.MinAxe) {
                        num2 = 0;
                    }
                    else {
                        if (TileLoader.GetTile(tile.TileType) is TileHooks.IResistToAxe resistToAxe && resistToAxe.CanBeApplied(x, y)) {
                            num2 = (int)((double)num2 * resistToAxe.ResistToPick);
                        }
                        else {
                            num2 = (int)((double)num2 * 0.75);
                        }
                    }
                }

                if (Main.dontStarveWorld && Main.myPlayer == self.whoAmI && num2 > 0 && tile.TileType == 80)
                    self.Hurt(PlayerDeathReason.ByOther(3), Main.DamageVar(6f, self.luck), 0, pvp: false, quiet: false, 0);

                if (self.hitTile.AddDamage(num, num2) >= 100) {
                    if (self.whoAmI == Main.myPlayer && sItem.type == 5095 && (TileID.Sets.IsATreeTrunk[tile.TileType] || tile.TileType == 323 || tile.TileType == 80)) {
                        LucyAxeMessage.MessageSource source = LucyAxeMessage.MessageSource.ChoppedTree;
                        if (TileID.Sets.CountsAsGemTree[tile.TileType])
                            source = LucyAxeMessage.MessageSource.ChoppedGemTree;

                        if (tile.TileType == 80) {
                            source = LucyAxeMessage.MessageSource.ChoppedCactus;
                            LucyAxeMessage.TryCreatingMessageWithCooldown(source, self.Top, new Vector2(self.direction * 7, -7f), 420);
                        }
                        else {
                            LucyAxeMessage.Create(source, self.Top, new Vector2(self.direction * 7, -7f));
                        }
                    }

                    Player_ClearMiningCacheAt(self, x, y, 1);
                    bool flag = Player_IsBottomOfTreeTrunkNoRoots(self, x, y);
                    WorldGen.KillTile(x, y);
                    if (Main.netMode == 1)
                        NetMessage.SendData(17, -1, -1, null, 0, x, y);

                    if (sItem.type == 5295 && flag)
                        Player_TryReplantingTree(self, x, y);
                }
                else {
                    WorldGen.KillTile(x, y, fail: true);
                    if (Main.netMode == 1)
                        NetMessage.SendData(17, -1, -1, null, 0, x, y, 1f);
                }

                if (num2 != 0)
                    self.hitTile.Prune();

                self.ApplyItemTime(sItem);
                AchievementsHelper.CurrentlyMining = false;
            }
        }
        else if (sItem.pick > 0) {
            self.PickTile(x, y, sItem.pick);
        }


        if (sItem.pick > 0)
            /*
                itemTime = (int)((float)sItem.useTime * pickSpeed);
            */
            self.ApplyItemTime(sItem, self.pickSpeed);

        Player_ItemCheck_UseMiningTools_TryPoundingTilee(self, sItem, num, ref canHitWalls, x, y);
    }

    void ILoadable.Unload() { }
}
