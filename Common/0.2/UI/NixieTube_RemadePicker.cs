using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using RoA.Common.UI.UILoading;
using RoA.Content.Tiles.Decorations;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class NixieTubePicker_DisableOnTalk : IInitializer {
    void ILoadable.Load(Mod mod) {
        On_Player.SetTalkNPC += On_Player_SetTalkNPC;
    }

    private void On_Player_SetTalkNPC(On_Player.orig_SetTalkNPC orig, Player self, int npcIndex, bool fromNet) {
        orig(self, npcIndex, fromNet);
        if (npcIndex != -1) {
            NixieTubePicker_RemadePicker.Active = false;
        }
    }
}

sealed class NixieTubePicker_RemadePicker : SmartUIState {
    public const byte NUMCOUNT = 11;
    public const byte ENGCOUNT = 26;
    public const byte MISCCOUNT = 4;

    private static UIElement _mainContainer;
    private static UINixieTubePanel numGrid;
    private static UINixieTubePanel engGrid;
    private static UINixieTubePanel miscGrid;

    private static Point16 _nixieTubeTilePosition;

    public static UINixieTubeDyeSlot DyeSlot1 { get; private set; }
    public static UINixieTubeDyeSlot DyeSlot2 { get; private set; }

    public static int PickedIndex { get; private set; }

    public static bool Active;

    public override bool Visible => Active;

    public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

    public override void OnInitialize() {
        RemoveAllChildren();
        _mainContainer = new UIElement {
            Width = StyleDimension.FromPixels(700),
            Height = StyleDimension.FromPixels(230),
            Top = StyleDimension.FromPixels(100f),
            HAlign = 0.5f,
            VAlign = 0f
        };

        Append(_mainContainer);
        UIPanel mainPanel = new UIPanel {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPercent(1f),
            BackgroundColor = new Color(33, 43, 79) * 0.8f
        };

        mainPanel.SetPadding(0f);
        _mainContainer.Append(mainPanel);

        numGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.3f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.02f)
        };

        _mainContainer.Append(numGrid);

        engGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.35f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.33f)
        };
        _mainContainer.Append(engGrid);

        miscGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.2f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.689f)
        };
        _mainContainer.Append(miscGrid);

        UIClickableNPCButton uIText = new UIClickableNPCButton("Reset", 0.82f) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(1f - 0.125f),
            Left = StyleDimension.FromPercent(0.05f),
            TextOriginX = 0f,
            TextOriginY = 0f
        };
        uIText.OnLeftClick += UIText_OnLeftClick;
        _mainContainer.Append(uIText);

        UIClickableNPCButton uIText2 = new UIClickableNPCButton("Close", 0.82f) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(1f - 0.125f),
            Left = StyleDimension.FromPercent(0.135f),
            TextOriginX = 0f,
            TextOriginY = 0f
        };
        uIText2.OnLeftClick += UIText2_OnLeftClick;
        _mainContainer.Append(uIText2);

        PopulateLists();

        DyeSlot1 = new UINixieTubeDyeSlot(true, ItemSlot.Context.EquipMiscDye) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.2f + 0.01f),
            Left = StyleDimension.FromPercent(1f - 0.095f),
            Scale = 0.85f
        };
        _mainContainer.Append(DyeSlot1);

        DyeSlot2 = new UINixieTubeDyeSlot(false, ItemSlot.Context.EquipMiscDye) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.5f - 0.06f + 0.03f),
            Left = StyleDimension.FromPercent(1f - 0.095f),
            Scale = 0.85f
        };
        _mainContainer.Append(DyeSlot2);
    }

    public static NixieTubeTE? GetTE() => NixieTube.GetTE(_nixieTubeTilePosition.X, _nixieTubeTilePosition.Y);

    private void UIText_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        ChangeNixieTubeSymbol(0);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void UIText2_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        Active = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    private void PopulateLists() {
        numGrid.RemoveAllChildren();
        List<NixieTubeInfo> buttons = [];
        int last = 0;
        for (int i = 1; i < NUMCOUNT; i++) {
            buttons.Add(new NixieTubeInfo((byte)i));
        }
        UINixieTubeGrid numEntries = new UINixieTubeGrid(buttons);
        numGrid.Append(numEntries);
        last += NUMCOUNT;

        engGrid.RemoveAllChildren();
        buttons = [];
        for (int i = last; i < last + ENGCOUNT; i++) {
            buttons.Add(new NixieTubeInfo((byte)i));
        }
        UINixieTubeGrid engEntries = new UINixieTubeGrid(buttons);
        engGrid.Append(engEntries);
        last += ENGCOUNT;

        miscGrid!.RemoveAllChildren();
        buttons = [];
        for (int i = last; i < last + MISCCOUNT; i++) {
            buttons.Add(new NixieTubeInfo((byte)i));
        }
        UINixieTubeGrid miscEntries = new UINixieTubeGrid(buttons);
        miscGrid.Append(miscEntries);
        last += MISCCOUNT;
    }

    public override void SafeUpdate(GameTime gameTime) {
        if (!Main.playerInventory) {
            Active = false;
        }

        Main.recBigList = false;
        Main.hidePlayerCraftingMenu = true;

        if (_mainContainer.GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint())) {
            Main.LocalPlayer.mouseInterface = true;
        }

        Recalculate();

        if (Keyboard.GetState().IsKeyDown(Keys.F5)) {
            OnInitialize();
        }

        Point16 attachedPositionInTile = _nixieTubeTilePosition;
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
        if (!Main.playerInventory || tooFar || _nixieTubeTilePosition == Point16.Zero || WorldGenHelper.GetTileSafely(_nixieTubeTilePosition.X, _nixieTubeTilePosition.Y).TileType != ModContent.TileType<NixieTube>()) {
            Active = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        PickedIndex = GetIndex();
        if (PickedIndex != 0 && PickedIndex < NUMCOUNT) {
            PickedIndex += 1;
        }
    }

    public static void ChangeNixieTubeSymbol(byte index) {
        TileObjectData tileData = TileObjectData.GetTileData(ModContent.TileType<NixieTube>(), 0);
        int tileWidth = tileData.CoordinateWidth + 2;
        int width = tileData.Width;
        int height = tileData.Height;
        if (index < NUMCOUNT) {
            index -= 1;
        }
        GetColumnAndRow(index, out byte column, out byte row);
        var tilePos = _nixieTubeTilePosition;
        NixieTube.GetTE(tilePos.X, tilePos.Y).Activate();
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Tile tile = Main.tile[tilePos.X + i, tilePos.Y + j];
                tile.TileFrameX = (short)(tileWidth * (width * column + i));
                int frameYOffset = j == 0 ? 0 : tileData.CoordinateHeights[j - 1] + 2;
                tile.TileFrameY = (short)((tileData.CoordinateHeights.Sum() + 2 * height) * row + frameYOffset * j);
            }
        }
    }

    private static void GetColumnAndRow(byte index, out byte column, out byte row) {
        column = 0;
        row = 0;

        int[] groupSizes = { NUMCOUNT, ENGCOUNT, MISCCOUNT };
        int cumulativeSum = 0;
        for (byte currentRow = 1; currentRow <= groupSizes.Length; currentRow++) {
            if (index < cumulativeSum + groupSizes[currentRow - 1]) {
                row = currentRow;
                column = (byte)(index - cumulativeSum);
                return;
            }
            cumulativeSum += groupSizes[currentRow - 1];
        }
    }

    private static byte GetIndex() {
        GetColumnAndRowFromTile(out byte column, out byte row);
        int[] groupSizes = { NUMCOUNT, ENGCOUNT, MISCCOUNT };
        int cumulativeSum = 0;
        for (int i = 0; i < row - 1; i++) {
            cumulativeSum += groupSizes[i];
        }
        return (byte)(cumulativeSum + column);
    }

    public static void Toggle(int i, int j) {
        Point16 checkPos = GetTopLeftOfNixieTube(new Point16(i, j));
        bool flag = Active && checkPos != _nixieTubeTilePosition;
        if (!Active || flag) {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Main.LocalPlayer.SetTalkNPC(-1);
            Main.playerInventory = true;
            Main.recBigList = false;
            Main.hidePlayerCraftingMenu = true;
            _nixieTubeTilePosition = checkPos;
        }
        else {
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        if (!flag) {
            Active = !Active;
        }
    }

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

    private static void GetColumnAndRowFromTile(out byte column, out byte row) {
        Tile tile = WorldGenHelper.GetTileSafely(_nixieTubeTilePosition);
        column = (byte)(tile.TileFrameX / 36);
        row = (byte)(tile.TileFrameY / 56);
    }

    public static string GetHoverText(byte index) {
        if (index < NUMCOUNT) {
            return index.ToString();
        }
        if (index < ENGCOUNT + NUMCOUNT) {
            return ((char)('A' + (index - NUMCOUNT))).ToString();
        }
        if (index < MISCCOUNT + ENGCOUNT + NUMCOUNT) {
            int checkIndex = index - ENGCOUNT - NUMCOUNT;
            return checkIndex switch {
                0 => ":",
                1 => "!",
                2 => "?",
                3 => ".",
                _ => string.Empty
            };
        }
        return string.Empty;
    }
}
