using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class JungleWreath : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetModPlayer<WreathHandler>();
        
		float value = 0.1f * handler.ActualProgress4;
        player.endurance += value;
        
		if (handler.IsFull1) {
            //if (player.thorns < 1f) player.thorns += 0.5f;
            player.GetModPlayer<JungleWreathPlayer>().poisonedSkin = true;
        }
    }
}

sealed class JungleWreathPlayer : ModPlayer {
    public bool poisonedSkin;

    public override void ResetEffects() => poisonedSkin = false;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (!poisonedSkin) {
            return;
        }

        int damage = 10;
        if (Main.masterMode)
            damage = 30;
        else if (Main.expertMode)
            damage = 20;
        damage *= 2;
        DruidStats.DamageAttacker(Player, npc, damage, hurtInfo, onDamage: (damageNPC) => {
            damageNPC.AddBuff(BuffID.Poisoned, 150, false);
        });
    }
}

sealed class JungleWreath2 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetModPlayer<WreathHandler>();

        float value = 0.1f * handler.ActualProgress4;
        player.endurance += value;

        if (handler.IsFull1) {
            //if (player.thorns < 1f) player.thorns += 0.5f;
            player.GetModPlayer<JungleWreathPlayer2>().poisonedSkin2 = true;
        }
    }
}

sealed class JungleWreathPlayer2 : ModPlayer {
    public bool poisonedSkin2;

    public override void ResetEffects() => poisonedSkin2 = false;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (!poisonedSkin2) {
            return;
        }

        int damage = 15;
        if (Main.masterMode)
            damage = 45;
        else if (Main.expertMode)
            damage = 30;
        damage *= 2;
        DruidStats.DamageAttacker(Player, npc, damage, hurtInfo, onDamage: (damageNPC) => {
            damageNPC.AddBuff(BuffID.Poisoned, 150, false);
        });
    }
}