using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class BackwoodsGrassWall2 : ModWall {
	public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
		AddMapEntry(new Color(0, 67, 17));

		WallID.Sets.WallSpreadStopsAtAir[Type] = true;

    }

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}