using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class NexusGateway : ModTile {
    private const byte FRAMERATE = 12;

    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        Main.tileLighted[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = false;
        Main.tileSolid[Type] = false;

        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.LavaDeath = false;
        int width = 7, height = 8;
        TileObjectData.newTile.Width = width;
        TileObjectData.newTile.Height = height;
        TileObjectData.newTile.CoordinateHeights = new int[height];
        for (int k = 0; k < height; k++) {
            TileObjectData.newTile.CoordinateHeights[k] = 16;
        }
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(0, 120, 154), CreateMapEntryName());

        //DustType = ModContent.DustType<BackwoodsPotDust1>();
        //HitSound = SoundID.Tink;

        TileID.Sets.DisableSmartCursor[Type] = true;

        AnimationFrameHeight = 144;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override bool CreateDust(int i, int j, ref int type) => false;

    public override bool KillSound(int i, int j, bool fail) => false;

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        frameCounter++;
        if (frameCounter > FRAMERATE) {
            frameCounter = 0;
            frame++;
            if (frame > 3) {
                frame = 0;
            }
        }
    }

    private static bool GetCondition(Tile tile) {
        List<(short, short)> properFramingXY = [
            (18, 18),
            (36, 0),
            (54, 0),
            (72, 0),
            (90, 0),
            (36, 18),
            (54, 18),
            (72, 18),
            (90, 18),
            (18, 36),
            (18, 54),
            (18, 108),
            (90, 54),
            (90, 90),
            (108, 54),
            (36, 54),
            (90, 72)
            ];
        return properFramingXY.Contains((tile.TileFrameX, (short)(tile.TileFrameY % 144)));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        if (GetCondition(tile)) {
            r = 0.25f * 0.75f;
            g = 0.65f * 0.75f;
            b = 0.85f * 0.75f;
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        //if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
        //    Tile tile = Main.tile[i, j];
        //    if (Main.rand.NextChance(0.35f) && GetCondition(tile)) {
        //        bool right = tile.TileFrameX > 90;
        //        int dust = Dust.NewDust(new Vector2(i * 16 - (!right ? 2 : 16), j * 16 + 2), 16, 4, ModContent.DustType<ElderTorchDust>(), 0f, 0f, 100, default, 1f);
        //        if (!Main.rand.NextBool(3)) {
        //            Main.dust[dust].noGravity = true;
        //        }
        //        Main.dust[dust].velocity *= 0.3f;
        //        Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
        //    }
        //}
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
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
        //var flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame").Value;
        var glowMaskTexture = _glowTexture.Value;
        Main.spriteBatch.Draw(glowMaskTexture, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new Color(100, 100, 100, 0), 0f, default, 1f, SpriteEffects.None, 0f);
        float progress = Main.tileFrameCounter[Type] / (float)FRAMERATE;
        int frame = Main.tileFrame[Type] + 1;
        if (frame > 3) {
            frame = 0;
        }
        Main.spriteBatch.Draw(glowMaskTexture, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero,
            new Rectangle(tile.TileFrameX, tile.TileFrameY + (144 * frame), 16, 16), new Color(100, 100, 100, 0) * progress, 0f, default, 1f, SpriteEffects.None, 0f);
        //ulong seed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
        //for (int c = 0; c < 2; c++) {
        //    float shakeX = Utils.RandomInt(ref seed, -10, 11) * 0.15f;
        //    float shakeY = Utils.RandomInt(ref seed, -10, 1) * 0.35f;
        //    Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero;
        //    Main.spriteBatch.DrawSelf(flameTexture, pos + Vector2.UnitY * c, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new DrawColor(100, 100, 100, 0), 0f, default, 1f, SpriteEffects.None, 0f);
        //}
        //for (int c = 0; c < 3; c++) {
        //    float shakeX = Utils.RandomInt(ref seed, -10, 11) * 0.15f;
        //    float shakeY = Utils.RandomInt(ref seed, -10, 1) * 0.35f;
        //    Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero;
        //    Main.spriteBatch.DrawSelf(flameTexture, pos - Vector2.UnitY * 2f * c, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), new DrawColor(100, 100, 100, 0), 0f, default, 1f, SpriteEffects.None, 0f);
        //}
    }
}
