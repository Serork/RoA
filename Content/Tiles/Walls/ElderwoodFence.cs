using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodFence : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();
        AddMapEntry(new Color(56, 42, 27));

        Main.wallLight[Type] = true;
    }
}