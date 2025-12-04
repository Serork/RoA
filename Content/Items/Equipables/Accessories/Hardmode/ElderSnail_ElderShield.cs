using RoA.Content.Dusts;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Shield)]
sealed class ElderShield : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(30, 44);

        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice(0, 1));

        Item.defense = 6;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsElderShellEffectActive = true;
        player.GetCommon().IsElderShieldEffectActive = true;

        player.noKnockback = true;
        if ((float)player.statLife > (float)player.statLifeMax2 * 0.25f) {
            player.hasPaladinShield = true;
            if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0) {
                int myPlayer = Main.myPlayer;
                if (Main.player[myPlayer].team == player.team && player.team != 0) {
                    float num = player.position.X - Main.player[myPlayer].position.X;
                    float num2 = player.position.Y - Main.player[myPlayer].position.Y;
                    if ((float)Math.Sqrt(num * num + num2 * num2) < 800f)
                        Main.player[myPlayer].AddBuff(BuffID.PaladinsShield, 20);
                }
            }
        }
    }
}
