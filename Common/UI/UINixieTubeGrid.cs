using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.UI;

namespace RoA.Common.UI;

readonly record struct NixieTubeInfo(byte Index, bool IsRussian = false) { }

sealed class UINixieTubeGrid : UIElement {
    private List<NixieTubeInfo>? _workingButtons;
    private int _atEntryIndex;
    private int _lastEntry;
    private bool _calculated;
    private bool _engRusGrid;

    private bool IsRussian => _engRusGrid && NixieTubePicker_RemadePicker.IsRussian();

    public UINixieTubeGrid(List<NixieTubeInfo>? workingSet = null, bool engRusGrid = false) {
        Width = new StyleDimension(0f, 1f);
        Height = new StyleDimension(0f, 1f);

        Init(workingSet, engRusGrid);

        SetPadding(0f);
        UpdateEntries();
        FillBestiarySpaceWithEntries();
        Recalculate();

        _calculated = true;
    }

    public void Init(List<NixieTubeInfo>? workingSet, bool engRusGrid = false) {
        _calculated = false;

        _workingButtons = workingSet;
        _engRusGrid = engRusGrid;
    }

    public override void OnInitialize() {
        //Recalculate();
        //SetPadding(0f);
        //UpdateEntries();
        //FillBestiarySpaceWithEntries();
    }

    public void UpdateEntries() {
        if (_workingButtons is null) {
            return;
        }

        _lastEntry = _workingButtons.Count;
    }

    public void FillBestiarySpaceWithEntries() {
        if (_workingButtons is null) {
            return;
        }

        RemoveAllChildren();
        UpdateEntries();
        GetEntriesToShow(out var maxEntriesWidth, out var maxEntriesHeight, out var maxEntriesToHave);
        FixBestiaryRange(0, maxEntriesToHave);
        int atEntryIndex = _atEntryIndex;
        int num = Math.Min(_lastEntry, atEntryIndex + maxEntriesToHave);
        List<NixieTubeInfo> list = new List<NixieTubeInfo>();
        for (int i = atEntryIndex; i < num; i++) {
            list.Add(_workingButtons[i]);
        }

        int num2 = 0;
        float num3 = 0.5f / (float)maxEntriesWidth;
        float num4 = 0.5f / (float)maxEntriesHeight;
        for (int j = 0; j < maxEntriesHeight; j++) {
            for (int k = 0; k < maxEntriesWidth; k++) {
                if (num2 >= list.Count)
                    break;

                UIElement uIElement = new UINixieTubePickButton(list[num2], IsRussian ? 1f : 1f);
                num2++;
                uIElement.VAlign = (uIElement.HAlign = 0.5f);
                uIElement.Left.Set(0f, (float)k / (float)maxEntriesWidth - 0.5f + num3 - (IsRussian ? 0f : 0f));
                uIElement.Top.Set(0f, (float)j / (float)maxEntriesHeight - 0.5f + num4 - (IsRussian ? 0f : 0f));
                uIElement.SetSnapPoint("Entries", num2, new Vector2(0.2f, 0.7f));
                Append(uIElement);
            }
        }
    }

    public void GetEntriesToShow(out int maxEntriesWidth, out int maxEntriesHeight, out int maxEntriesToHave) {
        Rectangle rectangle = GetDimensions().ToRectangle();
        int width = 24, height = 44;
        if (IsRussian) {
            width = (int)(width * 1f);
            height = (int)(height * 1f);
        }
        maxEntriesWidth = rectangle.Width / width;
        maxEntriesHeight = rectangle.Height / height;
        int num = 0;
        maxEntriesToHave = maxEntriesWidth * maxEntriesHeight - num;
    }

    private void FixBestiaryRange(int offset, int maxEntriesToHave) {
        _atEntryIndex = Utils.Clamp(_atEntryIndex + offset, 0, Math.Max(0, _lastEntry - maxEntriesToHave));
    }

    public override void Update(GameTime gameTime) {
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (!_calculated) {
            SetPadding(0f);
            UpdateEntries();
            FillBestiarySpaceWithEntries();

            _calculated = true;
        }

        base.Draw(spriteBatch);
    }
}
