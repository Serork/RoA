using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

abstract class Leaves : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.velocity *= 0.1f;
        dust.noGravity = true;
        dust.noLight = true;
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.99f;
        if (dust.scale < 0.5f) {
            dust.alpha += 5;
            if (dust.alpha >= 255) {
                dust.active = false;
            }
        }
        return false;
    }
}

sealed class ShadewoodLeaves : Leaves { }

sealed class EbonwoodLeaves : Leaves { }