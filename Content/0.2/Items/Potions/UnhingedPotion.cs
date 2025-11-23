using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class UnhingedPotion : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 20;

        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(118, 198, 14),
            new Color(180, 130, 9),
            new Color(255, 254, 135)
        ];
    }

    public override void SetDefaults() {
        Item.width = 20; Item.height = 32;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item3;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.buyPrice();
        Item.buffType = ModContent.BuffType<Unhinged>();
        Item.buffTime = MathUtils.SecondsToFrames(360);
    }
}
