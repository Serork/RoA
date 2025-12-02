using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
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

abstract class BackwoodsRockSpikes : ModTile, TileHooks.IGetTileDrawData {
    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = this.GetTileGlowTexture();
        glowColor = TileDrawingExtra.BackwoodsMossGlowColor;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool()) {
            type = ModContent.DustType<TealMossDust>();
        }

        return base.CreateDust(i, j, ref type);
    }

    protected virtual bool AnchorBottom { get; }

    protected virtual bool IsSmall { get; }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

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
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 0.01f;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? (IsSmall ? 1 : 2) : IsSmall ? 3 : 5;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
}
