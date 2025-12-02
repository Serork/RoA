using RoA.Content.Projectiles.Friendly.Ranged.Ammo;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Ammo;

sealed class MercuriumBullet : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
    }

    public override void SetDefaults() {
        Item.shoot = ModContent.ProjectileType<MercuriumBulletProjectile>();

        Item.shootSpeed = 3f;
        Item.damage = 8;
        Item.knockBack = 7f;

        Item.width = 20;
        Item.height = 20;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.ammo = AmmoID.Bullet;

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 0, 8);
        Item.DamageType = DamageClass.Ranged;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Ammo;
    }
}