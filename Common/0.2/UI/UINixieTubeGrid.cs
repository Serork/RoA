using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.UI;

namespace RoA.Common.UI;

readonly record struct NixieTubeInfo(byte Index) {
}

sealed class UINixieTubeGrid : UIElement {
    private List<NixieTubeInfo> _workingButtons;
    private int _atEntryIndex;
    private int _lastEntry;
    private bool _calculated;

    public UINixieTubeGrid(List<NixieTubeInfo> workingSet) {
        Width = new StyleDimension(0f, 1f);
        Height = new StyleDimension(0f, 1f);
        _workingButtons = workingSet;
    }

    public override void OnInitialize() {
        Recalculate();
        SetPadding(0f);
        UpdateEntries();
        FillBestiarySpaceWithEntries();
    }

    public void UpdateEntries() {
        _lastEntry = _workingButtons.Count;
    }

    public void FillBestiarySpaceWithEntries() {
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

                UIElement uIElement = new UINixieTubePickButton(list[num2]);
                num2++;
                uIElement.VAlign = (uIElement.HAlign = 0.5f);
                uIElement.Left.Set(0f, (float)k / (float)maxEntriesWidth - 0.5f + num3);
                uIElement.Top.Set(0f, (float)j / (float)maxEntriesHeight - 0.5f + num4);
                uIElement.SetSnapPoint("Entries", num2, new Vector2(0.2f, 0.7f));
                Append(uIElement);
            }
        }
    }

    public void GetEntriesToShow(out int maxEntriesWidth, out int maxEntriesHeight, out int maxEntriesToHave) {
        Rectangle rectangle = GetDimensions().ToRectangle();
        maxEntriesWidth = rectangle.Width / 24;
        maxEntriesHeight = rectangle.Height / 44;
        int num = 0;
        maxEntriesToHave = maxEntriesWidth * maxEntriesHeight - num;
    }

    private void FixBestiaryRange(int offset, int maxEntriesToHave) {
        _atEntryIndex = Utils.Clamp(_atEntryIndex + offset, 0, Math.Max(0, _lastEntry - maxEntriesToHave));
    }

    public override void Update(GameTime gameTime) {
        if (_calculated) {
            return;
        }

        SetPadding(0f);
        UpdateEntries();
        FillBestiarySpaceWithEntries();

        _calculated = true;
    }
}
