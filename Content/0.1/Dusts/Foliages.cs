using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

abstract class FolliageDust : ModDust {
    public override void OnSpawn(Dust dust) {
        int width = 26; int height = 24;
        int frame = Main.rand.Next(0, 3);
        dust.frame = new Rectangle(0, frame * height, width, height);

        dust.velocity *= 0.1f;
        dust.noGravity = true;
        dust.noLight = true;
        dust.scale *= 0.8f;
    }

    public override bool Update(Dust dust) {
        //Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        dust.position += dust.velocity;
        dust.rotation += 0.05f * dust.velocity.X;
        if (Main.rand.Next(3) <= 1) {
            dust.velocity *= 0.995f - Main.rand.NextFloat(0.001f, 0.1f);
        }
        if (dust.fadeIn > 0f) {
            dust.alpha = (int)(255 * dust.fadeIn);
            dust.fadeIn -= 0.1f;
        }
        else {
            dust.alpha += 2;
            if (dust.alpha >= 250) {
                dust.active = false;
            }
        }
        return false;
    }
}

sealed class CrimsonFoliage : FolliageDust { }
sealed class CorruptedFoliage : FolliageDust { }