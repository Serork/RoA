using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class DeathWardPotion : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Death Ward Potion");
        // Tooltip.SetDefault("Prevents death" + "\nMakes you potion sick for 60 seconds" + "\n'Rests in the hands of its master who sent it'");

        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(178, 34, 34),
            new Color(220, 20, 60),
            new Color(139, 0, 0)
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

        Item.buffType = ModContent.BuffType<DeathWard>();
        Item.buffTime = 3600;

        Item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public override void OnConsumeItem(Player player) => player.AddBuff(BuffID.PotionSickness, 3600);

    public override bool CanUseItem(Player player) {
        if (player.HasBuff(BuffID.PotionSickness)) {
            return false;
        }
        return true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BuffPotion;
    }
}