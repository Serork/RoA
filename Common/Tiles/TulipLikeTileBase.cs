﻿using Microsoft.Xna.Framework.Graphics;

using Terraria.ID;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

abstract class TulipLikeTileBase : SimpleTileBaseToGenerateOverTime {
    public override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public sealed override void SetStaticDefaults() {
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        base.SetStaticDefaults();
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 10;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
}