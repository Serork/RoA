using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CorruptedFoliage : ModDust {
    public override void OnSpawn(Dust dust) {
		int width = 24; int height = 24;
		int frame = Main.rand.Next(0, 3);
		dust.frame = new Rectangle(0, frame * height, width, height);

		dust.velocity *= 0.1f;
		dust.noGravity = true;
		dust.noLight = true;
		dust.scale *= 1f;
	}

    public override bool Update(Dust dust) {
		dust.position += dust.velocity;
		dust.alpha += 3;
		if (dust.alpha >= 250) dust.active = false;	
		return false;
	}
}