using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class WillpowerPotion : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Willpower Potion");
        //Tooltip.SetDefault("Increases Wreath charge speed");
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(221, 82, 24),
            new Color(248, 127, 63),
            new Color(255, 168, 82)
        };
    }

    protected override void SafeSetDefaults() {
        int width = 16; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTurn = true;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<Willpower>();
        Item.buffTime = 3600 * 6;

        Item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BuffPotion;
    }
}