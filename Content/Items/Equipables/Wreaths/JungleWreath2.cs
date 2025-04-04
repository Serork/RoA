using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class JungleWreath2 : BaseWreathItem {
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
            player.GetModPlayer<JungleWreathPlayer2>().poisonedSkin2 = true;
        }
    }
}

sealed class JungleWreathPlayer2 : ModPlayer {
    public bool poisonedSkin2;

    public override void ResetEffects() => poisonedSkin2 = false;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (poisonedSkin2) npc.AddBuff(BuffID.Poisoned, 300, false);
		
		if (Player.thorns < 1f) Player.thorns += 0.5f;

            float num2 = Player.thorns;
            Rectangle rectangle = new Rectangle((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height);
            Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
            bool num = Player.CanParryAgainst(rectangle, npcRect, npc.velocity);
            float knockback = 10f;
            if (Player.turtleThorns)
                num2 = 2f;

            if (num) {
                num2 = 2f;
                knockback = 5f;
            }
            if (Player.whoAmI == Main.myPlayer && num2 > 0f && !npc.dontTakeDamage) {
                int damage = 15;
                if (Main.masterMode)
                    damage = 45;
                else if (Main.expertMode)
                    damage = 30;

                Player.ApplyDamageToNPC(npc, damage, knockback, -hurtInfo.HitDirection, crit: false);
            }
    }
}