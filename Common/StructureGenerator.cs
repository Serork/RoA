using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common;

sealed class StructureGenerator : ModSystem {
    private static string FOLDERNAME => "RoAData";

    public enum StructureOriginType : byte {
        TopLeft,
        BottomCenter
    }

    private record struct TileData(bool HasTile, Point16 Position = default, ushort TileType = default, ushort TileFrameX = default, ushort TileFrameY = default, SlopeType TileSlopeType = default, byte TileColor = default, bool HasUnactuatedTile = default);
    private record struct WallData(Point16 Position, ushort WallType, byte WallColor = default);
    private record struct WireData(Point16 Position, bool BlueWire, bool RedWire, bool YellowWire, bool GreenWire);

    private static readonly HashSet<TileData> _structureData_Tiles = [];
    private static readonly HashSet<WallData> _structureData_Walls = [];
    private static readonly HashSet<WireData> _structureData_Wires = [];

    private static bool _captureActive = false, _captureReleased = false;
    private static Point16 _firstCapturedTile = Point16.Zero, _secondCapturedTile = Point16.Zero;
    private static Rectangle _capturedZone = Rectangle.Empty;

    private static bool StartedCapturing => _firstCapturedTile != Point16.Zero;

    private static bool ShouldCapture => Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z);
    private static bool Capturing => Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X);

    public override void PostUpdateInput() {
        //HandleLogic();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        //DrawLayout(layers);
    }

    private void HandleLogic() {
        if (!ShouldCapture) {
            _captureReleased = false;
            return;
        }
        if (_captureReleased) {
            return;
        }
        _captureReleased = true;
        _captureActive = !_captureActive;
    }

    private void DrawLayout(List<GameInterfaceLayer> layers) {
        string layerName = "Vanilla: Ruler";
        int rulerLayerIndex = layers.FindIndex(layer => layer.Name == layerName);
        if (rulerLayerIndex < 0) {
            return;
        }
        string structureMakerLayerName = "Structure Layer Layout";
        layers.Insert(rulerLayerIndex, new LegacyGameInterfaceLayer(
           structureMakerLayerName,
            delegate {
                DrawStructureLayerLayout();
                return true;
            },
            InterfaceScaleType.UI)
        );
    }

    private string CreateSaveFile(string extension) {
        string name = "RoAStructure";

        string basePath = ModLoader.ModPath.Replace("Mods", FOLDERNAME);
        if (!Directory.Exists(basePath)) {
            Directory.CreateDirectory(basePath);
        }
        string path = Path.Combine(basePath, name),
               finalPath = path + extension;
        if (File.Exists(finalPath)) {
            File.Delete(finalPath);
        }
        path += extension;

        return path;
    }

    private void SaveCapturedZoneToFile() {
        string path = CreateSaveFile(".str");

        using FileStream fileStream = File.Create(path);
        using StreamWriter writer = new(fileStream, System.Text.Encoding.UTF8);
        ushort top = (ushort)_capturedZone.Top,
               bottom = (ushort)_capturedZone.Bottom,
               left = (ushort)_capturedZone.Left,
               right = (ushort)_capturedZone.Right;
        writer.WriteLine(top);
        writer.WriteLine(bottom);
        writer.WriteLine(left);
        writer.WriteLine(right);
        for (int y = top; y <= bottom; y++) {
            for (int x = left; x <= right; x++) {
                Tile tile = Main.tile[x, y];
                bool hasTile = tile.HasTile;
                void write(object obj) {
                    writer.Write(obj);
                    writer.Write(',');
                }
                write(hasTile);
                ushort wallType = tile.WallType;
                write(wallType);
                if (wallType > WallID.None) {
                    byte wallColor = tile.WallColor;
                    write(wallColor);
                }
                bool blueWire = tile.BlueWire,
                     redWire = tile.RedWire,
                     yellowWire = tile.YellowWire,
                     greenWire = tile.GreenWire;
                write(blueWire);
                write(redWire);
                write(yellowWire);
                write(greenWire);
                bool end = false;
                if (hasTile) {
                    ushort tileType = tile.TileType;
                    write(tileType);
                    ushort tileFrameX = (ushort)tile.TileFrameX,
                           tileFrameY = (ushort)tile.TileFrameY;
                    write(tileFrameX);
                    write(tileFrameY);
                    byte tileSlopeType = (byte)tile.Slope;
                    write(tileSlopeType);
                    byte tileColor = tile.TileColor;
                    write(tileColor);
                    bool hasUnactuatedTile = tile.HasUnactuatedTile;
                    write(hasUnactuatedTile);
                    end = true;
                }
                else {
                    writer.Write('|');
                    writer.WriteLine();
                    continue;
                }
                if (end || x >= right) {
                    writer.Write('|');
                    writer.WriteLine();
                }
            }
        }
        Main.NewText($"Structure saved in {path}");
    }

    public static void ReadStructureFromMap(string structureMap, out int sizeX, out int sizeY) {
        _structureData_Tiles.Clear();
        _structureData_Walls.Clear();
        _structureData_Wires.Clear();

        using StringReader reader = new(structureMap);
        ushort asUshort_Size() => ushort.Parse(reader.ReadLine()!);
        ushort top = asUshort_Size(),
               bottom = asUshort_Size(),
               left = asUshort_Size(),
               right = asUshort_Size();
        sizeX = right - left + 1;
        sizeY = bottom - top + 1; 
        for (int y = top; y <= bottom; y++) {
            for (int x = left; x <= right; x++) {
                string? line = reader.ReadLine();
                if (line is null) {
                    continue;
                }
                if (line.EndsWith('|')) {
                    line = line[..^1];
                }
                string[] data = line.Split(',');
                bool asBool(string from) => bool.Parse(from);
                ushort asUshort(string from) => ushort.Parse(from);
                byte asByte(string from) => byte.Parse(from);
                bool hasTile = asBool(data[0]);
                ushort wallType = asUshort(data[1]);
                int nextIndex = 2;
                byte wallColor = 0;
                if (wallType > WallID.None) {
                    wallColor = asByte(data[nextIndex++]);
                }
                bool blueWire = asBool(data[nextIndex++]),
                     redWire = asBool(data[nextIndex++]),
                     yellowWire = asBool(data[nextIndex++]),
                     greenWire = asBool(data[nextIndex++]);
                Point16 tilePosition = new(x - left, y - top);
                if (hasTile) {
                    ushort tileType = asUshort(data[nextIndex++]);
                    ushort tileFrameX = asUshort(data[nextIndex++]),
                           tileFrameY = asUshort(data[nextIndex++]);
                    byte tileSlopeType = asByte(data[nextIndex++]);
                    byte tileColor = asByte(data[nextIndex++]);
                    bool hasUnactuatedTile = asBool(data[nextIndex++]);
                    _structureData_Tiles.Add(new TileData(true, tilePosition, tileType, tileFrameX, tileFrameY, (SlopeType)tileSlopeType, tileColor, hasUnactuatedTile));
                }
                else {
                    _structureData_Tiles.Add(new TileData(false));
                }
                _structureData_Walls.Add(new WallData(tilePosition, wallType, wallColor));
                _structureData_Wires.Add(new WireData(tilePosition, blueWire, redWire, yellowWire, greenWire));
            }
        }
    }

    public static void GenerateStructureFromMap(string structureMap, Point16 origin, StructureOriginType originType = StructureOriginType.TopLeft,
        int cleanUpFluffX = 0, int cleanUpFluffY = 0) {
        ReadStructureFromMap(structureMap, out int sizeX, out int sizeY);
        int x = origin.X,
            y = origin.Y;
        switch (originType) {
            case StructureOriginType.TopLeft:
                break;
            case StructureOriginType.BottomCenter:
                x -= sizeX / 2;
                y -= sizeY;
                break;
        }
        for (int i = 0; i < sizeX - cleanUpFluffX; i++) {
            for (int j = 0; j < sizeY - cleanUpFluffY; j++) {
                WorldGenHelper.GetTileSafely(x + i, y + j).ClearEverything();
            }
        }
        Point16 startPosition = new(x, y);
        foreach (TileData tileData in _structureData_Tiles) {
            Point16 position = startPosition + tileData.Position;
            Tile tile = WorldGenHelper.GetTileSafely(position);
            tile.HasTile = tileData.HasTile;
            if (tile.HasTile) {
                tile.TileType = tileData.TileType;
                tile.TileFrameX = (short)tileData.TileFrameX;
                tile.TileFrameY = (short)tileData.TileFrameY;
                tile.Slope = tileData.TileSlopeType;
                tile.TileColor = tileData.TileColor;
                tile.IsActuated = !tileData.HasUnactuatedTile;
            }
        }
        foreach (WallData wallData in _structureData_Walls) {
            Point16 position = startPosition + wallData.Position;
            Tile tile = WorldGenHelper.GetTileSafely(position);
            tile.WallType = wallData.WallType;
            tile.WallColor = wallData.WallColor;
            WorldGen.SquareWallFrame(position.X, position.Y);
        }
        foreach (WireData wireData in _structureData_Wires) {
            Point16 position = startPosition + wireData.Position;
            Tile tile = WorldGenHelper.GetTileSafely(position);
            tile.BlueWire = wireData.BlueWire;
            tile.RedWire = wireData.RedWire;
            tile.YellowWire = wireData.YellowWire;
            tile.GreenWire = wireData.GreenWire;
        }
    }

    private void ResetCapture() {
        if (!Capturing && StartedCapturing) {
            SaveCapturedZoneToFile();

            _firstCapturedTile = _secondCapturedTile = Point16.Zero;
            _capturedZone = Rectangle.Empty;
        }
    }

    private void StartCapturing() {
        if (Capturing && !StartedCapturing) {
            _firstCapturedTile = new Point16(Player.tileTargetX, Player.tileTargetY);
        }
    }

    private void ContinueCapturing() {
        if (!StartedCapturing) {
            return;
        }

        _secondCapturedTile = new Point16(Player.tileTargetX, Player.tileTargetY);
    }

    private void DrawStructureLayerLayout() {
        if (!_captureActive) {
            return;
        }

        float num = Main.LocalPlayer.velocity.Length();
        float num2 = 6f;
        float num3 = 2f;
        if (!(num <= num2))
            return;

        Main.LocalPlayer.mouseInterface = true;

        StartCapturing();
        ResetCapture();
        ContinueCapturing();

        float num4 = 1f;
        if (num >= num3)
            num4 = 1f - (num - num3) / num2;

        int num5 = 1;
        if ((float)Main.mouseX + Main.screenPosition.X < Main.player[Main.myPlayer].Center.X)
            num5 = -1;

        int num6 = (int)(Main.player[Main.myPlayer].position.X + (float)(Main.player[Main.myPlayer].width / 2)) / 16;
        int num7 = (int)(Main.player[Main.myPlayer].position.Y + (float)Main.player[Main.myPlayer].height - 2f) / 16;
        if (Main.player[Main.myPlayer].gravDir == -1f)
            num7--;

        Vector2 mouseWorld = Main.MouseWorld;
        if (Math.Abs(num6 - (int)(Main.MouseWorld.X / 16f)) > 0)
            num6 += num5;

        if (Main.player[Main.myPlayer].gravDir == -1f)
            mouseWorld.Y += 16f;

        mouseWorld /= 16f;
        new Vector2(num6, num7);
        int num8 = (int)mouseWorld.X;
        int num9 = (int)mouseWorld.Y;
        Math.Abs(num8);
        Math.Abs(num9);
        if (num8 == 0 && num9 == 0)
            return;

        Texture2D value = TextureAssets.Extra[2].Value;
        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);
        int num10 = num6;
        int num11 = num7;
        if (Main.player[Main.myPlayer].gravDir == -1f)
            num11--;

        float r = 0.8f;
        float g = 0.8f;
        float b = 0.2f;
        float a = 1f;
        float num12 = 0.8f;
        Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(r, g, b, a) * num12 * num4;

        void drawCapturedTile(Vector2 position) {
            position -= Vector2.One * 8f;
            Main.spriteBatch.Draw(value, Main.ReverseGravitySupport(position - Main.screenPosition, 16f), value2, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        if (!StartedCapturing) {
            drawCapturedTile(new Point16(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates());
            drawCapturedTile(_firstCapturedTile.ToWorldCoordinates());
            drawCapturedTile(_secondCapturedTile.ToWorldCoordinates());
        }

        int startX = Math.Min(_secondCapturedTile.X, _firstCapturedTile.X),
            startY = Math.Min(_secondCapturedTile.Y, _firstCapturedTile.Y);
        int endX = Math.Max(_secondCapturedTile.X, _firstCapturedTile.X),
            endY = Math.Max(_secondCapturedTile.Y, _firstCapturedTile.Y);
        _capturedZone = new Rectangle(startX, startY, endX - startX, endY - startY);
        for (int i = _capturedZone.Left; i <= _capturedZone.Right; i++) {
            for (int j = _capturedZone.Top; j <= _capturedZone.Bottom; j++) {
                drawCapturedTile(new Point16(i, j).ToWorldCoordinates());
            }
        }
    }
}
