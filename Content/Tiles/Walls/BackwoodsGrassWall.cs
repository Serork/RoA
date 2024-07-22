using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class BackwoodsGrassWall : ModWall {
	public override void SetStaticDefaults() {
		DustType = (ushort)ModContent.DustType<BackwoodsGrass>();
		AddMapEntry(new Color(19, 82, 44));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}