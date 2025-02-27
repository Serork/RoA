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
		Item.value = Item.sellPrice(gold: 1);
		Item.rare = ItemRarityID.Blue;
	}

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.GetModPlayer<WreathHandler>().IsFull) {
            player.GetModPlayer<JungleWreathPlayer>().poisonedSkin = true;
        }
    }
}

sealed class JungleWreathPlayer : ModPlayer {
    public bool poisonedSkin;

    public override void ResetEffects() => poisonedSkin = false;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (poisonedSkin) npc.AddBuff(BuffID.Poisoned, 300, false);
    }
}
