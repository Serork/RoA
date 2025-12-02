using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Tier2;

sealed class JungleWreathTier2 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        //WreathHandler handler = player.GetWreathHandler();

        //float value = 0.1f * handler.ActualProgress4;
        //player.endurance += value;

        //if (handler.IsFull1) {
        //    //if (player.thorns < 1f) player.thorns += 0.5f;
        //    player.GetModPlayer<JungleWreath2DamageAttackersHandler>().poisonedSkin2 = true;
        //}

        DruidStats.ApplyUpTo5ReducedDamageTaken(player);

        DruidStats.InflictPoisonOnNatureDamageWhenCharged(player);
    }

    //private class JungleWreath2DamageAttackersHandler : ModPlayer {
    //    public bool poisonedSkin2;

    //    public override void ResetEffectsManager() => poisonedSkin2 = false;

    //    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
    //        if (!poisonedSkin2) {
    //            return;
    //        }

    //        int damage = 15;
    //        if (Main.masterMode)
    //            damage = 45;
    //        else if (Main.expertMode)
    //            damage = 30;
    //        damage *= 2;
    //        DruidStats.DamageAttacker(Player, npc, damage, hurtInfo, onDamage: (damageNPC) => {
    //            damageNPC.AddBuff(BuffID.Poisoned, 150, false);
    //        });
    //    }
    //}
}