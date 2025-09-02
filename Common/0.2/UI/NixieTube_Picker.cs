using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.InterfaceElements;
using RoA.Content.Tiles.Decorations;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace RoA.Content.UI;

sealed class NixieTubePicker : InterfaceElement {
    private static byte SYMBOLCOUNTMAXINAROW => 26;
    private static byte SYMBOLCOUNTMAXINACOLUMN => 3;
    private static ushort SIZEX => 326;
    private static ushort SIZEY => 58;
    private static byte NUMBERCOUNT => 10;
    private static byte ENGSYMCOUNT => 26;
    private static byte MISCSYMCOUNT => 4;
    private static int LENGTH => NUMBERCOUNT + ENGSYMCOUNT + MISCSYMCOUNT;

    private static Vector2 _attachedPosition;
    private static Point16 _nixieTubeTilePosition;
    private static Asset<Texture2D>? _pickButton, _borderButton;
    private static bool[]? _soundPlayed, _mouseHovering;
    private static byte[]? _columnsPerRow;
    private static readonly string[] _specialSymbols = [":", "!", "?", "."];

    public NixieTubePicker() : base($"{RoA.ModName}: Nixie Tube", InterfaceScaleType.Game) { }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

    public override void Load(Mod mod) {
        if (Main.dedServ) {
            return;
        }

        _pickButton = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_PickButton");
        _borderButton = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_BorderButton");

        _soundPlayed = new bool[LENGTH];
        _mouseHovering = new bool[LENGTH];

        _columnsPerRow = [NUMBERCOUNT, ENGSYMCOUNT, MISCSYMCOUNT];
    }

    public override void Update() {
        Point16 attachedPositionInTile = _attachedPosition.ToTileCoordinates16();
        Player player = Main.LocalPlayer;
        int distance = 80;
        bool tooFar = true;
        for (int i = -1; i < 1; i++) {
            int x = attachedPositionInTile.X + i;
            for (int j = -1; j < 2; j++) {
                if (player.IsWithinSnappngRangeToTile(x, attachedPositionInTile.Y + j, distance)) {
                    tooFar = false;
                    goto BreakLoops;
                }
            }
        }
    BreakLoops:
        if (tooFar || _attachedPosition == Vector2.Zero || WorldGenHelper.GetTileSafely(_nixieTubeTilePosition.X, _nixieTubeTilePosition.Y).TileType != ModContent.TileType<NixieTube>()) {
            Active = false;
        }
    }

    protected override bool DrawSelf() {
        SpriteBatch batch = Main.spriteBatch;
        Vector2 position = _attachedPosition;
        position.X -= 8f;
        position.Y -= 16f * 6f;
        position.Y += SIZEY / 2;
        position -= Main.screenPosition;
        Vector2 zero = Vector2.Zero;
        float x = position.X, y = position.Y;
        int num17 = SIZEX;
        int num18 = SIZEY;
        Rectangle box = new((int)(x - num17), (int)(y - num18), (int)zero.X + num17 * 2, (int)zero.Y + num18 + num18 / 2);
        Color color = new Color(33, 43, 79) * 0.8f;
        Utils.DrawInvBG(batch, box, color);

        Vector2 startPosition = position + Main.screenPosition;
        startPosition.Y -= SIZEY / 2;
        startPosition.Y -= 11 - 2;
        DrawPickIcons(batch, startPosition);

        return true;
    }

    public static void Activate(Point16 tilePosition) {
        ref bool active = ref InterfaceElementsSystem.InterfaceElements[typeof(NixieTubePicker)].Active;
        if (IsAtAttachedPosition(tilePosition)) {
            active = !active;
        }

        Point16 nixieTubeTopLeft = GetTopLeftOfNixieTube(tilePosition);
        _attachedPosition = (nixieTubeTopLeft + new Point16(1, 1)).ToWorldCoordinates();
        _nixieTubeTilePosition = nixieTubeTopLeft;
    }

    public static void Deactivate(Point16 tilePosition) {
        if (IsAtAttachedPosition(tilePosition)) {
            _attachedPosition = Vector2.Zero;
        }
    }

    private static bool IsAtAttachedPosition(Point16 tilePosition) => GetTopLeftOfNixieTube(tilePosition) == _nixieTubeTilePosition;

    private static Point16 GetTopLeftOfNixieTube(Point16 tilePosition) {
        int left = tilePosition.X;
        int top = tilePosition.Y;
        Tile tile = Main.tile[left, top];
        if (tile.TileFrameX % 36 != 0) {
            left--;
        }
        if (tile.TileFrameY % 56 != 0) {
            top--;
        }
        if (WorldGenHelper.GetTileSafely(left, top + 2).TileType != ModContent.TileType<NixieTube>()) {
            top -= 1;
        }
        return new Point16(left, top);
    }

    private static void DrawPickIcons(SpriteBatch batch, Vector2 startPosition) {
        if (_pickButton?.IsLoaded != true || _borderButton?.IsLoaded != true) {
            return;
        }

        void drawPickIcon(byte index, SpriteBatch batch, Vector2 position, Rectangle clip, bool highligted = false) {
            Texture2D texture = _pickButton!.Value,
                      borderTexture = _borderButton!.Value;
            Vector2 mousePosition = Main.MouseWorld + Vector2.One * clip.Width / 2f;
            bool mouseOver = mousePosition.Between(position, position + clip.Size());
            string hoverText = GetHoverText(index);
            Color color = Color.White;
            if (!mouseOver && !highligted) {
                color *= 0.6f;
            }
            Vector2 origin = clip.Centered();
            batch.Draw(texture, position, DrawInfo.Default with {
                Color = color,
                Clip = clip,
                Origin = origin
            });
            ref bool mouseHovering = ref _mouseHovering![index];
            ref bool soundPlayed = ref _soundPlayed![index];
            if (mouseOver || highligted) {
                bool flag = !highligted || mouseOver;
                mouseHovering = true;
                batch.Draw(borderTexture, position, DrawInfo.Default with {
                    Color = color,
                    Clip = borderTexture.Bounds,
                    Origin = origin
                });
                if (flag) {
                    if (!Main.mouseText) {
                        Main.instance.MouseText(hoverText, 0, 0);
                        Main.mouseText = true;
                    }
                    if (!soundPlayed) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        soundPlayed = true;
                    }
                    if (Main.mouseLeft && Main.mouseLeftRelease) {
                        ChangeNixieTubeSymbol(index);
                    }
                    Main.LocalPlayer.mouseInterface = true;
                }
            }
            else {
                if (mouseHovering && soundPlayed) {
                    soundPlayed = false;
                }
                mouseHovering = false;
            }
        }

        Player player = Main.LocalPlayer;
        SpriteFrame frame = new(SYMBOLCOUNTMAXINAROW, SYMBOLCOUNTMAXINACOLUMN);
        Texture2D texture = _pickButton.Value;
        Rectangle clip = frame.GetSourceRectangle(texture);
        clip.Width += 1;
        clip.Height += 2;
        int width = clip.Width + 2,
            height = clip.Height + 2;
        startPosition.X += width / 2;
        for (int i = 0; i < LENGTH; i++) {
            GetColumnAndRow((byte)i, out byte column, out byte row);
            Vector2 position = startPosition;
            byte[] columnsPerRow = _columnsPerRow!;
            int currentSum = 0;
            int rowCount = 0;
            int totalColumnsBefore = 0;
            foreach (byte columnsInRow in columnsPerRow) {
                currentSum += columnsInRow;
                if (i >= currentSum) {
                    rowCount++;
                    totalColumnsBefore = currentSum;
                }
                else {
                    break;
                }
            }
            if (rowCount > 0) {
                position.Y += height * rowCount;
                position.X -= width * totalColumnsBefore;
            }
            frame.CurrentColumn = column;
            frame.CurrentRow = (byte)(row - 1);
            clip = frame.GetSourceRectangle(texture);
            clip.Width += 1;
            clip.Height += 2;
            position.X -= width * columnsPerRow[rowCount] / 2f;
            GetColumnAndRowFromTile(out byte column2, out byte row2);
            drawPickIcon((byte)i, batch, position, clip, column2 == column && row2 == row);
            startPosition.X += width;
        }
    }

    public static void ChangeNixieTubeSymbol(byte index) {
        TileObjectData tileData = TileObjectData.GetTileData(ModContent.TileType<NixieTube>(), 0);
        int tileWidth = tileData.CoordinateWidth + 2;
        int width = tileData.Width;
        int height = tileData.Height;
        GetColumnAndRow(index, out byte column, out byte row);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Tile tile = Main.tile[_nixieTubeTilePosition.X + i, _nixieTubeTilePosition.Y + j];
                tile.TileFrameX = (short)(tileWidth * (width * column + i));
                int frameYOffset = j == 0 ? 0 : tileData.CoordinateHeights[j - 1] + 2;
                tile.TileFrameY = (short)((tileData.CoordinateHeights.Sum() + 2 * height) * row + frameYOffset * j);
            }
        }
    }

    private static void GetColumnAndRowFromTile(out byte column, out byte row) {
        Tile tile = WorldGenHelper.GetTileSafely(_nixieTubeTilePosition);
        column = (byte)(tile.TileFrameX / 36);
        row = (byte)(tile.TileFrameY / 56);
    }

    private static string GetHoverText(byte index) {
        byte[] columnsPerRow = _columnsPerRow!;
        int cumulativeSum = 0;
        for (int i = 0; i < columnsPerRow.Length; i++) {
            int previousSum = cumulativeSum;
            cumulativeSum += columnsPerRow[i];
            if (index < cumulativeSum) {
                int positionInRow = index - previousSum;
                return i switch {
                    0 => positionInRow.ToString(),
                    1 => ((char)('A' + positionInRow)).ToString(),
                    2 => positionInRow < _specialSymbols.Length ? _specialSymbols[positionInRow] : string.Empty,
                    _ => string.Empty
                };
            }
        }
        return string.Empty;
    }

    private static void GetColumnAndRow(byte index, out byte column, out byte row) {
        column = 0;
        row = 1;
        byte[] columnsPerRow = _columnsPerRow!;
        int cumulativeSum = 0;
        for (int i = 0; i < columnsPerRow.Length; i++) {
            cumulativeSum += columnsPerRow[i];
            if (index < cumulativeSum) {
                column = (byte)(index - (cumulativeSum - columnsPerRow[i]));
                row += (byte)i;
                break;
            }
        }
    }
}
