using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ShadewoodLeaves : ModDust {
	public override void OnSpawn(Dust dust) {
		dust.velocity *= 0.1f;
		dust.noGravity = true;
		dust.noLight = true;
		dust.scale *= 1f;
	}

	public override bool Update(Dust dust) {
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.15f;
		dust.scale *= 0.99f;
		if (dust.scale < 0.5f) {
			dust.active = false;
		}
		return false;
	}
}