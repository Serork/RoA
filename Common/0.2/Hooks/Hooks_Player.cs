using RoA.Common.Players;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.UI;

using static Terraria.GameContent.Creative.ItemFilters.AAccessories;

namespace RoA.Common.Hooks;

sealed partial class Hooks : ModSystem {
    public partial void LoadPlayerHooks() {
        On_Player.TryAllowingItemReuse += On_Player_TryAllowingItemReuse;
        On_ItemSlot.AccCheck_ForLocalPlayer += On_ItemSlot_AccCheck_ForLocalPlayer;
        On_ItemFilters.AAccessories.IsAnAccessoryOfType += AAccessories_IsAnAccessoryOfType;
    }

    private bool AAccessories_IsAnAccessoryOfType(On_ItemFilters.AAccessories.orig_IsAnAccessoryOfType orig, ItemFilters.AAccessories self, Item entry, ItemFilters.AAccessories.AccessoriesCategory categoryType) {
        bool flag = entry.ModItem != null && entry.ModItem is WreathItem;
        if (flag && categoryType == AccessoriesCategory.Misc) {
            return true;
        }

        return orig(self, entry, categoryType);
    }

    private void On_Player_TryAllowingItemReuse(On_Player.orig_TryAllowingItemReuse orig, Player self, Item sItem) {
        orig(self, sItem);

        bool flag = false;
        if (self.autoReuseAllWeapons && sItem.IsANatureWeapon() && sItem.useStyle == ItemUseStyleID.HiddenAnimation && sItem.shoot == ProjectileID.WoodenArrowFriendly)
            flag = true;

        if (flag)
            self.releaseUseItem = true;
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
                var wreathSlot = WreathSlot.GetSlot(Main.LocalPlayer);
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
}
