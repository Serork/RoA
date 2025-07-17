using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Tulip : ModDust {
    private Vector2 _velocity;

    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor;

    public override void OnSpawn(Dust dust) {
        int maxFramesY = 3;
        dust.frame = (Texture2D?.Value?.Frame(5, maxFramesY, frameX: dust.alpha, frameY: Main.rand.Next(maxFramesY))).GetValueOrDefault();

        dust.noGravity = true;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        if (dust.customData is null) {
            return false;
        }

        float randomness = (float)dust.customData;

        _velocity = Vector2.SmoothStep(_velocity, dust.velocity *= 0.9f, 1f);
        dust.position += _velocity *= 0.99f;

        if (dust.alpha == 4) {
            dust.scale *= 0.95f;
            if (dust.scale <= 0.1f) {
                dust.active = false;
            }
        }

        if (!Collision.SolidCollision(dust.position, 4, 4)) {
            dust.rotation += Helper.Wave(-0.1f, 0.1f, 0.5f, randomness);

            dust.position.X += Helper.Wave(-1f, 1f, 4f, randomness);
            dust.position.Y += Helper.Wave(0f, 0.5f, 2f, randomness);

            dust.position.Y += 0.2f;
        }
        else {
            if ((dust.scale *= 0.99f) <= 0f) {
                dust.active = false;
            }
        }

        return false;
    }
}