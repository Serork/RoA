using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Content.UI;
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
    private static BlendState? _multiplyBlendState;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<NixieTubeTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.addTile(Type);

        DustType = DustID.WoodFurniture;

        AddMapEntry(new Color(153, 38, 0), CreateMapEntryName());
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        NixieTubePicker.Deactivate(new Point16(i, j));
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<NixieTubeTE>().Kill(i, j);

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

        int height = WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type ? 18 : 16;

        Color color = Lighting.GetColor(i, j);
        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                              new Rectangle(frameX % 36, frameY % 56, 16, height),
                              color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        if (flag && GetTE(i, j).Active) {
            Main.spriteBatch.Draw(texture,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                  new Rectangle(frameX, frameY, 16, height),
                                  Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            _multiplyBlendState ??= new() {
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.SourceAlpha
            };
            SpriteBatch batch = Main.spriteBatch;
            SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
            batch.Begin(snapshot with { blendState = _multiplyBlendState }, true);
            Main.spriteBatch.Draw(texture,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                  new Rectangle(36 + frameX % 36, frameY % 56, 16, height),
                                  color * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            batch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            Main.spriteBatch.Draw(texture,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                  new Rectangle(36 + frameX % 36, frameY % 56, 16, height),
                                  color * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            batch.Begin(snapshot, true);
        }

        return false;
    }

    public static NixieTubeTE GetTE(int i, int j) {
        while (TileHelper.GetTE<NixieTubeTE>(i, j) == null) {
            j++;
            if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != ModContent.TileType<NixieTube>()) {
                bool flag = TileHelper.GetTE<NixieTubeTE>(i - 1, j) != null && WorldGenHelper.GetTileSafely(i - 1, j).TileFrameX % 36 == 0;
                if (flag || (TileHelper.GetTE<NixieTubeTE>(i + 1, j) != null && WorldGenHelper.GetTileSafely(i + 1, j).TileFrameX % 18 == 0)) {
                    i += flag.ToDirectionInt() * -1;
                    break;
                }
            }
        }
        return TileHelper.GetTE<NixieTubeTE>(i, j);
    }
}
