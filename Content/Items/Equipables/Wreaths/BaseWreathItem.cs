using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

abstract class BaseWreathItem : ModItem {
    public override bool CanEquipAccessory(Player player, int slot, bool modded) => modded;

    public sealed override void SetStaticDefaults() {

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public sealed override void SetDefaults() {
        Item.accessory = true;

        SafeSetDefaults();
    }

    protected virtual void SafeSetDefaults() { }
}
