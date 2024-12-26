using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodFence : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        DustType = (ushort)ModContent.DustType<WoodTrash>();
        AddMapEntry(new Color(56, 42, 27));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}