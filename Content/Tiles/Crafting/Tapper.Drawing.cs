using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

partial class Tapper : ModTile {
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        tileFrameX = tileFrameY = 0;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (WorldGenHelper.ActiveTile(i - 1, j, TileID.Trees)) {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        if (++frameCounter >= 8) {
            frameCounter = 0;
            frame = ++frame % 3;
        }
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        // Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
        TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
        if (tapperTE != null && tapperTE.IsReadyToCollectGalipot) {
            int uniqueAnimationFrame = Main.tileFrame[Type] + (i + j);
            if ((i + j) % 2 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 3 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 4 == 0)
                uniqueAnimationFrame += 1;
            uniqueAnimationFrame %= 3;

            // frameYOffset = modTile.animationFrameHeight * Main.tileFrame [type] will already be set before this hook is called
            // But we have a horizontal animated texture, so we use frameXOffset instead of frameYOffset
            TileObjectData tileData = TileObjectData.GetTileData(type, 0);
            int num12 = 28;
            frameYOffset = uniqueAnimationFrame * num12;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        int tileType = WorldGenHelper.GetTileSafely(i, j).TileType;
        TileObjectData tileData = TileObjectData.GetTileData(tileType, 0);
        int coordinateWidth = 30;
        int num12 = 28;
        Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
        Vector2 vector = new Vector2(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen)
            vector = Vector2.Zero;
        Vector2 position = unscaledPosition - vector;
        int drawXOffset = tileData.DrawXOffset;
        int num5 = tileData.DrawYOffset;
        Color color = Lighting.GetColor(i, j);
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (WorldGenHelper.ActiveTile(i - 1, j, TileID.Trees)) {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
        if (WorldGenHelper.ActiveTile(i + 1, j, TileID.Trees)) {
            drawXOffset += 16;
        }
        Rectangle rect = new(0, 0, coordinateWidth, num12);
        Texture2D texture = TextureAssets.Tile[tileType].Value;
        Vector2 drawPosition = new Vector2(i * 16 - (int)(position.X + (float)(coordinateWidth - 16) / 2f) + drawXOffset, j * 16 - (int)position.Y + num5);
        spriteBatch.Draw(sourceRectangle: rect, texture: texture, position: drawPosition, color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
        TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
        if (tapperTE != null && tapperTE.IsReadyToCollectGalipot) {
            texture = ModContent.Request<Texture2D>((TileLoader.GetTile(Type) as Tapper).GalipotTexture).Value;
            int uniqueAnimationFrame = Main.tileFrame[Type] + (i + j) % 3;
            if ((i + j) % 2 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 3 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 4 == 0)
                uniqueAnimationFrame += 1;
            uniqueAnimationFrame %= 3;

            int frameXOffset = uniqueAnimationFrame * num12;
            rect.Y = num12 * uniqueAnimationFrame;
            spriteBatch.Draw(sourceRectangle: rect, texture: texture, position: drawPosition, color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
        }

        return false;
    }
}

sealed class TapperDrawing : GlobalTile {
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
        if (type == TileID.Trees) {
            ushort tapperTileType = (ushort)ModContent.TileType<Tapper>();
            bool flag = WorldGenHelper.GetTileSafely(i - 1, j).TileType == tapperTileType;
            bool flag2 = WorldGenHelper.GetTileSafely(i + 1, j).TileType == tapperTileType;
            if (flag2 || flag) {
                int coordinateWidth = 30;
                int num12 = 28;
                Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
                Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen)
                    zero = Vector2.Zero;
                Vector2 position = unscaledPosition - zero;
                Color color = Lighting.GetColor(i, j);
                int offset = WorldGenHelper.GetTileSafely(i + 1, j).TileType != (ushort)ModContent.TileType<Tapper>() ? -1 : 0;
                int offsetX = flag ? 0 : 1;
                bool flag4 = flag && flag2;
                bool flag5 = WorldGenHelper.GetTileSafely(i + 1, j).TileType == TileID.Trees;
                if (!flag4) {
                    if (WorldGenHelper.GetTileSafely(i - 1, j).TileType == TileID.Trees || flag5) {
                        flag4 = true;
                    }
                }
                int tileFrameX = WorldGenHelper.GetTileSafely(i, j).TileFrameX;
                List<int> values = [22, 44];
                int offsetX2 = -2;
                if (values.Contains(tileFrameX) && flag && !flag4) {
                    flag4 = flag5 = true;
                    offsetX2 = -3;
                }
                Asset<Texture2D>? tapperBracingAsset = ModContent.Request<Texture2D>((TileLoader.GetTile(tapperTileType) as Tapper).BracingTexture);
                spriteBatch.Draw(tapperBracingAsset.Value, new Vector2((float)((i + offset) * 16 - (int)position.X + offsetX - (flag4 ? flag5 ? offsetX2 : 2 : 0)), (float)(j * 16 - (int)position.Y) - 6), new Rectangle(0, flag4 ? num12 : 0, coordinateWidth + (flag4 ? 2 : 0), num12), color, 0f, Vector2.Zero, 1f, offset == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
        }
    }
}