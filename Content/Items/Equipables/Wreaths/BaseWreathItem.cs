using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.UI;

using static Terraria.GameContent.Creative.ItemFilters.AAccessories;

namespace RoA.Content.Items.Equipables.Wreaths;

abstract class BaseWreathItem : NatureItem {
    public override void Load() {
        On_ItemFilters.AAccessories.IsAnAccessoryOfType += AAccessories_IsAnAccessoryOfType;
    }

    private bool AAccessories_IsAnAccessoryOfType(On_ItemFilters.AAccessories.orig_IsAnAccessoryOfType orig, ItemFilters.AAccessories self, Item entry, ItemFilters.AAccessories.AccessoriesCategory categoryType) {
        bool flag = entry.ModItem != null && entry.ModItem is BaseWreathItem;
        if (flag && categoryType == AccessoriesCategory.Misc) {
            return true;
        }

        return orig(self, entry, categoryType);
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => modded;

    public sealed override void SetStaticDefaults() {

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    protected sealed override void SafeSetDefaults2() {
        Item.accessory = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)640;
    }
}
