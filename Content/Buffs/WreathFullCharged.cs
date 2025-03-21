﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class WreathFullCharged : ModBuff {
	public override void SetStaticDefaults() {
		Color color = Color.LightGreen;
		// DisplayName.SetDefault($"Wreath Charge [c/{color.ConvertToHex()}:II]");
		// Description.SetDefault("Nature damage is at full potential\nProvides full charge bonuses of equipped Wreath");

		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;
		Main.debuff[Type] = true;
	}
}