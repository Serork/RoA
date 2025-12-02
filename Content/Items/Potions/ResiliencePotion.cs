using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class ResiliencePotion : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Resilience Potion");
        //Tooltip.SetDefault("Decreases Wreath discharge speed");
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[4] {
            new Color(108, 145, 210),
            new Color(30, 183, 210),
            new Color(9, 230, 197),
            new Color(135, 255, 203)
        };
    }

    protected override void SafeSetDefaults() {
        int width = 18; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTurn = true;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<Resilience>();
        Item.buffTime = 3600 * 6;

        Item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BuffPotion;
    }
}