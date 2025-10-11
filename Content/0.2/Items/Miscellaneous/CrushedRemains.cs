using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Miscellaneous;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class CrushedRemains : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ExtractinatorMode[Type] = Type;
    }

    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;
        int width = 36, height = 24;
        Item.Size = new Vector2(width, height);
        Item.rare = ItemRarityID.Blue;
    }

    public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) { // Calls upon use of an extractinator. Below is the chance you will get ExampleOre from the extractinator.
        float chance = Main.rand.Next(10);
        if (chance < 3) {
            resultType = ItemID.Bone;
            resultStack = Main.rand.Next(1, 6);
        }
        else if (chance < 6) {
            resultType = ItemID.CopperCoin;
            resultStack = Main.rand.Next(1, 101);
        }
        else if (chance < 8) {
            resultType = ItemID.SilverCoin;
            resultStack = Main.rand.Next(1, 101);
        }
        else if (chance < 10) {
            resultStack = 1;
            if (Main.rand.NextBool()) {
                resultType = ModContent.ItemType<CarcassChestguard>();
                return;
            }
            resultType = ModContent.ItemType<CarcassSandals>();
        }
    }
}
