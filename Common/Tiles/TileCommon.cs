using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TileCommon : GlobalTile {
    public delegate void RandomUpdateDelegate(int i, int j, int type);
    public static event RandomUpdateDelegate RandomUpdateEvent;
    public override void RandomUpdate(int i, int j, int type) {
        RandomUpdateEvent?.Invoke(i, j, type);
    }
}
