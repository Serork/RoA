using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class PrismaticPotion : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[4] {
            new Color(169, 226, 232),
            new Color(218, 62, 184),
            new Color(195, 17, 79),
            new Color(88, 111, 156)
        };
    }

    public override void SetDefaults() {
        int width = 20; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTurn = true;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<PrismaticFavor>();
        Item.buffTime = 3600 * 4;

        Item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BuffPotion;
    }
}