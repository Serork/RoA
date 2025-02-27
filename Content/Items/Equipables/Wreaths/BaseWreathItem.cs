using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

abstract class BaseWreathItem : NatureItem {
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
