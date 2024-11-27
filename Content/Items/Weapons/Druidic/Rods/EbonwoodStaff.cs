using RoA.Common.Druid;
using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class EbonwoodStaff : BaseRodItem<EbonwoodStaff.EbonwoodStaffBase> {
    protected override void SafeSetDefaults() {
        Item.SetSize(44);
        Item.SetDefaultToUsable(-1, 22, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.25f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class EbonwoodStaffBase : BaseRodProjectile {
    }
}