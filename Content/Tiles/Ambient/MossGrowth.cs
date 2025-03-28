using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Solid;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class MossGrowth : ModTile, TileHooks.IGetTileDrawData {
    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = this.GetTileGlowTexture();
        glowColor = TileDrawingExtra.BackwoodsMossGlowColor;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        //Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;

        Main.tileObsidianKill[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        //TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(29, 106, 88));
        DustType = ModContent.DustType<TealMossDust>();
        HitSound = SoundID.Grass;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile = Main.tile[i, j];
        Tile tile9 = Main.tile[i, j - 1];
        Tile tile17 = Main.tile[i, j + 1];
        Tile tile24 = Main.tile[i - 1, j];
        Tile tile31 = Main.tile[i + 1, j];
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        int num5 = -1;
        ushort moss = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        ushort moss2 = (ushort)ModContent.TileType<BackwoodsGreenMossBrick>();
        if (tile9 != null && tile9.HasTile && (tile9.TileType == moss || tile9.TileType == moss2) && !tile9.BottomSlope) {
            num3 = tile9.TileType;
        }
        if (tile17 != null && tile17.HasTile && (tile17.TileType == moss || tile17.TileType == moss2) && !tile17.IsHalfBlock && !tile17.TopSlope) {
            num2 = tile17.TileType;
        }
        if (tile24 != null && tile24.HasTile && (tile24.TileType == moss || tile24.TileType == moss2)) {
            num4 = tile24.TileType;
        }
        if (tile31 != null && tile31.HasTile && (tile31.TileType == moss || tile31.TileType == moss2)) {
            num5 = tile31.TileType;
        }
        short num6 = (short)(WorldGen.genRand.Next(3) * 18);
        if (num2 >= 0) {
            if (tile.TileFrameY <= 0 || tile.TileFrameY > 36) {
                tile.TileFrameY = num6;
            }
        }
        else if (num3 >= 0) {
            if (tile.TileFrameY < 54 || tile.TileFrameY > 90) {
                tile.TileFrameY = (short)(54 + num6);
            }
        }
        else if (num4 >= 0) {
            if (tile.TileFrameY < 108 || tile.TileFrameY > 144) {
                tile.TileFrameY = (short)(108 + num6);
            }
        }
        else if (num5 >= 0) {
            if (tile.TileFrameY < 162 || tile.TileFrameY > 198) {
                tile.TileFrameY = (short)(162 + num6);
            }
        }
        else {
            WorldGen.KillTile(i, j);
        }

        return false;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        width = 22;
        short framesHeight = 54;
        ushort moss = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        ushort moss2 = (ushort)ModContent.TileType<BackwoodsGreenMossBrick>();
        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
        Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
        Tile rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        offsetY = 0;
        if (aboveTile.ActiveTile(moss) || aboveTile.ActiveTile(moss2)) {
            tileFrameY = framesHeight;
            offsetY = -2;
        }
        else if (belowTile.ActiveTile(moss) || belowTile.ActiveTile(moss2)) {
            offsetY = 2;
        }
        else {
            if (leftTile.ActiveTile(moss) || leftTile.ActiveTile(moss2)) {
                tileFrameY = (short)(framesHeight * 2);
            }
            if (rightTile.ActiveTile(moss) || rightTile.ActiveTile(moss2)) {
                tileFrameY = (short)(framesHeight * 3);
            }
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddSpecialPoint(i, j, 12);

        return false;
    }
}
