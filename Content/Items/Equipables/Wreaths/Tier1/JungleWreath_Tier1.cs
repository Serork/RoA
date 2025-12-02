using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Tier1;

sealed class JungleWreathTier1 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        //WreathHandler handler = player.GetWreathHandler();

        //float value = 0.1f * handler.ActualProgress4;
        //player.endurance += value;

        //if (handler.IsFull1) {
        //    //if (player.thorns < 1f) player.thorns += 0.5f;
        //    player.GetModPlayer<JungleWreathDamageAttackersHandler>().poisonedSkin = true;
        //}

        DruidStats.InflictPoisonOnNatureDamageWhenCharged(player);
    }

    //private class JungleWreathDamageAttackersHandler : ModPlayer {
    //    public bool poisonedSkin;

    //    public override void ResetEffectsManager() => poisonedSkin = false;

    //    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
    //        if (!poisonedSkin) {
    //            return;
    //        }

    //        int damage = 10;
    //        if (Main.masterMode)
    //            damage = 30;
    //        else if (Main.expertMode)
    //            damage = 20;
    //        damage *= 2;
    //        DruidStats.DamageAttacker(Player, npc, damage, hurtInfo, onDamage: (damageNPC) => {
    //            damageNPC.AddBuff(BuffID.Poisoned, 150, false);
    //        });
    //    }
    //}
}
