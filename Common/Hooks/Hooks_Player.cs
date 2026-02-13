using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Content.Items.Equipables.Accessories.Hardmode;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

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
        On_Player.CanVisuallyHoldItem += On_Player_CanVisuallyHoldItem;

        On_Player.LoadPlayer_LastMinuteFixes_Player_Item += On_Player_LoadPlayer_LastMinuteFixes_Player_Item;

        On_Player.UpdateEquips += On_Player_UpdateEquips;
    }

    private void On_Player_UpdateEquips(On_Player.orig_UpdateEquips orig, Player self, int i) {
        if (self.IsLocal() && Main.mouseItem.IsModded(out ModItem modItem) && modItem is RangedWeaponWithCustomAmmo rangedWeaponWithCustomAmmo) {
            rangedWeaponWithCustomAmmo.RecoverAmmo(self);
        }
        if (self.IsLocal() && self.GetCommon().IsChromaticScarfEffectActive && Main.mouseItem.IsAWeapon() && Main.mouseItem.TryGetGlobalItem(out ChromaticScarfDebuffPicker modItem2)) {
            modItem2.UpdateCurrentDebuff();
        }

        orig(self, i);
    }

    private void On_Player_LoadPlayer_LastMinuteFixes_Player_Item(On_Player.orig_LoadPlayer_LastMinuteFixes_Player_Item orig, Player newPlayer, Item item) {
        orig(newPlayer, item);

        if (item.type == ModContent.ItemType<ImmortalSnail>() || item.type == ModContent.ItemType<LifeSpiral>()) {
            newPlayer.pStone = true;
        }
    }

    private bool On_Player_CanVisuallyHoldItem(On_Player.orig_CanVisuallyHoldItem orig, Player self, Item item) {
        if (item.IsNatureClaws(out ClawsBaseItem clawsBaseItem)) {
            if (clawsBaseItem.IsHardmodeClaws) {
                return false;
            }
        }

        return orig(self, item);
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
        if ((self.autoReuseAllWeapons || sItem.autoReuse) && sItem.IsANatureWeapon() && sItem.useStyle == ItemUseStyleID.HiddenAnimation && sItem.shoot == ProjectileID.WoodenArrowFriendly)
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
