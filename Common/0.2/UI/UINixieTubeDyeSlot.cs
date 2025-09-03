﻿using Newtonsoft.Json.Linq;

using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubeDyeSlot : ItemSlotUIElement {
    public bool First { get; init; }

    protected override string HoverText => First ? Language.GetTextValue("Mods.RoA.NixieTubeDyeSlot1") : Language.GetTextValue("Mods.RoA.NixieTubeDyeSlot2");

    public UINixieTubeDyeSlot(bool first, int context = ItemSlot.Context.InventoryItem, float scale = 1f) : base(context, scale) {
        First = first;
    }

    protected override Item? GetStoredItem {
        get {
            if (First) {
                return NixieTubePicker_RemadePicker.GetTE()?.Dye1;
            }
            else {
                return NixieTubePicker_RemadePicker.GetTE()?.Dye2;
            }
        }
        set {
            if (First) {
                NixieTubePicker_RemadePicker.GetTE()?.SetDye1(value!);
            }
            else {
                NixieTubePicker_RemadePicker.GetTE()?.SetDye2(value!);
            }
        }
    }
}
