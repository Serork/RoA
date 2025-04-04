using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class JungleWreath : BaseWreathItem {
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
            player.GetModPlayer<JungleWreathPlayer>().poisonedSkin = true;
        }
    }
}

sealed class JungleWreathPlayer : ModPlayer {
    public bool poisonedSkin;

    public override void ResetEffects() => poisonedSkin = false;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (poisonedSkin) npc.AddBuff(BuffID.Poisoned, 150, false);
		
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
                int damage = 10;
                if (Main.masterMode)
                    damage = 30;
                else if (Main.expertMode)
                    damage = 20;

                Player.ApplyDamageToNPC(npc, damage, knockback, -hurtInfo.HitDirection, crit: false);
            }
    }
}