using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class SolidifiedTarWall : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = ModContent.DustType<Dusts.SolidifiedTar>();
        AddMapEntry(new Color(48, 40, 54));
    }
}

sealed class SolidifiedTarWall_Unsafe : ModWall {
    public override string Texture => WallLoader.GetWall(ModContent.WallType<SolidifiedTarWall>()).Texture;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = false;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
        WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;

        DustType = ModContent.DustType<Dusts.SolidifiedTar>();
        AddMapEntry(new Color(48, 40, 54));
    }
}
