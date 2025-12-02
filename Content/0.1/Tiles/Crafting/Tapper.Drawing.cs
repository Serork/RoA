using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.Tiles;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

partial class Tapper : ModTile, TileHooks.IPostDraw {
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
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddPostSolidTileDrawPoint(this, i, j);

        return false;
    }

    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X;
        int j = pos.Y;
        int type = Main.tile[i, j].TileType;
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }
        if (type == ModContent.TileType<Tapper>()) {
            int tileType = WorldGenHelper.GetTileSafely(i, j).TileType;
            TileObjectData tileData = TileObjectData.GetTileData(tileType, 0);
            int coordinateWidth = 30;
            int num12 = 28;
            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
            Vector2 vector = Vector2.Zero;
            Vector2 position = unscaledPosition - vector;
            int drawXOffset = tileData.DrawXOffset - 1;
            int num5 = tileData.DrawYOffset;
            Color color = Lighting.GetColor(i, j);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (WorldGenHelper.ActiveTile(i - 1, j, TileID.Trees)) {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            if (WorldGenHelper.ActiveTile(i + 1, j, TileID.Trees)) {
                drawXOffset += 18;
            }
            Rectangle rect = new(0, 0, coordinateWidth, num12);

            Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(Main.tile[i, j], i, j);
            texture ??= TextureAssets.Tile[type].Value;

            int uniqueAnimationFrame = Main.tileFrame[type] + (i + j) % 3;
            if ((i + j) % 2 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 3 == 0)
                uniqueAnimationFrame += 1;
            if ((i + j) % 4 == 0)
                uniqueAnimationFrame += 1;
            uniqueAnimationFrame %= 3;

            Vector2 drawPosition = new(i * 16 - (int)(position.X + (float)(coordinateWidth - 16) / 2f) + drawXOffset, j * 16 - (int)position.Y + num5);
            Main.spriteBatch.Draw(sourceRectangle: rect, texture: texture, position: drawPosition, color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);

            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            if (tapperTE != null && tapperTE.IsReadyToCollectGalipot) {
                texture = GalipotTexture.Value;

                int frameXOffset = uniqueAnimationFrame * num12;
                rect.Y = num12 * uniqueAnimationFrame;
                Main.spriteBatch.Draw(sourceRectangle: rect, texture: texture, position: drawPosition, color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
            }

            if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected)) {
                int num = (color.R + color.G + color.B) / 3;
                if (num > 10) {
                    Texture2D highlightTexture = HighlightGalipotTexture.Value;
                    Color highlightColor = Colors.GetSelectionGlowColor(actuallySelected, num);
                    rect = new(0, num12 * uniqueAnimationFrame, coordinateWidth, num12);
                    Main.spriteBatch.Draw(sourceRectangle: rect, texture: highlightTexture, position: drawPosition, color: highlightColor, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
                }
            }
        }
    }

    private class TapperBracingDrawing : GlobalTile {
        // separate
        public static List<Point> DrawPoints { get; private set; } = [];

        public override void Load() {
            On_Main.DoDraw_Tiles_Solid += On_Main_DoDraw_Tiles_Solid;

            On_Main.DrawTiles += On_Main_DrawTiles;
        }

        private void On_Main_DrawTiles(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride) {
            orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);

            if (CaptureManager.Instance.IsCapturing) {
                SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
                foreach (Point drawPoint in DrawPoints) {
                    int i = drawPoint.X;
                    int j = drawPoint.Y;
                    int type = Main.tile[i, j].TileType;
                    if (type == TileID.Trees) {
                        ushort tapperTileType = (ushort)ModContent.TileType<Tapper>();
                        bool flag = WorldGenHelper.GetTileSafely(i - 1, j).TileType == tapperTileType;
                        bool flag2 = WorldGenHelper.GetTileSafely(i + 1, j).TileType == tapperTileType;
                        if (flag2 || flag) {
                            int coordinateWidth = 30;
                            int num12 = 28;
                            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
                            Vector2 zero = Vector2.Zero;
                            Vector2 position = unscaledPosition - zero;
                            Color color = Lighting.GetColor(i, j);
                            var texture = PaintsRenderer.TryGetPaintedTexture(flag ? i - 1 : (i + 1), j, (TileLoader.GetTile(tapperTileType) as Tapper)!.BracingTexturePath);
                            Main.spriteBatch.Draw(texture,
                                new Vector2((float)(i * 16 - (int)position.X),
                                            (float)(j * 16 - (int)position.Y) - 6),
                                new Rectangle(0, flag ? 28 : 0, coordinateWidth,
                                num12), color, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                        }
                    }
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(in snapshot);
            }
        }

        // separate
        private void On_Main_DoDraw_Tiles_Solid(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
            orig(self);

            if (!CaptureManager.Instance.IsCapturing) {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
                foreach (Point drawPoint in DrawPoints) {
                    int i = drawPoint.X;
                    int j = drawPoint.Y;
                    int type = Main.tile[i, j].TileType;
                    if (type == TileID.Trees) {
                        ushort tapperTileType = (ushort)ModContent.TileType<Tapper>();
                        bool flag = WorldGenHelper.GetTileSafely(i - 1, j).TileType == tapperTileType;
                        bool flag2 = WorldGenHelper.GetTileSafely(i + 1, j).TileType == tapperTileType;
                        if (flag2 || flag) {
                            int coordinateWidth = 30;
                            int num12 = 28;
                            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
                            Vector2 zero = Vector2.Zero;
                            Vector2 position = unscaledPosition - zero;
                            Color color = Lighting.GetColor(i, j);
                            var texture = PaintsRenderer.TryGetPaintedTexture(flag ? i - 1 : (i + 1), j, (TileLoader.GetTile(tapperTileType) as Tapper)!.BracingTexturePath);
                            Main.spriteBatch.Draw(texture,
                                new Vector2((float)(i * 16 - (int)position.X),
                                            (float)(j * 16 - (int)position.Y) - 6),
                                new Rectangle(0, flag ? 28 : 0, coordinateWidth,
                                num12), color, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                        }
                    }
                }
                Main.spriteBatch.End();
            }
        }

        public override void Unload() {
            DrawPoints.Clear();
            DrawPoints = null;
        }

        public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
            if (type == TileID.Trees) {
                ushort tapperTileType = (ushort)ModContent.TileType<Tapper>();
                Point position = new(i, j);
                bool flag = WorldGenHelper.GetTileSafely(i - 1, j).TileType == tapperTileType;
                bool flag2 = WorldGenHelper.GetTileSafely(i + 1, j).TileType == tapperTileType;
                if (flag2 || flag) {
                    if (!DrawPoints.Contains(position)) {
                        DrawPoints.Add(position);
                    }
                }
                else {
                    DrawPoints.Remove(position);
                }
            }
        }
    }
}

