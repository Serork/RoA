using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class Hedgehog : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Hedgehog");
        // Tooltip.SetDefault("'Spiky, but cute'");

        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults() {
        int width = 28; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.maxStack = Item.CommonMaxStack;
        Item.noUseGraphic = true;
        Item.value = Item.sellPrice(silver: 20);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 20;
        Item.noMelee = true;
        Item.consumable = true;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.Hedgehog>();
        Item.shootSpeed = 3f;

        Item.UseSound = SoundID.Item7;

        Item.value = Item.sellPrice(0, 0, 10, 0);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Critters;
    }
}