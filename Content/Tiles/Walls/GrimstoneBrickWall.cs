using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class GrimstoneBrickWall : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        AddMapEntry(new Color(54, 55, 64));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}