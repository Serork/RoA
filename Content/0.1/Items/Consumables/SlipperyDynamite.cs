using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class SlipperyDynamite : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Slippery Dynamite");
        //Tooltip.SetDefault("A large explosion that will destroy most tiles\nSlips through solid tiles");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;

        ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
    }

    public override void SetDefaults() {
        int width = 10; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.damage = 0;
        Item.rare = ItemRarityID.White;

        Item.maxStack = Item.CommonMaxStack;
        Item.noUseGraphic = true;

        Item.UseSound = SoundID.Item1;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 40;

        Item.noMelee = true;
        Item.consumable = true;
        Item.autoReuse = false;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.SlipperyDynamite>();
        Item.shootSpeed = 4f;

        Item.value = Item.sellPrice(0, 0, 10, 0);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Bombs;
}
