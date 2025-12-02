using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsDirt : ModTile {
    public override void SetStaticDefaults() {
        TileHelper.Solid(Type);

        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        RegisterItemDrop(ItemID.DirtBlock);
        DustType = DustID.Dirt;
        AddMapEntry(new Color(151, 107, 75));
    }
}