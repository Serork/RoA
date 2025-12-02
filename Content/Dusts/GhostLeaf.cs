using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class GhostLeaf : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, Main.rand.NextBool() ? 0 : 16, 24, 16);
        dust.velocity *= 0.2f;
        dust.noGravity = true;
        dust.noLight = true;
        dust.scale *= 1.2f;
    }

    public override bool Update(Dust dust) {
        Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.alpha += 5;
        dust.scale *= 0.99f;
        float light = 0.4f * dust.scale;
        Lighting.AddLight(dust.position, light * 0.1f, light * 0.2f, light * 0.6f);
        if (dust.alpha >= 250) {
            dust.active = false;
        }
        return false;
    }
}