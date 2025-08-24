using Terraria;

namespace RoA.Core.Utility;

static class DustHelper {
    public static void ApplyDustScale(this Dust dust) {
        if (dust.position.Y > Main.screenPosition.Y + (float)Main.screenHeight)
            dust.active = false;

        float num113 = 0.1f;
        if ((double)Dust.dCount == 0.5)
            dust.scale -= 0.001f;

        if ((double)Dust.dCount == 0.6)
            dust.scale -= 0.0025f;

        if ((double)Dust.dCount == 0.7)
            dust.scale -= 0.005f;

        if ((double)Dust.dCount == 0.8)
            dust.scale -= 0.01f;

        if ((double)Dust.dCount == 0.9)
            dust.scale -= 0.02f;

        if ((double)Dust.dCount == 0.5)
            num113 = 0.11f;

        if ((double)Dust.dCount == 0.6)
            num113 = 0.13f;

        if ((double)Dust.dCount == 0.7)
            num113 = 0.16f;

        if ((double)Dust.dCount == 0.8)
            num113 = 0.22f;

        if ((double)Dust.dCount == 0.9)
            num113 = 0.25f;

        if (dust.scale < num113)
            dust.active = false;
    }

    public static void BasicDust(this Dust dust, bool applyGravity = true) {
        ApplyDustScale(dust);

        if (applyGravity && !dust.noGravity) {
            dust.velocity.Y += 0.1f;
        }
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.5f;
        if (dust.fadeIn > 0f && dust.fadeIn < 100f) {
            dust.scale += 0.03f;
            if (dust.scale > dust.fadeIn)
                dust.fadeIn = 0f;
        }
        else {
            dust.scale -= 0.01f;
        }

        if (dust.noGravity) {
            dust.velocity *= 0.92f;
            if (dust.fadeIn == 0f)
                dust.scale -= 0.04f;
        }
    }
}
