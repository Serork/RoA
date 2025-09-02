using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.UI;
using RoA.Content.World.Generations;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTube : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<NixieTubeEntity>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.addTile(Type);

        DustType = DustID.WoodFurniture;

        AddMapEntry(new Color(153, 38, 0), CreateMapEntryName());
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        NixieTubePicker.Deactivate();
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80)) {
            player.cursorItemIconID = ItemID.Cog;
            if (player.cursorItemIconID != -1) {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
            }
        }
    }

    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80)) {
            NixieTubePicker.Activate(new Point16(i, j));
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        int frameY = tile.TileFrameY;
        int frameX = tile.TileFrameX;
        bool flag = frameY >= 56;
        if (flag) {
            r = 224 / 255f;
            g = 74 / 255f;
            b = 0 / 255f;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (!TileDrawing.IsVisible(tile)) {
            return false;
        }

        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int frameY = tile.TileFrameY;
        int frameX = tile.TileFrameX;
        bool flag = frameY >= 56;
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;
        TileObjectData tileObjectData = TileObjectData.GetTileData(Type, 0);

        Color color = Lighting.GetColor(i, j);
        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                              new Rectangle(frameX % 36, frameY % 56, 16, WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type ? 18 : 16),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        if (flag) {
            Main.spriteBatch.Draw(texture,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                  new Rectangle(frameX, frameY, 16, WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type ? 18 : 16),
                                  Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        return false;
    }
}
