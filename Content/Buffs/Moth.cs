using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Moth : ModBuff {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Moth");
		//Description.SetDefault("The moth will fight for you");
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex) {

	}
}