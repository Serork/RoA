using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRockSpikes1 : BackwoodsRockSpikes { }

sealed class BackwoodsRockSpikes2 : BackwoodsRockSpikes {
    protected override bool AnchorBottom => true;
}

sealed class BackwoodsRockSpikes3 : BackwoodsRockSpikes {
    protected override bool IsSmall => true;
}

sealed class BackwoodsRockSpikes4 : BackwoodsRockSpikes {
    protected override bool AnchorBottom => true;

    protected override bool IsSmall => true;
}

abstract class BackwoodsRockSpikes : ModTile {
    protected virtual bool AnchorBottom { get; }

    protected virtual bool IsSmall { get; }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.DrawYOffset = AnchorBottom ? 2 : -2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = IsSmall ? 1 : 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        if (AnchorBottom) {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        }
        else {
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        }
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = IsSmall ? [16] : [16, 16];
        TileObjectData.newTile.CoordinateWidth = 18;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();
        //AddMapEntry(new Microsoft.Xna.Framework.Color(91, 74, 67), CreateMapEntryName());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsSmall ? 3 : 5;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
}
