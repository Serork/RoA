using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

class TealMossWall : ModWall {
    public override void SetStaticDefaults() {
        AddMapEntry(new Color(34, 37, 46));

        Main.wallHouse[Type] = true;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool(3)) {
            type = DustID.GreenMoss; 
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.Stone>();
        }

        return base.CreateDust(i, j, ref type);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}

class TealMossWall2 : TealMossWall {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        AddMapEntry(new Color(34, 37, 46));

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool(3)) {
            type = DustID.GreenMoss;
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.Stone>();
        }

        return base.CreateDust(i, j, ref type);
    }

    public override bool Drop(int i, int j, ref int type) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}