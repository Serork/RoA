using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class GrimstoneWall : ModWall {
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        AddMapEntry(new Color(34, 37, 46));
    }
}