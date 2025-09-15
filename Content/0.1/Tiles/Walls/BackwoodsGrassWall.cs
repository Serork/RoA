using Microsoft.Xna.Framework;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class BackwoodsGrassWall : ModWall {
    public override void SetStaticDefaults() {
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(0, 67, 17));

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;

        WallID.Sets.Conversion.Grass[Type] = true;

        HitSound = SoundID.Grass;
    }

    public override bool Drop(int i, int j, ref int type) => false;
}