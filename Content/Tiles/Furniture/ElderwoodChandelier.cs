using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Content.Dusts.Backwoods;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodChandelier : ModTile, TileHooks.ITileFluentlyDrawn, TileHooks.ITileFlameData {
    private static Asset<Texture2D> _flameTexture = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
        }

        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.Origin = new Point16(1, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 111;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = [TileID.Chandeliers];

        AddMapEntry(new Color(235, 166, 135), Language.GetText("MapObject.Chandelier"));

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        TileID.Sets.DisableSmartCursor[Type] = true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX < 54) {
            r = 1f;
            g = 0.95f;
            b = 0.8f;
        }
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChandelier>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void HitWire(int i, int j) {
        Tile tile = Main.tile[i, j];
        int topX = i - tile.TileFrameX / 18 % 3;
        int topY = j - tile.TileFrameY / 18 % 3;
        short frameAdjustment = (short)(tile.TileFrameX >= 54 ? -54 : 54);
        Main.tile[topX, topY].TileFrameX += frameAdjustment;
        Main.tile[topX, topY + 1].TileFrameX += frameAdjustment;
        Main.tile[topX, topY + 2].TileFrameX += frameAdjustment;
        Main.tile[topX + 1, topY].TileFrameX += frameAdjustment;
        Main.tile[topX + 1, topY + 1].TileFrameX += frameAdjustment;
        Main.tile[topX + 1, topY + 2].TileFrameX += frameAdjustment;
        Main.tile[topX + 2, topY].TileFrameX += frameAdjustment;
        Main.tile[topX + 2, topY + 1].TileFrameX += frameAdjustment;
        Main.tile[topX + 2, topY + 2].TileFrameX += frameAdjustment;
        Wiring.SkipWire(topX, topY);
        Wiring.SkipWire(topX, topY + 1);
        Wiring.SkipWire(topX, topY + 2);
        Wiring.SkipWire(topX + 1, topY);
        Wiring.SkipWire(topX + 1, topY + 1);
        Wiring.SkipWire(topX + 1, topY + 2);
        Wiring.SkipWire(topX + 2, topY);
        Wiring.SkipWire(topX + 2, topY + 1);
        Wiring.SkipWire(topX + 2, topY + 2);
        NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddFluentPoint(this, i, j);

        Tile tile = Main.tile[i, j];
        if (Main.rand.Next(40) == 0 && tile.TileFrameX < 54) {
            int num25 = tile.TileFrameY / 54;
            int num26 = tile.TileFrameX / 18 % 3;
            if (tile.TileFrameY < 36 && tile.TileFrameX != 18 && num26 != 1) {
                int num27 = 6;
                if (num27 != -1) {
                    int value = tile.TileFrameX != 0 ? 4 : 0;
                    int num28 = Dust.NewDust(new Vector2(i * 16 + value, j * 16), 6, 6, num27, 0f, 0f, 100);
                    if (Main.rand.Next(3) != 0)
                        Main.dust[num28].noGravity = true;

                    Main.dust[num28].velocity *= 0.3f;
                    Main.dust[num28].velocity.Y -= 1.5f;
                }
            }
        }

        return false;
    }

    void TileHooks.ITileFluentlyDrawn.FluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing) {
        TileHelper.Chandelier3x3FluentDraw(screenPosition, pos, spriteBatch, tileDrawing);
    }

    TileHooks.ITileFlameData.TileFlameData TileHooks.ITileFlameData.GetTileFlameData(int tileX, int tileY, int type, int tileFrameY) {
        TileHooks.ITileFlameData.TileFlameData tileFlameData = default;
        tileFlameData.flameTexture = _flameTexture.Value;
        tileFlameData.flameColor = new Color(100, 100, 100, 0);
        tileFlameData.flameCount = 7;
        tileFlameData.flameRangeXMin = -10;
        tileFlameData.flameRangeXMax = 11;
        tileFlameData.flameRangeYMin = -10;
        tileFlameData.flameRangeYMax = 1;
        tileFlameData.flameRangeMultX = 0.15f;
        tileFlameData.flameRangeMultY = 0.35f;
        return tileFlameData;
    }
}