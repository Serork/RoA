﻿using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRocks0 : BackwoodsRocks1 {
    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.TileFrameX <= 108) {
            type = ModContent.DustType<Dusts.Backwoods.Stone>();
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.WoodTrash>();
        }

        return true;
    }
}

sealed class BackwoodsRocks2 : BackwoodsRocks1 { }

class BackwoodsRocks1 : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
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

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
}