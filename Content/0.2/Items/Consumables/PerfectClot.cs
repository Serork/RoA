using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class PerfectClot : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

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

    public override void Update(ref float gravity, ref float maxFallSpeed) {
        Item.velocity.Y = MathF.Abs(Item.velocity.Y) * -1f;
        Item.velocity.X *= 0.925f;
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
