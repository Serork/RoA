using RoA.Common.Druid;
using RoA.Core.Defaults;

using Terraria;
using Terraria.GameContent.Prefixes;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class CrystalVileShard : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);

        //Item.mana = 13;
        Item.damage = 25;
        Item.useStyle = 5;
        Item.shootSpeed = 32f;
        Item.shoot = 494;
        //Item.width = 26;
        //Item.height = 28;
        Item.useAnimation = 33;
        Item.useTime = 33;
        Item.rare = 5;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(0, 8);
        //Item.magic = true;
        Item.autoReuse = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
