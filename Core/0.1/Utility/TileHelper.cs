using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.Tiles;
using RoA.Common.Utilities.Extensions;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.WorldGenerations;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

using static RoA.Common.Tiles.TileHooks;

namespace RoA.Core.Utility;

static partial class TileHelper {
    public static bool ArePositionsAdjacent(Point16 point1, Point16 point2, bool checkDiagonal = true) => ArePositionsAdjacent(point1, point2, 1, checkDiagonal);
    public static bool ArePositionsAdjacent(Point16 point1, Point16 point2, int check = 1, bool checkDiagonal = true) => ArePositionsAdjacent(point1.X, point1.Y, point2.X, point2.Y, check, checkDiagonal);
    public static bool ArePositionsAdjacent(int x1, int y1, int x2, int y2, bool checkDiagonal = true) => ArePositionsAdjacent(x1, y1, x2, y2, 1, checkDiagonal);
    public static bool ArePositionsAdjacent(int x1, int y1, int x2, int y2, int check = 1, bool checkDiagonal = true) {
        int dx = Math.Abs(x1 - x2);
        int dy = Math.Abs(y1 - y2);
        return checkDiagonal ? (dx <= check && dy <= check && (dx != 0 || dy != 0)) : ((dx == 1 && dy == 0) || (dx == 0 && dy == 1));
    }

    public readonly record struct HangingTileInfo(int? X, int? Y) {
        public static implicit operator HangingTileInfo(int value) {
            return new(null, value);
        }
    }

    public static int MossConversion(int thisType, int otherType) {
        if ((thisType == TileID.GreenMoss || thisType == TileID.GreenMossBrick) && otherType == ModContent.TileType<BackwoodsStone>())
            return ModContent.TileType<BackwoodsGreenMoss>();

        if (thisType == ModContent.TileType<BackwoodsGreenMoss>() && otherType == 38)
            return TileID.GreenMossBrick;

        if (thisType == ModContent.TileType<BackwoodsGreenMoss>() && otherType == 1)
            return TileID.GreenMoss;

        if (thisType == ModContent.TileType<BackwoodsGreenMoss>() && otherType == ModContent.TileType<BackwoodsStone>())
            return thisType;

        if (TileID.Sets.tileMossBrick[thisType] && otherType == 38)
            return thisType;

        if (Main.tileMoss[thisType] && otherType == 1)
            return thisType;

        switch (thisType) {
            case 182:
                return 515;
            case 515:
                return 182;
            case 180:
                return 513;
            case 513:
                return 180;
            case 179:
                return 512;
            case 512:
                return 179;
            case 381:
                return 517;
            case 517:
                return 381;
            case 534:
                return 535;
            case 535:
                return 534;
            case 536:
                return 537;
            case 537:
                return 536;
            case 539:
                return 540;
            case 540:
                return 539;
            case 625:
                return 626;
            case 626:
                return 625;
            case 627:
                return 628;
            case 628:
                return 627;
            case 183:
                return 516;
            case 516:
                return 183;
            case 181:
                return 514;
            case 514:
                return 181;
            default:
                return 0;
        }
    }

    private static Point[][] _addSpecialPointSpecialPositions;
    private static int[] _addSpecialPointSpecialsCount;
    private static List<Point> _addVineRootsPositions;

    private static List<(ModTile, Point)>? _fluentTiles = [];

    public static Dictionary<int, HangingTileInfo>? HangingTile { get; private set; }
    public static List<(ModTile, Point16)>? SolidTileDrawPoints { get; private set; }
    public static List<(ModTile, Point16)>? NonSolidTileDrawPoints { get; private set; }
    public static List<(ModTile, Point16)>? PostPlayerDrawPoints { get; private set; }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sunflowerWindCounter")]
    public extern static ref double TileDrawing_sunflowerWindCounter(TileDrawing tileDrawing);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rand")]
    public extern static ref UnifiedRandom TileDrawing_rand(TileDrawing tileDrawing);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawAnimatedTile_AdjustForVisionChangers")]
    public extern static void TileDrawing_DrawAnimatedTile_AdjustForVisionChangers(TileDrawing tileDrawing, int i, int j, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, ref Color tileLight, bool canDoDust);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetHighestWindGridPushComplex")]
    public extern static float TileDrawing_GetHighestWindGridPushComplex(TileDrawing tileDrawing, int topLeftX, int topLeftY, int sizeX, int sizeY, int totalPushTime, float pushForcePerFrame, int loops, bool swapLoopDir);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawTiles_GetLightOverride")]
    public extern static Color TileDrawing_DrawTiles_GetLightOverride(TileDrawing tileDrawing, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight);


    public static void AddFluentPoint(ModTile modTile, int i, int j) {
        if (_fluentTiles.Contains((modTile, new Point(i, j)))) {
            return;
        }
        _fluentTiles.Add((modTile, new Point(i, j)));
    }

    public static void AddPostSolidTileDrawPoint(ModTile modTile, int i, int j) {
        if (SolidTileDrawPoints.Contains((modTile, new Point16(i, j)))) {
            return;
        }
        SolidTileDrawPoints.Add((modTile, new Point16(i, j)));
    }

    public static void AddPostNonSolidTileDrawPoint(ModTile modTile, int i, int j) {
        if (NonSolidTileDrawPoints.Contains((modTile, new Point16(i, j)))) {
            return;
        }
        NonSolidTileDrawPoints.Add((modTile, new Point16(i, j)));
    }

    public static void AddPostPlayerDrawPoint(ModTile modTile, int i, int j) {
        if (PostPlayerDrawPoints.Contains((modTile, new Point16(i, j)))) {
            return;
        }
        PostPlayerDrawPoints.Add((modTile, new Point16(i, j)));
    }

    public static void Load() {
        _addSpecialPointSpecialPositions = (Point[][])typeof(TileDrawing).GetFieldValue("_specialPositions", Main.instance.TilesRenderer);
        _addSpecialPointSpecialsCount = (int[])typeof(TileDrawing).GetFieldValue("_specialsCount", Main.instance.TilesRenderer);
        _addVineRootsPositions = (List<Point>)typeof(TileDrawing).GetFieldValue("_vineRootsPositions", Main.instance.TilesRenderer);

        _fluentTiles = [];
        HangingTile = [];
        SolidTileDrawPoints = [];
        NonSolidTileDrawPoints = [];
        PostPlayerDrawPoints = [];

        On_TileDrawing.PreDrawTiles += (orig, self, solidLayer, forRenderTargets, intoRenderTargets) => {
            orig.Invoke(self, solidLayer, forRenderTargets, intoRenderTargets);
            bool flag = intoRenderTargets || Lighting.UpdateEveryFrame;
            if (!solidLayer && flag) {
                _fluentTiles.Clear();
            }
        };
        On_TileDrawing.DrawReverseVines += (orig, self) => {
            orig.Invoke(self);
            Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
            foreach ((ModTile modTile, Point position) in _fluentTiles) {
                if (modTile is ITileFluentlyDrawn tileFluent && modTile is not null) {
                    tileFluent.FluentDraw(unscaledPosition, position, Main.spriteBatch, self);
                }
            }
        };

        On_TileDrawing.DrawMultiTileVinesInWind += On_TileDrawing_DrawMultiTileVinesInWind;

        On_Main.DoDraw_Tiles_Solid += On_Main_DoDraw_Tiles_Solid;
        On_Main.DoDraw_Tiles_NonSolid += On_Main_DoDraw_Tiles_NonSolid;

        On_Main.DrawPlayers_AfterProjectiles += On_Main_DrawPlayers_AfterProjectiles;

        On_Main.DrawTiles += On_Main_DrawTiles;

        LoadImpl();
    }

    private static void On_Main_DoDraw_Tiles_NonSolid(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self) {
        if (!CaptureManager.Instance.IsCapturing) {
            PreNonSolidTileDraws();
        }

        orig(self);

        if (!CaptureManager.Instance.IsCapturing) {
            PostNonSolidTileDraws();
        }
    }

    static partial void LoadImpl();

    private static void On_Main_DrawTiles(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride) {
        if (CaptureManager.Instance.IsCapturing) {
            SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            PreSolidTileDraws();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }

        orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);

        if (CaptureManager.Instance.IsCapturing) {
            SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            PostSolidTileDraws();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }
    }

    private static void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        PostPlayerDraws();
        Main.spriteBatch.End();
    }

    private static void On_Main_DoDraw_Tiles_Solid(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
        if (!CaptureManager.Instance.IsCapturing) {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            PreSolidTileDraws();
            Main.spriteBatch.End();
        }

        orig(self);

        if (!CaptureManager.Instance.IsCapturing) {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            PostSolidTileDraws();
            Main.spriteBatch.End();
        }
    }

    private static void PostPlayerDraws() {
        for (int i = PostPlayerDrawPoints.Count - 1; i >= 0; i--) {
            (ModTile, Point16) info = PostPlayerDrawPoints[i];
            ModTile modTile = info.Item1;
            Point16 tilePosition = info.Item2;
            if (!Main.tile[tilePosition.X, tilePosition.Y].HasTile) {
                PostPlayerDrawPoints.RemoveAt(i);
            }
            if (modTile is ITileAfterPlayerDraw tileHaveExtras && modTile is not null) {
                //if (!TileDrawing.IsVisible(Main.tile[tilePosition.X, tilePosition.Y])) {
                //    continue;
                //}
                if (!Main.tile[tilePosition.X, tilePosition.Y].HasTile) {
                    PostPlayerDrawPoints.Remove((modTile, tilePosition));
                }
                tileHaveExtras.PostPlayerDraw(Main.spriteBatch, tilePosition);
            }
        }
    }

    private static void PreNonSolidTileDraws() {
        for (int i = NonSolidTileDrawPoints.Count - 1; i >= 0; i--) {
            (ModTile, Point16) info = NonSolidTileDrawPoints[i];
            ModTile modTile = info.Item1;
            Point16 position = info.Item2;
            if (!Main.tile[position.X, position.Y].HasTile) {
                NonSolidTileDrawPoints.RemoveAt(i);
            }
            if (modTile is IPreDraw tileHaveExtras && modTile is not null) {
                //if (!TileDrawing.IsVisible(Main.tile[position.X, position.Y])) {
                //    continue;
                //}
                tileHaveExtras.PreDrawExtra(Main.spriteBatch, position);
            }
        }
    }

    private static void PostNonSolidTileDraws() {
        for (int i = NonSolidTileDrawPoints.Count - 1; i >= 0; i--) {
            (ModTile, Point16) info = NonSolidTileDrawPoints[i];
            ModTile modTile = info.Item1;
            Point16 tilePosition = info.Item2;
            if (!Main.tile[tilePosition.X, tilePosition.Y].HasTile) {
                NonSolidTileDrawPoints.RemoveAt(i);
            }
            if (modTile is IPostDraw tileHaveExtras && modTile is not null) {
                tileHaveExtras.PostDrawExtra(Main.spriteBatch, tilePosition);
            }
        }
    }

    private static void PreSolidTileDraws() {
        for (int i = SolidTileDrawPoints.Count - 1; i >= 0; i--) {
            (ModTile, Point16) info = SolidTileDrawPoints[i];
            ModTile modTile = info.Item1;
            Point16 position = info.Item2;
            if (!Main.tile[position.X, position.Y].HasTile) {
                SolidTileDrawPoints.RemoveAt(i);
            }
            if (modTile is IPreDraw tileHaveExtras && modTile is not null) {
                //if (!TileDrawing.IsVisible(Main.tile[position.X, position.Y])) {
                //    continue;
                //}
                tileHaveExtras.PreDrawExtra(Main.spriteBatch, position);
            }
        }
    }

    private static void PostSolidTileDraws() {
        for (int i = SolidTileDrawPoints.Count - 1; i >= 0; i--) {
            (ModTile, Point16) info = SolidTileDrawPoints[i];
            ModTile modTile = info.Item1;
            Point16 tilePosition = info.Item2;
            if (!Main.tile[tilePosition.X, tilePosition.Y].HasTile) {
                SolidTileDrawPoints.RemoveAt(i);
            }
            if (modTile is IPostDraw tileHaveExtras && modTile is not null) {
                tileHaveExtras.PostDrawExtra(Main.spriteBatch, tilePosition);
            }
        }
    }

    public static void Unload() {
        _addSpecialPointSpecialPositions = null!;
        _addSpecialPointSpecialsCount = null!;
        _addVineRootsPositions = null!;

        _fluentTiles?.Clear();
        _fluentTiles = null!;

        HangingTile?.Clear();
        HangingTile = null!;

        SolidTileDrawPoints?.Clear();
        SolidTileDrawPoints = null!;

        NonSolidTileDrawPoints?.Clear();
        NonSolidTileDrawPoints = null!;

        PostPlayerDrawPoints?.Clear();
        PostPlayerDrawPoints = null!;

        BackwoodsBiomePass.Unload();
    }

    private static void On_TileDrawing_DrawMultiTileVinesInWind(On_TileDrawing.orig_DrawMultiTileVinesInWind orig, TileDrawing self, Vector2 screenPosition, Vector2 offSet, int topLeftX, int topLeftY, int sizeX, int sizeY) {
        if (HangingTile.TryGetValue(Main.tile[topLeftX, topLeftY].TileType, out HangingTileInfo value)) {
            sizeX = value.X ?? sizeX;
            sizeY = value.Y ?? sizeY;
        }

        orig(self, screenPosition, offSet, topLeftX, topLeftY, sizeX, sizeY);
    }


    public static void PrintTime() {
        string text = "AM";
        // Get current weird time
        double time = Main.time;
        if (!Main.dayTime) {
            // if it's night add this number
            time += 54000.0;
        }

        // Divide by seconds in a day * 24
        time = (time / 86400.0) * 24.0;
        // Dunno why we're taking 19.5. Something about hour formatting
        time = time - 7.5 - 12.0;
        // Format in readable time
        if (time < 0.0) {
            time += 24.0;
        }

        if (time >= 12.0) {
            text = "PM";
        }

        int intTime = (int)time;
        // Get the decimal points of time.
        double deltaTime = time - intTime;
        // multiply them by 60. Minutes, probably
        deltaTime = (int)(deltaTime * 60.0);
        // This could easily be replaced by deltaTime.ToString()
        string text2 = string.Concat(deltaTime);
        if (deltaTime < 10.0) {
            // if deltaTime is eg "1" (which would cause time to display as HH:M instead of HH:MM)
            text2 = "0" + text2;
        }

        if (intTime > 12) {
            // This is for AM/PM time rather than 24hour time
            intTime -= 12;
        }

        if (intTime == 0) {
            // 0AM = 12AM
            intTime = 12;
        }

        // Whack it all together to get a HH:MM format
        Main.NewText($"Time: {intTime}:{text2} {text}", 255, 240, 20);
    }

    public static void LanternFluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing) {
        int top = pos.Y - Main.tile[pos].TileFrameY / 18;
        HangingObjectFluentDraw(screenPosition, pos, spriteBatch, tileDrawing, new Point(pos.X, top), 0);
    }

    public static void Chandelier3x3FluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing) {
        int left = Main.tile[pos].TileFrameX / 18;
        left %= 3;
        left = pos.X - left;
        int top = pos.Y - Main.tile[pos].TileFrameY / 18;
        HangingObjectFluentDraw(screenPosition, pos, spriteBatch, tileDrawing, new Point(left, top), 0, 0.11f);
    }

    public static void HangingObjectFluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing, Point topLeft, float swayOffset = -4f, float swayStrength = 0.15f) {
        var tile = Main.tile[pos];
        var tileData = TileObjectData.GetTileData(tile.TileType, 0);

        if (!TileDrawing.IsVisible(tile) || tileData is null)
            return;

        Texture2D tex = tileDrawing.GetTileDrawTexture(tile, pos.X, pos.Y);

        short tileFrameX = tile.TileFrameX;
        short tileFrameY = tile.TileFrameY;

        int topTileX = topLeft.X + tileData.Origin.X;
        int topTileY = topLeft.Y + tileData.Origin.Y;
        int sizeX = tileData.Width;
        int sizeY = tileData.Height;

        int offsetY = tileData.DrawYOffset;
        if (WorldGen.IsBelowANonHammeredPlatform(topTileX, topTileY)) {
            offsetY -= 8;
        }

        float windCycle = 0;
        double sunflowerWindCounter = TileDrawing_sunflowerWindCounter(tileDrawing);
        if (WorldGen.InAPlaceWithWind(topLeft.X, topLeft.Y, sizeX, sizeY))
            windCycle = tileDrawing.GetWindCycle(topTileX, topTileY, sunflowerWindCounter);

        int totalPushTime = 60;
        float pushForcePerFrame = 1.26f;
        float highestWindGridPushComplex = TileDrawing_GetHighestWindGridPushComplex(tileDrawing, topTileX, topTileY, sizeX, sizeY, totalPushTime, pushForcePerFrame, 3, true);
        windCycle += highestWindGridPushComplex;

        UnifiedRandom rand = TileDrawing_rand(tileDrawing);
        Rectangle rectangle = new(tileFrameX, tileFrameY, 16, 16);
        Color tileLight = Lighting.GetColor(pos);
        TileDrawing_DrawAnimatedTile_AdjustForVisionChangers(tileDrawing, pos.X, pos.Y, tile, tile.TileType, tileFrameX, tileFrameY, ref tileLight, rand.NextBool(4));
        tileLight = TileDrawing_DrawTiles_GetLightOverride(tileDrawing, pos.Y, pos.X, tile, tile.TileType, tileFrameX, tileFrameY, tileLight);

        Vector2 center = new Vector2(topTileX, topTileY).ToWorldCoordinates(autoAddY: 0) - screenPosition;
        Vector2 offset = new Vector2(0f, offsetY);
        center += offset;

        float heightStrength = (pos.Y - topLeft.Y + 1) / (float)sizeY;
        if (heightStrength == 0f)
            heightStrength = 0.1f;

        Vector2 tileCoordPos = pos.ToWorldCoordinates(0, 0) - screenPosition;
        tileCoordPos += offset;
        float swayCorrection = Math.Abs(windCycle) * swayOffset * heightStrength;
        Vector2 finalOrigin = center - tileCoordPos;
        Vector2 finalDrawPos = center + new Vector2(0, swayCorrection);

        if (swayOffset == 0f)
            heightStrength = 1f;
        float rotation = -windCycle * swayStrength * heightStrength;

        spriteBatch.Draw(tex, finalDrawPos, rectangle, tileLight, rotation, finalOrigin, 1f, SpriteEffects.None, 0f);

        if (TileLoader.GetTile(tile.TileType) is not TileHooks.ITileFlameData tileFlame)
            return;

        TileHooks.ITileFlameData.TileFlameData tileFlameData = tileFlame.GetTileFlameData(pos.X, pos.Y, tile.TileType, tileFrameY);
        ulong seed = tileFlameData.flameSeed is 0 ? Main.TileFrameSeed ^ (ulong)(((long)pos.X << 32) | (uint)pos.Y) : tileFlameData.flameSeed;
        for (int k = 0; k < tileFlameData.flameCount; k++) {
            float x = Utils.RandomInt(ref seed, tileFlameData.flameRangeXMin, tileFlameData.flameRangeXMax) * tileFlameData.flameRangeMultX;
            float y = Utils.RandomInt(ref seed, tileFlameData.flameRangeYMin, tileFlameData.flameRangeYMax) * tileFlameData.flameRangeMultY;
            Main.spriteBatch.Draw(tileFlameData.flameTexture, finalDrawPos + new Vector2(x, y), rectangle, tileFlameData.flameColor, rotation, finalOrigin, 1f, SpriteEffects.None, 0f);
        }
    }

    public static T GetTE<T>(int i, int j) where T : ModTileEntity {
        if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity entity) && entity is T) {
            return entity as T;
        }

        return null;
    }

    public static void MergeWith(ushort TileType, params ushort[] TileTypes) {
        for (int i = 0; i < TileTypes.Length; ++i) {
            Main.tileMerge[TileType][TileTypes[i]] = true;
            Main.tileMerge[TileTypes[i]][TileType] = true;
        }
    }

    public static void Solid(ushort TileType, bool mergeDirt = true, bool blendAll = true, bool blockLight = true, bool brick = true) {
        Main.tileBrick[TileType] = brick;
        Main.tileSolid[TileType] = true;

        Main.tileMergeDirt[TileType] = mergeDirt;
        Main.tileBlendAll[TileType] = blendAll;
        Main.tileBlockLight[TileType] = blockLight;
    }

    public static void AddSpecialPoint(int i, int j, ushort tileTileType) => _addSpecialPointSpecialPositions[tileTileType][_addSpecialPointSpecialsCount[tileTileType]++] = new Point(i, j);
    public static void AddVineRootPosition(Point item) {
        if (!_addVineRootsPositions.Contains(item)) {
            _addVineRootsPositions.Add(item);
        }
    }

    public static int GetKillTileDust(int i, int j, int tileType, int tileFrameX, int tileFrameY) {
        var genRand = WorldGen.genRand;
        int num = 0;
        if (tileType == 216)
            num = -1;

        if (tileType == 324)
            num = ((tileFrameY != 0) ? (281 + tileFrameX / 18) : 280);

        if (tileType == 216)
            num = -1;

        if (tileType == 335)
            num = -1;

        if (tileType == 338)
            num = -1;

        if (tileType == 0)
            num = 0;

        if (tileType == 192)
            num = 3;

        if (tileType == 208)
            num = 126;
        else if (tileType == 408 || tileType == 409)
            num = 265;
        else if (tileType == 669)
            num = 314;
        else if (tileType == 670)
            num = 315;
        else if (tileType == 671)
            num = 316;
        else if (tileType == 672)
            num = 317;
        else if (tileType == 673)
            num = 318;
        else if (tileType == 674)
            num = 319;
        else if (tileType == 675)
            num = 320;
        else if (tileType == 676)
            num = 321;

        if (tileType == 16) {
            num = 1;
            if (tileFrameX >= 36)
                num = 82;
        }
        else if (tileType == 415 || tileType == 500) {
            num = 6;
        }
        else if (tileType == 416 || tileType == 501) {
            num = 61;
        }
        else if (tileType == 417 || tileType == 502) {
            num = 242;
        }
        else if (tileType == 418 || tileType == 503) {
            num = 135;
        }
        else if (tileType == 474) {
            num = 18;
        }

        if (tileType == 1 || tileType == 17 || tileType == 38 || tileType == 39 || tileType == 41 || tileType == 43 || tileType == 44 || tileType == 481 || tileType == 482 || tileType == 483 || tileType == 48 || Main.tileStone[tileType] || tileType == 85 || tileType == 90 || tileType == 92 || tileType == 96 || tileType == 97 || tileType == 99 || tileType == 117 || tileType == 130 || tileType == 131 || tileType == 132 || tileType == 135 || tileType == 135 || tileType == 142 || tileType == 143 || tileType == 144 || tileType == 210 || tileType == 207 || tileType == 235 || tileType == 247 || tileType == 272 || tileType == 273 || tileType == 283 || tileType == 410 || tileType == 480 || tileType == 509 || tileType == 618 || tileType == 657 || tileType == 658 || tileType == 677 || tileType == 678 || tileType == 679)
            num = 1;

        if (tileType == 379)
            num = 257;

        if (tileType == 311)
            num = 207;

        if (tileType == 312)
            num = 208;

        if (tileType == 313)
            num = 209;

        if (tileType == 104)
            num = -1;

        if (tileType == 95 || tileType == 98 || tileType == 100 || tileType == 174 || tileType == 173)
            num = 6;

        if (tileType == 30 || tileType == 86 || tileType == 94 || tileType == 106 || tileType == 114 || tileType == 124 || tileType == 128 || tileType == 269)
            num = 7;

        if (tileType == 372)
            num = 242;

        if (tileType == 646)
            num = 29;

        if (tileType == 49)
            num = 29;

        if (tileType == 371)
            num = 243;

        if (tileType == 334)
            num = 7;

        switch (tileType) {
            case 10:
            case 11:
            case 87:
            case 89:
            case 93:
            case 139:
            case 209:
            case 319:
            case 320:
            case 386:
            case 387:
            case 390:
            case 405:
            case 406:
            case 411:
            case 412:
            case 419:
            case 420:
            case 421:
            case 422:
            case 423:
            case 424:
            case 425:
            case 428:
            case 429:
            case 441:
            case 442:
            case 445:
            case 446:
            case 447:
            case 448:
            case 449:
            case 450:
            case 451:
            case 452:
            case 453:
            case 455:
            case 456:
            case 457:
            case 462:
            case 463:
            case 464:
            case 465:
            case 466:
            case 468:
            case 476:
            case 486:
            case 487:
            case 489:
            case 490:
            case 491:
            case 493:
            case 494:
            case 497:
            case 510:
            case 511:
            case 520:
            case 521:
            case 522:
            case 523:
            case 524:
            case 525:
            case 526:
            case 527:
            case 531:
            case 545:
            case 547:
            case 548:
            case 560:
            case 564:
            case 565:
            case 567:
            case 572:
            case 579:
            case 591:
            case 592:
            case 593:
            case 594:
            case 613:
            case 614:
            case 621:
            case 622:
            case 623:
            case 624:
            case 630:
            case 631:
            case 656:
                num = -1;
                break;
            case 668:
                num = 0;
                break;
            case 407:
                num = 10;
                break;
            case 454:
                num = 139;
                break;
            case 41:
            case 481:
            case 677:
                num = 275;
                break;
            case 43:
            case 482:
            case 678:
                num = 276;
                break;
            case 44:
            case 483:
            case 679:
                num = 277;
                break;
            case 473:
                num = 82;
                break;
            case 472:
            case 546:
            case 557:
                num = 8;
                break;
            case 498:
                num = 30;
                break;
            case 517:
            case 687:
                num = 258;
                break;
            case 535:
            case 689:
                num = 299;
                break;
            case 537:
            case 690:
                num = 300;
                break;
            case 540:
            case 688:
                num = 301;
                break;
            case 626:
            case 691:
                num = 305;
                break;
            case 184: {
                int num2 = tileFrameX / 22;
                switch (num2) {
                    case 5:
                        num = 258;
                        break;
                    case 6:
                        num = 299;
                        break;
                    case 7:
                        num = 300;
                        break;
                    case 8:
                        num = 301;
                        break;
                    case 9:
                        num = 305;
                        break;
                    case 10:
                        num = 267;
                        break;
                    default:
                        num = 93 + num2;
                        break;
                }

                break;
            }
            case 515:
                num = 96;
                break;
            case 516:
                num = 97;
                break;
            case 514:
                num = 95;
                break;
            case 513:
                num = 94;
                break;
            case 512:
                num = 93;
                break;
            case 541:
                num = 226;
                break;
            case 590:
                num = 1;
                break;
            case 583:
                num = ((genRand.Next(10) != 0) ? 1 : 87);
                break;
            case 584:
                num = ((genRand.Next(10) != 0) ? 1 : 86);
                break;
            case 585:
                num = ((genRand.Next(10) != 0) ? 1 : 88);
                break;
            case 586:
                num = ((genRand.Next(10) != 0) ? 1 : 89);
                break;
            case 587:
                num = ((genRand.Next(10) != 0) ? 1 : 90);
                break;
            case 588:
                num = ((genRand.Next(10) != 0) ? 1 : 91);
                break;
            case 589:
                num = ((genRand.Next(10) != 0) ? 1 : 138);
                break;
            case 595:
                num = 78;
                break;
            case 596:
                num = 78;
                break;
            case 615:
                num = 78;
                break;
            case 616:
                num = 78;
                break;
            case 633:
                num = ((genRand.Next(6) != 0) ? 237 : 36);
                break;
            case 637:
            case 638:
                num = 237;
                break;
            case 634:
                num = ((genRand.Next(10) != 0) ? 36 : 31);
                if (genRand.Next(12) == 0)
                    num = 6;
                break;
        }

        if (Main.tileMoss[tileType])
            num = ((tileType == 381) ? 258 : ((tileType == 534) ? 299 : ((tileType == 536) ? 300 : ((tileType == 539) ? 301 : ((tileType == 625) ? 305 : ((tileType != 627) ? (tileType - 179 + 93) : 267))))));

        if (tileType == 240) {
            int num3 = tileFrameX / 54;
            if (tileFrameY >= 54)
                num3 += 36 * (tileFrameY / 54);

            num = 7;
            if (num3 == 16 || num3 == 17)
                num = 26;

            if (num3 >= 46 && num3 <= 49)
                num = -1;
        }

        if (tileType == 241)
            num = 1;

        if (tileType == 242)
            num = -1;

        if (tileType == 529) {
            switch (Main.tile[i, j + 1].TileType) {
                case 116:
                    num = (num = 47);
                    break;
                case 234:
                    num = (num = 125);
                    break;
                case 112:
                    num = (num = 17);
                    break;
                default:
                    num = ((i >= WorldGen.beachDistance && i <= Main.maxTilesX - WorldGen.beachDistance) ? 289 : 290);
                    break;
            }
        }

        if (tileType == 356)
            num = -1;

        if (tileType == 663)
            num = -1;

        if (tileType == 351)
            num = -1;

        if (tileType == 246)
            num = -1;

        if (tileType == 36)
            num = -1;

        if (tileType == 365)
            num = 239;

        if (tileType == 366)
            num = 30;

        if (tileType == 504)
            num = -1;

        if (tileType == 357 || tileType == 367 || tileType == 561)
            num = 236;

        if (tileType == 368 || tileType == 369 || tileType == 576)
            num = 240;

        if (tileType == 170)
            num = 196;

        if (tileType == 315)
            num = 225;

        if (tileType == 641)
            num = ((genRand.Next(2) != 0) ? 161 : 243);

        if (tileType == 659)
            num = 308;

        if (tileType == 667)
            num = 308;

        if (tileType == 346)
            num = 128;

        if (tileType == 347)
            num = 117;

        if (tileType == 348)
            num = 42;

        if (tileType == 350)
            num = 226;

        if (tileType == 370)
            num = ((genRand.Next(2) != 0) ? 23 : 6);

        if (tileType == 171)
            num = ((genRand.Next(2) != 0) ? (-1) : 196);

        if (tileType == 326)
            num = 13;

        if (tileType == 327)
            num = 13;

        if (tileType == 345)
            num = 13;

        if (tileType == 458)
            num = 13;

        if (tileType == 459)
            num = 13;

        if (tileType == 336)
            num = 6;

        if (tileType == 340)
            num = 75;

        if (tileType == 341)
            num = 65;

        if (tileType == 342)
            num = 135;

        if (tileType == 343)
            num = 169;

        if (tileType == 344)
            num = 156;

        if (tileType == 328)
            num = 13;

        if (tileType == 329)
            num = 13;

        if (tileType == 507)
            num = 13;

        if (tileType == 508)
            num = 13;

        if (tileType == 562)
            num = -1;

        if (tileType == 571)
            num = 40;

        if (tileType == 563)
            num = -1;

        if (tileType == 330)
            num = 9;

        if (tileType == 331)
            num = 11;

        if (tileType == 332)
            num = 19;

        if (tileType == 333)
            num = 11;

        if (tileType == 101)
            num = -1;

        if (tileType == 19) {
            switch (tileFrameY / 18) {
                case 0:
                    num = 7;
                    break;
                case 1:
                    num = 77;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 79;
                    break;
                case 4:
                    num = 26;
                    break;
                case 5:
                    num = 126;
                    break;
                case 6:
                    num = 275;
                    break;
                case 7:
                    num = 277;
                    break;
                case 8:
                    num = 276;
                    break;
                case 9:
                    num = 1;
                    break;
                case 10:
                    num = 214;
                    break;
                case 11:
                    num = 214;
                    break;
                case 12:
                    num = 214;
                    break;
                case 13:
                    num = 109;
                    break;
                case 14:
                    num = 13;
                    break;
                case 15:
                    num = 189;
                    break;
                case 16:
                    num = 191;
                    break;
                case 17:
                    num = 215;
                    break;
                case 18:
                    num = 26;
                    break;
                case 19:
                    num = 214;
                    break;
                case 20:
                    num = 4;
                    break;
                case 21:
                    num = 10;
                    break;
                case 22:
                    num = 32;
                    break;
                case 23:
                    num = 78;
                    break;
                case 24:
                    num = 147;
                    break;
                case 25:
                    num = 40;
                    break;
                case 26:
                    num = 226;
                    break;
                case 27:
                    num = 23;
                    break;
                case 28:
                    num = 240;
                    break;
                case 29:
                    num = 236;
                    break;
                case 30:
                    num = 68 + Main.rand.Next(3);
                    break;
                case 31:
                    num = 10;
                    break;
                case 32:
                    num = 78;
                    break;
                case 33:
                    num = 148;
                    break;
                case 34:
                    num = 5;
                    break;
                case 35:
                    num = 80;
                    break;
                case 37:
                    num = 18;
                    break;
                case 38:
                    num = 6;
                    break;
                case 39:
                    num = 61;
                    break;
                case 40:
                    num = 242;
                    break;
                case 41:
                    num = 135;
                    break;
                case 42:
                    num = 287;
                    break;
                case 44:
                    num = 273;
                    break;
                case 45:
                    num = 243;
                    break;
                case 46:
                    num = 243;
                    break;
                case 47:
                    num = 36;
                    break;
                case 48:
                    num = 226;
                    break;
                default:
                    num = 1;
                    break;
            }
        }

        if (tileType == 79) {
            int num4 = tileFrameY / 36;
            num = ((num4 == 0) ? 7 : ((num4 == 1) ? 77 : ((num4 == 2) ? 78 : ((num4 == 3) ? 79 : ((num4 == 4) ? 126 : ((num4 == 8) ? 109 : ((num4 < 9) ? 1 : (-1))))))));
        }

        if (tileType == 18) {
            switch (tileFrameX / 36) {
                case 0:
                    num = 7;
                    break;
                case 1:
                    num = 77;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 79;
                    break;
                case 4:
                    num = 26;
                    break;
                case 5:
                    num = 40;
                    break;
                case 6:
                    num = 5;
                    break;
                case 7:
                    num = 26;
                    break;
                case 8:
                    num = 4;
                    break;
                case 9:
                    num = 126;
                    break;
                case 10:
                    num = 148;
                    break;
                case 11:
                case 12:
                case 13:
                    num = 1;
                    break;
                case 14:
                    num = 109;
                    break;
                case 15:
                    num = 126;
                    break;
                default:
                    num = -1;
                    break;
            }
        }

        if (tileType == 14 || tileType == 87 || tileType == 88 || tileType == 469)
            num = -1;

        if (tileType >= 255 && tileType <= 261) {
            int num5 = tileType - 255;
            num = 86 + num5;
            if (num5 == 6)
                num = 138;
        }

        if (tileType >= 262 && tileType <= 268) {
            int num6 = tileType - 262;
            num = 86 + num6;
            if (num6 == 6)
                num = 138;
        }

        if (tileType == 178) {
            int num7 = tileFrameX / 18;
            num = 86 + num7;
            if (num7 == 6)
                num = 138;
        }

        if (tileType == 440) {
            switch (tileFrameX / 54) {
                case 0:
                    num = 90;
                    break;
                case 1:
                    num = 88;
                    break;
                case 2:
                    num = 89;
                    break;
                case 3:
                    num = 87;
                    break;
                case 4:
                    num = 86;
                    break;
                case 5:
                    num = 91;
                    break;
                case 6:
                    num = 138;
                    break;
                default:
                    num = -1;
                    break;
            }

            if (tileFrameY < 54)
                num = -1;
        }

        switch (tileType) {
            case 426:
            case 427:
                num = 90;
                break;
            case 430:
            case 435:
                num = 89;
                break;
            case 431:
            case 436:
                num = 88;
                break;
            case 432:
            case 437:
                num = 87;
                break;
            case 433:
            case 438:
                num = 86;
                break;
            case 434:
            case 439:
                num = 91;
                break;
            case 496:
                num = 109;
                break;
            case 549:
                num = 3;
                break;
            case 552:
                num = 32;
                break;
        }

        if (tileType == 186)
            num = ((tileFrameX <= 360) ? 26 : ((tileFrameX <= 846) ? 1 : ((tileFrameX <= 954) ? 9 : ((tileFrameX <= 1062) ? 11 : ((tileFrameX <= 1170) ? 10 : ((tileFrameX > 1332) ? ((tileFrameX > 1386) ? 80 : 10) : 0))))));

        if (tileType == 187) {
            if (tileFrameX <= 144)
                num = 1;
            else if (tileFrameX <= 306)
                num = 38;
            else if (tileFrameX <= 468)
                num = 36;
            else if (tileFrameX <= 738)
                num = 30;
            else if (tileFrameX <= 970)
                num = 1;
            else if (tileFrameX <= 1132)
                num = 148;
            else if (tileFrameX <= 1132)
                num = 155;
            else if (tileFrameX <= 1348)
                num = 1;
            else if (tileFrameX <= 1564)
                num = 0;
            else if (tileFrameX <= 1890)
                num = 250;
            else if (tileFrameX <= 2196)
                num = 240;
            else if (tileFrameX <= 2520)
                num = 236;
        }

        if (tileType == 647) {
            int num8 = tileFrameX / 54;
            if (num8 < 7)
                num = 26;
            else if (num8 < 16)
                num = 1;
            else if (num8 < 18)
                num = 9;
            else if (num8 < 20)
                num = 11;
            else if (num8 < 22)
                num = 10;
            else if (num8 < 26)
                num = 7;
            else if (num8 < 32)
                num = 80;
            else if (num8 < 35)
                num = 80;
        }

        if (tileType == 648) {
            int num9 = tileFrameX / 54;
            num9 += tileFrameY / 36 * 35;
            if (num9 < 3)
                num = 1;
            else if (num9 < 6)
                num = 38;
            else if (num9 < 9)
                num = 36;
            else if (num9 < 14)
                num = 30;
            else if (num9 < 17)
                num = 1;
            else if (num9 < 18)
                num = 1;
            else if (num9 < 21)
                num = 148;
            else if (num9 < 29)
                num = 155;
            else if (num9 < 35)
                num = 287;
            else if (num9 < 41)
                num = 240;
            else if (num9 < 47)
                num = 236;
            else if (num9 < 50)
                num = 0;
            else if (num9 < 52)
                num = 2;
            else if (num9 < 55)
                num = 26;
        }

        if (tileType == 105) {
            num = 1;
            if (tileFrameX >= 1548 && tileFrameX <= 1654 && tileFrameY < 54)
                num = 148;
        }

        if (tileType == 349)
            num = 1;

        if (tileType == 337 || tileType == 506)
            num = 1;

        if (tileType == 239) {
            int num10 = tileFrameX / 18;
            if (num10 == 0)
                num = 9;

            if (num10 == 1)
                num = 81;

            if (num10 == 2)
                num = 8;

            if (num10 == 3)
                num = 82;

            if (num10 == 4)
                num = 11;

            if (num10 == 5)
                num = 83;

            if (num10 == 6)
                num = 10;

            if (num10 == 7)
                num = 84;

            if (num10 == 8)
                num = 14;

            if (num10 == 9)
                num = 23;

            if (num10 == 10)
                num = 25;

            if (num10 == 11)
                num = 48;

            if (num10 == 12)
                num = 144;

            if (num10 == 13)
                num = 49;

            if (num10 == 14)
                num = 145;

            if (num10 == 15)
                num = 50;

            if (num10 == 16)
                num = 146;

            if (num10 == 17)
                num = 128;

            if (num10 == 18)
                num = 84;

            if (num10 == 19)
                num = 117;

            if (num10 == 20)
                num = 42;

            if (num10 == 21)
                num = -1;

            if (num10 == 22)
                num = 265;
        }

        if (tileType == 185) {
            if (tileFrameY == 18) {
                int num11 = tileFrameX / 36;
                if (num11 < 6)
                    num = 1;
                else if (num11 < 16)
                    num = 26;
                else if (num11 == 16)
                    num = 9;
                else if (num11 == 17)
                    num = 11;
                else if (num11 == 18)
                    num = 10;
                else if (num11 == 19)
                    num = 86;
                else if (num11 == 20)
                    num = 87;
                else if (num11 == 21)
                    num = 88;
                else if (num11 == 22)
                    num = 89;
                else if (num11 == 23)
                    num = 90;
                else if (num11 == 24)
                    num = 91;
                else if (num11 < 31)
                    num = 80;
                else if (num11 < 33)
                    num = 7;
                else if (num11 < 34)
                    num = 8;
                else if (num11 < 38)
                    num = 30;
                else if (num11 < 41)
                    num = 1;
                else if (num11 < 47)
                    num = 287;
                else if (num11 < 53)
                    num = 240;
                else if (num11 < 59)
                    num = 236;
            }
            else {
                int num12 = tileFrameX / 18;
                if (num12 < 6)
                    num = 1;
                else if (num12 < 12)
                    num = 0;
                else if (num12 < 28)
                    num = 26;
                else if (num12 < 33)
                    num = 1;
                else if (num12 < 36)
                    num = 0;
                else if (num12 < 48)
                    num = 80;
                else if (num12 < 54)
                    num = 30;
                else if (num12 < 60)
                    num = 287;
                else if (num12 < 66)
                    num = 240;
                else if (num12 < 72)
                    num = 236;
                else if (num12 < 73)
                    num = 0;
                else if (num12 < 77)
                    num = 32;
            }
        }

        if (tileType == 649) {
            int num13 = tileFrameX / 36 + tileFrameY / 18 * 53;
            if (num13 < 6)
                num = 1;
            else if (num13 < 16)
                num = 26;
            else if (num13 == 16)
                num = 9;
            else if (num13 == 17)
                num = 11;
            else if (num13 == 18)
                num = 10;
            else if (num13 == 19)
                num = 86;
            else if (num13 == 20)
                num = 87;
            else if (num13 == 21)
                num = 88;
            else if (num13 == 22)
                num = 89;
            else if (num13 == 23)
                num = 90;
            else if (num13 == 24)
                num = 91;
            else if (num13 < 31)
                num = 80;
            else if (num13 < 33)
                num = 7;
            else if (num13 < 34)
                num = 8;
            else if (num13 < 38)
                num = 30;
            else if (num13 < 41)
                num = 1;
            else if (num13 < 47)
                num = 287;
            else if (num13 < 53)
                num = 240;
            else if (num13 < 59)
                num = 236;
            else if (num13 < 62)
                num = 0;
            else if (num13 < 65)
                num = 32;
        }

        if (tileType == 650) {
            int num14 = tileFrameX / 18;
            if (num14 < 6)
                num = 1;
            else if (num14 < 12)
                num = 0;
            else if (num14 < 28)
                num = 26;
            else if (num14 < 33)
                num = 1;
            else if (num14 < 36)
                num = 0;
            else if (num14 < 48)
                num = 80;
            else if (num14 < 54)
                num = 30;
            else if (num14 < 60)
                num = 287;
            else if (num14 < 66)
                num = 240;
            else if (num14 < 72)
                num = 236;
            else if (num14 < 73)
                num = 0;
            else if (num14 < 77)
                num = 32;
        }

        if (tileType == 237)
            num = 148;

        if (tileType == 157)
            num = 77;

        if (tileType == 158 || tileType == 232 || tileType == 383 || tileType == 575)
            num = 78;

        if (tileType == 159)
            num = 78;

        if (tileType == 15)
            num = -1;

        if (tileType == 191)
            num = 7;

        if (tileType == 5) {
            num = 7;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int num15 = i;
                int k = j;
                if (tileFrameX == 66 && tileFrameY <= 45)
                    num15++;

                if (tileFrameX == 88 && tileFrameY >= 66 && tileFrameY <= 110)
                    num15--;

                if (tileFrameX == 22 && tileFrameY >= 132 && tileFrameY <= 176)
                    num15--;

                if (tileFrameX == 44 && tileFrameY >= 132 && tileFrameY <= 176)
                    num15++;

                if (tileFrameX == 44 && tileFrameY >= 132 && tileFrameY <= 176)
                    num15++;

                if (tileFrameX == 44 && tileFrameY >= 198)
                    num15++;

                if (tileFrameX == 66 && tileFrameY >= 198)
                    num15--;

                for (; Main.tile[num15, k] != null && (!Main.tile[num15, k].HasTile || !Main.tileSolid[Main.tile[num15, k].TileType]); k++) {
                }

                if (Main.tile[num15, k] != null) {
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 23)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 661)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 60)
                        num = 78;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 70)
                        num = 26;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 109)
                        num = 79;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 199)
                        num = 121;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 662)
                        num = 121;

                    // Extra patch context.
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 147)
                        num = 122;

                    TileLoader.TreeDust(Main.tile[num15, k], ref num);
                }
            }
        }

        if (tileType == 323) {
            num = 215;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int l;
                for (l = j; Main.tile[i, l] != null && (!Main.tile[i, l].HasTile || !Main.tileSolid[Main.tile[i, l].TileType]); l++) {
                }

                if (Main.tile[i, l] != null) {
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 234)
                        num = 121;

                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 116)
                        num = 79;

                    // Extra patch context.
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 112)
                        num = 77;

                    TileLoader.PalmTreeDust(Main.tile[i, l], ref num);
                }
            }
        }

        if (tileType == 137) {
            switch (tileFrameY / 18) {
                default:
                    num = 1;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    num = 148;
                    break;
                case 5:
                    num = 1;
                    break;
            }
        }

        if (tileType == 443)
            num = 1;

        if (tileType == 444)
            num = -1;

        if (tileType == 212)
            num = -1;

        if (tileType == 213)
            num = 129;

        if (tileType == 214)
            num = 1;

        if (tileType == 215)
            num = -6;

        if (tileType == 325)
            num = 81;

        if (tileType == 251)
            num = 189;

        if (tileType == 252)
            num = 190;

        if (tileType == 253)
            num = 191;

        if (tileType == 254) {
            if (tileFrameX < 72) {
                num = 3;
            }
            else if (tileFrameX < 108) {
                num = 3;
                if (genRand.Next(3) == 0)
                    num = 189;
            }
            else if (tileFrameX < 144) {
                num = 3;
                if (genRand.Next(2) == 0)
                    num = 189;
            }
            else {
                num = 3;
                if (genRand.Next(4) != 0)
                    num = 189;
            }
        }

        if (tileType == 467)
            num = -1;

        if (tileType == 21)
            num = ((tileFrameX >= 1008) ? (-1) : ((tileFrameX >= 612) ? 11 : ((tileFrameX >= 576) ? 148 : ((tileFrameX >= 540) ? 26 : ((tileFrameX >= 504) ? 126 : ((tileFrameX >= 468) ? 116 : ((tileFrameX >= 432) ? 7 : ((tileFrameX >= 396) ? 11 : ((tileFrameX >= 360) ? 10 : ((tileFrameX >= 324) ? 79 : ((tileFrameX >= 288) ? 78 : ((tileFrameX >= 252) ? 77 : ((tileFrameX >= 216) ? 1 : ((tileFrameX >= 180) ? 7 : ((tileFrameX >= 108) ? 37 : ((tileFrameX < 36) ? 7 : 10))))))))))))))));

        if (tileType == 382)
            num = 3;

        if (tileType == 2 || tileType == 477)
            num = ((genRand.Next(2) != 0) ? 2 : 0);

        if (tileType == 127)
            num = 67;

        if (tileType == 91)
            num = -1;

        if (tileType == 198)
            num = 109;

        if (tileType == 26)
            num = ((tileFrameX < 54) ? 8 : 5);

        if (tileType == 34)
            num = -1;

        if (tileType == 6)
            num = 8;

        if (tileType == 7 || tileType == 47 || tileType == 284 || tileType == 682)
            num = 9;

        if (tileType == 8 || tileType == 45 || tileType == 102 || tileType == 680)
            num = 10;

        if (tileType == 9 || tileType == 42 || tileType == 46 || tileType == 126 || tileType == 136 || tileType == 681)
            num = 11;

        if (tileType == 166 || tileType == 175)
            num = 81;

        if (tileType == 167)
            num = 82;

        if (tileType == 168 || tileType == 176)
            num = 83;

        if (tileType == 169 || tileType == 177)
            num = 84;

        if (tileType == 199 || tileType == 662)
            num = 117;

        if (tileType == 205)
            num = 125;

        if (tileType == 201)
            num = 125;

        if (tileType == 211)
            num = 128;

        if (tileType == 227) {
            switch (tileFrameX / 34) {
                case 0:
                case 1:
                    num = 26;
                    break;
                case 3:
                    num = 3;
                    break;
                case 2:
                case 4:
                case 5:
                case 6:
                    num = 40;
                    break;
                case 7:
                    num = 117;
                    break;
                case 8:
                    num = 17;
                    break;
                case 9:
                    num = 6;
                    break;
                case 10:
                    num = 3;
                    break;
                case 11:
                    num = 26;
                    break;
            }
        }

        if (tileType == 204 || tileType == 478) {
            num = 117;
            if (genRand.Next(2) == 0)
                num = 1;
        }

        if (tileType == 203)
            num = 117;

        if (tileType == 243)
            num = ((genRand.Next(2) != 0) ? 13 : 7);

        if (tileType == 219)
            num = -1;

        if (tileType == 642)
            num = -128;

        if (tileType == 244)
            num = ((genRand.Next(2) == 0) ? 1 : 13);

        if (tileType == 597) {
            num = -1;
        }
        else if ((tileType >= 358 && tileType <= 364) || (tileType >= 275 && tileType <= 282) || tileType == 285 || tileType == 286 || (tileType >= 288 && tileType <= 297) || (tileType >= 316 && tileType <= 318) || tileType == 298 || tileType == 299 || tileType == 309 || tileType == 310 || tileType == 339 || tileType == 538 || tileType == 413 || tileType == 414 || tileType == 505 || tileType == 521 || tileType == 522 || tileType == 523 || tileType == 524 || tileType == 525 || tileType == 526 || tileType == 527 || tileType == 532 || tileType == 543 || tileType == 544 || tileType == 550 || tileType == 551 || tileType == 533 || tileType == 553 || tileType == 554 || tileType == 555 || tileType == 556 || tileType == 558 || tileType == 559 || tileType == 542 || tileType == 391 || tileType == 394 || tileType == 392 || tileType == 393 || tileType == 568 || tileType == 569 || tileType == 570 || tileType == 582 || tileType == 580 || tileType == 598 || tileType == 599 || tileType == 600 || tileType == 601 || tileType == 602 || tileType == 603 || tileType == 604 || tileType == 605 || tileType == 606 || tileType == 607 || tileType == 608 || tileType == 609 || tileType == 610 || tileType == 611 || tileType == 612 || tileType == 619 || tileType == 620 || tileType == 629 || tileType == 632 || tileType == 640 || tileType == 643 || tileType == 644 || tileType == 645) {
            num = 13;
            if (genRand.Next(3) != 0)
                num = -1;
        }

        if (tileType == 13)
            num = ((tileFrameX < 90) ? 13 : (-1));

        if (tileType == 189)
            num = 16;

        if (tileType == 460)
            num = 16;

        if (tileType == 530) {
            switch (Main.tile[i, j + 2 - tileFrameY / 18].TileType) {
                case 116:
                    num = 47;
                    break;
                case 234:
                    num = 125;
                    break;
                case 112:
                    num = 17;
                    break;
                default:
                    num = ((tileFrameX >= 270) ? 291 : 40);
                    break;
            }
        }

        if (tileType == 518) {
            if (tileFrameY == 0)
                num = 3;
            else if (tileFrameY == 18)
                num = 47;
            else if (tileFrameY == 36)
                num = 40;
        }
        else if (tileType == 519) {
            if (tileFrameY == 0)
                num = 3;
            else if (tileFrameY == 18)
                num = 40;
            else if (tileFrameY == 36)
                num = 47;
            else if (tileFrameY == 54)
                num = 125;
            else if (tileFrameY == 72)
                num = 17;
            else if (tileFrameY == 90)
                num = 26;
        }
        else if (tileType == 636) {
            num = 17;
        }
        else if (tileType == 528) {
            num = 26;
        }

        if (tileType == 12)
            num = 12;

        if (tileType == 639)
            num = 48;

        if (tileType == 3 || tileType == 73)
            num = 3;

        if (tileType == 54)
            num = 13;

        if (tileType == 22 || tileType == 140)
            num = 14;

        if (tileType == 78)
            num = 22;

        if (tileType == 28 || tileType == 653) {
            num = 22;
            if (tileFrameY >= 72 && tileFrameY <= 90)
                num = 1;

            if (tileFrameY >= 144 && tileFrameY <= 234)
                num = 48;

            if (tileFrameY >= 252 && tileFrameY <= 358)
                num = 85;

            if (tileFrameY >= 360 && tileFrameY <= 466)
                num = 26;

            if (tileFrameY >= 468 && tileFrameY <= 574)
                num = 36;

            if (tileFrameY >= 576 && tileFrameY <= 790)
                num = 18;

            if (tileFrameY >= 792 && tileFrameY <= 898)
                num = 5;

            if (tileFrameY >= 900 && tileFrameY <= 1006)
                num = 0;

            if (tileFrameY >= 1008 && tileFrameY <= 1114)
                num = 148;

            if (tileFrameY >= 1116 && tileFrameY <= 1222)
                num = 241;

            if (tileFrameY >= 1224 && tileFrameY <= 1330)
                num = 287;
        }

        if (tileType == 163)
            num = 118;

        if (tileType == 164)
            num = 119;

        if (tileType == 200)
            num = 120;

        if (tileType == 221 || tileType == 248)
            num = 144;

        if (tileType == 222 || tileType == 249)
            num = 145;

        if (tileType == 223 || tileType == 250)
            num = 146;

        if (tileType == 224)
            num = 149;

        if (tileType == 225)
            num = 147;

        if (tileType == 229)
            num = 153;

        if (tileType == 231) {
            num = 153;
            if (genRand.Next(3) == 0)
                num = 26;
        }

        if (tileType == 226)
            num = 148;

        if (tileType == 103)
            num = -1;

        if (tileType == 29)
            num = 23;

        if (tileType == 40)
            num = 28;

        if (tileType == 50)
            num = 22;

        if (tileType == 51)
            num = 30;

        if (tileType == 52 || tileType == 353)
            num = 3;

        if (tileType == 53 || tileType == 81 || tileType == 151 || tileType == 202 || tileType == 274 || tileType == 495)
            num = 32;

        if (tileType == 56 || tileType == 152)
            num = 37;

        if (tileType == 75 || tileType == 683)
            num = 109;

        if (tileType == 57 || tileType == 119 || tileType == 141 || tileType == 234 || tileType == 635 || tileType == 654)
            num = 36;

        if (tileType == 59 || tileType == 120)
            num = 38;

        if (tileType == 61 || tileType == 62 || tileType == 74 || tileType == 80 || tileType == 188 || tileType == 233 || tileType == 236 || tileType == 384 || tileType == 652 || tileType == 651)
            num = 40;

        if (tileType == 485)
            num = 32;

        if (tileType == 238)
            num = ((genRand.Next(3) != 0) ? 166 : 167);

        if (tileType == 69)
            num = 7;

        if (tileType == 655)
            num = 166;

        if (tileType == 71 || tileType == 72 || tileType == 190 || tileType == 578)
            num = 26;

        if (tileType == 70)
            num = 17;

        if (tileType == 112)
            num = 14;

        if (tileType == 123)
            num = 53;

        if (tileType == 161)
            num = 80;

        if (tileType == 206)
            num = 80;

        if (tileType == 162)
            num = 80;

        if (tileType == 165) {
            switch (tileFrameX / 54) {
                case 0:
                    num = 80;
                    break;
                case 1:
                    num = 1;
                    break;
                case 2:
                    num = 30;
                    break;
                case 3:
                    num = 147;
                    break;
                case 4:
                    num = 1;
                    break;
                case 5:
                    num = 14;
                    break;
                case 6:
                    num = 117;
                    break;
                case 7:
                    num = 250;
                    break;
                case 8:
                    num = 240;
                    break;
                case 9:
                    num = 236;
                    break;
                default:
                    num = 1;
                    break;
            }
        }

        if (tileType == 666)
            num = 322;

        if (tileType == 193)
            num = 4;

        if (tileType == 194)
            num = 26;

        if (tileType == 195)
            num = 5;

        if (tileType == 196)
            num = 108;

        if (tileType == 460)
            num = 108;

        if (tileType == 197)
            num = 4;

        if (tileType == 153)
            num = 26;

        if (tileType == 154)
            num = 32;

        if (tileType == 155)
            num = 2;

        if (tileType == 156)
            num = 1;

        if (tileType == 116 || tileType == 118 || tileType == 147 || tileType == 148)
            num = 51;

        if (tileType == 109 || tileType == 492)
            num = ((genRand.Next(2) != 0) ? 47 : 0);

        if (tileType == 110 || tileType == 113 || tileType == 115)
            num = 47;

        if (tileType == 107 || tileType == 121 || tileType == 685)
            num = 48;

        if (tileType == 108 || tileType == 122 || tileType == 146 || tileType == 686)
            num = 49;

        if (tileType == 111 || tileType == 145 || tileType == 150)
            num = 50;

        if (tileType == 133) {
            num = 50;
            if (tileFrameX >= 54)
                num = 146;
        }

        if (tileType == 134) {
            num = 49;
            if (tileFrameX >= 36)
                num = 145;
        }

        if (tileType == 149)
            num = 49;

        if (Main.tileAlch[tileType]) {
            int num16 = tileFrameX / 18;
            if (num16 == 0)
                num = 3;

            if (num16 == 1)
                num = 3;

            if (num16 == 2)
                num = 7;

            if (num16 == 3)
                num = 17;

            if (num16 == 4)
                num = 289;

            if (num16 == 5)
                num = 6;

            if (num16 == 6)
                num = 224;
        }

        if (tileType == 58 || tileType == 76 || tileType == 77 || tileType == 684)
            num = ((genRand.Next(2) != 0) ? 25 : 6);

        if (tileType == 37)
            num = ((genRand.Next(2) != 0) ? 23 : 6);

        if (tileType == 32)
            num = ((genRand.Next(2) != 0) ? 24 : 14);

        if (tileType == 352)
            num = ((genRand.Next(3) != 0) ? 125 : 5);

        if (tileType == 23 || tileType == 24 || tileType == 661)
            num = ((genRand.Next(2) != 0) ? 17 : 14);

        if (tileType == 25 || tileType == 31)
            num = ((tileType == 31 && tileFrameX >= 36) ? 5 : ((genRand.Next(2) != 0) ? 1 : 14));

        if (tileType == 20) {
            switch (tileFrameX / 54) {
                case 1:
                    num = 122;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 77;
                    break;
                case 4:
                    num = 121;
                    break;
                case 5:
                    num = 79;
                    break;
                default:
                    num = 7;
                    break;
            }
        }

        if (tileType == 27)
            num = ((genRand.Next(2) != 0) ? 19 : 3);

        if (tileType == 129) {
            if (tileFrameX >= 324)
                num = 69;

            num = ((tileFrameX != 0 && tileFrameX != 54 && tileFrameX != 108) ? ((tileFrameX != 18 && tileFrameX != 72 && tileFrameX != 126) ? 70 : 69) : 68);
        }

        if (tileType == 385)
            num = genRand.Next(68, 71);

        if (tileType == 4) {
            int num17 = (int)MathHelper.Clamp(tileFrameY / 22, 0f, TorchID.Count - 1);
            num = TorchID.Dust[num17];
        }

        if (tileType == 35) {
            num = 189;
            if (tileFrameX < 36 && genRand.Next(2) == 0)
                num = 6;
        }

        if ((tileType == 34 || tileType == 42) && genRand.Next(2) == 0)
            num = 6;

        if (tileType == 270)
            num = -1;

        if (tileType == 271)
            num = -1;

        if (tileType == 581)
            num = -1;

        if (tileType == 660)
            num = -1;

        if (tileType == 79 || tileType == 90 || tileType == 101)
            num = -1;

        if (tileType == 33 || tileType == 34 || tileType == 42 || tileType == 93 || tileType == 100)
            num = -1;

        if (tileType == 321 || tileType == 574)
            num = 214;

        if (tileType == 322)
            num = 215;

        if (tileType == 635)
            num = 36;

        bool flag = tileType == 178 || tileType == 440;
        if (tileType == 178 || (uint)(tileType - 426) <= 1u || (uint)(tileType - 430) <= 10u)
            flag = true;

        if (TileLoader.GetTile(tileType) is ModTile modTile)
            num = modTile.DustType;

        return num;
    }

    public static int GetKillTileDust(int i, int j, Tile tileCache) {
        var genRand = WorldGen.genRand;
        int num = 0;
        if (tileCache.TileType == 216)
            num = -1;

        if (tileCache.TileType == 324)
            num = ((tileCache.TileFrameY != 0) ? (281 + tileCache.TileFrameX / 18) : 280);

        if (tileCache.TileType == 216)
            num = -1;

        if (tileCache.TileType == 335)
            num = -1;

        if (tileCache.TileType == 338)
            num = -1;

        if (tileCache.TileType == 0)
            num = 0;

        if (tileCache.TileType == 192)
            num = 3;

        if (tileCache.TileType == 208)
            num = 126;
        else if (tileCache.TileType == 408 || tileCache.TileType == 409)
            num = 265;
        else if (tileCache.TileType == 669)
            num = 314;
        else if (tileCache.TileType == 670)
            num = 315;
        else if (tileCache.TileType == 671)
            num = 316;
        else if (tileCache.TileType == 672)
            num = 317;
        else if (tileCache.TileType == 673)
            num = 318;
        else if (tileCache.TileType == 674)
            num = 319;
        else if (tileCache.TileType == 675)
            num = 320;
        else if (tileCache.TileType == 676)
            num = 321;

        if (tileCache.TileType == 16) {
            num = 1;
            if (tileCache.TileFrameX >= 36)
                num = 82;
        }
        else if (tileCache.TileType == 415 || tileCache.TileType == 500) {
            num = 6;
        }
        else if (tileCache.TileType == 416 || tileCache.TileType == 501) {
            num = 61;
        }
        else if (tileCache.TileType == 417 || tileCache.TileType == 502) {
            num = 242;
        }
        else if (tileCache.TileType == 418 || tileCache.TileType == 503) {
            num = 135;
        }
        else if (tileCache.TileType == 474) {
            num = 18;
        }

        if (tileCache.TileType == 1 || tileCache.TileType == 17 || tileCache.TileType == 38 || tileCache.TileType == 39 || tileCache.TileType == 41 || tileCache.TileType == 43 || tileCache.TileType == 44 || tileCache.TileType == 481 || tileCache.TileType == 482 || tileCache.TileType == 483 || tileCache.TileType == 48 || Main.tileStone[tileCache.TileType] || tileCache.TileType == 85 || tileCache.TileType == 90 || tileCache.TileType == 92 || tileCache.TileType == 96 || tileCache.TileType == 97 || tileCache.TileType == 99 || tileCache.TileType == 117 || tileCache.TileType == 130 || tileCache.TileType == 131 || tileCache.TileType == 132 || tileCache.TileType == 135 || tileCache.TileType == 135 || tileCache.TileType == 142 || tileCache.TileType == 143 || tileCache.TileType == 144 || tileCache.TileType == 210 || tileCache.TileType == 207 || tileCache.TileType == 235 || tileCache.TileType == 247 || tileCache.TileType == 272 || tileCache.TileType == 273 || tileCache.TileType == 283 || tileCache.TileType == 410 || tileCache.TileType == 480 || tileCache.TileType == 509 || tileCache.TileType == 618 || tileCache.TileType == 657 || tileCache.TileType == 658 || tileCache.TileType == 677 || tileCache.TileType == 678 || tileCache.TileType == 679)
            num = 1;

        if (tileCache.TileType == 379)
            num = 257;

        if (tileCache.TileType == 311)
            num = 207;

        if (tileCache.TileType == 312)
            num = 208;

        if (tileCache.TileType == 313)
            num = 209;

        if (tileCache.TileType == 104)
            num = -1;

        if (tileCache.TileType == 95 || tileCache.TileType == 98 || tileCache.TileType == 100 || tileCache.TileType == 174 || tileCache.TileType == 173)
            num = 6;

        if (tileCache.TileType == 30 || tileCache.TileType == 86 || tileCache.TileType == 94 || tileCache.TileType == 106 || tileCache.TileType == 114 || tileCache.TileType == 124 || tileCache.TileType == 128 || tileCache.TileType == 269)
            num = 7;

        if (tileCache.TileType == 372)
            num = 242;

        if (tileCache.TileType == 646)
            num = 29;

        if (tileCache.TileType == 49)
            num = 29;

        if (tileCache.TileType == 371)
            num = 243;

        if (tileCache.TileType == 334)
            num = 7;

        switch (tileCache.TileType) {
            case 10:
            case 11:
            case 87:
            case 89:
            case 93:
            case 139:
            case 209:
            case 319:
            case 320:
            case 386:
            case 387:
            case 390:
            case 405:
            case 406:
            case 411:
            case 412:
            case 419:
            case 420:
            case 421:
            case 422:
            case 423:
            case 424:
            case 425:
            case 428:
            case 429:
            case 441:
            case 442:
            case 445:
            case 446:
            case 447:
            case 448:
            case 449:
            case 450:
            case 451:
            case 452:
            case 453:
            case 455:
            case 456:
            case 457:
            case 462:
            case 463:
            case 464:
            case 465:
            case 466:
            case 468:
            case 476:
            case 486:
            case 487:
            case 489:
            case 490:
            case 491:
            case 493:
            case 494:
            case 497:
            case 510:
            case 511:
            case 520:
            case 521:
            case 522:
            case 523:
            case 524:
            case 525:
            case 526:
            case 527:
            case 531:
            case 545:
            case 547:
            case 548:
            case 560:
            case 564:
            case 565:
            case 567:
            case 572:
            case 579:
            case 591:
            case 592:
            case 593:
            case 594:
            case 613:
            case 614:
            case 621:
            case 622:
            case 623:
            case 624:
            case 630:
            case 631:
            case 656:
                num = -1;
                break;
            case 668:
                num = 0;
                break;
            case 407:
                num = 10;
                break;
            case 454:
                num = 139;
                break;
            case 41:
            case 481:
            case 677:
                num = 275;
                break;
            case 43:
            case 482:
            case 678:
                num = 276;
                break;
            case 44:
            case 483:
            case 679:
                num = 277;
                break;
            case 473:
                num = 82;
                break;
            case 472:
            case 546:
            case 557:
                num = 8;
                break;
            case 498:
                num = 30;
                break;
            case 517:
            case 687:
                num = 258;
                break;
            case 535:
            case 689:
                num = 299;
                break;
            case 537:
            case 690:
                num = 300;
                break;
            case 540:
            case 688:
                num = 301;
                break;
            case 626:
            case 691:
                num = 305;
                break;
            case 184: {
                int num2 = tileCache.TileFrameX / 22;
                switch (num2) {
                    case 5:
                        num = 258;
                        break;
                    case 6:
                        num = 299;
                        break;
                    case 7:
                        num = 300;
                        break;
                    case 8:
                        num = 301;
                        break;
                    case 9:
                        num = 305;
                        break;
                    case 10:
                        num = 267;
                        break;
                    default:
                        num = 93 + num2;
                        break;
                }

                break;
            }
            case 515:
                num = 96;
                break;
            case 516:
                num = 97;
                break;
            case 514:
                num = 95;
                break;
            case 513:
                num = 94;
                break;
            case 512:
                num = 93;
                break;
            case 541:
                num = 226;
                break;
            case 590:
                num = 1;
                break;
            case 583:
                num = ((genRand.Next(10) != 0) ? 1 : 87);
                break;
            case 584:
                num = ((genRand.Next(10) != 0) ? 1 : 86);
                break;
            case 585:
                num = ((genRand.Next(10) != 0) ? 1 : 88);
                break;
            case 586:
                num = ((genRand.Next(10) != 0) ? 1 : 89);
                break;
            case 587:
                num = ((genRand.Next(10) != 0) ? 1 : 90);
                break;
            case 588:
                num = ((genRand.Next(10) != 0) ? 1 : 91);
                break;
            case 589:
                num = ((genRand.Next(10) != 0) ? 1 : 138);
                break;
            case 595:
                num = 78;
                break;
            case 596:
                num = 78;
                break;
            case 615:
                num = 78;
                break;
            case 616:
                num = 78;
                break;
            case 633:
                num = ((genRand.Next(6) != 0) ? 237 : 36);
                break;
            case 637:
            case 638:
                num = 237;
                break;
            case 634:
                num = ((genRand.Next(10) != 0) ? 36 : 31);
                if (genRand.Next(12) == 0)
                    num = 6;
                break;
        }

        if (Main.tileMoss[tileCache.TileType])
            num = ((tileCache.TileType == 381) ? 258 : ((tileCache.TileType == 534) ? 299 : ((tileCache.TileType == 536) ? 300 : ((tileCache.TileType == 539) ? 301 : ((tileCache.TileType == 625) ? 305 : ((tileCache.TileType != 627) ? (tileCache.TileType - 179 + 93) : 267))))));

        if (tileCache.TileType == 240) {
            int num3 = tileCache.TileFrameX / 54;
            if (tileCache.TileFrameY >= 54)
                num3 += 36 * (tileCache.TileFrameY / 54);

            num = 7;
            if (num3 == 16 || num3 == 17)
                num = 26;

            if (num3 >= 46 && num3 <= 49)
                num = -1;
        }

        if (tileCache.TileType == 241)
            num = 1;

        if (tileCache.TileType == 242)
            num = -1;

        if (tileCache.TileType == 529) {
            switch (Main.tile[i, j + 1].TileType) {
                case 116:
                    num = (num = 47);
                    break;
                case 234:
                    num = (num = 125);
                    break;
                case 112:
                    num = (num = 17);
                    break;
                default:
                    num = ((i >= WorldGen.beachDistance && i <= Main.maxTilesX - WorldGen.beachDistance) ? 289 : 290);
                    break;
            }
        }

        if (tileCache.TileType == 356)
            num = -1;

        if (tileCache.TileType == 663)
            num = -1;

        if (tileCache.TileType == 351)
            num = -1;

        if (tileCache.TileType == 246)
            num = -1;

        if (tileCache.TileType == 36)
            num = -1;

        if (tileCache.TileType == 365)
            num = 239;

        if (tileCache.TileType == 366)
            num = 30;

        if (tileCache.TileType == 504)
            num = -1;

        if (tileCache.TileType == 357 || tileCache.TileType == 367 || tileCache.TileType == 561)
            num = 236;

        if (tileCache.TileType == 368 || tileCache.TileType == 369 || tileCache.TileType == 576)
            num = 240;

        if (tileCache.TileType == 170)
            num = 196;

        if (tileCache.TileType == 315)
            num = 225;

        if (tileCache.TileType == 641)
            num = ((genRand.Next(2) != 0) ? 161 : 243);

        if (tileCache.TileType == 659)
            num = 308;

        if (tileCache.TileType == 667)
            num = 308;

        if (tileCache.TileType == 346)
            num = 128;

        if (tileCache.TileType == 347)
            num = 117;

        if (tileCache.TileType == 348)
            num = 42;

        if (tileCache.TileType == 350)
            num = 226;

        if (tileCache.TileType == 370)
            num = ((genRand.Next(2) != 0) ? 23 : 6);

        if (tileCache.TileType == 171)
            num = ((genRand.Next(2) != 0) ? (-1) : 196);

        if (tileCache.TileType == 326)
            num = 13;

        if (tileCache.TileType == 327)
            num = 13;

        if (tileCache.TileType == 345)
            num = 13;

        if (tileCache.TileType == 458)
            num = 13;

        if (tileCache.TileType == 459)
            num = 13;

        if (tileCache.TileType == 336)
            num = 6;

        if (tileCache.TileType == 340)
            num = 75;

        if (tileCache.TileType == 341)
            num = 65;

        if (tileCache.TileType == 342)
            num = 135;

        if (tileCache.TileType == 343)
            num = 169;

        if (tileCache.TileType == 344)
            num = 156;

        if (tileCache.TileType == 328)
            num = 13;

        if (tileCache.TileType == 329)
            num = 13;

        if (tileCache.TileType == 507)
            num = 13;

        if (tileCache.TileType == 508)
            num = 13;

        if (tileCache.TileType == 562)
            num = -1;

        if (tileCache.TileType == 571)
            num = 40;

        if (tileCache.TileType == 563)
            num = -1;

        if (tileCache.TileType == 330)
            num = 9;

        if (tileCache.TileType == 331)
            num = 11;

        if (tileCache.TileType == 332)
            num = 19;

        if (tileCache.TileType == 333)
            num = 11;

        if (tileCache.TileType == 101)
            num = -1;

        if (tileCache.TileType == 19) {
            switch (tileCache.TileFrameY / 18) {
                case 0:
                    num = 7;
                    break;
                case 1:
                    num = 77;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 79;
                    break;
                case 4:
                    num = 26;
                    break;
                case 5:
                    num = 126;
                    break;
                case 6:
                    num = 275;
                    break;
                case 7:
                    num = 277;
                    break;
                case 8:
                    num = 276;
                    break;
                case 9:
                    num = 1;
                    break;
                case 10:
                    num = 214;
                    break;
                case 11:
                    num = 214;
                    break;
                case 12:
                    num = 214;
                    break;
                case 13:
                    num = 109;
                    break;
                case 14:
                    num = 13;
                    break;
                case 15:
                    num = 189;
                    break;
                case 16:
                    num = 191;
                    break;
                case 17:
                    num = 215;
                    break;
                case 18:
                    num = 26;
                    break;
                case 19:
                    num = 214;
                    break;
                case 20:
                    num = 4;
                    break;
                case 21:
                    num = 10;
                    break;
                case 22:
                    num = 32;
                    break;
                case 23:
                    num = 78;
                    break;
                case 24:
                    num = 147;
                    break;
                case 25:
                    num = 40;
                    break;
                case 26:
                    num = 226;
                    break;
                case 27:
                    num = 23;
                    break;
                case 28:
                    num = 240;
                    break;
                case 29:
                    num = 236;
                    break;
                case 30:
                    num = 68 + Main.rand.Next(3);
                    break;
                case 31:
                    num = 10;
                    break;
                case 32:
                    num = 78;
                    break;
                case 33:
                    num = 148;
                    break;
                case 34:
                    num = 5;
                    break;
                case 35:
                    num = 80;
                    break;
                case 37:
                    num = 18;
                    break;
                case 38:
                    num = 6;
                    break;
                case 39:
                    num = 61;
                    break;
                case 40:
                    num = 242;
                    break;
                case 41:
                    num = 135;
                    break;
                case 42:
                    num = 287;
                    break;
                case 44:
                    num = 273;
                    break;
                case 45:
                    num = 243;
                    break;
                case 46:
                    num = 243;
                    break;
                case 47:
                    num = 36;
                    break;
                case 48:
                    num = 226;
                    break;
                default:
                    num = 1;
                    break;
            }
        }

        if (tileCache.TileType == 79) {
            int num4 = tileCache.TileFrameY / 36;
            num = ((num4 == 0) ? 7 : ((num4 == 1) ? 77 : ((num4 == 2) ? 78 : ((num4 == 3) ? 79 : ((num4 == 4) ? 126 : ((num4 == 8) ? 109 : ((num4 < 9) ? 1 : (-1))))))));
        }

        if (tileCache.TileType == 18) {
            switch (tileCache.TileFrameX / 36) {
                case 0:
                    num = 7;
                    break;
                case 1:
                    num = 77;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 79;
                    break;
                case 4:
                    num = 26;
                    break;
                case 5:
                    num = 40;
                    break;
                case 6:
                    num = 5;
                    break;
                case 7:
                    num = 26;
                    break;
                case 8:
                    num = 4;
                    break;
                case 9:
                    num = 126;
                    break;
                case 10:
                    num = 148;
                    break;
                case 11:
                case 12:
                case 13:
                    num = 1;
                    break;
                case 14:
                    num = 109;
                    break;
                case 15:
                    num = 126;
                    break;
                default:
                    num = -1;
                    break;
            }
        }

        if (tileCache.TileType == 14 || tileCache.TileType == 87 || tileCache.TileType == 88 || tileCache.TileType == 469)
            num = -1;

        if (tileCache.TileType >= 255 && tileCache.TileType <= 261) {
            int num5 = tileCache.TileType - 255;
            num = 86 + num5;
            if (num5 == 6)
                num = 138;
        }

        if (tileCache.TileType >= 262 && tileCache.TileType <= 268) {
            int num6 = tileCache.TileType - 262;
            num = 86 + num6;
            if (num6 == 6)
                num = 138;
        }

        if (tileCache.TileType == 178) {
            int num7 = tileCache.TileFrameX / 18;
            num = 86 + num7;
            if (num7 == 6)
                num = 138;
        }

        if (tileCache.TileType == 440) {
            switch (tileCache.TileFrameX / 54) {
                case 0:
                    num = 90;
                    break;
                case 1:
                    num = 88;
                    break;
                case 2:
                    num = 89;
                    break;
                case 3:
                    num = 87;
                    break;
                case 4:
                    num = 86;
                    break;
                case 5:
                    num = 91;
                    break;
                case 6:
                    num = 138;
                    break;
                default:
                    num = -1;
                    break;
            }

            if (tileCache.TileFrameY < 54)
                num = -1;
        }

        switch (tileCache.TileType) {
            case 426:
            case 427:
                num = 90;
                break;
            case 430:
            case 435:
                num = 89;
                break;
            case 431:
            case 436:
                num = 88;
                break;
            case 432:
            case 437:
                num = 87;
                break;
            case 433:
            case 438:
                num = 86;
                break;
            case 434:
            case 439:
                num = 91;
                break;
            case 496:
                num = 109;
                break;
            case 549:
                num = 3;
                break;
            case 552:
                num = 32;
                break;
        }

        if (tileCache.TileType == 186)
            num = ((tileCache.TileFrameX <= 360) ? 26 : ((tileCache.TileFrameX <= 846) ? 1 : ((tileCache.TileFrameX <= 954) ? 9 : ((tileCache.TileFrameX <= 1062) ? 11 : ((tileCache.TileFrameX <= 1170) ? 10 : ((tileCache.TileFrameX > 1332) ? ((tileCache.TileFrameX > 1386) ? 80 : 10) : 0))))));

        if (tileCache.TileType == 187) {
            if (tileCache.TileFrameX <= 144)
                num = 1;
            else if (tileCache.TileFrameX <= 306)
                num = 38;
            else if (tileCache.TileFrameX <= 468)
                num = 36;
            else if (tileCache.TileFrameX <= 738)
                num = 30;
            else if (tileCache.TileFrameX <= 970)
                num = 1;
            else if (tileCache.TileFrameX <= 1132)
                num = 148;
            else if (tileCache.TileFrameX <= 1132)
                num = 155;
            else if (tileCache.TileFrameX <= 1348)
                num = 1;
            else if (tileCache.TileFrameX <= 1564)
                num = 0;
            else if (tileCache.TileFrameX <= 1890)
                num = 250;
            else if (tileCache.TileFrameX <= 2196)
                num = 240;
            else if (tileCache.TileFrameX <= 2520)
                num = 236;
        }

        if (tileCache.TileType == 647) {
            int num8 = tileCache.TileFrameX / 54;
            if (num8 < 7)
                num = 26;
            else if (num8 < 16)
                num = 1;
            else if (num8 < 18)
                num = 9;
            else if (num8 < 20)
                num = 11;
            else if (num8 < 22)
                num = 10;
            else if (num8 < 26)
                num = 7;
            else if (num8 < 32)
                num = 80;
            else if (num8 < 35)
                num = 80;
        }

        if (tileCache.TileType == 648) {
            int num9 = tileCache.TileFrameX / 54;
            num9 += tileCache.TileFrameY / 36 * 35;
            if (num9 < 3)
                num = 1;
            else if (num9 < 6)
                num = 38;
            else if (num9 < 9)
                num = 36;
            else if (num9 < 14)
                num = 30;
            else if (num9 < 17)
                num = 1;
            else if (num9 < 18)
                num = 1;
            else if (num9 < 21)
                num = 148;
            else if (num9 < 29)
                num = 155;
            else if (num9 < 35)
                num = 287;
            else if (num9 < 41)
                num = 240;
            else if (num9 < 47)
                num = 236;
            else if (num9 < 50)
                num = 0;
            else if (num9 < 52)
                num = 2;
            else if (num9 < 55)
                num = 26;
        }

        if (tileCache.TileType == 105) {
            num = 1;
            if (tileCache.TileFrameX >= 1548 && tileCache.TileFrameX <= 1654 && tileCache.TileFrameY < 54)
                num = 148;
        }

        if (tileCache.TileType == 349)
            num = 1;

        if (tileCache.TileType == 337 || tileCache.TileType == 506)
            num = 1;

        if (tileCache.TileType == 239) {
            int num10 = tileCache.TileFrameX / 18;
            if (num10 == 0)
                num = 9;

            if (num10 == 1)
                num = 81;

            if (num10 == 2)
                num = 8;

            if (num10 == 3)
                num = 82;

            if (num10 == 4)
                num = 11;

            if (num10 == 5)
                num = 83;

            if (num10 == 6)
                num = 10;

            if (num10 == 7)
                num = 84;

            if (num10 == 8)
                num = 14;

            if (num10 == 9)
                num = 23;

            if (num10 == 10)
                num = 25;

            if (num10 == 11)
                num = 48;

            if (num10 == 12)
                num = 144;

            if (num10 == 13)
                num = 49;

            if (num10 == 14)
                num = 145;

            if (num10 == 15)
                num = 50;

            if (num10 == 16)
                num = 146;

            if (num10 == 17)
                num = 128;

            if (num10 == 18)
                num = 84;

            if (num10 == 19)
                num = 117;

            if (num10 == 20)
                num = 42;

            if (num10 == 21)
                num = -1;

            if (num10 == 22)
                num = 265;
        }

        if (tileCache.TileType == 185) {
            if (tileCache.TileFrameY == 18) {
                int num11 = tileCache.TileFrameX / 36;
                if (num11 < 6)
                    num = 1;
                else if (num11 < 16)
                    num = 26;
                else if (num11 == 16)
                    num = 9;
                else if (num11 == 17)
                    num = 11;
                else if (num11 == 18)
                    num = 10;
                else if (num11 == 19)
                    num = 86;
                else if (num11 == 20)
                    num = 87;
                else if (num11 == 21)
                    num = 88;
                else if (num11 == 22)
                    num = 89;
                else if (num11 == 23)
                    num = 90;
                else if (num11 == 24)
                    num = 91;
                else if (num11 < 31)
                    num = 80;
                else if (num11 < 33)
                    num = 7;
                else if (num11 < 34)
                    num = 8;
                else if (num11 < 38)
                    num = 30;
                else if (num11 < 41)
                    num = 1;
                else if (num11 < 47)
                    num = 287;
                else if (num11 < 53)
                    num = 240;
                else if (num11 < 59)
                    num = 236;
            }
            else {
                int num12 = tileCache.TileFrameX / 18;
                if (num12 < 6)
                    num = 1;
                else if (num12 < 12)
                    num = 0;
                else if (num12 < 28)
                    num = 26;
                else if (num12 < 33)
                    num = 1;
                else if (num12 < 36)
                    num = 0;
                else if (num12 < 48)
                    num = 80;
                else if (num12 < 54)
                    num = 30;
                else if (num12 < 60)
                    num = 287;
                else if (num12 < 66)
                    num = 240;
                else if (num12 < 72)
                    num = 236;
                else if (num12 < 73)
                    num = 0;
                else if (num12 < 77)
                    num = 32;
            }
        }

        if (tileCache.TileType == 649) {
            int num13 = tileCache.TileFrameX / 36 + tileCache.TileFrameY / 18 * 53;
            if (num13 < 6)
                num = 1;
            else if (num13 < 16)
                num = 26;
            else if (num13 == 16)
                num = 9;
            else if (num13 == 17)
                num = 11;
            else if (num13 == 18)
                num = 10;
            else if (num13 == 19)
                num = 86;
            else if (num13 == 20)
                num = 87;
            else if (num13 == 21)
                num = 88;
            else if (num13 == 22)
                num = 89;
            else if (num13 == 23)
                num = 90;
            else if (num13 == 24)
                num = 91;
            else if (num13 < 31)
                num = 80;
            else if (num13 < 33)
                num = 7;
            else if (num13 < 34)
                num = 8;
            else if (num13 < 38)
                num = 30;
            else if (num13 < 41)
                num = 1;
            else if (num13 < 47)
                num = 287;
            else if (num13 < 53)
                num = 240;
            else if (num13 < 59)
                num = 236;
            else if (num13 < 62)
                num = 0;
            else if (num13 < 65)
                num = 32;
        }

        if (tileCache.TileType == 650) {
            int num14 = tileCache.TileFrameX / 18;
            if (num14 < 6)
                num = 1;
            else if (num14 < 12)
                num = 0;
            else if (num14 < 28)
                num = 26;
            else if (num14 < 33)
                num = 1;
            else if (num14 < 36)
                num = 0;
            else if (num14 < 48)
                num = 80;
            else if (num14 < 54)
                num = 30;
            else if (num14 < 60)
                num = 287;
            else if (num14 < 66)
                num = 240;
            else if (num14 < 72)
                num = 236;
            else if (num14 < 73)
                num = 0;
            else if (num14 < 77)
                num = 32;
        }

        if (tileCache.TileType == 237)
            num = 148;

        if (tileCache.TileType == 157)
            num = 77;

        if (tileCache.TileType == 158 || tileCache.TileType == 232 || tileCache.TileType == 383 || tileCache.TileType == 575)
            num = 78;

        if (tileCache.TileType == 159)
            num = 78;

        if (tileCache.TileType == 15)
            num = -1;

        if (tileCache.TileType == 191)
            num = 7;

        if (tileCache.TileType == 5) {
            num = 7;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int num15 = i;
                int k = j;
                if (tileCache.TileFrameX == 66 && tileCache.TileFrameY <= 45)
                    num15++;

                if (tileCache.TileFrameX == 88 && tileCache.TileFrameY >= 66 && tileCache.TileFrameY <= 110)
                    num15--;

                if (tileCache.TileFrameX == 22 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15--;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15++;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15++;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 198)
                    num15++;

                if (tileCache.TileFrameX == 66 && tileCache.TileFrameY >= 198)
                    num15--;

                for (; Main.tile[num15, k] != null && (!Main.tile[num15, k].HasTile || !Main.tileSolid[Main.tile[num15, k].TileType]); k++) {
                }

                if (Main.tile[num15, k] != null) {
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 23)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 661)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 60)
                        num = 78;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 70)
                        num = 26;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 109)
                        num = 79;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 199)
                        num = 121;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 662)
                        num = 121;

                    // Extra patch context.
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 147)
                        num = 122;

                    TileLoader.TreeDust(Main.tile[num15, k], ref num);
                }
            }
        }

        if (tileCache.TileType == 323) {
            num = 215;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int l;
                for (l = j; Main.tile[i, l] != null && (!Main.tile[i, l].HasTile || !Main.tileSolid[Main.tile[i, l].TileType]); l++) {
                }

                if (Main.tile[i, l] != null) {
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 234)
                        num = 121;

                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 116)
                        num = 79;

                    // Extra patch context.
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 112)
                        num = 77;

                    TileLoader.PalmTreeDust(Main.tile[i, l], ref num);
                }
            }
        }

        if (tileCache.TileType == 137) {
            switch (tileCache.TileFrameY / 18) {
                default:
                    num = 1;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    num = 148;
                    break;
                case 5:
                    num = 1;
                    break;
            }
        }

        if (tileCache.TileType == 443)
            num = 1;

        if (tileCache.TileType == 444)
            num = -1;

        if (tileCache.TileType == 212)
            num = -1;

        if (tileCache.TileType == 213)
            num = 129;

        if (tileCache.TileType == 214)
            num = 1;

        if (tileCache.TileType == 215)
            num = -6;

        if (tileCache.TileType == 325)
            num = 81;

        if (tileCache.TileType == 251)
            num = 189;

        if (tileCache.TileType == 252)
            num = 190;

        if (tileCache.TileType == 253)
            num = 191;

        if (tileCache.TileType == 254) {
            if (tileCache.TileFrameX < 72) {
                num = 3;
            }
            else if (tileCache.TileFrameX < 108) {
                num = 3;
                if (genRand.Next(3) == 0)
                    num = 189;
            }
            else if (tileCache.TileFrameX < 144) {
                num = 3;
                if (genRand.Next(2) == 0)
                    num = 189;
            }
            else {
                num = 3;
                if (genRand.Next(4) != 0)
                    num = 189;
            }
        }

        if (tileCache.TileType == 467)
            num = -1;

        if (tileCache.TileType == 21)
            num = ((tileCache.TileFrameX >= 1008) ? (-1) : ((tileCache.TileFrameX >= 612) ? 11 : ((tileCache.TileFrameX >= 576) ? 148 : ((tileCache.TileFrameX >= 540) ? 26 : ((tileCache.TileFrameX >= 504) ? 126 : ((tileCache.TileFrameX >= 468) ? 116 : ((tileCache.TileFrameX >= 432) ? 7 : ((tileCache.TileFrameX >= 396) ? 11 : ((tileCache.TileFrameX >= 360) ? 10 : ((tileCache.TileFrameX >= 324) ? 79 : ((tileCache.TileFrameX >= 288) ? 78 : ((tileCache.TileFrameX >= 252) ? 77 : ((tileCache.TileFrameX >= 216) ? 1 : ((tileCache.TileFrameX >= 180) ? 7 : ((tileCache.TileFrameX >= 108) ? 37 : ((tileCache.TileFrameX < 36) ? 7 : 10))))))))))))))));

        if (tileCache.TileType == 382)
            num = 3;

        if (tileCache.TileType == 2 || tileCache.TileType == 477)
            num = ((genRand.Next(2) != 0) ? 2 : 0);

        if (tileCache.TileType == 127)
            num = 67;

        if (tileCache.TileType == 91)
            num = -1;

        if (tileCache.TileType == 198)
            num = 109;

        if (tileCache.TileType == 26)
            num = ((tileCache.TileFrameX < 54) ? 8 : 5);

        if (tileCache.TileType == 34)
            num = -1;

        if (tileCache.TileType == 6)
            num = 8;

        if (tileCache.TileType == 7 || tileCache.TileType == 47 || tileCache.TileType == 284 || tileCache.TileType == 682)
            num = 9;

        if (tileCache.TileType == 8 || tileCache.TileType == 45 || tileCache.TileType == 102 || tileCache.TileType == 680)
            num = 10;

        if (tileCache.TileType == 9 || tileCache.TileType == 42 || tileCache.TileType == 46 || tileCache.TileType == 126 || tileCache.TileType == 136 || tileCache.TileType == 681)
            num = 11;

        if (tileCache.TileType == 166 || tileCache.TileType == 175)
            num = 81;

        if (tileCache.TileType == 167)
            num = 82;

        if (tileCache.TileType == 168 || tileCache.TileType == 176)
            num = 83;

        if (tileCache.TileType == 169 || tileCache.TileType == 177)
            num = 84;

        if (tileCache.TileType == 199 || tileCache.TileType == 662)
            num = 117;

        if (tileCache.TileType == 205)
            num = 125;

        if (tileCache.TileType == 201)
            num = 125;

        if (tileCache.TileType == 211)
            num = 128;

        if (tileCache.TileType == 227) {
            switch (tileCache.TileFrameX / 34) {
                case 0:
                case 1:
                    num = 26;
                    break;
                case 3:
                    num = 3;
                    break;
                case 2:
                case 4:
                case 5:
                case 6:
                    num = 40;
                    break;
                case 7:
                    num = 117;
                    break;
                case 8:
                    num = 17;
                    break;
                case 9:
                    num = 6;
                    break;
                case 10:
                    num = 3;
                    break;
                case 11:
                    num = 26;
                    break;
            }
        }

        if (tileCache.TileType == 204 || tileCache.TileType == 478) {
            num = 117;
            if (genRand.Next(2) == 0)
                num = 1;
        }

        if (tileCache.TileType == 203)
            num = 117;

        if (tileCache.TileType == 243)
            num = ((genRand.Next(2) != 0) ? 13 : 7);

        if (tileCache.TileType == 219)
            num = -1;

        if (tileCache.TileType == 642)
            num = -128;

        if (tileCache.TileType == 244)
            num = ((genRand.Next(2) == 0) ? 1 : 13);

        if (tileCache.TileType == 597) {
            num = -1;
        }
        else if ((tileCache.TileType >= 358 && tileCache.TileType <= 364) || (tileCache.TileType >= 275 && tileCache.TileType <= 282) || tileCache.TileType == 285 || tileCache.TileType == 286 || (tileCache.TileType >= 288 && tileCache.TileType <= 297) || (tileCache.TileType >= 316 && tileCache.TileType <= 318) || tileCache.TileType == 298 || tileCache.TileType == 299 || tileCache.TileType == 309 || tileCache.TileType == 310 || tileCache.TileType == 339 || tileCache.TileType == 538 || tileCache.TileType == 413 || tileCache.TileType == 414 || tileCache.TileType == 505 || tileCache.TileType == 521 || tileCache.TileType == 522 || tileCache.TileType == 523 || tileCache.TileType == 524 || tileCache.TileType == 525 || tileCache.TileType == 526 || tileCache.TileType == 527 || tileCache.TileType == 532 || tileCache.TileType == 543 || tileCache.TileType == 544 || tileCache.TileType == 550 || tileCache.TileType == 551 || tileCache.TileType == 533 || tileCache.TileType == 553 || tileCache.TileType == 554 || tileCache.TileType == 555 || tileCache.TileType == 556 || tileCache.TileType == 558 || tileCache.TileType == 559 || tileCache.TileType == 542 || tileCache.TileType == 391 || tileCache.TileType == 394 || tileCache.TileType == 392 || tileCache.TileType == 393 || tileCache.TileType == 568 || tileCache.TileType == 569 || tileCache.TileType == 570 || tileCache.TileType == 582 || tileCache.TileType == 580 || tileCache.TileType == 598 || tileCache.TileType == 599 || tileCache.TileType == 600 || tileCache.TileType == 601 || tileCache.TileType == 602 || tileCache.TileType == 603 || tileCache.TileType == 604 || tileCache.TileType == 605 || tileCache.TileType == 606 || tileCache.TileType == 607 || tileCache.TileType == 608 || tileCache.TileType == 609 || tileCache.TileType == 610 || tileCache.TileType == 611 || tileCache.TileType == 612 || tileCache.TileType == 619 || tileCache.TileType == 620 || tileCache.TileType == 629 || tileCache.TileType == 632 || tileCache.TileType == 640 || tileCache.TileType == 643 || tileCache.TileType == 644 || tileCache.TileType == 645) {
            num = 13;
            if (genRand.Next(3) != 0)
                num = -1;
        }

        if (tileCache.TileType == 13)
            num = ((tileCache.TileFrameX < 90) ? 13 : (-1));

        if (tileCache.TileType == 189)
            num = 16;

        if (tileCache.TileType == 460)
            num = 16;

        if (tileCache.TileType == 530) {
            switch (Main.tile[i, j + 2 - tileCache.TileFrameY / 18].TileType) {
                case 116:
                    num = 47;
                    break;
                case 234:
                    num = 125;
                    break;
                case 112:
                    num = 17;
                    break;
                default:
                    num = ((tileCache.TileFrameX >= 270) ? 291 : 40);
                    break;
            }
        }

        if (tileCache.TileType == 518) {
            if (tileCache.TileFrameY == 0)
                num = 3;
            else if (tileCache.TileFrameY == 18)
                num = 47;
            else if (tileCache.TileFrameY == 36)
                num = 40;
        }
        else if (tileCache.TileType == 519) {
            if (tileCache.TileFrameY == 0)
                num = 3;
            else if (tileCache.TileFrameY == 18)
                num = 40;
            else if (tileCache.TileFrameY == 36)
                num = 47;
            else if (tileCache.TileFrameY == 54)
                num = 125;
            else if (tileCache.TileFrameY == 72)
                num = 17;
            else if (tileCache.TileFrameY == 90)
                num = 26;
        }
        else if (tileCache.TileType == 636) {
            num = 17;
        }
        else if (tileCache.TileType == 528) {
            num = 26;
        }

        if (tileCache.TileType == 12)
            num = 12;

        if (tileCache.TileType == 639)
            num = 48;

        if (tileCache.TileType == 3 || tileCache.TileType == 73)
            num = 3;

        if (tileCache.TileType == 54)
            num = 13;

        if (tileCache.TileType == 22 || tileCache.TileType == 140)
            num = 14;

        if (tileCache.TileType == 78)
            num = 22;

        if (tileCache.TileType == 28 || tileCache.TileType == 653) {
            num = 22;
            if (tileCache.TileFrameY >= 72 && tileCache.TileFrameY <= 90)
                num = 1;

            if (tileCache.TileFrameY >= 144 && tileCache.TileFrameY <= 234)
                num = 48;

            if (tileCache.TileFrameY >= 252 && tileCache.TileFrameY <= 358)
                num = 85;

            if (tileCache.TileFrameY >= 360 && tileCache.TileFrameY <= 466)
                num = 26;

            if (tileCache.TileFrameY >= 468 && tileCache.TileFrameY <= 574)
                num = 36;

            if (tileCache.TileFrameY >= 576 && tileCache.TileFrameY <= 790)
                num = 18;

            if (tileCache.TileFrameY >= 792 && tileCache.TileFrameY <= 898)
                num = 5;

            if (tileCache.TileFrameY >= 900 && tileCache.TileFrameY <= 1006)
                num = 0;

            if (tileCache.TileFrameY >= 1008 && tileCache.TileFrameY <= 1114)
                num = 148;

            if (tileCache.TileFrameY >= 1116 && tileCache.TileFrameY <= 1222)
                num = 241;

            if (tileCache.TileFrameY >= 1224 && tileCache.TileFrameY <= 1330)
                num = 287;
        }

        if (tileCache.TileType == 163)
            num = 118;

        if (tileCache.TileType == 164)
            num = 119;

        if (tileCache.TileType == 200)
            num = 120;

        if (tileCache.TileType == 221 || tileCache.TileType == 248)
            num = 144;

        if (tileCache.TileType == 222 || tileCache.TileType == 249)
            num = 145;

        if (tileCache.TileType == 223 || tileCache.TileType == 250)
            num = 146;

        if (tileCache.TileType == 224)
            num = 149;

        if (tileCache.TileType == 225)
            num = 147;

        if (tileCache.TileType == 229)
            num = 153;

        if (tileCache.TileType == 231) {
            num = 153;
            if (genRand.Next(3) == 0)
                num = 26;
        }

        if (tileCache.TileType == 226)
            num = 148;

        if (tileCache.TileType == 103)
            num = -1;

        if (tileCache.TileType == 29)
            num = 23;

        if (tileCache.TileType == 40)
            num = 28;

        if (tileCache.TileType == 50)
            num = 22;

        if (tileCache.TileType == 51)
            num = 30;

        if (tileCache.TileType == 52 || tileCache.TileType == 353)
            num = 3;

        if (tileCache.TileType == 53 || tileCache.TileType == 81 || tileCache.TileType == 151 || tileCache.TileType == 202 || tileCache.TileType == 274 || tileCache.TileType == 495)
            num = 32;

        if (tileCache.TileType == 56 || tileCache.TileType == 152)
            num = 37;

        if (tileCache.TileType == 75 || tileCache.TileType == 683)
            num = 109;

        if (tileCache.TileType == 57 || tileCache.TileType == 119 || tileCache.TileType == 141 || tileCache.TileType == 234 || tileCache.TileType == 635 || tileCache.TileType == 654)
            num = 36;

        if (tileCache.TileType == 59 || tileCache.TileType == 120)
            num = 38;

        if (tileCache.TileType == 61 || tileCache.TileType == 62 || tileCache.TileType == 74 || tileCache.TileType == 80 || tileCache.TileType == 188 || tileCache.TileType == 233 || tileCache.TileType == 236 || tileCache.TileType == 384 || tileCache.TileType == 652 || tileCache.TileType == 651)
            num = 40;

        if (tileCache.TileType == 485)
            num = 32;

        if (tileCache.TileType == 238)
            num = ((genRand.Next(3) != 0) ? 166 : 167);

        if (tileCache.TileType == 69)
            num = 7;

        if (tileCache.TileType == 655)
            num = 166;

        if (tileCache.TileType == 71 || tileCache.TileType == 72 || tileCache.TileType == 190 || tileCache.TileType == 578)
            num = 26;

        if (tileCache.TileType == 70)
            num = 17;

        if (tileCache.TileType == 112)
            num = 14;

        if (tileCache.TileType == 123)
            num = 53;

        if (tileCache.TileType == 161)
            num = 80;

        if (tileCache.TileType == 206)
            num = 80;

        if (tileCache.TileType == 162)
            num = 80;

        if (tileCache.TileType == 165) {
            switch (tileCache.TileFrameX / 54) {
                case 0:
                    num = 80;
                    break;
                case 1:
                    num = 1;
                    break;
                case 2:
                    num = 30;
                    break;
                case 3:
                    num = 147;
                    break;
                case 4:
                    num = 1;
                    break;
                case 5:
                    num = 14;
                    break;
                case 6:
                    num = 117;
                    break;
                case 7:
                    num = 250;
                    break;
                case 8:
                    num = 240;
                    break;
                case 9:
                    num = 236;
                    break;
                default:
                    num = 1;
                    break;
            }
        }

        if (tileCache.TileType == 666)
            num = 322;

        if (tileCache.TileType == 193)
            num = 4;

        if (tileCache.TileType == 194)
            num = 26;

        if (tileCache.TileType == 195)
            num = 5;

        if (tileCache.TileType == 196)
            num = 108;

        if (tileCache.TileType == 460)
            num = 108;

        if (tileCache.TileType == 197)
            num = 4;

        if (tileCache.TileType == 153)
            num = 26;

        if (tileCache.TileType == 154)
            num = 32;

        if (tileCache.TileType == 155)
            num = 2;

        if (tileCache.TileType == 156)
            num = 1;

        if (tileCache.TileType == 116 || tileCache.TileType == 118 || tileCache.TileType == 147 || tileCache.TileType == 148)
            num = 51;

        if (tileCache.TileType == 109 || tileCache.TileType == 492)
            num = ((genRand.Next(2) != 0) ? 47 : 0);

        if (tileCache.TileType == 110 || tileCache.TileType == 113 || tileCache.TileType == 115)
            num = 47;

        if (tileCache.TileType == 107 || tileCache.TileType == 121 || tileCache.TileType == 685)
            num = 48;

        if (tileCache.TileType == 108 || tileCache.TileType == 122 || tileCache.TileType == 146 || tileCache.TileType == 686)
            num = 49;

        if (tileCache.TileType == 111 || tileCache.TileType == 145 || tileCache.TileType == 150)
            num = 50;

        if (tileCache.TileType == 133) {
            num = 50;
            if (tileCache.TileFrameX >= 54)
                num = 146;
        }

        if (tileCache.TileType == 134) {
            num = 49;
            if (tileCache.TileFrameX >= 36)
                num = 145;
        }

        if (tileCache.TileType == 149)
            num = 49;

        if (Main.tileAlch[tileCache.TileType]) {
            int num16 = tileCache.TileFrameX / 18;
            if (num16 == 0)
                num = 3;

            if (num16 == 1)
                num = 3;

            if (num16 == 2)
                num = 7;

            if (num16 == 3)
                num = 17;

            if (num16 == 4)
                num = 289;

            if (num16 == 5)
                num = 6;

            if (num16 == 6)
                num = 224;
        }

        if (tileCache.TileType == 58 || tileCache.TileType == 76 || tileCache.TileType == 77 || tileCache.TileType == 684)
            num = ((genRand.Next(2) != 0) ? 25 : 6);

        if (tileCache.TileType == 37)
            num = ((genRand.Next(2) != 0) ? 23 : 6);

        if (tileCache.TileType == 32)
            num = ((genRand.Next(2) != 0) ? 24 : 14);

        if (tileCache.TileType == 352)
            num = ((genRand.Next(3) != 0) ? 125 : 5);

        if (tileCache.TileType == 23 || tileCache.TileType == 24 || tileCache.TileType == 661)
            num = ((genRand.Next(2) != 0) ? 17 : 14);

        if (tileCache.TileType == 25 || tileCache.TileType == 31)
            num = ((tileCache.TileType == 31 && tileCache.TileFrameX >= 36) ? 5 : ((genRand.Next(2) != 0) ? 1 : 14));

        if (tileCache.TileType == 20) {
            switch (tileCache.TileFrameX / 54) {
                case 1:
                    num = 122;
                    break;
                case 2:
                    num = 78;
                    break;
                case 3:
                    num = 77;
                    break;
                case 4:
                    num = 121;
                    break;
                case 5:
                    num = 79;
                    break;
                default:
                    num = 7;
                    break;
            }
        }

        if (tileCache.TileType == 27)
            num = ((genRand.Next(2) != 0) ? 19 : 3);

        if (tileCache.TileType == 129) {
            if (tileCache.TileFrameX >= 324)
                num = 69;

            num = ((tileCache.TileFrameX != 0 && tileCache.TileFrameX != 54 && tileCache.TileFrameX != 108) ? ((tileCache.TileFrameX != 18 && tileCache.TileFrameX != 72 && tileCache.TileFrameX != 126) ? 70 : 69) : 68);
        }

        if (tileCache.TileType == 385)
            num = genRand.Next(68, 71);

        if (tileCache.TileType == 4) {
            int num17 = (int)MathHelper.Clamp(tileCache.TileFrameY / 22, 0f, TorchID.Count - 1);
            num = TorchID.Dust[num17];
        }

        if (tileCache.TileType == 35) {
            num = 189;
            if (tileCache.TileFrameX < 36 && genRand.Next(2) == 0)
                num = 6;
        }

        if ((tileCache.TileType == 34 || tileCache.TileType == 42) && genRand.Next(2) == 0)
            num = 6;

        if (tileCache.TileType == 270)
            num = -1;

        if (tileCache.TileType == 271)
            num = -1;

        if (tileCache.TileType == 581)
            num = -1;

        if (tileCache.TileType == 660)
            num = -1;

        if (tileCache.TileType == 79 || tileCache.TileType == 90 || tileCache.TileType == 101)
            num = -1;

        if (tileCache.TileType == 33 || tileCache.TileType == 34 || tileCache.TileType == 42 || tileCache.TileType == 93 || tileCache.TileType == 100)
            num = -1;

        if (tileCache.TileType == 321 || tileCache.TileType == 574)
            num = 214;

        if (tileCache.TileType == 322)
            num = 215;

        if (tileCache.TileType == 635)
            num = 36;

        bool flag = tileCache.TileType == 178 || tileCache.TileType == 440;
        ushort TileType = tileCache.TileType;
        if (TileType == 178 || (uint)(TileType - 426) <= 1u || (uint)(TileType - 430) <= 10u)
            flag = true;

        if (TileLoader.GetTile(tileCache.TileType) is ModTile modTile)
            num = modTile.DustType;

        return num;
    }
}
