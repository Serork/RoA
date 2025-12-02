using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class SolidifiedTar : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;

        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Color(68, 57, 77));
    }
}
