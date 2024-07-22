using RoA.Core;
using RoA.Core.Utility;

using Terraria.ModLoader;

namespace RoA.Content.Items;

abstract class NatureItem : ModItem {
    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Item.IsAWeapon()) {
            Item.SetDefaultToDruidicWeapon();
        }

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
}
