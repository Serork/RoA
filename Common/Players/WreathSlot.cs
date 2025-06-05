﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.InterfaceElements;
using RoA.Common.Items;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace RoA.Common.Players;

class WreathSlot3 : WreathSlot {
    public override bool IsHidden() => IsHiddenBase() || Main.LocalPlayer.CurrentLoadoutIndex != 2;
}

class WreathSlot2 : WreathSlot {
    public override bool IsHidden() => IsHiddenBase() || Main.LocalPlayer.CurrentLoadoutIndex != 1;
}

class WreathSlot : ModAccessorySlot {
    private static bool _equipped, _equipped2;

    public static int ActiveSlot => ModContent.GetInstance<WreathSlot>().Type;

    public static WreathSlot GetSlot(Player player) {
        return player.CurrentLoadoutIndex switch {
            1 => LoaderManager.Get<AccessorySlotLoader>().Get(ModContent.GetInstance<WreathSlot2>().Type, player) as WreathSlot2,
            2 => LoaderManager.Get<AccessorySlotLoader>().Get(ModContent.GetInstance<WreathSlot3>().Type, player) as WreathSlot3,
            _ => LoaderManager.Get<AccessorySlotLoader>().Get(ModContent.GetInstance<WreathSlot>().Type, player) as WreathSlot,
        };
    }
    public static Item GetFunctionalItem(Player player) => GetSlot(player).FunctionalItem;
    public static Item GetVanityItem(Player player) => GetSlot(player).VanityItem;
    public static Item GetDyeItem(Player player) => GetSlot(player).DyeItem;
    public static bool GetHideVisuals(Player player) => GetSlot(player).HideVisuals;

    public static Vector2 GetCustomLocation() {
        int mapH = ((FancyWreathDrawing.MapEnabled || Main.mapEnabled) && !Main.mapFullscreen && Main.mapStyle == 1) ? 256 : 0;
        mapH = ((FancyWreathDrawing.MapEnabled || Main.mapEnabled) && (mapH + 600) > Main.screenHeight) ? Main.screenHeight - 600 : mapH;
        int offsetX = 47 * 2;
        int x = (Main.netMode == NetmodeID.MultiplayerClient) ? Main.screenWidth - 135 - offsetX - 47 : Main.screenWidth - 94 - offsetX + 2 - 64;
        int y = mapH + 174;

        return new Vector2(x, y);
    }

    public override void ApplyEquipEffects() {
        if (!IsHidden()) {
            base.ApplyEquipEffects();
        }
    }

    public override Vector2? CustomLocation => GetCustomLocation();

    protected bool IsHiddenBase() => (!IsItemValidForSlot(Main.mouseItem) && !BeltButton.IsUsed) || Main.EquipPage >= 1;

    public override bool IsHidden() => IsHiddenBase() || Main.LocalPlayer.CurrentLoadoutIndex != 0;

    public override string FunctionalTexture => ResourceManager.UITextures + "Wreath_SlotBackground";

    public override void Load() {
        On_ItemSlot.AccCheck_ForLocalPlayer += On_ItemSlot_AccCheck_ForLocalPlayer;
    }

    private bool On_ItemSlot_AccCheck_ForLocalPlayer(On_ItemSlot.orig_AccCheck_ForLocalPlayer orig, Item[] itemCollection, Item item, int slot) {
        if (ItemSlot.isEquipLocked(item.type))
            return true;

        if (slot != -1) {
            if (itemCollection[slot].type == item.type)
                return false;

            if (itemCollection[slot].wingSlot > 0 && item.wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(itemCollection[slot], item))
                return !ItemLoader.CanEquipAccessory(item, slot % 20, slot >= 20);
        }

        var modSlotPlayer = Main.LocalPlayer.GetModPlayer<ModAccessorySlotPlayer>();
        var modCount = modSlotPlayer.SlotCount;
        bool targetVanity = slot >= 20 && (slot >= modCount + 20) || slot < 20 && slot >= 10;

        for (int i = targetVanity ? 13 : 3; i < (targetVanity ? 20 : 10); i++) {
            if (!targetVanity && item.wingSlot > 0 && itemCollection[i].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(itemCollection[i], item))
                return true;
        }

        for (int i = (targetVanity ? modCount : 0) + 20; i < (targetVanity ? modCount * 2 : modCount) + 20; i++) {
            if (!targetVanity && item.wingSlot > 0 && itemCollection[i].wingSlot > 0 || !ItemLoader.CanAccessoryBeEquippedWith(itemCollection[i], item))
                return true;
        }

        for (int i = 0; i < itemCollection.Length; i++) {
            if (item.type == itemCollection[i].type) {
                var wreathSlot = GetSlot(Main.LocalPlayer);
                if (wreathSlot.FunctionalItem.type == item.type ||
                    wreathSlot.VanityItem.type == item.type) {
                    return true;
                }
                if (item.ModItem != null && item.ModItem is WreathItem) {
                    continue;
                }
                return true;
            }
        }

        return !ItemLoader.CanEquipAccessory(item, slot % 20, slot >= 20);
    }

    public override bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) {
        if (isHovered) {
            _equipped = !FunctionalItem.IsEmpty() || !VanityItem.IsEmpty() || !DyeItem.IsEmpty();
            if (_equipped && !_equipped2) {
                BeltButton.ToggleTo(true);
            }
        }

        if (context == AccessorySlotType.FunctionalSlot) {
            Item[] items = [FunctionalItem];
            MannequinWreathSlotSupport.Draw(Main.spriteBatch, items, 8, 0, position, mainTexture: ModContent.Request<Texture2D>(FunctionalTexture).Value);

            return false;
        }

        if (context == AccessorySlotType.VanitySlot) {
            Item[] items = [VanityItem];
            MannequinWreathSlotSupport.Draw(Main.spriteBatch, items, 11, 0, position);

            return false;
        }

        if (context == AccessorySlotType.DyeSlot) {
            Item[] items = [DyeItem];
            MannequinWreathSlotSupport.Draw(Main.spriteBatch, items, 12, 0, position);

            return false;
        }

        return true;
    }

    public override void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) {
        if (isHovered) {
            _equipped2 = _equipped;
            _equipped = false;
        }
    }

    public override void OnMouseHover(AccessorySlotType context) {
        string slotName = Language.GetTextValue("Mods.RoA.WreathSlot");
        string vanitySlotName = Language.GetTextValue("Mods.RoA.SocialWreathSlot");
        switch (context) {
            case AccessorySlotType.FunctionalSlot:
                Main.hoverItemName = slotName;
                break;
            case AccessorySlotType.VanitySlot:
                Main.hoverItemName = vanitySlotName;
                break;
        }
    }

    public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
        bool result = IsItemValidForSlot(checkItem);
        return result;
    }

    public static bool IsItemValidForSlot(Item item) => item.ModItem is WreathItem;

    public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
        if (IsHidden()) {
            return false;
        }
        bool result = IsItemValidForSlot(item);
        if (result) {
            BeltButton.ToggleTo(true);
        }
        return result;
    }
}
