using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Trees;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Platforms;

class TreeBranch : ModTile, TileHooks.IRequireMinAxePower {
    public override void Load() {
        On_Main.DrawSmartCursor += On_Main_DrawSmartCursor;
    }

    // TODO: separate
    private void On_Main_DrawSmartCursor(On_Main.orig_DrawSmartCursor orig) {
        if (Main.SmartCursorShowing && !Main.player[Main.myPlayer].dead && TileLoader.GetTile(Main.tile[Main.SmartCursorX, Main.SmartCursorY].TileType) is TreeBranch) {
            Vector2 vector = new Vector2(Main.SmartCursorX, Main.SmartCursorY) * 16f;
            new Vector2(Main.offScreenRange, Main.offScreenRange);
            _ = Main.drawToScreen;
            vector -= Main.screenPosition;
            if (Main.player[Main.myPlayer].gravDir == -1f)
                vector.Y = (float)Main.screenHeight - vector.Y - 16f;

            bool left = PrimordialTree.IsPrimordialTree(Main.SmartCursorX - 1, Main.SmartCursorY);
            Microsoft.Xna.Framework.Color newColor = Lighting.GetColor(Main.SmartCursorX, Main.SmartCursorY) * 1f;
            Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1);
            float r = 1f;
            float g = 0.9f;
            float b = 0.1f;
            float a = 1f;
            float num = 0.6f;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (!left) {
                vector.X -= 14f;
            }
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, 8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitX * 8f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, 8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + new Vector2(8f, 8f), value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, 8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitX * 16f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(16f, 8f), SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitY * 8f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, 8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + new Vector2(16f, 8f), value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(16f, 8f), SpriteEffects.None, 0f);
            b = 0.3f;
            g = 0.95f;
            a = (num = 1f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitX * -2f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(2f, 16f), SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitX * 32f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(2f, 16f), SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitY * -2f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(32f, 2f), SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, vector + Vector2.UnitY * 16f, value, Main.buffColor(newColor, r, g, b, a) * num, 0f, Vector2.Zero, new Vector2(32f, 2f), SpriteEffects.None, 0f);

            return;
        }

        orig();
    }

    int TileHooks.IRequireMinAxePower.MinAxe => PrimordialTree.MINAXEREQUIRED;

    protected virtual int FrameCount => 2;

    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;

        TileID.Sets.Platforms[Type] = true;

        CantBeSlopedTileSystem.Included[Type] = true;

        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 32;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Platforms];

        RegisterItemDrop(ModContent.ItemType<Items.Placeable.Solid.Elderwood>());
        DustType = ModContent.DustType<WoodTrash>();

        AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());
    }

    public override bool CanExplode(int i, int j) {
        if (!NPC.downedBoss2) {
            return false;
        }

        return base.CanExplode(i, j);
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

    public override void NearbyEffects(int i, int j, bool closer) {
        //if (!closer && Main.rand.NextBool(20)) {
        //    Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j), rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        //    if ((!rightTile.HasTile && !leftTile.HasTile) ||
        //        (!PrimordialTree.IsPrimordialTree(i + 1, j) && !PrimordialTree.IsPrimordialTree(i - 1, j))) {
        //        WorldGen.KillTile(i, j);
        //        if (Main.netMode == NetmodeID.MultiplayerClient) {
        //            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 1f);
        //        }
        //    }
        //}
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        //if (Main.netMode == NetmodeID.MultiplayerClient)
        //    return false;

        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j), rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        if ((!rightTile.HasTile && !leftTile.HasTile) ||
            (!PrimordialTree.IsPrimordialTree(i + 1, j) && !PrimordialTree.IsPrimordialTree(i - 1, j))) {
            WorldGen.KillTile(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 1f);
            }
        }

        return false;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        ulong seedForRandomness = (ulong)(i + j);
        int frame = Math.Min(Utils.RandomInt(ref seedForRandomness, FrameCount + 1), FrameCount - 1);
        bool reversed = true;
        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j), rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        bool hasRightTile = rightTile.HasTile;
        if (hasRightTile && leftTile.TileType != TileID.Trees) {
            reversed = false;
        }
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(Main.tile[i, j], i, j);
        texture ??= TextureAssets.Tile[Type].Value;
        int frameWidth = texture.Width, frameHeight = texture.Height / FrameCount;
        Vector2 drawPosition = new Point(i, j).ToVector2() * 16f + zero;
        drawPosition.X += -(!reversed ? (frameWidth / 2 - 2) : 2);
        Rectangle? sourceRectangle = new Rectangle(0, frameHeight * frame, frameWidth, frameHeight);
        Main.EntitySpriteDraw(texture,
                              drawPosition - Main.screenPosition,
                              sourceRectangle,
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f,
                              !reversed ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

        return false;
    }
}
