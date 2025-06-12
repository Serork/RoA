using Terraria.ModLoader;

namespace RoA.Common.BackwoodsSystems;

sealed class SpawnNPCOnKillingBackwoodsTree : GlobalTile {
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
        //if (!fail) {
        //    if (PrimordialTree.IsPrimordialTree(i, j)) {
        //        if (!(!WorldGenHelper.ActiveTile(i - 1, j, TileID.Trees) && !WorldGenHelper.ActiveTile(i + 1, j, TileID.Trees) && !WorldGenHelper.ActiveTile(i, j - 1, TileID.Trees)
        //        && !WorldGenHelper.ActiveTile(i - 1, j, ModContent.TileType<TreeBranch>()) && !WorldGenHelper.ActiveTile(i + 1, j, ModContent.TileType<TreeBranch>()))) {
        //            return;
        //        }
        //        NPC.NewNPC(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16 + 8, (j - 1) * 16, ModContent.NPCType<Fleder>());
        //    }
        //}
    }
}
