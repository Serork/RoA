using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class MercuriumNugget : ModTile {
    public override void SetStaticDefaults() {
        Main.tileShine[Type] = 1100;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<MercuriumOre>();

        AddMapEntry(new Color(224, 194, 101), Language.GetText("MapObject.MetalBar"));
    }
}