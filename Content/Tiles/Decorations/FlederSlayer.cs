using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class FlederSlayerDecoration : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.AvoidedByNPCs[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 6;
        TileObjectData.newTile.Origin = new Point16(1, 5);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];
        TileObjectData.newTile.DrawYOffset = 4;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(172, 157, 148), CreateMapEntryName());
        //DustType = ModContent.DustType<CosmicCrystalDust>();
        //AnimationFrameHeight = 18 * 5;

        //DustType = ModContent.DustType<LothorEnrageMonolithDust>();
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Decorations.FlederSlayerDecoration>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = 0;
        //num = (int)(num / 2);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        int frameX = Main.tile[i, j].TileFrameX;
        int frameY = Main.tile[i, j].TileFrameY;
        int height = frameY % (18 * 6 + 2) == 54 ? 18 : 16;
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Color color = Lighting.GetColor(i, j);
        var t = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, Main.tile[i, j].TileColor);
        if (t == null)
            t = TextureAssets.Tile[Type].Value;
        int frameHeight = 18 * 6 + 2;
        spriteBatch.Draw(t, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition + new Vector2(0f, 4f), new Rectangle(frameX, frameY, 16, height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }
}