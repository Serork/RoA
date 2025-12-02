using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

class ElderTorch : ModTile {
    private static Asset<Texture2D> _flameTexture = null!;

    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileWaterDeath[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.Torch[Type] = true;

        DustType = ModContent.DustType<ElderTorchDust>();
        AdjTiles = [TileID.Torches];

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

        // Placement
        TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorAlternateTiles = [124];
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorAlternateTiles = [124];
        TileObjectData.addAlternate(2);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newAlternate.AnchorWall = true;
        TileObjectData.addAlternate(0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(253, 221, 3), Language.GetText("ItemName.Torch"));

        if (Main.dedServ) {
            return;
        }

        _flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;

        int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
        player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
    }

    public override float GetTorchLuck(Player player) {
        bool inBackwoods = player.InModBiome<BackwoodsBiome>();
        return inBackwoods ? 1f : -0.1f;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Tile tile = Main.tile[i, j];
        int tileFrameX = tile.TileFrameX;
        if (Main.rand.Next(40) == 0 && tileFrameX < 66) {
            int num14 = DustType;
            int num15 = 0;
            switch (tileFrameX) {
                case 22:
                    num15 = Dust.NewDust(new Vector2(i * 16 + 6, j * 16), 4, 4, num14, 0f, 0f, 100);
                    break;
                case 44:
                    num15 = Dust.NewDust(new Vector2(i * 16 + 2, j * 16), 4, 4, num14, 0f, 0f, 100);
                    break;
                default:
                    num15 = Dust.NewDust(new Vector2(i * 16 + 4, j * 16), 4, 4, num14, 0f, 0f, 100);
                    break;
            }
            if (Main.rand.Next(3) != 0)
                Main.dust[num15].noGravity = true;

            Main.dust[num15].velocity *= 0.3f;
            Main.dust[num15].velocity.Y -= 1.5f;
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX < 66) {
            int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
            if (style == 0) {
                r = 0.25f;
                g = 0.65f;
                b = 0.85f;
            }
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = 0;

        if (WorldGen.SolidTile(i, j - 1)) {
            offsetY = 2;

            if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1)) {
                offsetY = 4;
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        var tile = Main.tile[i, j];

        if (!TileDrawing.IsVisible(tile)) {
            return;
        }

        int offsetY = 0;

        if (WorldGen.SolidTile(i, j - 1)) {
            offsetY = 4;
        }

        Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }

        ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
        Color color = new(100, 100, 100, 0);
        int width = 20;
        int height = 20;
        int frameX = tile.TileFrameX;
        int frameY = tile.TileFrameY;
        int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
        if (style == 1) {
            color.G = 255;
        }

        for (int k = 0; k < 7; k++) {
            float xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
            float yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

            spriteBatch.Draw(_flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
        }
    }
}