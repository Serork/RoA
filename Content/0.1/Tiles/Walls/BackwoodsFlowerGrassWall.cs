using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

class BackwoodsFlowerGrassWall : ModWall {
    public override void SetStaticDefaults() {
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(0, 67, 17));

        HitSound = SoundID.Grass;

        WallID.Sets.Conversion.Grass[Type] = true;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
    }

    public override bool Drop(int i, int j, ref int type) => false;
}