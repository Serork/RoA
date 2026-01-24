using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Food;

sealed class MysteryCake : ModItem {
    public override void SetStaticDefaults() {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] = [
            new Color(230, 218, 211),
            new Color(200, 189, 182),
            new Color(189, 53, 0),
            new Color(135, 26, 16)
        ];
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(28, 28, BuffID.WellFed3, 43200);
        Item.SetShopValues(ItemRarityColor.Orange3, Item.buyPrice(0, 1));
    }
}
