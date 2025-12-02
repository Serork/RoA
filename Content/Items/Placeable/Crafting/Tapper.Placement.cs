using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

partial class Tapper : ModItem {
    public override void Load() {
        On_Player.PlaceThing_Tiles_CheckRopeUsability += On_Player_PlaceThing_Tiles_CheckRopeUsability;
    }

    private bool On_Player_PlaceThing_Tiles_CheckRopeUsability(On_Player.orig_PlaceThing_Tiles_CheckRopeUsability orig, Player self, bool canUse) {
        Item item = self.inventory[self.selectedItem];
        int tileToCreate = item.createTile;
        if (Tiles.Crafting.Tapper.ImATapper[tileToCreate]) {
            bool flag = (WorldGenHelper.ActiveTile(Player.tileTargetX - 1, Player.tileTargetY - 1, TileID.Trees) || WorldGenHelper.ActiveTile(Player.tileTargetX + 1, Player.tileTargetY - 1, TileID.Trees)) && !WorldGenHelper.ActiveTile(Player.tileTargetX, Player.tileTargetY - 1, tileToCreate) && !WorldGenHelper.ActiveTile(Player.tileTargetX, Player.tileTargetY + 1, tileToCreate);
            for (int testJ = Player.tileTargetY; testJ > Player.tileTargetY - 8; testJ--) {
                if (WorldGenHelper.ActiveTile(Player.tileTargetX, testJ, tileToCreate)) {
                    flag = false;
                }
            }
            for (int testJ = Player.tileTargetY; testJ < Player.tileTargetY + 8; testJ++) {
                if (WorldGenHelper.ActiveTile(Player.tileTargetX, testJ, tileToCreate)) {
                    flag = false;
                }
            }
            return flag;
        }

        return orig(self, canUse);
    }
}