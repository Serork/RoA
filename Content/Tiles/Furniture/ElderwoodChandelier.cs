using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts.Backwoods;
using RoA.Core.Utility;

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
	public override void SetStaticDefaults() {
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 111;
		TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = [TileID.Chandeliers];

		AddMapEntry(new Color(191, 142, 111), Language.GetText("MapObject.Chandelier"));

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		TileID.Sets.DisableSmartCursor[Type] = true;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		Tile tile = Main.tile[i, j];
		if (tile.TileFrameX < 18) {
			r = 0.9f;
			g = 0.8f;
			b = 0.9f;
		}
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
        TileHelper.AddFluentPoint(this, i, j);
        return false;
    }

    void TileHooks.ITileFluentlyDrawn.FluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing) {
        TileHelper.Chandelier3x3FluentDraw(screenPosition, pos, spriteBatch, tileDrawing);
    }

    TileHooks.ITileFlameData.TileFlameData TileHooks.ITileFlameData.GetTileFlameData(int tileX, int tileY, int type, int tileFrameY) {
        TileHooks.ITileFlameData.TileFlameData tileFlameData = default;
        tileFlameData.flameTexture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Flame");
        tileFlameData.flameColor = new Color(100, 100, 100, 0);
        tileFlameData.flameCount = 5;
        tileFlameData.flameRangeXMin = -12;
        tileFlameData.flameRangeXMax = 13;
        tileFlameData.flameRangeYMin = -12;
        tileFlameData.flameRangeYMax = 13;
        tileFlameData.flameRangeMultX = 0.075f;
        tileFlameData.flameRangeMultY = 0.075f;
        return tileFlameData;
    }
}