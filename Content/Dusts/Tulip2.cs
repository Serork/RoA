using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Tulip2 : ModDust {
    private Vector2 _velocity;

    public const byte COLUMNCOUNT = 8;
    public const byte ROWCOUNT = 3;

    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor;

    public override void OnSpawn(Dust dust) {
        dust.frame = (Texture2D?.Value?.Frame(COLUMNCOUNT, ROWCOUNT, frameX: dust.alpha, frameY: Main.rand.Next(ROWCOUNT))).GetValueOrDefault();

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

        dust.scale *= 0.975f;
        dust.scale *= 0.9825f;
        if (dust.scale <= 1f) {
            dust.scale *= 0.95f;
        }
        if (dust.scale <= 0.15f) {
            dust.active = false;
        }

        if (!Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

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