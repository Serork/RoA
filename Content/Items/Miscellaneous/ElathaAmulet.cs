using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class ElathaAmulet : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Elatha Scepter");
		//Tooltip.SetDefault("Changes the phases of the Moon");
	}

	public override void SetDefaults() {
		Item.Size = new Vector2(16, 40);
        Item.rare = ItemRarityID.Green;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.reuseDelay = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item28;
        Item.mana = 30;
	}

	public override bool CanUseItem(Player player) {
		if (player.statMana >= 30) {
            Main.moonPhase++;
            if (Main.moonPhase > 7) {
                Main.moonPhase = 0;
            }
        }
		return true;
	}
}
