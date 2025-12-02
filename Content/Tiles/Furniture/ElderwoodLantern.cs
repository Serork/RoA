using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
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

public class ElderwoodLantern : ModTile, TileHooks.ITileFluentlyDrawn {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoFail[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

        AdjTiles = [TileID.HangingLanterns];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.DrawYOffset = -10;
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
        TileObjectData.addAlternate(0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(251, 235, 127), Language.GetText("MapObject.Lantern"));
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodLantern>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
    }

    public override void HitWire(int i, int j) {
        Tile tile = Main.tile[i, j];
        int topY = j - tile.TileFrameY / 18 % 2;
        short frameAdjustment = (short)(tile.TileFrameX > 0 ? -18 : 18);
        Main.tile[i, topY].TileFrameX += frameAdjustment;
        Main.tile[i, topY + 1].TileFrameX += frameAdjustment;
        Wiring.SkipWire(i, topY);
        Wiring.SkipWire(i, topY + 1);
        NetMessage.SendTileSquare(-1, i, topY + 1, 2, TileChangeType.None);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX == 0) {
            r = 0.25f;
            g = 0.65f;
            b = 0.95f;
        }
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddFluentPoint(this, i, j);
        /*if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
            Tile tile = Main.tile[i, j];
            if (Main.rand.NextBool(40) && tile.TileFrameX == 0 && tile.TileFrameY != 0) {
                int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16), 4, 4, ModContent.DustType<ElderTorchDust>(), 0f, 0f, 100, default, 1f);
                if (!Main.rand.NextBool(3)) {
                    Main.dust[dust].noGravity = true;
                }
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
            }
        }*/
        return false;
    }

    void TileHooks.ITileFluentlyDrawn.FluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing) {
        TileHelper.LanternFluentDraw(screenPosition, pos, spriteBatch, tileDrawing);
    }

    /*TileHooks.ITileFlameData.TileFlameData TileHooks.ITileFlameData.GetTileFlameData(int tileX, int tileY, int type, int tileFrameY) {
        TileHooks.ITileFlameData.TileFlameData tileFlameData = default;
        tileFlameData.flameTexture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Flame");
        tileFlameData.flameColor = new DrawColor(100, 100, 100, 0);
        tileFlameData.flameCount = 7;
        tileFlameData.flameRangeXMin = -10;
        tileFlameData.flameRangeXMax = 11;
        tileFlameData.flameRangeYMin = -10;
        tileFlameData.flameRangeYMax = 1;
        tileFlameData.flameRangeMultX = 0.15f;
        tileFlameData.flameRangeMultY = 0.35f;
        return tileFlameData;
    }*/
}