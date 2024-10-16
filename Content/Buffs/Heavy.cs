using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Heavy : ModBuff {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Heavy");
		//Description.SetDefault("Press DOWN to increase falling speed");
	}

	public override void Update(Player player, ref int buffIndex) {
		player.noFallDmg = true;
		player.maxFallSpeed += 6;
		if (player.velocity.Y > 0) {
			player.velocity.Y *= 1.035f;
            if (player.controlDown) player.velocity.Y *= 1.035f;

        }
	}
}