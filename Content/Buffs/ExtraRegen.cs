using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class ExtraRegen : ModBuff {
	private int _healTimer;

	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Extra Regen");
		//Description.SetDefault("Slowly recovering from injuries");
	}

	public override void Update(Player player, ref int buffIndex) {
		_healTimer++;
		if (_healTimer % 60 == 0) {
			player.statLife += 2;
			player.HealEffect(2);
		}
	}
}