using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodWall3 : ElderwoodWall {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        Main.wallHouse[Type] = false;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
        WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
    }

    public override string Texture => base.Texture[..^1];
}

sealed class ElderwoodWall2 : ElderwoodWall {
    public override string Texture => base.Texture[..^1];
}

class ElderwoodWall : ModWall {
	public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        DustType = (ushort)ModContent.DustType<WoodTrash>();
		AddMapEntry(new Color(112, 55, 31));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}