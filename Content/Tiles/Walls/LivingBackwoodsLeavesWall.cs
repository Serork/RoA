using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class LivingBackwoodsLeavesWall2 : LivingBackwoodsLeavesWall {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        Main.wallHouse[Type] = false;
    }

    public override bool Drop(int i, int j, ref int type) => false;
}

class LivingBackwoodsLeavesWall : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(0, 75, 38));

        HitSound = SoundID.Dig;
    }
}