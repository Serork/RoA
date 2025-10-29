using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    public override bool InstancePerEntity => true;

    public override void SetStaticDefaults() {
        VanillaSkullSetStaticDefaults();

        TarEnchantmentSetStaticDefaults();
    }

    public partial void TarEnchantmentSetStaticDefaults();
    public partial void VanillaSkullSetStaticDefaults();

    public override void Load() {
        VanillaSkullLoad();

        TarEnchantmentLoad();
    }

    public partial void TarEnchantmentLoad();
    public partial void VanillaSkullLoad();
}
