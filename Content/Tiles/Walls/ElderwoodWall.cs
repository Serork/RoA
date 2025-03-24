using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.Tiles.TileHooks;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodWall3 : ElderwoodWall, IRequireMinHammerPower, IResistToHammer {
    int IRequireMinHammerPower.MinHammer => 55;

    bool IResistToHammer.CanBeApplied(int i, int j) => true;
    float IResistToHammer.ResistToPick => 0.25f;

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