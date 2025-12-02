using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

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

sealed class ElderwoodCandelabra : ModTile {
    private static Asset<Texture2D> _flameTexture = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
        }

        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

        bool cantPlaceInWater = false;
        if (cantPlaceInWater) {
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
        }

        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        AddMapEntry(new Color(253, 221, 3), Language.GetText("ItemName.Candelabra"));
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodCandelabra>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override void HitWire(int i, int j) {
        Tile tile = Main.tile[i, j];
        int topX = i - tile.TileFrameX / 18 % 2;
        int topY = j - tile.TileFrameY / 18 % 2;
        short frameAdjustment = (short)(tile.TileFrameX >= 36 ? -36 : 36);
        Main.tile[topX, topY].TileFrameX += frameAdjustment;
        Main.tile[topX, topY + 1].TileFrameX += frameAdjustment;
        Main.tile[topX + 1, topY].TileFrameX += frameAdjustment;
        Main.tile[topX + 1, topY + 1].TileFrameX += frameAdjustment;
        Wiring.SkipWire(topX, topY);
        Wiring.SkipWire(topX, topY + 1);
        Wiring.SkipWire(topX + 1, topY);
        Wiring.SkipWire(topX + 1, topY + 1);
        NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Framing.GetTileSafely(i, j);
        if (tile.TileFrameX <= 18 && tile.TileFrameY == 0) {
            r = 1f;
            g = 0.95f;
            b = 0.8f;
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
            Tile tile = Main.tile[i, j];
            if (Main.rand.NextBool(40) && tile.TileFrameX <= 18 && tile.TileFrameY == 0) {
                int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, DustID.Torch, 0f, 0f, 100, default, 1f);
                if (!Main.rand.NextBool(3)) {
                    Main.dust[dust].noGravity = true;
                }
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX > 36 || tile.TileFrameY != 0) {
            return;
        }
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int width = 32;
        int offsetY = 0;
        int height = 16;
        TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tile.TileFrameX, ref tile.TileFrameY);
        var flameTexture = _flameTexture.Value;
        ulong seed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
        for (int c = 0; c < 7; c++) {
            float shakeX = Utils.RandomInt(ref seed, -10, 11) * 0.15f;
            float shakeY = Utils.RandomInt(ref seed, -10, 1) * 0.35f;
            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero;
            Main.spriteBatch.Draw(flameTexture, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new Color(100, 100, 100, 0), 0f, default, 1f, SpriteEffects.None, 0f);
        }
    }
}
