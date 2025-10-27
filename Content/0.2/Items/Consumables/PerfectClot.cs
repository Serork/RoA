using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class PerfectClot : ModItem {
    public override void SetDefaults() {
        Item.useStyle = 4;
        Item.consumable = true;
        Item.useAnimation = 45;
        Item.useTime = 45;
        Item.UseSound = SoundID.Item92;
        Item.width = 18;
        Item.height = 34;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 2;
    }

    public override bool? UseItem(Player player) {
        ref bool activated = ref player.GetCommon().PerfectClotActivated;
        if (player.ItemAnimationJustStarted && !activated) {
            activated = true;

            Item.stack--;
            if (Item.stack <= 0) {
                Item.active = false;
                Item.TurnToAir();
            }
        }

        return base.UseItem(player);
    }
}
