using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

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

sealed class NixieTubePicker_TextureLoader : IInitializer {
    public static Asset<Texture2D> NixieTubeLanguageButton { get; private set; }
    public static Asset<Texture2D> NixieTubeLanguageButtonBorder { get; private set; }
    public static Asset<Texture2D> NixieTubePickButton { get; private set; }
    public static Asset<Texture2D> NixieTubePickButtonBorder { get; private set; }

    public static Asset<Texture2D> NixieTubeLightButton { get; private set; }
    public static Asset<Texture2D> NixieTubeLightButtonBorder { get; private set; }

    void ILoadable.Load(Mod mod) {
        On_Player.Spawn += On_Player_Spawn;

        if (Main.dedServ) {
            return;
        }

        NixieTubeLanguageButton = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_LanguageButton");
        NixieTubeLanguageButtonBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_LanguageButton_Border");
        NixieTubePickButton = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_PickButton2");
        NixieTubePickButtonBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_PickButton2_Border");

        NixieTubeLightButton = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_LightButton");
        NixieTubeLightButtonBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_LanguageButton_Border");
    }

    private void On_Player_Spawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context) {
        orig(self, context);
        if (context == PlayerSpawnContext.SpawningIntoWorld) {
            UILoader.GetUIState<NixieTubePicker_RemadePicker>().Recalculate();
        }
    }
}

sealed class NixieTubePicker_RemadePicker : SmartUIState {
    public const byte NUMCOUNT = 11;
    public const byte ENGCOUNT = 26;
    public const byte RUSCOUNT = 32;
    public const byte MISCCOUNT = 25;

    private readonly UIElement _mainContainer;
    private readonly UINixieTubePanel _numGrid;
    private readonly UINixieTubePanel _engRusGrid;
    private readonly UINixieTubePanel _miscGrid;
    private readonly UINixieTubeLanguageButton _languageButton;
    private readonly UINixieTubeLightButton _lightButton;

    private static int _oldPickedIndexRussian, _oldPickedIndexEnglish;

    public static bool ShouldUpdateIndex = false;
    public static bool CurrentEnglish = false;

    public static bool IsRussian { get; private set; }
    public static bool IsFlickerOff { get; private set; }
    public static byte ENGRUSCOUNT => IsRussian ? RUSCOUNT : ENGCOUNT;

    private static Point16 _nixieTubeTilePosition;
    private static readonly char[] _miscSymbols = [':', '!', '?', '.', ',', '(', ')', '/', '|', '\u005c', '+', '-', '=', '#', '%', '&', '<', '>', '[', ']', '"', '\u0027', '_', '*', '^'];

    public UINixieTubeDyeSlot DyeSlot1 { get; init; }
    public UINixieTubeDyeSlot DyeSlot2 { get; init; }

    public static int PickedIndex { get; private set; }

    public static bool Active;

    public NixieTubePicker_RemadePicker() {
        RemoveAllChildren();
        _mainContainer = new UIElement {
            Width = StyleDimension.FromPixels(800),
            Height = StyleDimension.FromPixels(230),
            Top = StyleDimension.FromPixels(100f),
            HAlign = 0.55f,
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

        _numGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.2f - 0.015f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.02f)
        };

        _mainContainer.Append(_numGrid);

        _engRusGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.35f + 0.0175f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.23f - 0.016f)
        };
        _mainContainer.Append(_engRusGrid);

        _miscGrid = new UINixieTubePanel {
            Width = StyleDimension.FromPercent(0.3f),
            Height = StyleDimension.FromPercent(0.75f),
            Top = StyleDimension.FromPercent(0.065f),
            Left = StyleDimension.FromPercent(0.59f)
        };
        _mainContainer.Append(_miscGrid);

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

        _languageButton = new UINixieTubeLanguageButton(NixieTubePicker_TextureLoader.NixieTubeLanguageButton) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.65f),
            Left = StyleDimension.FromPercent(1f - 0.1f),
            Scale = 0.75f
        };
        _languageButton.SetHoverImage(NixieTubePicker_TextureLoader.NixieTubeLanguageButtonBorder);
        _languageButton.OnLeftClick += _languageButton_OnLeftClick;

        _lightButton = new UINixieTubeLightButton(NixieTubePicker_TextureLoader.NixieTubeLightButton) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.65f),
            Left = StyleDimension.FromPercent(1f - 0.0564f),
            Scale = 0.75f
        };
        _lightButton.SetHoverImage(NixieTubePicker_TextureLoader.NixieTubeLightButtonBorder);
        _lightButton.OnLeftClick += _lightButton_OnLeftClick;

        DyeSlot1 = new UINixieTubeDyeSlot(true, ItemSlot.Context.EquipMiscDye) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.2f + 0.01f - 0.115f),
            Left = StyleDimension.FromPercent(1f - 0.0875f),
            Scale = 0.85f
        };
        _mainContainer.Append(DyeSlot1);

        DyeSlot2 = new UINixieTubeDyeSlot(false, ItemSlot.Context.EquipMiscDye) {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPercent(0.5f - 0.06f + 0.03f - 0.115f),
            Left = StyleDimension.FromPercent(1f - 0.0875f),
            Scale = 0.85f
        };
        _mainContainer.Append(DyeSlot2);

        _mainContainer.Append(_languageButton);
        _mainContainer.Append(_lightButton);

        Recalculate();
    }

    private void _languageButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        bool previousRussian = IsRussian;
        if (PickedIndex != 0) {
            if (previousRussian) {
                _oldPickedIndexRussian = PickedIndex;
            }
            else {
                _oldPickedIndexEnglish = PickedIndex;
            }
        }
        bool flag = false;
        if (CurrentEnglish && previousRussian) {
            PickedIndex = _oldPickedIndexEnglish;
            flag = true;
        }
        if (!CurrentEnglish && previousRussian) {
            PickedIndex = _oldPickedIndexRussian;
            flag = true;
        }
        if (!flag && PickedIndex > NUMCOUNT && PickedIndex <= ENGRUSCOUNT + NUMCOUNT) {
            PickedIndex = 0;
            ShouldUpdateIndex = false;
        }
        IsRussian = !IsRussian;
        PopulateLists();
        //SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void OnInitialize() {
        Recalculate();
    }

    public override bool Visible => Active;

    public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

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
        _numGrid.RemoveAllChildren();
        List<NixieTubeInfo> buttons = [];
        int last = 0;
        for (int i = 1; i < NUMCOUNT; i++) {
            buttons.Add(new NixieTubeInfo((byte)i));
        }
        UINixieTubeGrid numEntries = new UINixieTubeGrid(buttons);
        _numGrid.Append(numEntries);
        last += NUMCOUNT;

        _engRusGrid.RemoveAllChildren();
        buttons = [];
        if (IsRussian) {
            for (int i = last; i < last + RUSCOUNT; i++) {
                buttons.Add(new NixieTubeInfo((byte)i, true));
            }
            last += RUSCOUNT;
        }
        else {
            for (int i = last; i < last + ENGCOUNT; i++) {
                buttons.Add(new NixieTubeInfo((byte)i));
            }
            last += ENGCOUNT;
        }
        UINixieTubeGrid engEntries = new UINixieTubeGrid(buttons, true);
        _engRusGrid.Append(engEntries);

        _miscGrid!.RemoveAllChildren();
        buttons = [];
        for (int i = last; i < last + MISCCOUNT; i++) {
            buttons.Add(new NixieTubeInfo((byte)i));
        }
        UINixieTubeGrid miscEntries = new UINixieTubeGrid(buttons);
        _miscGrid.Append(miscEntries);
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
            Main.LocalPlayer.releaseUseItem = false;
        }

        Recalculate();

        Point16 attachedPositionInTile = _nixieTubeTilePosition;
        Player player = Main.LocalPlayer;
        int distance = 100;
        bool tooFar = true;
        for (int i = -1; i < 1; i++) {
            int x = attachedPositionInTile.X + i;
            for (int j = -1; j < 2; j++) {
                if (player.IsWithinSnappngRangeToTile(x, attachedPositionInTile.Y + j, distance)) {
                    tooFar = false;
                }
            }
        }
        if (!Main.playerInventory || tooFar || _nixieTubeTilePosition == Point16.Zero || WorldGenHelper.GetTileSafely(_nixieTubeTilePosition.X, _nixieTubeTilePosition.Y).TileType != ModContent.TileType<NixieTube>()) {
            Active = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        if (ShouldUpdateIndex) {
            PickedIndex = GetIndex();
            if (PickedIndex != 0 && PickedIndex < NUMCOUNT) {
                PickedIndex += 1;
            }
        }

        GetColumnAndRowFromTile(out _, out _);
    }

    private void _lightButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        IsFlickerOff = !IsFlickerOff;
        var tilePos = _nixieTubeTilePosition;
        NixieTube.GetTE(tilePos.X, tilePos.Y).IsFlickerOff = IsFlickerOff;
        //SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public static void ResetPickedIndex() {
        PickedIndex = 0;
        ShouldUpdateIndex = true;
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
        if (index >= NUMCOUNT && index < NUMCOUNT + RUSCOUNT && IsRussian) {
            row += 2;
        }
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
        if (IsRussian) {
            groupSizes = [NUMCOUNT, RUSCOUNT, MISCCOUNT];
        }
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
        int[] groupSizes = [NUMCOUNT, ENGCOUNT, MISCCOUNT];
        if (IsRussian) {
            groupSizes = [NUMCOUNT, RUSCOUNT, MISCCOUNT];
        }
        int cumulativeSum = 0;
        for (int i = 0; i < row - 1; i++) {
            cumulativeSum += groupSizes[i];
        }
        return (byte)(cumulativeSum + column);
    }

    public static void Toggle(int i, int j, bool flickerOn = false) {
        Point16 checkPos = GetTopLeftOfNixieTube(new Point16(i, j));
        bool flag = Active && checkPos != _nixieTubeTilePosition;
        if (!Active || flag) {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Main.LocalPlayer.SetTalkNPC(-1);
            Main.playerInventory = true;
            Main.recBigList = false;
            Main.hidePlayerCraftingMenu = true;
            _nixieTubeTilePosition = checkPos;
            ResetPickedIndex();

            IsFlickerOff = flickerOn;
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
        if (IsRussian && tile.TileFrameY / 56 >= 4) {
            CurrentEnglish = false;
            row -= 2;
            return;
        }
        CurrentEnglish = true;
    }

    public static string GetHoverText(byte index) {
        if (index < NUMCOUNT) {
            if (index < NUMCOUNT) {
                index -= 1;
            }
            return index.ToString();
        }
        int count = ENGRUSCOUNT;
        if (index < count + NUMCOUNT) {
            return ((char)((IsRussian ? 'А' : 'A') + (index - NUMCOUNT))).ToString();
        }
        if (index < MISCCOUNT + count + NUMCOUNT) {
            int checkIndex = index - count - NUMCOUNT;
            return _miscSymbols[checkIndex].ToString();
        }
        return string.Empty;
    }
}
