using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class BackwoodsGrassWall : ModWall {
	public override void SetStaticDefaults() {
		DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
		AddMapEntry(new Color(0, 67, 17));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}