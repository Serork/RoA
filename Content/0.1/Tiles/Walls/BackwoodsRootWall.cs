using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

class BackwoodsRootWall : ModWall {
    public override void SetStaticDefaults() {
        AddMapEntry(new Color(59, 46, 42));

        Main.wallHouse[Type] = true;

        Main.wallLight[Type] = true;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        type = ModContent.DustType<WoodTrash>();

        return base.CreateDust(i, j, ref type);
    }
}

class BackwoodsRootWall2 : BackwoodsRootWall {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        AddMapEntry(new Color(59, 46, 42));

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;

        Main.wallLight[Type] = true;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        type = ModContent.DustType<WoodTrash>();

        return base.CreateDust(i, j, ref type);
    }

    public override bool Drop(int i, int j, ref int type) => false;
}