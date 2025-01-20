using Microsoft.Xna.Framework;

using RoA.Utilities;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FeatherDust : ModDust {
    private Vector2 _velocity;

    public override void OnSpawn(Dust dust) {
        dust.noGravity = true;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        float randomness = (float)dust.customData;

        _velocity = Vector2.SmoothStep(_velocity, dust.velocity *= 0.9f, 1f);
        dust.position += _velocity *= 0.99f;

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