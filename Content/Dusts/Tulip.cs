using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Utilities;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Tulip : ModDust {
    private const int FRAMECOUNT = 3;

    private Vector2 _velocity;

    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor;

    public override void OnSpawn(Dust dust) {
        dust.frame = (Texture2D?.Value?.Frame(FRAMECOUNT, FRAMECOUNT, frameX: dust.alpha, frameY: Main.rand.Next(FRAMECOUNT))).GetValueOrDefault();

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