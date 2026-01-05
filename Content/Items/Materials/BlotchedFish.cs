using ModLiquidLib.ModLoader;

using RoA.Common.Players;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Liquids;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class BlotchedFish : ModItem {
    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 30;
        Item.height = 34;
        Item.value = Item.sellPrice(0, 0, 5);
    }
}
