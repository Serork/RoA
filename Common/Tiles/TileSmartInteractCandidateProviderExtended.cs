using RoA.Common.Utilities.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TileSmartInteractCandidateProviderExtended : ILoadable {
    public static bool[] AddMe = TileID.Sets.Factory.CreateBoolSet();

    public void Load(Mod mod) {
        On_TileSmartInteractCandidateProvider.FillPotentialTargetTiles += On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
    }

    public void Unload() { }

    private void On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles(On_TileSmartInteractCandidateProvider.orig_FillPotentialTargetTiles orig, TileSmartInteractCandidateProvider self, SmartInteractScanSettings settings) {
        for (int i = settings.LX; i <= settings.HX; i++) {
            for (int j = settings.LY; j <= settings.HY; j++) {
                Tile tile = Main.tile[i, j];
                if (!tile.HasTile || !AddMe[tile.TileType]) {
                    continue;
                }

                if (settings.player.IsWithinSnappngRangeToTile(i, j, 80)) {
                    SmartInteractSystem smartInteractSys = typeof(Player).GetFieldValue<SmartInteractSystem>("_smartInteractSys", settings.player);
                    List<ISmartInteractCandidateProvider> candidateProvidersByOrderOfPriority = typeof(SmartInteractSystem).GetFieldValue<List<ISmartInteractCandidateProvider>>("_candidateProvidersByOrderOfPriority", smartInteractSys);
                    List<Tuple<int, int>> targets = typeof(TileSmartInteractCandidateProvider).GetFieldValue<List<Tuple<int, int>>>("targets", candidateProvidersByOrderOfPriority.Last());
                    targets.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        orig(self, settings);
    }
}
