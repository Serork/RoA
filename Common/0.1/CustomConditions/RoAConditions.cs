using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Equipables.Vanity.Developer;
using RoA.Content.Items.Weapons.Melee;
using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.CustomConditions;

static class RoAConditions {
    public static string ConditionLang => "Mods.RoA.Conditions.";

    private static bool HasDevItem(params int[] devSetItems) {
        bool result = false;
        Player player = Main.LocalPlayer;
        foreach (int devItem in devSetItems) {
            if (player.HasItemInInventoryOrOpenVoidBag(devItem)) {
                result = true;
                break;
            }
        }
        if (player.armor.Any(x => !x.IsEmpty() && devSetItems.Contains(x.type))) {
            result = true;
        }
        return result;
    }

    public static Condition HasPeegeonSet = new("Mods.RoA.Conditions.HasPeegeonSet",
        () => {
            return HasDevItem(ModContent.ItemType<PeegeonChestguard>(), ModContent.ItemType<PeegeonGreaves>(), ModContent.ItemType<PeegeonHood>());
        });

    public static Condition HasNFASet = new("Mods.RoA.Conditions.HasNFASet",
        () => {
            return HasDevItem(ModContent.ItemType<NFAHorns>(), ModContent.ItemType<NFAJacket>(), ModContent.ItemType<NFAPants>());
        });

    public static Condition HasHas2rSet = new("Mods.RoA.Conditions.HasHas2rSet",
        () => {
            return HasDevItem(ModContent.ItemType<Has2rJacket>(), ModContent.ItemType<Has2rMask>(), ModContent.ItemType<Has2rPants>(), ModContent.ItemType<Has2rShades>());
        });

    public static Condition LothorEnrageMonolith = new("Mods.RoA.Conditions.LothorEnrageMonolith", () => Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(ModContent.ItemType<FlederSlayer>()));
    public static Condition InBackwoods = new("Mods.RoA.Conditions.BackwoodsBiome", () => Main.LocalPlayer.InModBiome<BackwoodsBiome>());
    public static Condition HasAnySaddle = new("Mods.RoA.Conditions.HasSaddle",
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

    public static Condition Has05LuckOrMore = new("Mods.RoA.Conditions.HasSaddle",
        () => {
            bool flag = false;
            if (Main.LocalPlayer.luck >= 0.5f) {
                flag = true;
            }
            return flag;
        });

    public static Condition SailorHatCondition = new("Mods.RoA.Conditions.SailorHat",
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
