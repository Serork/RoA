using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class BackwoodsFlowerGrassWall2 : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(0, 67, 17));

        HitSound = SoundID.Grass;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
    }
}