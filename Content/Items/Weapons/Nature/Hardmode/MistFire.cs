using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class MistFire : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
        //PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 32);

        //Item.mana = 12;
        Item.damage = 35;
        Item.useStyle = 5;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<Mist>();
        //Item.width = 26;
        //Item.height = 28;
        Item.useAnimation = Item.useTime = 36;
        Item.autoReuse = true;
        Item.rare = 7;
        Item.noMelee = true;
        Item.knockBack = 1f;
        Item.value = 200000;
        //Item.magic = true;

        Item.noUseGraphic = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
