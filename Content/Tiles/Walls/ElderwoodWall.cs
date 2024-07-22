using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodWall2 : ElderwoodWall {
    public override string Texture => base.Texture[..^1];
}

class ElderwoodWall : ModWall {
	public override void SetStaticDefaults() {
        DustType = (ushort)ModContent.DustType<BackwoodsWoodTrash>();
		AddMapEntry(new Color(112, 55, 31));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}