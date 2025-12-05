using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRoots1 : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        //Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;

        Main.tileObsidianKill[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        //TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 0, 0);
        //TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(110, 91, 74));
        DustType = ModContent.DustType<WoodTrash>();
    }

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
        if (tile9 != null && tile9.HasTile && !tile9.BottomSlope && WorldGen.SolidTile(tile9)) {
            num3 = tile9.TileType;
        }
        if (tile17 != null && tile17.HasTile && !tile17.IsHalfBlock && !tile17.TopSlope && WorldGen.SolidTile(tile17)) {
            num2 = tile17.TileType;
        }
        if (tile24 != null && tile24.HasTile && WorldGen.SolidTile(tile24)) {
            num4 = tile24.TileType;
        }
        if (tile31 != null && tile31.HasTile && WorldGen.SolidTile(tile31)) {
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

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;
        short frameX = tile.TileFrameX;
        short frameY = tile.TileFrameY;
        short width = 16;
        short height = 16;
        Tile tile9 = Main.tile[i, j - 1];
        Tile tile17 = Main.tile[i, j + 1];
        Tile tile24 = Main.tile[i - 1, j];
        Tile tile31 = Main.tile[i + 1, j];
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        int num5 = -1;
        if (tile9 != null && tile9.HasTile && !tile9.BottomSlope && WorldGen.SolidTile(tile9)) {
            num3 = tile9.TileType;
        }
        if (tile17 != null && tile17.HasTile && !tile17.IsHalfBlock && !tile17.TopSlope && WorldGen.SolidTile(tile17)) {
            num2 = tile17.TileType;
        }
        if (tile24 != null && tile24.HasTile && WorldGen.SolidTile(tile24)) {
            num4 = tile24.TileType;
        }
        if (tile31 != null && tile31.HasTile && WorldGen.SolidTile(tile31)) {
            num5 = tile31.TileType;
        }
        int offsetX = 0;
        int offsetY = 0;
        if (num2 >= 0) {
            offsetY += 2;
        }
        else if (num3 >= 0) {
            offsetX -= 2;
            width += 2;
            offsetY -= 2;
        }
        else if (num4 >= 0) {
            offsetX -= 4;
        }
        else if (num5 >= 0) {
            offsetX += 1;
            width += 1;
        }
        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X + offsetX, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero,
                              new Rectangle(0, frameY, width, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }
}
