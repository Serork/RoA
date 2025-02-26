using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Weapons.Melee;
using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.CustomConditions;

static class RoAConditions {
    public static Condition InBackwoods = new("Mods.RoA.Conditions.BackwoodsBiome", () => Main.LocalPlayer.InModBiome<BackwoodsBiome>());
    public static Condition HasAnySaddle = new("Mods.RoA.Conditions.HasSaddleCondition", 
        () => {
            bool flag = false;
            int[] saddles = [ItemID.DarkHorseSaddle, ItemID.MajesticHorseSaddle, ItemID.PaintedHorseSaddle];
            Player player = Main.LocalPlayer;
            if (player.inventory.Any(x => !x.IsEmpty() && saddles.Contains(x.type)) ||
                saddles.Contains(player.miscEquips[3].type)) {
                flag = true;
            }
            return flag;
        });

    public static Condition Has05LuckOrMore = new("Mods.RoA.Conditions.HasSaddleCondition",
        () => {
            bool flag = false;
            if (Main.LocalPlayer.luck >= 0.5f) {
                flag = true;
            }
            return flag;
        });

    public static Condition SailorHatCondition = new("Mods.RoA.Conditions.SailorHatCondition",
        () => {
            bool flag = false;
            Player player = Main.LocalPlayer;
            int[] golfBalls = [3989, 4242, 4243, 4244, 4245, 4246, 4247, 4248, 4249, 4250, 4251, 4252, 4253, 4254, 4255];
            bool flag2 = false;
            foreach (int type in golfBalls) {
                if (player.HasItemInInventoryOrOpenVoidBag(type)) {
                    flag2 = true;
                    break;
                }
                if (player.HasItem(type, player.armor)) {
                    flag2 = true;
                    break;
                }
            }
            if (player.HasItemInInventoryOrOpenVoidBag(ItemID.Snowball) &&
                player.HasItemInInventoryOrOpenVoidBag(ItemID.SpikyBall) &&
                flag2 &&
                player.HasItemInInventoryOrOpenVoidBag(ItemID.MusketBall)) {
                flag = true;
            }
            return flag;
        });

    public static readonly IItemDropRule ShouldDropFlederSlayer = ItemDropRule.ByCondition(new FlederSlayerDropCondition(), ModContent.ItemType<FlederSlayer>());

}
