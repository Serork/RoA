using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class SnakeIdol : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsSnakeIdolEffectActive = true;

        int debuffCount = 0;
        for (int i = 0; i < Player.MaxBuffs; i++) {
            if (player.buffTime[i] >= 1 && player.buffType[i] > 0) {
                if (Main.debuff[player.buffType[i]] || Main.pvpBuff[player.buffType[i]]) {
                    debuffCount++;
                }
            }
        }
        int defenseDecrease = 5 * debuffCount;
        player.statDefense -= defenseDecrease;
    }
}
