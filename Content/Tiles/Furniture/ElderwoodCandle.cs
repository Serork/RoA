﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodCandle : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        AddMapEntry(new Color(253, 221, 3), Language.GetText("ItemName.Candle"));
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodCandle>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void HitWire(int i, int j) {
        Tile tile = Main.tile[i, j];
        int topX = i - tile.TileFrameX / 18 % 1;
        int topY = j - tile.TileFrameY / 18 % 1;
        short frameAdjustment = (short)(tile.TileFrameX >= 18 ? -18 : 18);
        Main.tile[topX, topY].TileFrameX += frameAdjustment;
        Wiring.SkipWire(topX, topY);
        NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = SpriteEffects.None;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        if (Framing.GetTileSafely(i, j).TileFrameX == 0) {
            r = 0.25f;
            g = 0.65f;
            b = 0.95f;
        }
    }

    /*public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
            Tile tile = Main.tile[i, j];
            if (Main.rand.NextBool(40) && tile.TileFrameX == 0 && tile.TileFrameY / 18 % 3 == 0) {
                int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, ModContent.DustType<ElderTorchDust>(), 0f, 0f, 100, default, 1f);
                if (!Main.rand.NextBool(3)) {
                    Main.dust[dust].noGravity = true;
                }
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
            }
        }
    }*/

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        if (Framing.GetTileSafely(i, j).TileFrameX != 0 || Framing.GetTileSafely(i, j).TileFrameY != 0) {
            return;
        }
        var tile = Main.tile[i, j];
        if (!TileDrawing.IsVisible(tile)) {
            return;
        }
        SpriteEffects effects = SpriteEffects.None;
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
        int width = 16;
        int offsetY = 0;
        int height = 16;
        TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tile.TileFrameX, ref tile.TileFrameY);
        /*var flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame").Value;
        ulong seed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
        for (int c = 0; c < 7; c++) {
            float shakeX = Utils.RandomInt(ref seed, -10, 11) * 0.15f;
            float shakeY = Utils.RandomInt(ref seed, -10, 1) * 0.35f;
            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero;
            Main.spriteBatch.Draw(flameTexture, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new Color(100, 100, 100, 0), 0f, default, 1f, effects, 0f);
        }*/
    }
}
