using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class GaeanBiedermeier : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(-1, 40, autoReuse: true, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<BiedermeierFlower>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());
        Item.UseSound = SoundID.Item65 with { Volume = 3f, Pitch = -1f };

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
